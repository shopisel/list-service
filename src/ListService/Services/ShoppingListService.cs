using ListService.Infrastructure;
using ListService.Models;

namespace ListService.Services;

public class ShoppingListService : IShoppingListService
{
    private readonly IShoppingListRepository _repository;

    public ShoppingListService(IShoppingListRepository repository)
    {
        _repository = repository;
    }

    public Task<IEnumerable<ShoppingList>> GetAllShoppingListsAsync()
    {
        return _repository.GetAllAsync();
    }

    public Task<ShoppingList?> GetShoppingListByIdAsync(Guid id)
    {
        return _repository.GetByIdAsync(id);
    }
}
