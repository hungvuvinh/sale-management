using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SaleManagement.Api.Data;
using SaleManagement.Api.Models;
using SaleManagement.Api.Repositories;

namespace SaleManagement.Api.Controllers;

/// <summary>
/// Inventory Controller — xem tồn kho tại từng kho/variant & quản lý lô FIFO.
/// </summary>
[ApiController]
[Route("api/inventory")]
[Authorize]
public class InventoryController : ControllerBase
{
    private readonly IInventoryRepository _inventoryRepo;
    private readonly AppDbContext _context;

    public InventoryController(IInventoryRepository inventoryRepo, AppDbContext context)
    {
        _inventoryRepo = inventoryRepo;
        _context = context;
    }

    // GET /api/inventory/fifo-lots?warehouseId=1&variantId=5
    [HttpGet("fifo-lots")]
    public async Task<IActionResult> GetFifoLots(
        [FromQuery] long? warehouseId,
        [FromQuery] long? variantId)
    {
        var query = _context.FifoLots
            .Include(f => f.Variant)
            .Include(f => f.Warehouse)
            .AsQueryable();

        if (warehouseId.HasValue) query = query.Where(f => f.WarehouseId == warehouseId.Value);
        if (variantId.HasValue) query = query.Where(f => f.VariantId == variantId.Value);

        var lots = await query.OrderByDescending(f => f.Id).ToListAsync();
        var lotIds = lots.Select(l => l.Id).ToList();
        var stagePrices = await _context.FifoLotStagePrices
            .Where(sp => lotIds.Contains(sp.FifoLotId))
            .ToListAsync();

        var stagePriceMap = stagePrices
            .GroupBy(sp => sp.FifoLotId)
            .ToDictionary(g => g.Key, g => g.ToDictionary(x => x.Stage, x => x.PriceAud));

        var dtos = lots.Select(l => new {
            id = l.Id,
            lotNumber = l.LotNumber,
            variantId = l.VariantId,
            variantSku = l.Variant?.Sku ?? string.Empty,
            variantName = l.Variant?.Name ?? string.Empty,
            warehouseId = l.WarehouseId,
            warehouseName = l.Warehouse?.Name ?? string.Empty,
            initialQuantity = l.InitialQuantity,
            remainingQuantity = l.RemainingQuantity,
            unitLandedCostAud = l.UnitLandedCostAud,
            currentStage = l.CurrentStage,
            receivedDate = l.ReceivedDate.ToString("yyyy-MM-dd"),
            status = l.Status,
            stagePrices = stagePriceMap.ContainsKey(l.Id) ? stagePriceMap[l.Id] : new Dictionary<short, decimal>()
        });

        return Ok(new { success = true, data = dtos });
    }


    // GET /api/inventory?warehouseId=1&variantId=5
    [HttpGet]
    public async Task<ActionResult<IEnumerable<WarehouseInventoryDto>>> GetInventory(
        [FromQuery] long? warehouseId,
        [FromQuery] long? variantId)
    {
        var rows = await _inventoryRepo.GetInventoryAsync(warehouseId, variantId);
        var dtos = rows.Select(wi => new WarehouseInventoryDto(
            WarehouseId: wi.WarehouseId,
            WarehouseName: wi.Warehouse?.Name ?? string.Empty,
            VariantId: wi.VariantId,
            VariantSku: wi.Variant?.Sku ?? string.Empty,
            VariantName: wi.Variant?.Name ?? string.Empty,
            OnHandQuantity: wi.OnHandQuantity,
            ReservedQuantity: wi.ReservedQuantity,
            AvailableQuantity: wi.AvailableQuantity,
            LowStockThreshold: wi.LowStockThreshold,
            IsLowStock: wi.AvailableQuantity <= wi.LowStockThreshold
        ));
        return Ok(dtos);
    }

    // GET /api/inventory/{warehouseId}/{variantId}
    [HttpGet("{warehouseId:long}/{variantId:long}")]
    public async Task<ActionResult<WarehouseInventoryDto>> GetInventoryItem(long warehouseId, long variantId)
    {
        var wi = await _inventoryRepo.GetInventoryAsync(warehouseId, variantId);
        if (wi == null)
            return NotFound(new { message = $"Không có dữ liệu tồn kho cho kho {warehouseId} / variant {variantId}." });

        return Ok(new WarehouseInventoryDto(
            WarehouseId: wi.WarehouseId,
            WarehouseName: wi.Warehouse?.Name ?? string.Empty,
            VariantId: wi.VariantId,
            VariantSku: wi.Variant?.Sku ?? string.Empty,
            VariantName: wi.Variant?.Name ?? string.Empty,
            OnHandQuantity: wi.OnHandQuantity,
            ReservedQuantity: wi.ReservedQuantity,
            AvailableQuantity: wi.AvailableQuantity,
            LowStockThreshold: wi.LowStockThreshold,
            IsLowStock: wi.AvailableQuantity <= wi.LowStockThreshold
        ));
    }
}
