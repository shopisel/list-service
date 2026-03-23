namespace ListService.Data.Entities;

public class ShoppingListItemEntity
{
    public int Id { get; set; }

    public string ListId { get; set; } = string.Empty;

    public string ProductId { get; set; } = string.Empty;

    public bool Checked { get; set; }

    public ShoppingListEntity? List { get; set; }
}
