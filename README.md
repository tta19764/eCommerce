# eCommerce Shared Library

SharedLibrary contains cross-service building blocks for the eCommerce microservices.

## Application DI

Use `AddSharedApplication(params Assembly[] applicationAssemblies)` from each service application project.

The shared registration adds:

- MediatR handlers from the shared application assembly and the supplied service assemblies.
- FluentValidation validators from the same assemblies.
- Shared MediatR pipeline behaviors for logging and validation.

Service application projects should not call `AddMediatR` or `AddValidatorsFromAssembly` separately unless they need behavior that is intentionally not shared.

## Infrastructure DI

Use `AddSharedInfrastructure<TContext>(IConfiguration)` from each service infrastructure project.

The shared registration adds:

- JWT bearer authentication from the `Authentication` configuration section.
- Npgsql EF Core registration for `TContext` from `ConnectionStrings:Database`.
- Gateway validation options from the `Gateway` configuration section.

Service infrastructure projects should keep only service-specific registrations locally, such as repositories, unit-of-work aliases, message consumers, and external HTTP clients.

## Authentication Configuration

`AuthenticationOptions` is bound from the `Authentication` section:

```json
{
  "Authentication": {
    "Audience": "product-api",
    "MetadataUrl": "http://localhost:8080/realms/ecommerce/.well-known/openid-configuration",
    "RequireHttpsMetadata": false,
    "Issuer": "http://localhost:8080/realms/ecommerce"
  }
}
```

`Audience` must match an `aud` claim emitted by Keycloak for the target API. If each API uses a distinct audience such as `product-api`, `order-api`, or `user-api`, configure Keycloak audience mappers so tokens include the resource API audience.

For production, override `MetadataUrl`, `Issuer`, and `RequireHttpsMetadata` with environment-specific Keycloak values. Local `RequireHttpsMetadata: false` is only appropriate for local HTTP Keycloak development.
