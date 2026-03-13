using ListService.Models;

namespace ListService.Infrastructure;

public interface IShoppingListRepository
{
    Task<IEnumerable<ShoppingList>> GetAllAsync();
    Task<ShoppingList?> GetByIdAsync(Guid id);
}
