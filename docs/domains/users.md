# Users

## Purpose

UserApi owns commerce profile data, while AuthenticationApi owns credentials/account lifecycle. This split lets other domains use a stable profile `UserId` without owning Keycloak identities.

## Entities and relationships

`User` contains `Id`, validated first/last-name value objects, email, optional ImageApi `ImageId`, and timestamps. Authentication `Account` stores the linked `UserId` and Keycloak `IdentityId`. Reviews and orders reference the User ID as reviewer/client; they do not have database foreign keys to UserApi.

## Business rules

- Profile creation validates required names and email. The current handler and consumer do not check for an existing profile or email, so a repeated create request can create another record.
- Updates may independently change first name or last name; value-object validation remains authoritative. A null `ImageId` clears the current image because the command does not distinguish an omitted image field from an explicit clear.
- Own-profile endpoints never trust a browser-supplied user ID: they resolve the bearer-token identity through AuthenticationApi.
- Account registration creates the profile through `CreateUserProfileRequest`; account deletion coordinates profile deletion.
- A supplied uploaded profile image is attached through ImageApi messaging before local profile validation and persistence. ImageApi and UserApi do not share a transaction, so a later UserApi failure can leave an attached image that the profile does not reference.

## Application services and repositories

`CreateUserCommandHandler`, `GetUserQueryHandler`, `UpdateUserCommandHandler`, and `DeleteUserCommandHandler` use `IUserRepository`/`UserRepository` and `UserDbContext`. Updates mutate the tracked `User` through its domain method and are committed by the unit of work. Consumers implement create/delete/details contracts used by AuthenticationApi and ProductApi.

Deletion asks OrderApi whether the user owns any order. Any existing order blocks profile deletion. The profile-create consumer returns domain validation failures in its response, but it has no idempotency or account-correlation key and can create duplicate profiles after request redelivery.

## API and frontend

- `GET/PUT /user-api/v1/users/own` — authenticated self service.
- `GET /user-api/v1/users/{userId}` — `users:read`.
- `PUT /user-api/v1/users/{userId}` — `users:update`.

`ProfilePage` calls `UsersApiClient.getOwn/updateOwn` and `ImagesApiClient.upload`. `AdminUsersPage` combines account pages with explicit user updates. `UserStore` loads the current profile after authentication.

## Dependencies

Depends on AuthenticationApi for identity mapping and ImageApi for profile images. Supplies identity/display data to AuthenticationApi account pages and [[Reviews]]. Referenced by [[Orders]] and [[Authentication Flow]].
