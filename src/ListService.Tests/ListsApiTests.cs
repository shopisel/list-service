using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

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
                new { productId = "prod_1", storeId = "store_1", quantity = 2, @checked = false },
                new { productId = "prod_2", storeId = "store_2", quantity = 1, @checked = true }
            }
        };

        var createResponse = await _client.PostAsJsonAsync("/lists", createRequest);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<ListResponse>();
        Assert.NotNull(created);
        Assert.StartsWith("list_", created!.Id);
        Assert.NotEqual(Guid.Empty, created.Version);
        Assert.Equal(2, created.Items.Count);
        Assert.All(created.Items, item => Assert.True(item.Id > 0));

        var getResponse = await _client.GetAsync($"/lists/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var fetched = await getResponse.Content.ReadFromJsonAsync<ListResponse>();
        Assert.NotNull(fetched);
        Assert.Equal("Compras Semanais", fetched!.Name);

        var updateResponse = await SendPutAsync($"/lists/{created.Id}", new
        {
            name = "Compras Semanais Atualizada",
            items = new[]
            {
                new { productId = "prod_3", storeId = "store_3", quantity = 4, @checked = false }
            }
        }, created.Version);
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var updated = await updateResponse.Content.ReadFromJsonAsync<ListResponse>();
        Assert.NotNull(updated);
        Assert.Equal("Compras Semanais Atualizada", updated!.Name);
        Assert.NotEqual(created.Version, updated.Version);
        Assert.Single(updated.Items);
        Assert.Equal("prod_3", updated.Items[0].ProductId);
        Assert.Equal("store_3", updated.Items[0].StoreId);
        Assert.Equal(4, updated.Items[0].Quantity);
        Assert.False(updated.Items[0].Checked);

        var deleteResponse = await SendDeleteAsync($"/lists/{created.Id}", updated.Version);
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

        var otherUserDeleteResponse = await SendDeleteAsync(otherUserClient, $"/lists/{created.Id}", created.Version);
        Assert.Equal(HttpStatusCode.NotFound, otherUserDeleteResponse.StatusCode);
    }

    [Fact]
    public async Task GetAll_ReturnsOnlyListsFromAuthenticatedUser()
    {
        var createRequest = new
        {
            name = "Lista Utilizador Atual",
            items = new[]
            {
                new { productId = "prod_20", storeId = "store_20", quantity = 1, @checked = false }
            }
        };

        var createResponse = await _client.PostAsJsonAsync("/lists", createRequest);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        using var otherUserClient = factory.CreateClient();
        otherUserClient.DefaultRequestHeaders.Remove("X-Test-User");
        otherUserClient.DefaultRequestHeaders.Add("X-Test-User", "other-user");

        var otherUserCreateResponse = await otherUserClient.PostAsJsonAsync("/lists", new
        {
            name = "Lista Outro Utilizador",
            items = new[]
            {
                new { productId = "prod_30", storeId = "store_30", quantity = 2, @checked = false }
            }
        });
        Assert.Equal(HttpStatusCode.Created, otherUserCreateResponse.StatusCode);

        var getAllResponse = await _client.GetAsync("/lists");
        Assert.Equal(HttpStatusCode.OK, getAllResponse.StatusCode);

        var lists = await getAllResponse.Content.ReadFromJsonAsync<List<ListResponse>>();
        Assert.NotNull(lists);
        Assert.Contains(lists!, list => list.Name == "Lista Utilizador Atual");
        Assert.DoesNotContain(lists!, list => list.Name == "Lista Outro Utilizador");
    }

    [Fact]
    public async Task Create_WithInvalidName_ReturnsValidationProblem()
    {
        var response = await _client.PostAsJsonAsync("/lists", new
        {
            name = "   ",
            items = Array.Empty<object>()
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var payload = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        Assert.True(payload.RootElement.TryGetProperty("errors", out var errors));
        Assert.True(errors.TryGetProperty("request", out var nameErrors));
        Assert.Contains("The list name is required.", nameErrors[0].GetString());
    }

    [Fact]
    public async Task Create_WithInvalidItemQuantity_ReturnsValidationProblem()
    {
        var response = await _client.PostAsJsonAsync("/lists", new
        {
            name = "Lista com erro",
            items = new[]
            {
                new { productId = "prod_1", storeId = "store_1", quantity = 0, @checked = false }
            }
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var payload = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        Assert.True(payload.RootElement.TryGetProperty("errors", out var errors));
        Assert.True(errors.TryGetProperty("items", out var itemErrors));
        Assert.Contains("quantity >= 1", itemErrors[0].GetString());
    }

    [Fact]
    public async Task Update_WithInvalidName_ReturnsValidationProblem()
    {
        var createResponse = await _client.PostAsJsonAsync("/lists", new
        {
            name = "Lista Base",
            items = Array.Empty<object>()
        });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<ListResponse>();
        Assert.NotNull(created);

        var response = await SendPutAsync($"/lists/{created!.Id}", new
        {
            name = " ",
            items = Array.Empty<object>()
        }, created.Version);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var payload = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        Assert.True(payload.RootElement.TryGetProperty("errors", out var errors));
        Assert.True(errors.TryGetProperty("request", out var nameErrors));
        Assert.Contains("The list name cannot be empty.", nameErrors[0].GetString());
    }

    [Fact]
    public async Task Update_WithStaleVersion_ReturnsConflict()
    {
        var createResponse = await _client.PostAsJsonAsync("/lists", new
        {
            name = "Lista Conflito",
            items = Array.Empty<object>()
        });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<ListResponse>();
        Assert.NotNull(created);

        var firstUpdate = await SendPutAsync($"/lists/{created!.Id}", new
        {
            name = "Lista Conflito Atualizada",
            items = Array.Empty<object>()
        }, created.Version);
        Assert.Equal(HttpStatusCode.OK, firstUpdate.StatusCode);

        var staleUpdate = await SendPutAsync($"/lists/{created.Id}", new
        {
            name = "Lista Conflito 2",
            items = Array.Empty<object>()
        }, created.Version);

        Assert.Equal(HttpStatusCode.Conflict, staleUpdate.StatusCode);
    }

    [Fact]
    public async Task Delete_WithoutIfMatch_ReturnsPreconditionRequired()
    {
        var createResponse = await _client.PostAsJsonAsync("/lists", new
        {
            name = "Lista Sem Header",
            items = Array.Empty<object>()
        });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<ListResponse>();
        Assert.NotNull(created);

        var response = await _client.DeleteAsync($"/lists/{created!.Id}");

        Assert.Equal((HttpStatusCode)428, response.StatusCode);
    }

    [Fact]
    public async Task UnexpectedException_IsConvertedToProblemDetails()
    {
        using var factory = new FaultyListServiceApiFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/lists");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var payload = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        Assert.Equal("An unexpected error occurred.", payload.RootElement.GetProperty("title").GetString());
    }

    [Fact]
    public async Task InvalidJsonBody_ReturnsBadRequestProblemDetails()
    {
        var request = new StringContent("{ invalid json", Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/lists", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var payload = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        Assert.Equal("The request body is invalid.", payload.RootElement.GetProperty("title").GetString());
    }

    private Task<HttpResponseMessage> SendPutAsync<TBody>(string url, TBody body, Guid version)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, url)
        {
            Content = JsonContent.Create(body)
        };

        request.Headers.TryAddWithoutValidation("If-Match", $"\"{version:D}\"");
        return _client.SendAsync(request);
    }

    private Task<HttpResponseMessage> SendDeleteAsync(string url, Guid version)
    {
        return SendDeleteAsync(_client, url, version);
    }

    private static async Task<HttpResponseMessage> SendDeleteAsync(HttpClient client, string url, Guid version)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, url);
        request.Headers.TryAddWithoutValidation("If-Match", $"\"{version:D}\"");
        return await client.SendAsync(request);
    }

    private sealed record ListItemResponse(int Id, string ProductId, string StoreId, int Quantity, bool Checked);

    private sealed record ListResponse(
        string Id,
        string Name,
        DateTime CreatedAt,
        Guid Version,
        List<ListItemResponse> Items);
}
