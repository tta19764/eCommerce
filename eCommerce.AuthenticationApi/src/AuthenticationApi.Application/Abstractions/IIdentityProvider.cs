using SharedLibrary.Application.Authorization;
using SharedLibrary.Domain.Abstractions;

namespace AuthenticationApi.Application.Abstractions;

/// <summary>
/// External identity provider used for credentials and access tokens.
/// </summary>
public interface IIdentityProvider
{
    /// <summary>Creates an enabled, unverified Customer identity with a permanent password.</summary>
    /// <param name="accountId">The local account identifier available for provider correlation.</param>
    /// <param name="email">The identity username and email.</param>
    /// <param name="password">The permanent password.</param>
    /// <param name="firstName">The identity first name.</param>
    /// <param name="lastName">The identity last name.</param>
    /// <param name="cancellationToken">The token that cancels provider operations.</param>
    /// <returns>The provider subject identifier, or an identity-registration failure.</returns>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
    Task<Result<string>> RegisterAsync(
        Guid accountId,
        string email,
        string password,
        string firstName,
        string lastName,
        CancellationToken cancellationToken = default);

    /// <summary>Creates an enabled, unverified identity and assigns the specified realm role.</summary>
    /// <param name="accountId">The local account identifier available for provider correlation.</param>
    /// <param name="email">The identity username and email.</param>
    /// <param name="password">The permanent password.</param>
    /// <param name="firstName">The identity first name.</param>
    /// <param name="lastName">The identity last name.</param>
    /// <param name="roleName">The application role whose name identifies the Keycloak realm role.</param>
    /// <param name="cancellationToken">The token that cancels provider operations.</param>
    /// <returns>The provider subject identifier, or an identity-registration failure.</returns>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
    Task<Result<string>> RegisterAsync(
        Guid accountId,
        string email,
        string password,
        string firstName,
        string lastName,
        ApplicationRoles roleName,
        CancellationToken cancellationToken = default);

    /// <summary>Exchanges email and password credentials for access and refresh tokens.</summary>
    /// <param name="email">The identity username.</param>
    /// <param name="password">The identity password.</param>
    /// <param name="cancellationToken">The token that cancels provider authentication.</param>
    /// <returns>Tokens and calculated UTC expiry times, or an invalid-credentials failure.</returns>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
    Task<Result<TokenResponse>> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default);

    /// <summary>Exchanges a provider refresh token for new tokens.</summary>
    /// <param name="refreshToken">The provider refresh token.</param>
    /// <param name="cancellationToken">The token that cancels provider exchange.</param>
    /// <returns>New tokens and calculated UTC expiry times, or an invalid-credentials failure.</returns>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
    Task<Result<TokenResponse>> RefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default);

    /// <summary>Marks an identity email as verified.</summary>
    /// <param name="identityId">The provider subject identifier.</param>
    /// <param name="cancellationToken">The token that cancels the provider update.</param>
    /// <returns>A success result, or an email-confirmation failure.</returns>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
    Task<Result> ConfirmEmailAsync(string identityId, CancellationToken cancellationToken = default);

    /// <summary>Deletes an identity. Implementations treat an already-missing identity as success.</summary>
    /// <param name="identityId">The provider subject identifier.</param>
    /// <param name="cancellationToken">The token that cancels provider deletion.</param>
    /// <returns>A success result, or an identity-deletion failure.</returns>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
    Task<Result> DeleteAsync(string identityId, CancellationToken cancellationToken = default);
}
