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

The current create consumer does not correlate the request to an existing account or profile. Request redelivery can create another profile. Names and email are validated, but UserApi does not enforce profile or email uniqueness during creation.

### Own Profile

Authenticated users can read and update their own profile through `/users/own`. The backend reads the identity id from token claims and requests the linked profile `UserId` from `AuthenticationApi`, so the frontend does not send a user ID for self-service profile workflows.

Profile pictures use the Image API first. The frontend uploads the file through `POST /image-api/v1/images`, then calls `PUT /user-api/v1/users/own` with the returned `imageId`.

When an image ID is supplied, UserApi asks ImageApi to attach it before it validates and commits the local profile update. The services do not share a transaction. A later UserApi failure can leave the image attached without a profile reference. A null image ID clears the current image; the update contract does not distinguish omission from an explicit clear.

### Admin Profile Update

Admins can read and update user profile details by explicit user ID. The current update model supports changing the image without requiring first and last name changes.

Profile deletion is allowed only when OrderApi reports that the user owns no orders. Existing order history blocks deletion.

### Account Pages With User Data

`AuthenticationApi` can page accounts with linked user profile data by requesting user data from `UserApi`.

## Endpoints

| Endpoint | Authorization | Description |
| --- | --- | --- |
| `GET /user-api/v1/users/own` | Authenticated | Get current user's profile from claims |
| `PUT /user-api/v1/users/own` | Authenticated | Update current user's profile from claims |
| `GET /user-api/v1/users/{userId}` | `users:read` | Get user profile |
| `PUT /user-api/v1/users/{userId}` | `users:update` | Update profile names and/or image |

## Frontend Mapping

Frontend feature folders:

| Folder | Responsibility |
| --- | --- |
| `features/profile/pages/profile-page` | Current user's own profile read/update and profile-picture upload |
| `features/admin/pages/admin-users-page` | Admin user and account management |
| `core/api/accounts-api.client.ts` | Account and role data from Authentication API |
