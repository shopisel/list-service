using ListService.Contracts;
using ListService.Data;
using ListService.Data.Entities;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("ListService");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Connection string 'ConnectionStrings:ListService' is required.");
}

builder.Services.AddDbContext<ListServiceDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

await InitializeDatabaseAsync(app);

var lists = app.MapGroup("/lists").WithTags("List");

lists.MapGet(string.Empty, async (ListServiceDbContext dbContext) =>
{
    var result = await dbContext.Lists
        .AsNoTracking()
        .Include(list => list.Items)
        .OrderByDescending(list => list.CreatedAt)
        .ToListAsync();

    return Results.Ok(result.Select(MapToResponse));
})
.WithName("GetLists")
.WithSummary("Listar listas do utilizador");

lists.MapGet("/{listId}", async (string listId, ListServiceDbContext dbContext) =>
{
    var list = await dbContext.Lists
        .AsNoTracking()
        .Include(currentList => currentList.Items)
        .FirstOrDefaultAsync(currentList => currentList.Id == listId);

    return list is null
        ? Results.NotFound()
        : Results.Ok(MapToResponse(list));
})
.WithName("GetListById")
.WithSummary("Obter lista especifica");

lists.MapPost(string.Empty, async (CreateListRequest request, ListServiceDbContext dbContext) =>
{
    if (string.IsNullOrWhiteSpace(request.Name))
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            ["name"] = ["The list name is required."]
        });
    }

    var shoppingList = new ShoppingListEntity
    {
        Id = GenerateListId(),
        Name = request.Name.Trim(),
        CreatedAt = DateTime.UtcNow,
        Items = MapItems(request.Items)
    };

    dbContext.Lists.Add(shoppingList);
    await dbContext.SaveChangesAsync();

    return Results.Created($"/lists/{shoppingList.Id}", MapToResponse(shoppingList));
})
.WithName("CreateList")
.WithSummary("Criar nova lista");

lists.MapPut("/{listId}", async (string listId, UpdateListRequest request, ListServiceDbContext dbContext) =>
{
    var shoppingList = await dbContext.Lists
        .Include(list => list.Items)
        .FirstOrDefaultAsync(list => list.Id == listId);

    if (shoppingList is null)
    {
        return Results.NotFound();
    }

    if (request.Name is not null)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["name"] = ["The list name cannot be empty."]
            });
        }

        shoppingList.Name = request.Name.Trim();
    }

    if (request.Items is not null)
    {
        dbContext.Items.RemoveRange(shoppingList.Items);
        shoppingList.Items = MapItems(request.Items);
    }

    await dbContext.SaveChangesAsync();

    return Results.Ok(MapToResponse(shoppingList));
})
.WithName("UpdateList")
.WithSummary("Atualizar lista");

lists.MapDelete("/{listId}", async (string listId, ListServiceDbContext dbContext) =>
{
    var shoppingList = await dbContext.Lists
        .FirstOrDefaultAsync(list => list.Id == listId);

    if (shoppingList is null)
    {
        return Results.NotFound();
    }

    dbContext.Lists.Remove(shoppingList);
    await dbContext.SaveChangesAsync();

    return Results.NoContent();
})
.WithName("DeleteList")
.WithSummary("Remover lista");

await app.RunAsync();

static ListResponse MapToResponse(ShoppingListEntity entity)
{
    return new ListResponse(
        entity.Id,
        entity.Name,
        entity.CreatedAt,
        entity.Items
            .OrderBy(item => item.Id)
            .Select(item => new ListItemResponse(item.ProductId, item.Checked))
            .ToList());
}

static List<ShoppingListItemEntity> MapItems(IEnumerable<ListItemRequest>? requestItems)
{
    if (requestItems is null)
    {
        return [];
    }

    return requestItems
        .Where(item => !string.IsNullOrWhiteSpace(item.ProductId))
        .Select(item => new ListItemRequest(item.ProductId.Trim(), item.Checked))
        .DistinctBy(item => item.ProductId)
        .Select(item => new ShoppingListItemEntity
        {
            ProductId = item.ProductId,
            Checked = item.Checked
        })
        .ToList();
}

static string GenerateListId()
{
    return $"list_{Guid.NewGuid():N}";
}

static async Task InitializeDatabaseAsync(WebApplication application)
{
    using var scope = application.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ListServiceDbContext>();
    if (dbContext.Database.ProviderName?.Contains("Npgsql", StringComparison.OrdinalIgnoreCase) == true)
    {
        await dbContext.Database.MigrateAsync();
        return;
    }

    await dbContext.Database.EnsureCreatedAsync();
}

public partial class Program;
