# API

Browser clients use the gateway URL from `environment.gatewayUrl` and service prefixes such as `/product-api`. Service endpoints are versioned under `/v1` and generally return `ApiResponse<T>`; paged reads wrap `PagedListResponse<T>`.

- [[API Endpoints]] — implemented route inventory and authorization
- [[Payment API Contracts]] — planned Stripe/payment routes and message boundary
- [[Authentication Flow]] — token acquisition and caller resolution
- [[Backend Architecture]] — gateway and service execution model

The gateway also exposes aggregated OpenAPI support. Raw image content and successful `204` mutations are exceptions to the JSON envelope pattern.
