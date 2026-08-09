# Authentication Flow

## Registration

1. `RegisterPage` calls `AuthStore`, then `AuthApiClient` at `POST /auth-api/v1/auth/register` or `/register/seller`.
2. GatewayApi proxies to `AuthenticationEndpoints`, which dispatches the corresponding registration command.
3. The handler normalizes/checks email, creates a Keycloak user with Customer or Seller role, creates a local `Account`, assigns its local `Role`, and saves through `AccountRepository`/`AuthenticationDbContext`.
4. It requests UserApi to create a [[Users|User]] profile and stores the returned User ID on the account.
5. It publishes/sends email-confirmation work consumed by NotificationApi, which persists a notification job and later sends through SMTP.
6. The email link opens `/confirm-email`; `ConfirmEmailPage` calls the public confirmation endpoint, which verifies account/email, updates Keycloak email verification, and records local confirmation time.

Admin registration follows the same orchestration but `POST /register/admin` requires `accounts:create-admin`.

## Login, session, and refresh

1. `LoginPage` -> `AuthStore.login` -> `POST /auth/login`.
2. `LoginCommandHandler` checks the local account is active and email-confirmed, then requests Keycloak's password grant.
3. Tokens are returned to `AuthStore`, stored in `sessionStorage`, and decoded only to drive UI role state.
4. The HTTP interceptor attaches the access token. On eligible authorization failure it uses `POST /auth/refresh` and retries; logout clears session state.
5. Backend JWT validation and permission/ownership policies remain authoritative.

## Identity-to-profile resolution

Own-resource endpoints read the Keycloak subject claim and issue `GetAccountUserIdByIdentityIdRequest` to AuthenticationApi. The response supplies the linked User ID used by [[Users]], [[Orders]], [[Reviews]], and MessagingApi.

## Failure/consistency boundary

Registration spans Keycloak, Authentication DB, User DB, and notification messaging; it is orchestration rather than one atomic transaction. Handler compensation/error paths should be reviewed whenever this flow changes.
