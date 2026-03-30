using ListService.Contracts;
using Microsoft.EntityFrameworkCore;

namespace ListService.Services;

public partial class ShoppingListService
{
    public async Task<IEnumerable<ListResponse>> GetAllAsync(string ownerId, CancellationToken cancellationToken = default)
    {
        var result = await _dbContext.Lists
            .AsNoTracking()
            .Where(list => list.OwnerId == ownerId)
            .Include(list => list.Items)
            .OrderByDescending(list => list.CreatedAt)
            .ToListAsync(cancellationToken);

        return result.Select(MapToResponse);
    }
}
