# Requirements

## User story
As a developer evaluating the microservice exercise, I need a .NET 10 solution skeleton that separates the two API services and shared contracts so that each service can evolve independently while honoring clean architecture boundaries.

## EARS requirements
- WHEN the solution is opened, THE SYSTEM SHALL expose a single .NET solution named `MicroservicesDemo`.
- WHEN the solution is built, THE SYSTEM SHALL compile the `Shared`, `UserAccess.Api`, and `Operative.Api` projects successfully.
- WHEN the user access service starts, THE SYSTEM SHALL host a minimal Web API with health and metadata endpoints.
- WHEN the operative service starts, THE SYSTEM SHALL host a minimal Web API with health and metadata endpoints.
- WHEN the application is extended, THE SYSTEM SHALL keep API hosting concerns, infrastructure concerns, and shared contracts separated by project boundaries.

## Acceptance criteria for Task 1
- The solution contains a root-level `MicroservicesDemo.sln` file.
- There are three .NET projects: `Shared`, `UserAccess.Api`, and `Operative.Api`.
- Each project targets .NET 10.
- The solution builds successfully without errors.
- Both API projects are ready for further feature implementation in a clean, modular structure.

## EARS requirements for Task 3
- WHEN the user access service persists users, THE SYSTEM SHALL store user records in its own SQLite database with a securely-hashable password field, optional image path, and UTC creation timestamp.
- WHEN the operative service persists dice rolls, THE SYSTEM SHALL store dice records in its own SQLite database, associated with the authenticated user identifier and a UTC creation timestamp.
- WHEN either database is initialized, THE SYSTEM SHALL apply schema migrations owned by that service.
- IF a user email is empty, malformed at the basic database-constraint level, or duplicated without regard to case, THEN THE SYSTEM SHALL reject the record.
- IF a dice value is outside 1 through 6 or its sum does not match both dice, THEN THE SYSTEM SHALL reject the record.
- THE SYSTEM SHALL keep EF Core persistence dependencies and entities inside their owning service and SHALL NOT create a database foreign key across service boundaries.

## Acceptance criteria for Task 3
- Each API owns a separate EF Core `DbContext`, SQLite connection, and migration set.
- The user model includes id, first name, last name, email, password hash, optional image path, and created timestamp; email is unique case-insensitively and has a basic database validity check.
- The dice model includes id, user id, both die values, sum, and created timestamp; database constraints enforce die bounds and sum correctness.
- The solution restores and builds with EF Core SQLite dependencies.
