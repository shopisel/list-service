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
            return list is null ? Results.NotFound() : Results.Ok(list);
        })
        .WithName("GetListById")
        .WithSummary("Obter lista especifica");

        lists.MapPost(string.Empty, async ([FromBody] CreateListRequest request, IShoppingListService listService, HttpContext httpContext, CancellationToken ct) =>
        {
            try
            {
                var ownerId = GetOwnerId(httpContext.User);
                if (ownerId is null)
                {
                    return Results.Unauthorized();
                }

                var createdList = await listService.CreateAsync(ownerId, request, ct);
                return Results.Created($"/lists/{createdList.Id}", createdList);
            }
            catch (ArgumentException ex)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    [ex.ParamName ?? "error"] = [ex.Message]
                });
            }
        })
        .WithName("CreateList")
        .WithSummary("Criar nova lista");

        lists.MapPut("/{listId}", async (string listId, [FromBody] UpdateListRequest request, IShoppingListService listService, HttpContext httpContext, CancellationToken ct) =>
        {
            try
            {
                var ownerId = GetOwnerId(httpContext.User);
                if (ownerId is null)
                {
                    return Results.Unauthorized();
                }

                var updatedList = await listService.UpdateAsync(ownerId, listId, request, ct);
                return updatedList is null ? Results.NotFound() : Results.Ok(updatedList);
            }
            catch (ArgumentException ex)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    [ex.ParamName ?? "error"] = [ex.Message]
                });
            }
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

            var success = await listService.DeleteAsync(ownerId, listId, ct);
            return success ? Results.NoContent() : Results.NotFound();
        })
        .WithName("DeleteList")
        .WithSummary("Remover lista");

        static string? GetOwnerId(ClaimsPrincipal principal)
        {
            // "sub" may be remapped in some JWT handlers/environments.
            return principal.FindFirst("sub")?.Value;
        }
    }
}
