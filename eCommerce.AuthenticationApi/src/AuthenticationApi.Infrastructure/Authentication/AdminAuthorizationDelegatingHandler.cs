using System.Net.Http.Headers;
using System.Net.Http.Json;
using AuthenticationApi.Infrastructure.Authentication.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;

namespace AuthenticationApi.Infrastructure.Authentication;

/// <summary>
/// Adds an admin client-credentials bearer token to Keycloak admin API calls.
/// </summary>
/// <param name="options">The Keycloak token URL and admin service-account credentials.</param>
/// <remarks>A new service-account token is requested for every outgoing admin request; tokens are not cached.</remarks>
public sealed class AdminAuthorizationDelegatingHandler(IOptions<KeycloakOptions> options) : DelegatingHandler
{
    private readonly KeycloakOptions _options = options.Value;

    /// <summary>Acquires an admin token, attaches it as a bearer token, and sends the original request.</summary>
    /// <param name="request">The Keycloak Admin API request.</param>
    /// <param name="cancellationToken">The token that cancels token acquisition and the admin request.</param>
    /// <returns>The Keycloak Admin API response.</returns>
    /// <exception cref="HttpRequestException">The token endpoint returns a non-success status.</exception>
    /// <exception cref="InvalidOperationException">The token response has no deserializable body.</exception>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var token = await GetAuthorizationTokenAsync(cancellationToken);

        // Keycloak admin endpoints require a service-account token, separate from user login tokens.
        request.Headers.Authorization = new AuthenticationHeaderValue(
            JwtBearerDefaults.AuthenticationScheme,
            token.AccessToken);

        return await base.SendAsync(request, cancellationToken);
    }

    private async Task<AuthorizationToken> GetAuthorizationTokenAsync(CancellationToken cancellationToken)
    {
        var requestParameters = new KeyValuePair<string, string>[]
        {
            new("client_id", _options.AdminClientId),
            new("client_secret", _options.AdminClientSecret),
            new("scope", "openid email"),
            new("grant_type", "client_credentials")
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, _options.TokenUrl)
        {
            Content = new FormUrlEncodedContent(requestParameters)
        };

        var response = await base.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<AuthorizationToken>(cancellationToken) ??
               throw new InvalidOperationException("Keycloak token response was empty.");
    }
}
