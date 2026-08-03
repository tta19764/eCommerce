# Users Slice

## General Description

The users slice owns profile data linked to authentication accounts. Authentication creates profiles through MassTransit when accounts are registered. The User API exposes protected endpoints for reading and updating user profiles.

## Backend Projects

| Project | Responsibility |
| --- | --- |
| `UserApi.Api` | User profile endpoints |
| `UserApi.Application` | User commands, queries, and message consumers |
| `UserApi.Domain` | User profile domain rules |
| `UserApi.Infrastructure` | EF Core persistence and repositories |
| `UserApi.Messages` | User profile message contracts |

## Main Workflows

### Profile Creation

`AuthenticationApi` sends a `CreateUserProfileRequest` through MassTransit after a Keycloak account has been created. `UserApi` creates the profile and returns the profile ID. Authentication stores that ID on the account.

### Profile Update

Admins can update user profile details. The current update model supports changing the image without requiring first and last name changes.

### Account Pages With User Data

`AuthenticationApi` can page accounts with linked user profile data by requesting user data from `UserApi`.

## Endpoints

| Endpoint | Authorization | Description |
| --- | --- | --- |
| `GET /user-api/v1/users/{userId}` | `users:read` | Get user profile |
| `PUT /user-api/v1/users/{userId}` | `users:update` | Update profile names and/or image |

## Frontend Mapping

Frontend feature folders:

| Folder | Responsibility |
| --- | --- |
| `features/admin/pages/admin-users-page` | Admin user and account management |
| `core/api/accounts-api.client.ts` | Account and role data from Authentication API |
