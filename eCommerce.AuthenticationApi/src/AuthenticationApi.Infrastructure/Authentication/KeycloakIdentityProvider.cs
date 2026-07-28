using System.Net.Http.Json;
using AuthenticationApi.Application;
using AuthenticationApi.Application.Abstractions;
using AuthenticationApi.Domain.Accounts;
using AuthenticationApi.Infrastructure.Authentication.Models;
using Microsoft.Extensions.Options;
using SharedLibrary.Domain.Abstractions;

namespace AuthenticationApi.Infrastructure.Authentication;

/// <summary>
/// Keycloak-backed identity provider for credentials and access tokens.
/// </summary>
public sealed class KeycloakIdentityProvider(
    HttpClient adminClient,
    IHttpClientFactory httpClientFactory,
    IOptions<KeycloakOptions> options) : IIdentityProvider
{
    private const string PasswordCredentialType = "password";
    private const string TokenClientName = "Keycloak.Token";

    private readonly KeycloakOptions _options = options.Value;

    public async Task<Result<string>> RegisterAsync(
        Guid accountId,
        string email,
        string password,
        string firstName,
        string lastName,
        CancellationToken cancellationToken = default)
    {
        var user = new UserRepresentationModel
        {
            // Supplying the account id keeps the Keycloak subject aligned with local account/profile ids.
            Id = accountId.ToString(),
            Username = email,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            Enabled = true,
            EmailVerified = true,
            CreatedTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            Credentials =
            [
                new CredentialRepresentationModel
                {
                    Type = PasswordCredentialType,
                    Value = password,
                    Temporary = false
                }
            ]
        };

        try
        {
            var response = await adminClient.PostAsJsonAsync("users", user, cancellationToken);

            return response.IsSuccessStatusCode
                ? Result.Success(accountId.ToString())
                : Result.Failure<string>(AccountErrors.IdentityRegistrationFailed);
        }
        catch (HttpRequestException)
        {
            return Result.Failure<string>(AccountErrors.IdentityRegistrationFailed);
        }
    }

    public async Task<Result<TokenResponse>> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        var requestParameters = new KeyValuePair<string, string>[]
        {
            new("client_id", _options.AuthClientId),
            new("client_secret", _options.AuthClientSecret),
            // Roles are requested for future resource-api authorization once Keycloak mappers are configured.
            new("scope", "openid email roles"),
            new("grant_type", PasswordCredentialType),
            new("username", email),
            new("password", password)
        };

        try
        {
            var tokenClient = httpClientFactory.CreateClient(TokenClientName);
            var response = await tokenClient.PostAsync(
                string.Empty,
                new FormUrlEncodedContent(requestParameters),
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return Result.Failure<TokenResponse>(AccountErrors.InvalidCredentials);
            }

            var token = await response.Content.ReadFromJsonAsync<AuthorizationToken>(cancellationToken);

            if (token is null || string.IsNullOrWhiteSpace(token.AccessToken))
            {
                return Result.Failure<TokenResponse>(AccountErrors.InvalidCredentials);
            }

            var expiresAtUtc = DateTime.UtcNow.AddSeconds(token.ExpiresIn);

            return Result.Success(new TokenResponse(token.AccessToken, expiresAtUtc));
        }
        catch (HttpRequestException)
        {
            return Result.Failure<TokenResponse>(AccountErrors.InvalidCredentials);
        }
    }

    public async Task<Result> DeleteAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await adminClient.DeleteAsync($"users/{accountId}", cancellationToken);

            // Deleting an already-missing Keycloak user is idempotent from the service's perspective.
            return response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.NotFound
                ? Result.Success()
                : Result.Failure(AccountErrors.IdentityDeletionFailed);
        }
        catch (HttpRequestException)
        {
            return Result.Failure(AccountErrors.IdentityDeletionFailed);
        }
    }
}
