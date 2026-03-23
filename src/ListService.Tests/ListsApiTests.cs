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
                new { productId = "prod_1", @checked = false },
                new { productId = "prod_2", @checked = true }
            }
        };

        var createResponse = await _client.PostAsJsonAsync("/lists", createRequest);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<ListResponse>();
        Assert.NotNull(created);
        Assert.StartsWith("list_", created!.Id);
        Assert.Equal(2, created.Items.Count);

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
                new { productId = "prod_3", @checked = false }
            }
        };

        var updateResponse = await _client.PutAsJsonAsync($"/lists/{created.Id}", updateRequest);
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var updated = await updateResponse.Content.ReadFromJsonAsync<ListResponse>();
        Assert.NotNull(updated);
        Assert.Equal("Compras Semanais Atualizada", updated!.Name);
        Assert.Single(updated.Items);
        Assert.Equal("prod_3", updated.Items[0].ProductId);
        Assert.False(updated.Items[0].Checked);

        var deleteResponse = await _client.DeleteAsync($"/lists/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var afterDeleteResponse = await _client.GetAsync($"/lists/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, afterDeleteResponse.StatusCode);
    }

    private sealed record ListItemResponse(string ProductId, bool Checked);

    private sealed record ListResponse(
        string Id,
        string Name,
        DateTime CreatedAt,
        List<ListItemResponse> Items);
}
