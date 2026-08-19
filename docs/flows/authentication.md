# Authentication Flow

## Registration

1. `RegisterPage` calls `AuthStore`, then `AuthApiClient` at `POST /auth-api/v1/auth/register` or `/register/seller`.
2. GatewayApi proxies to `AuthenticationEndpoints`, which dispatches the corresponding registration command.
3. The handler normalizes/checks email, creates a Keycloak user with Customer or Seller role, creates a local `Account`, attempts to assign its local `Role`, and saves through `AccountRepository`/`AuthenticationDbContext`. A missing local role currently does not fail registration.
4. It requests UserApi to create a [[Users|User]] profile and stores the returned User ID on the account.
5. It publishes/sends email-confirmation work consumed by NotificationApi, which persists a notification job and later sends through SMTP.
6. The email link opens `/confirm-email`; `ConfirmEmailPage` calls the public confirmation endpoint, which verifies account/email, updates Keycloak email verification, and records local confirmation time.

The local account is committed before UserApi profile creation, and the profile link is committed before confirmation work is published. These operations do not share a transaction. Profile-creation failure deletes the local account and requests Keycloak identity deletion. If UserApi creates a profile but the local link fails, compensation deletes the account and identity but does not delete the created profile. A persistence or compensation failure can leave partial state. A broker failure after the final commit can leave a registered account without queued confirmation email.

Admin registration follows the same orchestration but `POST /register/admin` requires `accounts:create-admin`.

Seller registration creates credentials, the Seller role, and the normal UserApi profile. It does not create or approve a store. The user must submit a [[Sellers and Stores|store application]], and an administrator must approve it before ProductApi permits product creation.

## Account query cache

The administrator account-page query caches each page and page-size variant for five minutes. AuthenticationApi records each populated account-page cache key in a registry. Successful customer, seller, or administrator registration, email confirmation, and account deletion remove every registered account-page entry. Registration compensation also invalidates the cache when it deletes an account that was already persisted. Role pages use a separate cache and are not invalidated by account mutations because these operations do not change role or permission definitions.

## Development administrator bootstrap

When `BootstrapAdmin:Enabled` is explicitly true in Development, AuthenticationApi checks whether any local account already has the `Admin` role. A PostgreSQL advisory lock serializes this check across replicas and remains held during cross-service registration and confirmation. If none exists, the bootstrap uses the normal administrator registration workflow to create the Keycloak identity, local account/role link, and UserApi profile, then verifies email in both Keycloak and the local account. If the process previously reached account creation but not verification, the next run repairs confirmation for the configured bootstrap account. Any other existing administrator causes a no-op. The service makes at most five attempts with five seconds between failures; enabling it outside Development fails startup.

The bootstrap password has no source-controlled default. For AppHost development, store it with:

```powershell
dotnet user-secrets set "Parameters:bootstrap-admin-password" "choose-a-local-password" --project eCommerce.AppHost/src/eCommerce.AppHost
```

AppHost injects the secret as `BootstrapAdmin__Password`; only non-secret development identity fields are stored in `appsettings.Development.json`. No bootstrap configuration belongs in a non-development `appsettings.json`.

## Login, session, and refresh

1. `LoginPage` -> `AuthStore.login` -> `POST /auth/login`.
2. `LoginCommandHandler` checks the local account is active and email-confirmed, then requests Keycloak's password grant.
3. Tokens are returned to `AuthStore`, stored in `sessionStorage`, and decoded only to drive UI role state.
4. The HTTP interceptor attaches the access token. On eligible authorization failure it uses `POST /auth/refresh` and retries; logout clears session state.
5. Backend JWT validation and permission/ownership policies remain authoritative.

Login maps all Keycloak token failures to invalid credentials. Refresh-token exchange calls Keycloak directly and does not re-check the local account's active or confirmation state.

## Identity-to-profile resolution

Own-resource endpoints read the Keycloak subject claim and issue `GetAccountUserIdByIdentityIdRequest` to AuthenticationApi. The response supplies the linked User ID used by [[Users]], [[Orders]], [[Reviews]], and MessagingApi.

## Failure/consistency boundary

Registration spans Keycloak, Authentication DB, User DB, cache storage, and notification messaging; it is orchestration rather than one atomic transaction. Email confirmation marks Keycloak verified before committing the local timestamp, so a local save failure can temporarily split confirmation state.

Account deletion runs in the opposite cross-service sequence: it deletes the UserApi profile, commits local account deletion, invalidates account-page caches, and then requests Keycloak identity deletion. A local save failure can leave an account linked to a deleted profile. Keycloak deletion failure is logged after the local deletion and the command still returns success, leaving identity cleanup as an operational task.
