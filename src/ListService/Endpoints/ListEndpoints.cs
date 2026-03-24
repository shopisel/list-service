using ListService.Contracts;
using ListService.Services;
using Microsoft.AspNetCore.Mvc;

namespace ListService.Endpoints;

public static class ListEndpoints
{
    public static void MapListEndpoints(this IEndpointRouteBuilder app)
    {
        var lists = app.MapGroup("/lists").WithTags("List");

        lists.MapGet(string.Empty, async (IShoppingListService listService, CancellationToken ct) =>
        {
            var result = await listService.GetAllAsync(ct);
            return Results.Ok(result);
        })
        .WithName("GetLists")
        .WithSummary("Listar listas do utilizador");

        lists.MapGet("/{listId}", async (string listId, IShoppingListService listService, CancellationToken ct) =>
        {
            var list = await listService.GetByIdAsync(listId, ct);
            return list is null ? Results.NotFound() : Results.Ok(list);
        })
        .WithName("GetListById")
        .WithSummary("Obter lista especifica");

        lists.MapPost(string.Empty, async ([FromBody] CreateListRequest request, IShoppingListService listService, CancellationToken ct) =>
        {
            try
            {
                var createdList = await listService.CreateAsync(request, ct);
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

        lists.MapPut("/{listId}", async (string listId, [FromBody] UpdateListRequest request, IShoppingListService listService, CancellationToken ct) =>
        {
            try
            {
                var updatedList = await listService.UpdateAsync(listId, request, ct);
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

        lists.MapDelete("/{listId}", async (string listId, IShoppingListService listService, CancellationToken ct) =>
        {
            var success = await listService.DeleteAsync(listId, ct);
            return success ? Results.NoContent() : Results.NotFound();
        })
        .WithName("DeleteList")
        .WithSummary("Remover lista");
    }
}
