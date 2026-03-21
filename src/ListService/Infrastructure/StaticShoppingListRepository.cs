using ListService.Models;

namespace ListService.Infrastructure;

public class StaticShoppingListRepository : IShoppingListRepository
{
    private static readonly List<ShoppingList> _lists = new()
    {
        new ShoppingList(
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            "Compras da Semana - Pingo Doce",
            new List<ShoppingListItem>
            {
                new ShoppingListItem(Guid.NewGuid(), Guid.NewGuid(), 2, false),
                new ShoppingListItem(Guid.NewGuid(), Guid.NewGuid(), 1, true)
            }
        ),
        new ShoppingList(
            Guid.Parse("22222222-2222-2222-2222-222222222222"),
            "Festa de Anos",
            new List<ShoppingListItem>
            {
                new ShoppingListItem(Guid.NewGuid(), Guid.NewGuid(), 5, false)
            }
        )
    };

    public Task<IEnumerable<ShoppingList>> GetAllAsync()
    {
        return Task.FromResult(_lists.AsEnumerable());
    }

    public Task<ShoppingList?> GetByIdAsync(Guid id)
    {
        var list = _lists.FirstOrDefault(l => l.Id == id);
        return Task.FromResult(list);
    }
}
