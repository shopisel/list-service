using ListService.Contracts;
using Microsoft.EntityFrameworkCore;

namespace ListService.Services;

public partial class ShoppingListService
{
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
}
