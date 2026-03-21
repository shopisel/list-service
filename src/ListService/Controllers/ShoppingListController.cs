using ListService.Models;
using ListService.Services;
using Microsoft.AspNetCore.Mvc;

namespace ListService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ShoppingListController : ControllerBase
{
    private readonly IShoppingListService _service;

    public ShoppingListController(IShoppingListService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ShoppingList>>> GetAll()
    {
        var lists = await _service.GetAllShoppingListsAsync();
        return Ok(lists);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ShoppingList>> GetById(Guid id)
    {
        var list = await _service.GetShoppingListByIdAsync(id);
        
        if (list == null)
            return NotFound();

        return Ok(list);
    }
}
