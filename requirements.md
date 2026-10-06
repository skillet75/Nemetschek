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
