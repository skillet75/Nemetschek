# Tasks

## Task 1 — Solution skeleton
Status: Complete

### Deliverables
- `MicroservicesDemo.sln`
- `src/UserAccess.Api/UserAccess.Api.csproj`
- `src/Operative.Api/Operative.Api.csproj`
- `src/Shared/Shared.csproj`
- Shared response contracts
- API bootstrap classes

### Verification
- Solution built successfully using `dotnet build MicroservicesDemo.sln -nologo`.
- Result: build succeeded in 4.3s.

## Task 2 — Shared baseline and configuration
Status: Complete

## Task 3 — EF Core SQLite data model
Status: Complete

### Deliverables
- Service-owned `UserDbContext` and `OperativeDbContext`, entities, and EF Core mappings.
- Separate SQLite files under each API's `App_Data` directory.
- Independent initial migrations and model snapshots for both services.
- Database constraints for case-insensitive unique user email, basic email shape, valid dice values, and correct dice sums.

### Verification
- `dotnet build MicroservicesDemo.sln -nologo` succeeded.
- Both initial migrations were applied successfully to their respective SQLite files.
- `dotnet ef migrations has-pending-model-changes` reported no model changes for either service.
- `git diff --check` passed; service-local SQLite files are ignored by Git.

## Task 4 — user creation endpoint
Status: Complete

### Deliverables
- `POST /api/users` registration flow
- request DTO validation and duplicate email checks
- password hashing with PBKDF2
- user response contract

### Verification
- `dotnet test .\tests\UserAccess.Api.Tests\UserAccess.Api.Tests.csproj -nologo` passed.

## Task 5 — token creation endpoint
Status: Complete

### Deliverables
- `POST /api/auth/token`
- `CreateTokenRequest` validation for email/password
- password verification against stored hash
- JWT creation using configured issuer, audience, expiration, and signing key
- `AuthTokenResponse` payload with access token and expiry metadata

### Verification
- `dotnet test .\tests\UserAccess.Api.Tests\UserAccess.Api.Tests.csproj -nologo` passed.

## Remaining tasks

1. Task 6: JWT auth in operative service
2. Task 7: dice roll endpoint
3. Task 8: history query with filters
4. Task 9: sorting and precedence
5. Task 10: pagination
6. Task 11: centralized error handling and validation
7. Task 12: tests
8. Task 13: deployment-ready docs and configuration
