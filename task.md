# .NET 10 Microservices Implementation Plan

## Source requirements

This plan is based on the task description in the repository and the PDF version of the same brief. The solution should be a .NET Core 10 Web API implementation with two microservices:

1. User data access service
   - `POST /api/users` to create a user
   - `POST /api/auth/token` to create an access token
   - Inputs: first name, last name, email, password, image
   - Passwords must be stored securely
   - Token response should be a generated JWT or equivalent access token

2. Operative service
   - `POST /api/dice/roll` to simulate rolling two dice
   - `GET /api/dice/history` to retrieve saved records for the current user
   - All calls require token-based authentication
   - Saved dice results must be stored per user
   - History supports filters by all records / year / month-year / day
   - Sorting supports date/time and dice-sum in both directions
   - Sorting precedence: when both sort directions are used, the date filter group has more weight
   - Pagination is required with page size and page index metadata

## Recommended architecture

- Solution name: `MicroservicesDemo`
- Projects:
  - `UserAccess.Api`
  - `Operative.Api`
  - `Shared` (shared DTOs, constants, validation helpers, JWT settings)
  - `Infrastructure` or `Data` projects if needed for persistence reuse
- Database: SQLite with Entity Framework Core, using a separate database file owned by each service
- SQLite trade-offs: no database server installation is needed, and the files are easy to create and move for local development. SQLite is appropriate for this interview exercise, but its single-writer model and limited suitability for shared, multi-instance deployments make PostgreSQL or SQL Server a better choice if usage or deployment requirements grow.
- Authentication: JWT Bearer tokens issued by `UserAccess.Api` and validated by `Operative.Api`
- Error handling: centralized exception middleware and consistent API error response contracts
- Validation: FluentValidation or DataAnnotations on request DTOs and API model validation

## Agent-ready implementation tasks

### Task 1: Create solution skeleton
- Goal: create the .NET solution and project structure
- Deliverables:
  - `MicroservicesDemo.sln`
  - `UserAccess.Api` ASP.NET Core Web API project
  - `Operative.Api` ASP.NET Core Web API project
  - `Shared` class library
- Acceptance criteria:
  - Both projects compile in isolation
  - The solution can run locally with each service's configured SQLite database file

### Task 2: Set up the shared baseline
- Goal: establish common conventions for the project
- Deliverables:
  - `Program.cs` bootstrap setup for both APIs
  - appsettings configuration for each service
  - logging configuration
  - environment / configuration pattern
  - `ApiException` / error contract model
- Acceptance criteria:
  - Each API can start without crashing on blank configuration
  - Basic logging and config structure are in place

### Task 3: Design and implement the database model
- Goal: represent the data for users and dice results in a persistent store
- Deliverables:
  - User entity with fields: id, first name, last name, email, password hash, image path or base64 payload, created date
  - Dice record entity with fields: id, user id, die1, die2, sum, created date
  - Separate EF Core DbContexts and SQLite database files for each service to preserve data ownership
  - Initial migrations
- Acceptance criteria:
  - Users can be saved to the database
  - Dice rolls are saved per authenticated user
  - Model constraints prevent duplicates or invalid email values

### Task 4: Implement the user creation endpoint
- Goal: add user registration flow in the user access service
- Deliverables:
  - Request DTO for `CreateUserRequest`
  - Validation rules for all required fields
  - Password hashing (Argon2id, bcrypt, or PBKDF2 preferred)
  - Unique email check
  - Response DTO for created user
- Acceptance criteria:
  - A new user is created with a hashed password
  - Invalid payloads return 400 with validation errors
  - Duplicate email addresses are rejected cleanly

### Task 5: Implement access token creation
- Goal: authenticate by email and password and issue a token
- Deliverables:
  - `POST /api/auth/token` endpoint
  - DTO validation for email and password
  - password verification logic
  - JWT generation with configured issuer/audience/expiration
  - response model containing token and optional expiry details
- Acceptance criteria:
  - Valid credentials return a JWT
  - Invalid credentials return 401 Unauthorized
  - Token contains enough identity data for downstream authorization

