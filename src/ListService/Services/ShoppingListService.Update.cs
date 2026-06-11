using ListService.Contracts;
using Microsoft.EntityFrameworkCore;

namespace ListService.Services;

public partial class ShoppingListService
{
    public async Task<ListResponse?> UpdateAsync(
        string ownerId,
        string id,
        Guid expectedVersion,
        UpdateListRequest request,
        CancellationToken cancellationToken = default)
    {
        var shoppingList = await _dbContext.Lists
            .Include(list => list.Items)
            .FirstOrDefaultAsync(list => list.Id == id && list.OwnerId == ownerId, cancellationToken);

        if (shoppingList is null)
        {
            return null; // Not found
        }

        _dbContext.Entry(shoppingList).Property(list => list.Version).OriginalValue = expectedVersion;

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

        shoppingList.Version = Guid.NewGuid();
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToResponse(shoppingList);
    }
}
