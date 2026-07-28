using SharedLibrary.Domain.Abstractions;

namespace AuthenticationApi.Application.Abstractions;

/// <summary>
/// External identity provider used for credentials and access tokens.
/// </summary>
public interface IIdentityProvider
{
    Task<Result> RegisterAsync(
        Guid accountId,
        string email,
        string password,
        string firstName,
        string lastName,
        CancellationToken cancellationToken = default);

    Task<Result<TokenResponse>> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(Guid accountId, CancellationToken cancellationToken = default);
}
