using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaleManagement.Api.Models;
using System.Security.Claims;
using SaleManagement.Api.Services;

namespace SaleManagement.Api.Controllers;

[ApiController]
[Route("api/stock-orders")]
[Authorize(Roles = "SUPER_ADMIN,STORE_MANAGER,WAREHOUSE_STAFF")]
public class StockOrdersController : ControllerBase
{
    private readonly StockOrdersService _stockOrdersService;

    public StockOrdersController(StockOrdersService stockOrdersService)
    {
        _stockOrdersService = stockOrdersService;
    }

    [HttpGet]
    public async Task<IActionResult> GetStockOrders()
    {
        var orders = await _stockOrdersService.GetStockOrdersAsync();
        return Ok(new { success = true, data = orders });
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetStockOrderById(long id)
    {
        var order = await _stockOrdersService.GetStockOrderByIdAsync(id);
        if (order == null)
        {
            return NotFound(new { success = false, message = $"Stock Order with ID {id} not found." });
        }
        return Ok(new { success = true, data = order });
    }

    [HttpPost]
    public async Task<IActionResult> CreateStockOrder([FromBody] CreateStockOrderDto dto)
    {
        var order = await _stockOrdersService.CreateStockOrderAsync(dto);
        return CreatedAtAction(nameof(GetStockOrderById), new { id = order.Id }, new { success = true, data = order });
    }

    [HttpPost("landed-cost")]
    public IActionResult CalculateLandedCost([FromBody] CalculateLandedCostRequestDto req)
    {
        try
        {
            var result = _stockOrdersService.CalculateLandedCost(req);
            return Ok(new { success = true, data = result });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpPut("{id:long}/status")]
    public async Task<IActionResult> UpdateStatus(long id, [FromBody] UpdateStatusDto dto)
    {
        var ok = await _stockOrdersService.UpdateStockOrderStatusAsync(id, dto.Status);
        if (!ok)
        {
            return NotFound(new { success = false, message = $"Stock Order with ID {id} not found." });
        }
        return Ok(new { success = true, message = $"Stock Order status updated to '{dto.Status}'." });
    }

    [HttpPost("{id:long}/receive")]
    public async Task<IActionResult> Receive(long id, [FromBody] ReceiveStockOrderDto dto)
    {
        try
        {
            var actor = long.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var actorId)
                ? actorId
                : (long?)null;
            var order = await _stockOrdersService.ReceiveStockOrderAsync(id, dto, actor);
            return order == null
                ? NotFound(new { success = false, message = $"Stock Order with ID {id} not found." })
                : Ok(new { success = true, data = order });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { success = false, message = ex.Message });
        }
    }
}

public record UpdateStatusDto(string Status);
