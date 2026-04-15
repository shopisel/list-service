# List Service

The `list-service` manages shopping lists in the Shopisel ecosystem.

## Responsibilities

- Create new shopping lists
- Retrieve all lists or a specific list
- Update list metadata and items
- Mark products as checked or unchecked
- Delete existing lists

## API Overview

Main list endpoints (all require bearer token):

- `GET /lists`
- `POST /lists`
- `GET /lists/{listId}`
- `PUT /lists/{listId}`
- `DELETE /lists/{listId}`

## Authentication (Keycloak)

The API validates JWT bearer tokens issued by Keycloak.

Configuration keys:

- `Keycloak:Authority` (example: `https://<keycloak-host>/realms/shopisel`)
- `Keycloak:Audience` (legacy single expected `azp` claim, example: `shopisel-list-api`)
- `Keycloak:AuthorizedParties` (preferred list of accepted `azp` values, for example `shopisel-list-api`, `shopisel-web`, `shopisel-mobile`)
- `Keycloak:RequireHttpsMetadata` (`true` in production)

Each list is stored with `owner_id` using the JWT `sub` claim, and all `/lists` operations are filtered by owner.

## Project Structure

```text
src/
├── ListService/
├── ListService.Tests/
└── ListService.slnx
```
