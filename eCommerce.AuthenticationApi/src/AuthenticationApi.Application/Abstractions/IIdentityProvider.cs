using SharedLibrary.Application.Authorization;
using SharedLibrary.Domain.Abstractions;

namespace AuthenticationApi.Application.Abstractions;

/// <summary>
/// External identity provider used for credentials and access tokens.
/// </summary>
public interface IIdentityProvider
{
    Task<Result<string>> RegisterAsync(
        Guid accountId,
        string email,
        string password,
        string firstName,
        string lastName,
        CancellationToken cancellationToken = default);

    Task<Result<string>> RegisterAsync(
        Guid accountId,
        string email,
        string password,
        string firstName,
        string lastName,
        ApplicationRoles roleName,
        CancellationToken cancellationToken = default);

    Task<Result<TokenResponse>> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default);

    Task<Result<TokenResponse>> RefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default);

    Task<Result> ConfirmEmailAsync(string identityId, CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(string identityId, CancellationToken cancellationToken = default);
}
