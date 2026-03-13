using ListService.Infrastructure;
using ListService.Services;

namespace ListService.Tests;

public class ShoppingListServiceTests
{
    private readonly IShoppingListRepository _repository;
    private readonly ShoppingListService _service;

    public ShoppingListServiceTests()
    {
        // For simplicity we are testing with the actual Static repository since it mocks data anyway
        _repository = new StaticShoppingListRepository();
        _service = new ShoppingListService(_repository);
    }

    [Fact]
    public async Task GetAllShoppingListsAsync_ReturnsAllStaticLists()
    {
        // Act
        var result = await _service.GetAllShoppingListsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count()); // The static repo currently has exactly 2 lists
    }

    [Fact]
    public async Task GetShoppingListByIdAsync_ValidId_ReturnsList()
    {
        // Arrange
        var validId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        // Act
        var result = await _service.GetShoppingListByIdAsync(validId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(validId, result.Id);
        Assert.Equal("Compras da Semana - Pingo Doce", result.Title);
    }

    [Fact]
    public async Task GetShoppingListByIdAsync_InvalidId_ReturnsNull()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var result = await _service.GetShoppingListByIdAsync(invalidId);

        // Assert
        Assert.Null(result);
    }
}
