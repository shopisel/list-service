using ListService.Contracts;
using ListService.Data.Entities;

namespace ListService.Services;

public partial class ShoppingListService
{
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
}
