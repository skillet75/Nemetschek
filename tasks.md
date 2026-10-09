# Tasks

## Task 1 — Solution skeleton
Status: Complete

The solution contains the `UserAccess.Api`, `Operative.Api`, and `Shared` projects with service-specific bootstraps and shared response contracts.

## Task 2 — Shared baseline and configuration
Status: Complete

Both APIs configure logging, health and info endpoints, OpenAPI in Development, and shared Problem Details behavior. Database connections and JWT settings are configurable through app settings and environment variables.

## Task 3 — EF Core SQLite data model
Status: Complete

Each service owns its entities, EF Core context, mappings, SQLite database path, and migration history. The user email is case-insensitively unique. Dice constraints enforce valid face values and sums. Each service applies its migrations at startup.

## Task 4 — User registration
Status: Complete

`POST /api/users` validates input, optionally accepts supported image data, hashes passwords, rejects duplicate email with `409`, and returns the created user. The repository maps only the SQLite unique violation for `Users.Email` to the duplicate result so concurrent registrations receive the same conflict response.

## Task 5 — Token creation
Status: Complete

`POST /api/auth/token` validates credentials, verifies the stored PBKDF2-SHA256 hash, and issues a configured JWT. Invalid credentials return `401`.

## Task 6 — JWT authentication in Operative.Api
Status: Complete

Operative validates issuer, audience, signing key, lifetime, and algorithm. Dice endpoints require bearer authentication; `GET /api/me` returns the authenticated subject identifier.

## Task 7 — Dice roll endpoint
Status: Complete

`POST /api/dice/roll` generates and persists a two-die roll for the authenticated user.

## Task 8 — History filters
Status: Complete

`GET /api/dice/history` scopes results to the authenticated user and supports year, month/year, and complete calendar-day filters. Unknown and repeated query parameters are rejected.

## Task 9 — History sorting
Status: Complete

History supports ascending and descending date and sum sorting, deterministic ID tie ordering, and sum-first precedence when both sort fields are set.

## Task 10 — History pagination
Status: Complete

History uses bounded page and page-size values, database-side pagination, and total count/page metadata.

## Task 11 — Error handling and validation
Status: Complete

Both APIs use shared Problem Details handling, structured validation responses, trace IDs, generic unexpected-error details, and safe application 4xx errors.

## Task 12 — Tests
Status: Complete

UserAccess tests cover registration, validation, duplicate email, image data, hashing, and authentication. Operative tests cover authorization, rolling, user scoping, filters, sorting, pagination, and OpenAPI metadata. Tests were not run as part of this review-resolution change.

## Task 13 — Run documentation and configuration
Status: Complete

`README.md` documents requirements, build/test/run commands, configuration, migrations, API contracts, and examples. `design.md` records service boundaries and startup migration behavior.

## Verification for this review update

- `dotnet build MicroservicesDemo.sln --no-restore` succeeded with zero warnings and errors after the review-resolution changes.
- `git diff --check` passed. Tests were not run as part of this update.
