using Microsoft.EntityFrameworkCore;

namespace ListService.Services;

public partial class ShoppingListService
{
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
}
