using ListService.Contracts;
using ListService.Services;

namespace ListService.Tests;

public sealed class ThrowingShoppingListService : IShoppingListService
{
    public Task<IEnumerable<ListResponse>> GetAllAsync(string ownerId, CancellationToken cancellationToken = default)
    {
        throw new InvalidOperationException("Simulated failure.");
    }

    public Task<ListResponse?> GetByIdAsync(string ownerId, string id, CancellationToken cancellationToken = default)
    {
        throw new InvalidOperationException("Simulated failure.");
    }

    public Task<ListResponse> CreateAsync(string ownerId, CreateListRequest request, CancellationToken cancellationToken = default)
    {
        throw new InvalidOperationException("Simulated failure.");
    }

    public Task<ListResponse?> UpdateAsync(string ownerId, string id, UpdateListRequest request, CancellationToken cancellationToken = default)
    {
        throw new InvalidOperationException("Simulated failure.");
    }

    public Task<bool> DeleteAsync(string ownerId, string id, CancellationToken cancellationToken = default)
    {
        throw new InvalidOperationException("Simulated failure.");
    }
}
