# MicroservicesDemo

A .NET 10 interview demo with two ASP.NET Core services. `UserAccess.Api` registers users and issues JWTs. `Operative.Api` validates those JWTs and stores per-user dice rolls. Each service owns a separate SQLite database and applies its EF Core migrations on startup.

The project is intended to run locally. Its checked-in SQLite paths and JWT key are demo defaults; use environment variables to override them for a local run when needed.

## Requirements

- .NET 10 SDK
- Windows, macOS, or Linux

## Build and tests

From the repository root:

```sh
dotnet restore MicroservicesDemo.sln
dotnet build MicroservicesDemo.sln
dotnet test MicroservicesDemo.sln
```

Integration tests use a unique SQLite file in the operating system temporary directory for each test fixture. The fixture deletes the database and SQLite sidecar files when disposed, so test-created users and dice rolls do not accumulate in the demo databases under the service projects.

## EF Core migrations

The repository pins the EF Core CLI to the same `10.0.12` version as the EF Core packages. Restore the local tool once after cloning (or after changing the tool manifest):

```sh
dotnet tool restore
```

Check whether either service's EF model has changes that need a migration:

```sh
dotnet tool run dotnet-ef -- migrations has-pending-model-changes --project src/UserAccess.Api/UserAccess.Api.csproj
dotnet tool run dotnet-ef -- migrations has-pending-model-changes --project src/Operative.Api/Operative.Api.csproj
```

Both commands should report that no changes have been made to the model since the last migration. To add a migration, run `dotnet tool run dotnet-ef -- migrations add <MigrationName> --project <service-project>` from the repository root.

## Run locally

Run each service in a separate terminal from the repository root:

```sh
dotnet run --project src/UserAccess.Api
```

```sh
dotnet run --project src/Operative.Api
```

The default launch URLs are `http://localhost:5216` for `UserAccess.Api` and `http://localhost:5253` for `Operative.Api`; ASP.NET Core prints the active URLs when each service starts. In Development, each service exposes its OpenAPI document at `/openapi/v1.json` and Scalar UI at `/scalar`. Both expose `/health`. SQLite files are created under each service's `App_Data` directory by default. To reset demo data, stop both services and remove those database files.

## Configuration

Configuration can be supplied through ASP.NET Core environment variables. Double underscores (`__`) represent nested configuration keys. The defaults are enough to run the demo. To override them, set the database path and the same JWT issuer, audience, and key in each service's terminal. The signing key must contain at least 32 UTF-8 bytes.

PowerShell example for `UserAccess.Api`:

```powershell
$env:ConnectionStrings__DefaultConnection = 'Data Source=App_Data/users.db'
$env:Jwt__Issuer = 'https://localhost'
$env:Jwt__Audience = 'UserAccess.Api'
$env:Jwt__Key = 'local-demo-signing-key-change-this-value-123456789'
dotnet run --project src/UserAccess.Api
```

In a second terminal, use its own database path and the same JWT values for `Operative.Api`:

```powershell
$env:ConnectionStrings__DefaultConnection = 'Data Source=App_Data/operative.db'
$env:Jwt__Issuer = 'https://localhost'
$env:Jwt__Audience = 'UserAccess.Api'
$env:Jwt__Key = 'local-demo-signing-key-change-this-value-123456789'
dotnet run --project src/Operative.Api
```

The app resolves relative database paths from each service's content root and creates the containing directory.

## API contract

Most successful endpoints wrap data in `{ "data": ..., "message": ... }`. `GET /api/users/{id}` returns the `UserResponse` object directly. Validation and application errors use Problem Details; validation errors use `application/problem+json`.

### UserAccess.Api

| Method and path | Auth | Behavior |
| --- | --- | --- |
| `POST /api/users` | None | Creates a user. Body: `firstName`, `lastName`, `email`, `password`, and optional `image`. When present, `image` must be a PNG, JPEG, or WebP data URI (`data:image/<type>;base64,...`) whose decoded image is no larger than 5 MiB and whose bytes start with the matching image file signature. The API stores decoded bytes as a SQLite BLOB with its media type and returns the image as a data URI. Returns `201` with user id, names, email, image, and creation time. Password is PBKDF2-SHA256 hashed. Duplicate email returns `409`; invalid input returns `400`. |
| `GET /api/users/{id}` | None | Returns a user by GUID, or `404`. |
| `POST /api/auth/token` | None | Body: `email`, `password`. Returns `200` with `accessToken`, `tokenType` (`Bearer`), and `expiresAtUtc`; bad credentials return `401`. |
| `GET /health` | None | Health check. |
| `GET /api/info` | None | Service description. |

Example login:

```sh
curl -X POST http://localhost:<user-access-port>/api/auth/token \
  -H 'Content-Type: application/json' \
  -d '{"email":"ada@example.com","password":"Strong123!"}'
```

### Operative.Api

Except for the health, info, and Development-only documentation endpoints, requests require `Authorization: Bearer <accessToken>` issued by `UserAccess.Api`.

| Method and path | Behavior |
| --- | --- |
| `POST /api/dice/roll` | Rolls two six-sided dice, persists the result for the JWT subject, and returns id, user id, each die, sum, and UTC creation time. |
| `GET /api/dice/history` | Returns only the authenticated user's rolls in a paged response (`items`, `pageNumber`, `pageSize`, `totalCount`, `totalPages`). |
| `GET /api/me` | Returns the authenticated user's GUID. |
| `GET /health` | Health check. |
| `GET /api/info` | Service description. |

History query options:

- Date filter: `year`; optionally `month` (requires year); optionally `day` (requires year and month, and must be a valid calendar date).
- Sort: `dateSort=asc|desc`, `sumSort=asc|desc`. When both are supplied, sum is primary and date/time is the secondary key. Results have deterministic tie ordering.
- Pagination: `page` starts at 1 (default 1); `pageSize` is 1 through 100 (default 20).
- Unknown or repeated query parameters and invalid filters/sorts/pagination return `400`.

Example authenticated roll and history:

```sh
curl -X POST http://localhost:<operative-port>/api/dice/roll \
  -H 'Authorization: Bearer <accessToken>'
curl 'http://localhost:<operative-port>/api/dice/history?page=1&pageSize=20&year=2026&sumSort=desc' \
  -H 'Authorization: Bearer <accessToken>'
```

## Data and scope

The user database and dice history database are separate SQLite files, matching the service ownership in the assignment. The current-user identity in `Operative.Api` comes from a validated JWT; no user database lookup is performed there. This is appropriate for the demo's two-service flow, where authentication and account lifecycle remain owned by `UserAccess.Api`.
