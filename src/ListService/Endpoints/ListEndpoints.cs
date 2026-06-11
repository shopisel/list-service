using ListService.Contracts;
using ListService.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace ListService.Endpoints;

public static class ListEndpoints
{
    public static void MapListEndpoints(this IEndpointRouteBuilder app)
    {
        var lists = app
            .MapGroup("/lists")
            .WithTags("List")
            .RequireAuthorization();

        lists.MapGet(string.Empty, async (IShoppingListService listService, HttpContext httpContext, CancellationToken ct) =>
        {
            var ownerId = GetOwnerId(httpContext.User);
            if (ownerId is null)
            {
                return Results.Unauthorized();
            }

            var result = await listService.GetAllAsync(ownerId, ct);
            return Results.Ok(result);
        })
        .WithName("GetLists")
        .WithSummary("Listar listas do utilizador");

        lists.MapGet("/{listId}", async (string listId, IShoppingListService listService, HttpContext httpContext, CancellationToken ct) =>
        {
            var ownerId = GetOwnerId(httpContext.User);
            if (ownerId is null)
            {
                return Results.Unauthorized();
            }

            var list = await listService.GetByIdAsync(ownerId, listId, ct);
            SetResponseVersionHeaders(httpContext, list?.Version);
            return list is null ? Results.NotFound() : Results.Ok(list);
        })
        .WithName("GetListById")
        .WithSummary("Obter lista especifica");

        lists.MapPost(string.Empty, async ([FromBody] CreateListRequest request, IShoppingListService listService, HttpContext httpContext, CancellationToken ct) =>
        {
            var ownerId = GetOwnerId(httpContext.User);
            if (ownerId is null)
            {
                return Results.Unauthorized();
            }

            var createdList = await listService.CreateAsync(ownerId, request, ct);
            SetResponseVersionHeaders(httpContext, createdList.Version);
            return Results.Created($"/lists/{createdList.Id}", createdList);
        })
        .WithName("CreateList")
        .WithSummary("Criar nova lista");

        lists.MapPut("/{listId}", async (string listId, [FromBody] UpdateListRequest request, IShoppingListService listService, HttpContext httpContext, CancellationToken ct) =>
        {
            var ownerId = GetOwnerId(httpContext.User);
            if (ownerId is null)
            {
                return Results.Unauthorized();
            }

            if (!TryGetIfMatchVersion(httpContext, out var expectedVersion, out var errorResult))
            {
                return errorResult!;
            }

            var updatedList = await listService.UpdateAsync(ownerId, listId, expectedVersion, request, ct);
            if (updatedList is not null)
            {
                SetResponseVersionHeaders(httpContext, updatedList.Version);
            }
            return updatedList is null ? Results.NotFound() : Results.Ok(updatedList);
        })
        .WithName("UpdateList")
        .WithSummary("Atualizar lista");

        lists.MapDelete("/{listId}", async (string listId, IShoppingListService listService, HttpContext httpContext, CancellationToken ct) =>
        {
            var ownerId = GetOwnerId(httpContext.User);
            if (ownerId is null)
            {
                return Results.Unauthorized();
            }

            if (!TryGetIfMatchVersion(httpContext, out var expectedVersion, out var errorResult))
            {
                return errorResult!;
            }

            var success = await listService.DeleteAsync(ownerId, listId, expectedVersion, ct);
            return success ? Results.NoContent() : Results.NotFound();
        })
        .WithName("DeleteList")
        .WithSummary("Remover lista");

        static string? GetOwnerId(ClaimsPrincipal principal)
        {
            // "sub" may be remapped in some JWT handlers/environments.
            return principal.FindFirst("sub")?.Value;
        }

        static bool TryGetIfMatchVersion(HttpContext httpContext, out Guid expectedVersion, out IResult? errorResult)
        {
            expectedVersion = default;
            errorResult = null;

            var header = httpContext.Request.Headers.IfMatch.ToString();
            if (string.IsNullOrWhiteSpace(header))
            {
                errorResult = Results.Problem(
                    statusCode: StatusCodes.Status428PreconditionRequired,
                    title: "Missing If-Match header.");
                return false;
            }

            var candidate = header.Trim();
            if (candidate.StartsWith("W/", StringComparison.OrdinalIgnoreCase))
            {
                candidate = candidate[2..].Trim();
            }

            candidate = candidate.Trim('"');
            if (!Guid.TryParse(candidate, out expectedVersion))
            {
                errorResult = Results.Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Invalid If-Match header.");
                return false;
            }

            return true;
        }

        static void SetResponseVersionHeaders(HttpContext httpContext, Guid? version)
        {
            if (version is null || version == Guid.Empty)
            {
                return;
            }

            httpContext.Response.Headers.ETag = $"\"{version.Value:D}\"";
            httpContext.Response.Headers.CacheControl = "no-store";
        }
    }
}
