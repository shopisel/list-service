using System.Net;
using System.Net.Http.Json;

namespace ListService.Tests;

public class ListsApiTests(ListServiceApiFactory factory) : IClassFixture<ListServiceApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task ListsCrudFlow_WorksEndToEnd()
    {
        var createRequest = new
        {
            name = "Compras Semanais",
            items = new[]
            {
                new { productId = "prod_1", storeId = "store_1", quantity = 2, price = 3.49m, @checked = false },
                new { productId = "prod_2", storeId = "store_2", quantity = 1, price = 1.99m, @checked = true }
            }
        };

        var createResponse = await _client.PostAsJsonAsync("/lists", createRequest);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<ListResponse>();
        Assert.NotNull(created);
        Assert.StartsWith("list_", created!.Id);
        Assert.Equal(2, created.Items.Count);
        Assert.All(created.Items, item => Assert.True(item.Id > 0));

        var getResponse = await _client.GetAsync($"/lists/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var fetched = await getResponse.Content.ReadFromJsonAsync<ListResponse>();
        Assert.NotNull(fetched);
        Assert.Equal("Compras Semanais", fetched!.Name);

        var updateRequest = new
        {
            name = "Compras Semanais Atualizada",
            items = new[]
            {
                new { productId = "prod_3", storeId = "store_3", quantity = 4, price = 2.79m, @checked = false }
            }
        };

        var updateResponse = await _client.PutAsJsonAsync($"/lists/{created.Id}", updateRequest);
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var updated = await updateResponse.Content.ReadFromJsonAsync<ListResponse>();
        Assert.NotNull(updated);
        Assert.Equal("Compras Semanais Atualizada", updated!.Name);
        Assert.Single(updated.Items);
        Assert.Equal("prod_3", updated.Items[0].ProductId);
        Assert.Equal("store_3", updated.Items[0].StoreId);
        Assert.Equal(4, updated.Items[0].Quantity);
        Assert.Equal(2.79m, updated.Items[0].Price);
        Assert.False(updated.Items[0].Checked);

        var deleteResponse = await _client.DeleteAsync($"/lists/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var afterDeleteResponse = await _client.GetAsync($"/lists/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, afterDeleteResponse.StatusCode);
    }

    [Fact]
    public async Task ListOwnerIsolation_OtherUserCannotAccessList()
    {
        var createRequest = new
        {
            name = "Lista Privada",
            items = new[]
            {
                new { productId = "prod_10", storeId = "store_10", quantity = 1, price = 10.00m, @checked = false }
            }
        };

        var createResponse = await _client.PostAsJsonAsync("/lists", createRequest);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<ListResponse>();
        Assert.NotNull(created);

        using var otherUserClient = factory.CreateClient();
        otherUserClient.DefaultRequestHeaders.Remove("X-Test-User");
        otherUserClient.DefaultRequestHeaders.Add("X-Test-User", "other-user");

        var otherUserGetResponse = await otherUserClient.GetAsync($"/lists/{created!.Id}");
        Assert.Equal(HttpStatusCode.NotFound, otherUserGetResponse.StatusCode);

        var otherUserDeleteResponse = await otherUserClient.DeleteAsync($"/lists/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, otherUserDeleteResponse.StatusCode);
    }

    private sealed record ListItemResponse(int Id, string ProductId, string StoreId, int Quantity, decimal Price, bool Checked);

    private sealed record ListResponse(
        string Id,
        string Name,
        DateTime CreatedAt,
        List<ListItemResponse> Items);
}
