# ADR 0001: Centralized Error Handling and Optimistic Concurrency for Shopping Lists

Date: 2026-06-11

## Status

Accepted

## Context

The list-service exposes shopping list CRUD endpoints to multiple users, and the same list can be updated concurrently from different clients.

Previously, error handling was split across endpoints and service methods:

- validation errors were translated locally in `POST` and `PUT`
- not-found cases were returned as `404`
- unexpected exceptions were not handled centrally

This left the API inconsistent for infrastructure failures, malformed requests, and unexpected runtime errors.

At the same time, list updates were vulnerable to lost updates. If two clients loaded the same list and both edited it, the second write could overwrite the first without noticing.

## Decision

We will use two complementary mechanisms:

1. A global exception-handling middleware for the API.
2. Optimistic concurrency for shopping lists using a `Guid Version` concurrency token and `If-Match`/`ETag`.

### Error handling

The API now uses a centralized middleware to translate exceptions into consistent HTTP responses:

- `ArgumentException` -> `400 Bad Request` with `ValidationProblem`
- `BadHttpRequestException` -> `400 Bad Request`
- `DbUpdateConcurrencyException` -> `409 Conflict`
- `DbUpdateException` -> `503 Service Unavailable`
- any other unhandled exception -> `500 Internal Server Error`

The middleware also logs the error before returning the response.

Endpoint-level handling remains only for intentional application flow:

- `401 Unauthorized` when the authenticated user cannot be resolved
- `404 Not Found` when the requested list does not exist or belongs to another user

### Optimistic concurrency

Shopping lists now have a `Version` property on the entity and on the API response contract.

- `GET /lists/{id}` returns the list plus its current version
- `POST /lists` returns the created list with a version
- `PUT /lists/{id}` requires an `If-Match` header containing the expected version
- `DELETE /lists/{id}` also requires `If-Match`

On updates and deletes, EF Core compares the expected version with the current database version. If another client updated the row in the meantime, EF raises `DbUpdateConcurrencyException`, and the middleware converts that into `409 Conflict`.

## Alternatives considered

### SQL Server `rowversion` / `[Timestamp] byte[]`

Rejected because the service uses PostgreSQL as the production database and SQLite in tests. A database-specific `rowversion` model would reduce portability.

### PostgreSQL `xmin`

Considered because PostgreSQL supports it as a native concurrency token. Rejected for this service because the codebase also runs against SQLite in tests, and an application-managed token keeps the implementation portable and easier to reason about.

### Pessimistic locking / long transactions

Rejected because the service should not hold transactions open while waiting for user interaction. Optimistic concurrency is a better fit for list editing scenarios.

## Consequences

### Positive

- Consistent error responses across the API
- Clear separation between business errors and infrastructure failures
- Protection against lost updates
- Better logging and easier troubleshooting
- Portable concurrency behavior across PostgreSQL and SQLite

### Trade-offs

- Clients must now send back the current version on write operations
- Client applications may need to refresh and retry after `409 Conflict`
- The API contract is slightly more explicit because concurrency is part of the request flow

## Database impact

Yes, the database schema changes.

- A new non-null `version` column was added to the `lists` table
- Existing rows are initialized with `Guid.Empty` during migration
- New rows are assigned a new `Guid` in application code

This means the database must be updated with the new migration:

- PostgreSQL production database: apply the EF Core migration
- SQLite test database: the tests recreate the schema automatically

The relevant migration is `AddListVersionConcurrency`.

## Implementation notes

- The API now emits `ETag` on list reads and uses `If-Match` for write operations.
- The middleware sits early in the pipeline so unexpected errors are translated before they reach the client.
- Validation still happens in the domain/service layer to keep business rules close to the data model.

