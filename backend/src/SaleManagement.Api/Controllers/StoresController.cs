using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaleManagement.Api.Models;
using SaleManagement.Api.Services;

namespace SaleManagement.Api.Controllers;

[ApiController]
[Route("api")]
public class StoresController : ControllerBase
{
    private readonly StoresService _storesService;

    public StoresController(StoresService storesService)
    {
        _storesService = storesService;
    }

    [HttpGet("stores")]
    [AllowAnonymous]
    public async Task<IActionResult> GetStores()
    {
        var stores = await _storesService.GetStoresAsync();
        return Ok(new { success = true, data = stores });
    }

    [HttpGet("warehouses")]
    [AllowAnonymous]
    public async Task<IActionResult> GetWarehouses()
    {
        var warehouses = await _storesService.GetWarehousesAsync();
        return Ok(new { success = true, data = warehouses });
    }

    [HttpGet("postcodes/lookup")]
    [AllowAnonymous]
    public async Task<IActionResult> LookupPostcode([FromQuery] string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return BadRequest(new { success = false, message = "Postcode parameter 'code' is required." });
        }

        var result = await _storesService.LookupPostcodeAsync(code);
        if (result == null)
        {
            return NotFound(new { success = false, message = $"Postcode '{code}' not found." });
        }

        return Ok(new { success = true, data = result });
    }

    [HttpPost("postcodes/assign-store")]
    [Authorize(Roles = "SUPER_ADMIN,STORE_MANAGER")]
    public async Task<IActionResult> AssignPostcode([FromBody] AssignPostcodeDto dto)
    {
        await _storesService.AssignPostcodeToStoreAsync(dto.PostcodeId, dto.StoreId);
        return Ok(new { success = true, message = "Postcode assigned to store successfully." });
    }

    [HttpPost("postcodes/select-store")]
    [AllowAnonymous]
    public async Task<IActionResult> SelectStore([FromBody] SelectPostcodeStoreDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Postcode))
            return BadRequest(new { success = false, message = "Postcode is required." });

        try
        {
            var result = await _storesService.SelectStoreForPostcodeAsync(dto.Postcode, dto.StoreId);
            return Ok(new { success = true, data = result });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }
}
