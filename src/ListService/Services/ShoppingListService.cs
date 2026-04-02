using ListService.Contracts;
using ListService.Data;
using ListService.Data.Entities;

namespace ListService.Services;

public partial class ShoppingListService : IShoppingListService
{
    private readonly ListServiceDbContext _dbContext;

    public ShoppingListService(ListServiceDbContext dbContext)
    {
        _dbContext = dbContext;
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
