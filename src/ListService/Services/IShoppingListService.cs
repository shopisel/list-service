using ListService.Contracts;

namespace ListService.Services;

public interface IShoppingListService
{
    Task<IEnumerable<ListResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ListResponse?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<ListResponse> CreateAsync(CreateListRequest request, CancellationToken cancellationToken = default);
    Task<ListResponse?> UpdateAsync(string id, UpdateListRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
}
