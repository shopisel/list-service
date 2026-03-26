# List Service

The `list-service` is responsible for managing shopping lists in the Shopisel ecosystem.

It provides the core functionality for creating, retrieving, updating, and deleting user shopping lists, while keeping track of the products inside each list and their checked state. This service is designed to act as the source of truth for list management and to support integration with other services, such as product-related endpoints.

## Responsibilities

- Create new shopping lists
- Retrieve all lists or a specific list
- Update list metadata and items
- Mark products as checked or unchecked
- Delete existing lists
- Support loading product details for a set of product IDs

## API Overview

Main list endpoints:

- `GET /lists` — return all shopping lists
- `POST /lists` — create a new shopping list
- `GET /lists/{listId}` — return a specific shopping list
- `PUT /lists/{listId}` — update a list name, items, or checked state
- `DELETE /lists/{listId}` — delete a shopping list

Product helper endpoint:

- `GET /products?ids=...` — fetch product details for multiple product IDs

## Data Model

A shopping list includes:

- `id`
- `name`
- `createdAt`
- `items`

Each item contains:

- `productId`
- `checked`

This structure makes it possible to keep lists lightweight while delegating richer product information to a dedicated product-related flow.

## Project Structure

```text
src/
├── ListService/
├── ListService.Tests/
└── ListService.slnx
