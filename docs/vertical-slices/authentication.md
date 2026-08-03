# Authentication Slice

## General Description

The authentication slice owns account registration, administrator registration, login, token refresh, email confirmation, account deletion, roles, permissions, and Keycloak integration. It is implemented primarily in `AuthenticationApi` and uses `UserApi` and `NotificationApi` through MassTransit for profile creation and confirmation email scheduling.

## Backend Projects

| Project | Responsibility |
| --- | --- |
| `AuthenticationApi.Api` | Versioned auth endpoints |
| `AuthenticationApi.Application` | Commands, queries, validators, handlers |
| `AuthenticationApi.Domain` | Account, role, permission, email value object |
| `AuthenticationApi.Infrastructure` | EF Core, Keycloak, repositories, migrations |

## Main Workflows

### Customer Registration

1. Client posts registration data.
2. Account email is normalized and checked for duplicates.
3. Keycloak user is created with the `Customer` role.
4. Local account is created and linked to the Keycloak identity.
5. User profile is created through MassTransit request/response to `UserApi`.
6. Email confirmation notification is published to `NotificationApi`.

### Admin Registration

Administrator registration uses the same core registration flow but assigns the `Admin` role. The endpoint requires `accounts:create-admin`, so only existing administrators can create administrator accounts.

### Email Confirmation

The email confirmation page calls:

```http
GET /auth-api/v1/auth/confirm-email?accountId={accountId}&email={email}
```

The handler verifies that the account exists, is active, and matches the supplied email. It then marks the Keycloak user as email verified and stores `EmailConfirmedAtUtc` locally.

### Login And Refresh

Login validates the local account first, including active status and email confirmation. Keycloak validates credentials and issues access and refresh tokens. Refresh uses the Keycloak refresh-token grant through `AuthenticationApi`.

## Endpoints

| Endpoint | Authorization | Description |
| --- | --- | --- |
| `POST /auth-api/v1/auth/register` | Public | Register customer account |
| `POST /auth-api/v1/auth/register/admin` | `accounts:create-admin` | Register administrator account |
| `GET /auth-api/v1/auth/confirm-email` | Public | Confirm account email |
| `POST /auth-api/v1/auth/login` | Public | Login with email and password |
| `POST /auth-api/v1/auth/refresh` | Public | Refresh tokens |
| `GET /auth-api/v1/auth/roles` | `users:read` | Page roles with permissions |
| `GET /auth-api/v1/auth/accounts` | `users:read` | Page accounts with user profile data |
| `DELETE /auth-api/v1/auth/accounts/{accountId}` | `users:update` | Delete account |

## Roles And Permissions

| Role | Permissions |
| --- | --- |
| `Customer` | `products:read`, `orders:create` |
| `Admin` | All configured application permissions |

Important permissions:

| Permission | Description |
| --- | --- |
| `accounts:create-admin` | Create administrator accounts |
| `users:read` | Read users, accounts, roles |
| `users:update` | Update users and delete accounts |

## Persistence

The authentication database stores:

- Accounts
- Account roles
- Roles
- Permissions
- Role permissions
- Email confirmation timestamp

EF Core migrations seed roles and permissions as reference data.
