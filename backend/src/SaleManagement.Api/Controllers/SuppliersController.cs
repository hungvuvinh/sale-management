using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaleManagement.Api.Models;
using SaleManagement.Api.Services;

namespace SaleManagement.Api.Controllers;

[ApiController]
[Route("api/suppliers")]
[Authorize]
public class SuppliersController : ControllerBase
{
    private readonly StockOrdersService _stockOrdersService;

    public SuppliersController(StockOrdersService stockOrdersService)
    {
        _stockOrdersService = stockOrdersService;
    }

    [HttpGet]
    public async Task<IActionResult> GetSuppliers()
    {
        var suppliers = await _stockOrdersService.GetSuppliersAsync();
        return Ok(new { success = true, data = suppliers });
    }

    [HttpPost]
    public async Task<IActionResult> CreateSupplier([FromBody] CreateSupplierDto dto)
    {
        var supplier = await _stockOrdersService.CreateSupplierAsync(dto);
        return CreatedAtAction(nameof(GetSuppliers), new { success = true, data = supplier });
    }
}
