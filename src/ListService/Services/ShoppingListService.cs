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
                .Select(item => new ListItemResponse(
                    item.Id,
                    item.ProductId,
                    item.StoreId,
                    item.Quantity,
                    item.Price,
                    item.Checked))
                .ToList());
    }

    private static List<ShoppingListItemEntity> MapItems(IEnumerable<ListItemRequest>? requestItems)
    {
        if (requestItems is null)
        {
            return [];
        }

        var items = new List<ShoppingListItemEntity>();
        var seenKeys = new HashSet<string>(StringComparer.Ordinal);

        foreach (var item in requestItems)
        {
            if (string.IsNullOrWhiteSpace(item.ProductId))
            {
                throw new ArgumentException("Each item must include a non-empty productId.", "items");
            }

            if (string.IsNullOrWhiteSpace(item.StoreId))
            {
                throw new ArgumentException("Each item must include a non-empty storeId.", "items");
            }

            if (item.Quantity < 1)
            {
                throw new ArgumentException("Each item must include quantity >= 1.", "items");
            }

            if (item.Price < 0m)
            {
                throw new ArgumentException("Each item must include price >= 0.", "items");
            }

            if (decimal.Round(item.Price, 2) != item.Price)
            {
                throw new ArgumentException("Each item price must have up to 2 decimal places.", "items");
            }

            var productId = item.ProductId.Trim();
            var storeId = item.StoreId.Trim();
            var itemKey = $"{productId}::{storeId}";
            if (!seenKeys.Add(itemKey))
            {
                continue;
            }

            items.Add(new ShoppingListItemEntity
            {
                ProductId = productId,
                StoreId = storeId,
                Quantity = item.Quantity,
                Price = item.Price,
                Checked = item.Checked
            });
        }

        return items;
    }

    private static string GenerateListId()
    {
        return $"list_{Guid.NewGuid():N}";
    }
}
