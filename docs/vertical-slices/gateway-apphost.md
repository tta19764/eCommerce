# Gateway And AppHost

## General Description

The gateway and AppHost slice owns local orchestration and browser entry. `GatewayApi` is the only backend endpoint the frontend should call. AppHost wires services, infrastructure containers, configuration, ports, dependencies, and development tooling.

## Gateway API

`GatewayApi` reverse-proxies browser requests to downstream services and aggregates Swagger documents.

Gateway base URL:

```text
https://localhost:7059
```

Route prefixes:

| Prefix | Service |
| --- | --- |
| `/auth-api` | Authentication API |
| `/product-api` | Product API |
| `/order-api` | Order API |
| `/user-api` | User API |
| `/image-api` | Image API |

The gateway adds a shared gateway signature header when forwarding requests. Downstream services validate this header through shared middleware.

## AppHost

AppHost is the local orchestration entry point:

```text
eCommerce.AppHost/src/eCommerce.AppHost
```

It starts:

| Resource | Purpose |
| --- | --- |
| PostgreSQL | Per-service logical databases |
| RabbitMQ | MassTransit messaging |
| Keycloak | Identity provider |
| Redis | Distributed cache |
| Seq | Centralized logs |
| MinIO | Object storage for images |
| Mailpit | Local SMTP inbox |
| pgAdmin | Database inspection |
| Angular Web App | Local frontend development server |

## Configuration Rules

AppHost reads development configuration from:

```text
eCommerce.AppHost/src/eCommerce.AppHost/appsettings.Development.json
```

Service runtime values are passed through environment variables and Aspire references. Secrets should come from user secrets, environment variables, or deployment configuration rather than non-development appsettings.

SMTP credentials are supplied as AppHost parameters, not committed development settings:

| Value | Environment variable | User-secrets key |
| --- | --- | --- |
| Sender address | `Parameters__notification-from-address` | `Parameters:notification-from-address` |
| SMTP username | `Parameters__notification-smtp-user-name` | `Parameters:notification-smtp-user-name` |
| SMTP password | `Parameters__notification-smtp-password` | `Parameters:notification-smtp-password` |

## Frontend Integration

The Angular app should use the gateway URL from its environment configuration and should not know direct service ports.
