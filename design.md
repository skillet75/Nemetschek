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
- User registration and token authentication endpoints
- Application services for registration and token authentication, backed by repository and password-hashing abstractions

### Operative.Api
- Startup and middleware configuration
- Health and info route exposure
- Dice rolling and user-scoped history endpoints with filtering, sorting, and pagination
- JWT validation configuration

## Persistence design (Task 3)
- `UserAccess.Api` owns `UserDbContext`, the `User` entity, its EF Core configuration, migrations, and the `users.db` SQLite file.
- `Operative.Api` owns `OperativeDbContext`, the `DiceRoll` entity, its EF Core configuration, migrations, and the `operative.db` SQLite file.
- Entities live in their service domain namespaces; EF Core contexts, mappings, and migrations live under that service's infrastructure persistence namespace.
- SQLite provider and EF Core packages are referenced by each owning API only. `Shared` remains provider- and persistence-agnostic.
- Each API reads `ConnectionStrings:DefaultConnection`, with a service-specific SQLite file as its local default. The containing directory is created by the persistence registration before database use.
- `DiceRoll.UserId` is an external identity value from the user access service. There is intentionally no cross-service foreign key or navigation property; user ownership is enforced from authenticated identity by operative endpoints.
- Database constraints enforce required user names, a basic email shape, case-insensitive email uniqueness, die values from 1 to 6, and `Sum = Die1 + Die2`. Full request validation remains an application/API responsibility.
- Initial migrations are generated and stored independently for each service. Both API bootstraps call EF Core `Database.Migrate()` during startup, so starting a service can create or change its database schema. This is convenient for the local demo; production deployments should use a controlled migration step with appropriate database permissions and rollout coordination.

## Registration and authentication boundaries
- Controllers handle HTTP binding and response mapping; registration and authentication orchestration live in application services.
- Application services depend on `IUserRepository` and `IPasswordHasher`, not on EF Core or the concrete password hashing implementation.
- Infrastructure implements those abstractions with EF Core SQLite persistence and PBKDF2-SHA256 hashing.
- The UserAccess API exposes registration and token creation. It does not expose a public user lookup endpoint because that operation is not part of the source task or current API contract.

## Why this matches SOLID and OOP
- Single Responsibility: each project owns a discrete domain boundary.
- Open/Closed: new features can be added by extending capabilities without rewriting bootstraps.
- Dependency Inversion: shared contracts are defined in the shared assembly, not in the concrete APIs.
- Encapsulation: bootstrap logic is isolated in static application builders.
