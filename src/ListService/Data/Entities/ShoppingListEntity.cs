namespace ListService.Data.Entities;

public class ShoppingListEntity
{
    public string Id { get; set; } = string.Empty;

    public string OwnerId { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public List<ShoppingListItemEntity> Items { get; set; } = [];
}
