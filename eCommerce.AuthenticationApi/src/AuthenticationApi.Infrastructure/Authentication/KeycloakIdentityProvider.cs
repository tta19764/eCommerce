using System.Net.Http.Json;
using AuthenticationApi.Application;
using AuthenticationApi.Application.Abstractions;
using AuthenticationApi.Domain.Accounts;
using AuthenticationApi.Infrastructure.Authentication.Models;
using Microsoft.Extensions.Options;
using SharedLibrary.Application.Authorization;
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
    private const string RefreshTokenGrantType = "refresh_token";
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
        return await RegisterAsync(
            accountId,
            email,
            password,
            firstName,
            lastName,
            ApplicationRoles.Customer,
            cancellationToken);
    }

    public async Task<Result<string>> RegisterAsync(
        Guid accountId,
        string email,
        string password,
        string firstName,
        string lastName,
        ApplicationRoles roleName,
        CancellationToken cancellationToken = default)
    {
        var user = new UserRepresentationModel
        {
            Username = email,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            Enabled = true,
            EmailVerified = false,
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

            if (!response.IsSuccessStatusCode)
            {
                return Result.Failure<string>(AccountErrors.IdentityRegistrationFailed);
            }

            var identityId = await GetCreatedUserIdAsync(response, email, cancellationToken);

            if (string.IsNullOrWhiteSpace(identityId))
            {
                return Result.Failure<string>(AccountErrors.IdentityRegistrationFailed);
            }

            var roleAssignmentResult = await AssignRealmRoleAsync(
                identityId,
                roleName,
                cancellationToken);

            if (roleAssignmentResult.IsFailure)
            {
                await DeleteAsync(identityId, cancellationToken);
                return Result.Failure<string>(AccountErrors.IdentityRegistrationFailed);
            }

            return Result.Success(identityId);
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

        return await RequestTokenAsync(requestParameters, cancellationToken);
    }

    public async Task<Result<TokenResponse>> RefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        var requestParameters = new KeyValuePair<string, string>[]
        {
            new("client_id", _options.AuthClientId),
            new("client_secret", _options.AuthClientSecret),
            new("grant_type", RefreshTokenGrantType),
            new("refresh_token", refreshToken)
        };

        return await RequestTokenAsync(requestParameters, cancellationToken);
    }

    public async Task<Result> DeleteAsync(string identityId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await adminClient.DeleteAsync($"users/{Uri.EscapeDataString(identityId)}", cancellationToken);

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

    public async Task<Result> ConfirmEmailAsync(string identityId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await adminClient.PutAsJsonAsync(
                $"users/{Uri.EscapeDataString(identityId)}",
                new { emailVerified = true },
                cancellationToken);

            return response.IsSuccessStatusCode
                ? Result.Success()
                : Result.Failure(AccountErrors.EmailConfirmationFailed);
        }
        catch (HttpRequestException)
        {
            return Result.Failure(AccountErrors.EmailConfirmationFailed);
        }
    }

    private async Task<Result<TokenResponse>> RequestTokenAsync(
        IEnumerable<KeyValuePair<string, string>> requestParameters,
        CancellationToken cancellationToken)
    {
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

            if (token is null ||
                string.IsNullOrWhiteSpace(token.AccessToken) ||
                string.IsNullOrWhiteSpace(token.RefreshToken))
            {
                return Result.Failure<TokenResponse>(AccountErrors.InvalidCredentials);
            }

            var issuedAtUtc = DateTime.UtcNow;

            return Result.Success(new TokenResponse(
                token.AccessToken,
                issuedAtUtc.AddSeconds(token.ExpiresIn),
                token.RefreshToken,
                issuedAtUtc.AddSeconds(token.RefreshExpiresIn)));
        }
        catch (HttpRequestException)
        {
            return Result.Failure<TokenResponse>(AccountErrors.InvalidCredentials);
        }
    }

    private async Task<Result> AssignRealmRoleAsync(
        string identityId,
        string roleName,
        CancellationToken cancellationToken)
    {
        var role = await GetRealmRoleAsync(roleName, cancellationToken);

        if (role is null)
        {
            return Result.Failure(AccountErrors.IdentityRegistrationFailed);
        }

        var response = await adminClient.PostAsJsonAsync(
            $"users/{Uri.EscapeDataString(identityId)}/role-mappings/realm",
            new[] { role },
            cancellationToken);

        return response.IsSuccessStatusCode
            ? Result.Success()
            : Result.Failure(AccountErrors.IdentityRegistrationFailed);
    }

    private async Task<RoleRepresentationModel?> GetRealmRoleAsync(
        string roleName,
        CancellationToken cancellationToken)
    {
        var response = await adminClient.GetAsync(
            $"roles/{Uri.EscapeDataString(roleName)}",
            cancellationToken);

        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<RoleRepresentationModel>(cancellationToken)
            : null;
    }

    private async Task<string?> GetCreatedUserIdAsync(
        HttpResponseMessage response,
        string email,
        CancellationToken cancellationToken)
    {
        var location = response.Headers.Location?.ToString();

        if (!string.IsNullOrWhiteSpace(location))
        {
            var identityId = location.Split('/', StringSplitOptions.RemoveEmptyEntries).LastOrDefault();

            if (!string.IsNullOrWhiteSpace(identityId))
            {
                return identityId;
            }
        }

        var users = await adminClient.GetFromJsonAsync<UserRepresentationModel[]>(
            $"users?email={Uri.EscapeDataString(email)}&exact=true",
            cancellationToken);

        return users?.SingleOrDefault()?.Id;
    }
}