### Task 6: Add JWT-based authentication to the operative service
- Goal: secure operative endpoints with token validation
- Deliverables:
  - JWT bearer configuration in `Operative.Api`
  - custom user claims extraction or principal mapping
  - `[Authorize]` attributes on the operative endpoints
  - a current-user accessor for retrieving the logged-in user id from claims
- Acceptance criteria:
  - Calls without a valid token are rejected
  - Authenticated requests can resolve the current user identity reliably

### Task 7: Implement dice rolling endpoint
- Goal: simulate a dice roll and store it for the authenticated user
- Deliverables:
  - `POST /api/dice/roll`
  - random dice generation logic for values 1..6
  - saved record containing both dice and sum
  - database persistence for the current user
  - response payload with die values and sum
- Acceptance criteria:
  - Each roll returns two values between 1 and 6
  - Values are persisted in the database
  - The endpoint is protected by bearer authentication

### Task 8: Implement the history query endpoint with filters
- Goal: retrieve all saved data for the logged-in user with filtering logic
- Deliverables:
  - `GET /api/dice/history`
  - query parameters for filters: `all`, `year`, `monthYear`, `day`
  - filter logic for the current user only
  - data retrieval from the database by user id
- Acceptance criteria:
  - History returns only records belonging to the authenticated user
  - Filters work correctly for all supported date granularities
  - Empty sets return a valid empty response, not an exception

### Task 9: Add sorting and sorting precedence rules
- Goal: implement the two sorting dimensions and precedence behavior
- Deliverables:
  - sorting parameters for date/time and dice sum
  - descending/ascending direction flags
  - when both sorts are requested, sort by dice sum first and use date/time as the secondary sort
  - deterministic ordering for same-value rows
- Acceptance criteria:
  - Sorting order matches requested direction for each column
  - When both sorts are combined, dice sum has higher priority as specified in the task, with date/time breaking ties
  - The order is stable across repeated requests

### Task 10: Add pagination support
- Goal: return paginated result sets for the dice history endpoint
- Deliverables:
  - `page`, `pageSize` or equivalent query parameters
  - paginated response model with `items`, `pageNumber`, `pageSize`, `totalCount`, `totalPages`
  - database `Skip`/`Take` logic
  - validation for page number and page size bounds
- Acceptance criteria:
  - Only the requested page size is returned
  - The response metadata is accurate
  - Requests with invalid pagination values fail with 400

### Task 11: Add centralized error handling and request validation
- Goal: ensure clean API behavior across both services
- Deliverables:
  - exception middleware or filters
  - consistent problem-details response format
  - request validation pipeline with `ModelState` handling
  - custom validation messages for required fields and bad inputs
- Acceptance criteria:
  - Invalid DTOs return structured 400 responses
  - Unhandled exceptions are converted into safe API errors
  - No stack traces or sensitive internals are exposed to clients

### Task 12: Add integration and unit tests
- Goal: validate correctness and protect against regression
- Deliverables:
  - unit tests for hashing/validation logic
  - integration tests for user creation, token generation, and authenticated dice endpoints
  - tests covering invalid token, invalid credentials, and pagination/filter edge cases
- Acceptance criteria:
  - All happy-path and error-path scenarios are covered
  - Tests run successfully in CI/local build pipeline

### Task 13: Prepare deployment-ready configuration and documentation
- Goal: leave the solution ready for agent continuation and future implementation
- Deliverables:
  - README with setup steps
  - environment variable examples for DB and JWT secrets
  - Docker or local run instructions if needed
  - final API contract summary for both services
- Acceptance criteria:
  - A new engineer can start the services locally with minimal configuration
  - API contracts are documented clearly for the next implementation phase

## Suggested execution order

1. Task 1
2. Task 2
3. Task 3
4. Task 4
5. Task 5
6. Task 6
7. Task 7
8. Task 8
9. Task 9
10. Task 10
11. Task 11
12. Task 12
13. Task 13

## Definition of done

The solution is complete when:
- both microservices are implemented in .NET 10 ASP.NET Core Web API
- user creation and token creation work correctly
- dice roll and history endpoints are protected by JWT authentication
- history filtering, sorting, and pagination behave as required
- validation and base error handling are enforced
- the solution is documented and testable
