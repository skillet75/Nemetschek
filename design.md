# Design

## Architecture overview
The solution follows a thin, layered architecture aligned with Clean Architecture principles:

- `Shared` project holds shared contracts and reusable cross-cutting models.
- `UserAccess.Api` owns user registration, authentication, and API hosting concerns.
- `Operative.Api` owns dice-related operations and API hosting concerns.
- Service bootstrapping is centralized in each API project through dedicated infrastructure bootstrap classes.

## Dependency direction
Dependencies flow inward:

- API projects depend on the shared contracts project.
- Shared is intentionally infrastructure-agnostic and reusable across services.
- No service depends on the implementation details of the other service.

## Project responsibilities
### Shared
- Cross-service DTOs and contract models
- Standard response envelopes and error models
- Reusable constants or validation helpers as future requirements require

### UserAccess.Api
- Startup and middleware configuration
- Health and info route exposure
- Future endpoint controllers and authentication services

### Operative.Api
- Startup and middleware configuration
- Health and info route exposure
- Future dice logic and JWT validation configuration

## Why this matches SOLID and OOP
- Single Responsibility: each project owns a discrete domain boundary.
- Open/Closed: new features can be added by extending capabilities without rewriting bootstraps.
- Dependency Inversion: shared contracts are defined in the shared assembly, not in the concrete APIs.
- Encapsulation: bootstrap logic is isolated in static application builders.
