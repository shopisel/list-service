using ListService.Contracts;
using Microsoft.EntityFrameworkCore;

namespace ListService.Services;

public partial class ShoppingListService
{
    public async Task<IEnumerable<ListResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var result = await _dbContext.Lists
            .AsNoTracking()
            .Include(list => list.Items)
            .OrderByDescending(list => list.CreatedAt)
            .ToListAsync(cancellationToken);

        return result.Select(MapToResponse);
    }
}
