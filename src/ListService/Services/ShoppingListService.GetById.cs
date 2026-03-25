using ListService.Contracts;
using Microsoft.EntityFrameworkCore;

namespace ListService.Services;

public partial class ShoppingListService
{
    public async Task<ListResponse?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var list = await _dbContext.Lists
            .AsNoTracking()
            .Include(currentList => currentList.Items)
            .FirstOrDefaultAsync(currentList => currentList.Id == id, cancellationToken);

        return list is null ? null : MapToResponse(list);
    }
}
