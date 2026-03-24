using ListService.Contracts;
using ListService.Data;
using ListService.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ListService.Services;

public class ShoppingListService : IShoppingListService
{
    private readonly ListServiceDbContext _dbContext;

    public ShoppingListService(ListServiceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<ListResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var result = await _dbContext.Lists
            .AsNoTracking()
            .Include(list => list.Items)
            .OrderByDescending(list => list.CreatedAt)
            .ToListAsync(cancellationToken);

        return result.Select(MapToResponse);
    }

    public async Task<ListResponse?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var list = await _dbContext.Lists
            .AsNoTracking()
            .Include(currentList => currentList.Items)
            .FirstOrDefaultAsync(currentList => currentList.Id == id, cancellationToken);

        return list is null ? null : MapToResponse(list);
    }

    public async Task<ListResponse> CreateAsync(CreateListRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ArgumentException("The list name is required.", nameof(request));
        }

        var shoppingList = new ShoppingListEntity
        {
            Id = GenerateListId(),
            Name = request.Name.Trim(),
            CreatedAt = DateTime.UtcNow,
            Items = MapItems(request.Items)
        };

        _dbContext.Lists.Add(shoppingList);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToResponse(shoppingList);
    }

    public async Task<ListResponse?> UpdateAsync(string id, UpdateListRequest request, CancellationToken cancellationToken = default)
    {
        var shoppingList = await _dbContext.Lists
            .Include(list => list.Items)
            .FirstOrDefaultAsync(list => list.Id == id, cancellationToken);

        if (shoppingList is null)
        {
            return null; // Not found
        }

        if (request.Name is not null)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new ArgumentException("The list name cannot be empty.", nameof(request));
            }

            shoppingList.Name = request.Name.Trim();
        }

        if (request.Items is not null)
        {
            _dbContext.Items.RemoveRange(shoppingList.Items);
            shoppingList.Items = MapItems(request.Items);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToResponse(shoppingList);
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var shoppingList = await _dbContext.Lists
            .FirstOrDefaultAsync(list => list.Id == id, cancellationToken);

        if (shoppingList is null)
        {
            return false;
        }

        _dbContext.Lists.Remove(shoppingList);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static ListResponse MapToResponse(ShoppingListEntity entity)
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

    private static List<ShoppingListItemEntity> MapItems(IEnumerable<ListItemRequest>? requestItems)
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

    private static string GenerateListId()
    {
        return $"list_{Guid.NewGuid():N}";
    }
}
