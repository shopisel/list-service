namespace ListService.Contracts;

public sealed record ListItemRequest(string ProductId, string StoreId, int Quantity, decimal Price, bool Checked);

public sealed record CreateListRequest(string Name, List<ListItemRequest>? Items);

public sealed record UpdateListRequest(string? Name, List<ListItemRequest>? Items);

public sealed record ListItemResponse(int Id, string ProductId, string StoreId, int Quantity, decimal Price, bool Checked);

public sealed record ListResponse(
    string Id,
    string Name,
    DateTime CreatedAt,
    IReadOnlyList<ListItemResponse> Items);
