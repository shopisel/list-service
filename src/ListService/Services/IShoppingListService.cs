using ListService.Models;

namespace ListService.Services;

public interface IShoppingListService
{
    Task<IEnumerable<ShoppingList>> GetAllShoppingListsAsync();
    Task<ShoppingList?> GetShoppingListByIdAsync(Guid id);
}
