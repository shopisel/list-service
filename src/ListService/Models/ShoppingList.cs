namespace ListService.Models;

public record ShoppingListItem(Guid Id, Guid ProductId, int Quantity, bool IsChecked);

public record ShoppingList(Guid Id, string Title, List<ShoppingListItem> Items);
