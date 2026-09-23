using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaleManagement.Api.Models;
using SaleManagement.Api.Services;

namespace SaleManagement.Api.Controllers;

[ApiController]
[Route("api")]
public class ProductsController : ControllerBase
{
    private readonly ProductsService _productsService;
    private readonly PricingEngineService _pricingEngineService;

    public ProductsController(ProductsService productsService, PricingEngineService pricingEngineService)
    {
        _productsService = productsService;
        _pricingEngineService = pricingEngineService;
    }

    [HttpGet("categories")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _productsService.GetCategoriesAsync();
        return Ok(new { success = true, data = categories });
    }

    [HttpPost("categories")]
    [Authorize]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto dto)
    {
        var category = await _productsService.CreateCategoryAsync(dto);
        return CreatedAtAction(nameof(GetCategories), new { success = true, data = category });
    }

    [HttpGet("products")]
    [AllowAnonymous]
    public async Task<IActionResult> GetProducts(
        [FromQuery] string? keyword,
        [FromQuery] long? categoryId,
        [FromQuery] int? stage,
        [FromQuery] long? storeId,
        [FromQuery] long? warehouseId)
    {
        var products = await _productsService.GetProductsAsync(keyword, categoryId, stage, storeId, warehouseId);
        return Ok(new { success = true, data = products });
    }

    [HttpGet("products/{id:long}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetProductById(long id, [FromQuery] long? storeId = null)
    {
        var product = await _productsService.GetProductByIdAsync(id, storeId);
        if (product == null)
        {
            return NotFound(new { success = false, message = $"Product with ID {id} not found." });
        }
        return Ok(new { success = true, data = product });
    }

    [HttpPost("products")]
    [Authorize]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto dto)
    {
        var product = await _productsService.CreateProductAsync(dto);
        return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, new { success = true, data = product });
    }

    [HttpPut("products/{id:long}")]
    [Authorize]
    public async Task<IActionResult> UpdateProduct(long id, [FromBody] UpdateProductDto dto)
    {
        var ok = await _productsService.UpdateProductAsync(id, dto);
        if (!ok)
        {
            return NotFound(new { success = false, message = $"Product with ID {id} not found." });
        }
        return Ok(new { success = true, message = "Product updated successfully." });
    }

    [HttpDelete("products/{id:long}")]
    [Authorize]
    public async Task<IActionResult> DeleteProduct(long id)
    {
        var ok = await _productsService.DeleteProductAsync(id);
        if (!ok)
        {
            return NotFound(new { success = false, message = $"Product with ID {id} not found." });
        }
        return Ok(new { success = true, message = "Product deactivated successfully." });
    }

    [HttpPost("products/{id:long}/variants")]
    [Authorize]
    public async Task<IActionResult> CreateVariant(long id, [FromBody] CreateVariantDto dto)
    {
        var variant = await _productsService.CreateVariantAsync(id, dto);
        return Ok(new { success = true, data = variant });
    }

    [HttpPut("products/variants/{variantId:long}")]
    [Authorize(Roles = "SUPER_ADMIN,STORE_MANAGER,WAREHOUSE_STAFF")]
    public async Task<IActionResult> UpdateVariant(long variantId, [FromBody] UpdateVariantDto dto)
    {
        var ok = await _productsService.UpdateVariantAsync(variantId, dto);
        return ok
            ? Ok(new { success = true, message = "Variant updated successfully." })
            : NotFound(new { success = false, message = $"Variant with ID {variantId} not found." });
    }

    [HttpDelete("products/variants/{variantId:long}")]
    [Authorize(Roles = "SUPER_ADMIN,STORE_MANAGER,WAREHOUSE_STAFF")]
    public async Task<IActionResult> DeleteVariant(long variantId)
    {
        var ok = await _productsService.DeleteVariantAsync(variantId);
        return ok
            ? Ok(new { success = true, message = "Variant deactivated successfully." })
            : NotFound(new { success = false, message = $"Variant with ID {variantId} not found." });
    }

    [HttpPut("products/variants/{variantId:long}/lots/{lotId:long}/stage-prices")]
    [Authorize]
    public async Task<IActionResult> UpdateStagePrices(long variantId, long lotId, [FromBody] UpdateStagePricesDto dto)
    {
        var dtoWithLot = dto with { FifoLotId = lotId };
        await _productsService.UpdateStagePricesAsync(dtoWithLot);
        return Ok(new { success = true, message = "5-stage prices updated successfully." });
    }

    [HttpPost("products/price-stage-check")]
    [Authorize]
    public async Task<IActionResult> CheckPriceStage([FromBody] PriceStageCheckRequestDto req)
    {
        var results = await _pricingEngineService.CheckAndUpdatePriceStageAsync(req);
        return Ok(new { success = true, data = results });
    }

    [HttpGet("products/variants/{variantId:long}/lots/{lotId:long}/price-history")]
    [Authorize]
    public async Task<IActionResult> GetPriceHistory(long variantId, long lotId)
    {
        var history = await _productsService.GetPriceHistoryAsync(variantId, lotId);
        return Ok(new { success = true, data = history });
    }

    [HttpGet("products/variants/{variantId:long}/stage-configs")]
    [Authorize]
    public async Task<IActionResult> GetStageConfigs(long variantId)
    {
        var configs = await _productsService.GetPricingStageConfigsAsync(variantId);
        return Ok(new { success = true, data = configs });
    }

    [HttpPost("products/variants/{variantId:long}/stage-configs")]
    [Authorize]
    public async Task<IActionResult> SaveStageConfigs(long variantId, [FromBody] SavePricingStageConfigsDto dto)
    {
        await _productsService.SavePricingStageConfigsAsync(variantId, dto);
        return Ok(new { success = true, message = "Pricing stage configurations saved successfully by Manager." });
    }

    [HttpGet("stores/{storeId:long}/variant-prices")]
    [AllowAnonymous]
    public async Task<IActionResult> GetStoreVariantPrices(long storeId, [FromQuery] long? variantId)
    {
        var prices = await _productsService.GetStoreVariantPricesAsync(storeId, variantId);
        return Ok(new { success = true, data = prices });
    }

    [HttpPut("stores/{storeId:long}/variants/{variantId:long}/daily-price")]
    [Authorize]
    public async Task<IActionResult> UpdateStoreVariantDailyPrice(long storeId, long variantId, [FromBody] UpdateVariantDailyPriceDto dto)
    {
        var ok = await _productsService.UpdateStoreVariantDailyPriceAsync(storeId, variantId, dto.CurrentDailyPrice);
        if (!ok)
        {
            return NotFound(new { success = false, message = $"Store or Variant not found." });
        }
        return Ok(new { success = true, message = $"Daily price for variant {variantId} at store {storeId} updated to ${dto.CurrentDailyPrice:F2}." });
    }
}

