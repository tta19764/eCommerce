# Users

## Purpose

UserApi owns commerce profile data, while AuthenticationApi owns credentials/account lifecycle. This split lets other domains use a stable profile `UserId` without owning Keycloak identities.

## Entities and relationships

`User` contains `Id`, validated first/last-name value objects, email, optional ImageApi `ImageId`, and timestamps. Authentication `Account` stores the linked `UserId` and Keycloak `IdentityId`. Reviews and orders reference the User ID as reviewer/client; they do not have database foreign keys to UserApi.

## Business rules

- Profile creation validates names/email and rejects duplicates through the handler/repository.
- Updates may independently change first name, last name, or image; value-object validation remains authoritative.
- Own-profile endpoints never trust a browser-supplied user ID: they resolve the bearer-token identity through AuthenticationApi.
- Account registration creates the profile through `CreateUserProfileRequest`; account deletion coordinates profile deletion.
- Uploaded profile images are attached through ImageApi messaging after a successful profile update.

## Application services and repositories

`CreateUserCommandHandler`, `GetUserQueryHandler`, `UpdateUserCommandHandler`, and `DeleteUserCommandHandler` use `IUserRepository`/`UserRepository` and `UserDbContext`. Updates mutate the tracked `User` through its domain method and are committed by the unit of work. Consumers implement create/delete/details contracts used by AuthenticationApi and ProductApi.

## API and frontend

- `GET/PUT /user-api/v1/users/own` — authenticated self service.
- `GET /user-api/v1/users/{userId}` — `users:read`.
- `PUT /user-api/v1/users/{userId}` — `users:update`.

`ProfilePage` calls `UsersApiClient.getOwn/updateOwn` and `ImagesApiClient.upload`. `AdminUsersPage` combines account pages with explicit user updates. `UserStore` loads the current profile after authentication.

## Dependencies

Depends on AuthenticationApi for identity mapping and ImageApi for profile images. Supplies identity/display data to AuthenticationApi account pages and [[Reviews]]. Referenced by [[Orders]] and [[Authentication Flow]].
