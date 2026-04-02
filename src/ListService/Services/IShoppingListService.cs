using ListService.Contracts;

namespace ListService.Services;

public interface IShoppingListService
{
    Task<IEnumerable<ListResponse>> GetAllAsync(string ownerId, CancellationToken cancellationToken = default);
    Task<ListResponse?> GetByIdAsync(string ownerId, string id, CancellationToken cancellationToken = default);
    Task<ListResponse> CreateAsync(string ownerId, CreateListRequest request, CancellationToken cancellationToken = default);
    Task<ListResponse?> UpdateAsync(string ownerId, string id, UpdateListRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string ownerId, string id, CancellationToken cancellationToken = default);
}
