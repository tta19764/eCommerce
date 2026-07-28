using System.Net.Http.Headers;
using System.Net.Http.Json;
using AuthenticationApi.Infrastructure.Authentication.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;

namespace AuthenticationApi.Infrastructure.Authentication;

/// <summary>
/// Adds an admin client-credentials bearer token to Keycloak admin API calls.
/// </summary>
public sealed class AdminAuthorizationDelegatingHandler(IOptions<KeycloakOptions> options) : DelegatingHandler
{
    private readonly KeycloakOptions _options = options.Value;

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var token = await GetAuthorizationTokenAsync(cancellationToken);

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
