using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaleManagement.Api.Entities;
using SaleManagement.Api.Models;
using SaleManagement.Api.Repositories;

namespace SaleManagement.Api.Controllers;

[ApiController]
[Route("api/inventory/transfers")]
[Authorize(Roles = "SUPER_ADMIN,STORE_MANAGER,WAREHOUSE_STAFF")]
public class InventoryTransfersController : ControllerBase
{
    private readonly IInventoryRepository _inventoryRepository;

    public InventoryTransfersController(IInventoryRepository inventoryRepository)
    {
        _inventoryRepository = inventoryRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetTransfers([FromQuery] string? status)
    {
        var transfers = await _inventoryRepository.GetTransfersAsync(status);
        return Ok(new { success = true, data = transfers.Select(MapToDto) });
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetTransfer(long id)
    {
        var transfer = await _inventoryRepository.GetTransferByIdAsync(id);
        return transfer == null
            ? NotFound(new { success = false, message = $"Inventory transfer {id} not found." })
            : Ok(new { success = true, data = MapToDto(transfer) });
    }

    [HttpPost]
    public async Task<IActionResult> CreateTransfer([FromBody] CreateInventoryTransferDto dto)
    {
        if (dto.FromWarehouseId == dto.ToWarehouseId)
            return BadRequest(new { success = false, message = "Source and destination warehouses must be different." });
        if (dto.Items.Count == 0 || dto.Items.Any(item => item.QuantityRequested <= 0))
            return BadRequest(new { success = false, message = "Transfer items must have a positive quantity." });
        if (dto.Items.Select(item => item.VariantId).Distinct().Count() != dto.Items.Count)
            return BadRequest(new { success = false, message = "Each variant may appear only once in a transfer." });

        var transfer = await _inventoryRepository.CreateTransferAsync(new InventoryTransferEntity
        {
            TransferCode = $"TR-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}"[..24],
            FromWarehouseId = dto.FromWarehouseId,
            ToWarehouseId = dto.ToWarehouseId,
            Status = "REQUESTED",
            RequestedBy = GetCurrentUserId()
        }, dto.Items.Select(item => new InventoryTransferItemEntity
        {
            VariantId = item.VariantId,
            QuantityRequested = item.QuantityRequested
        }).ToList());

        return CreatedAtAction(nameof(GetTransfer), new { id = transfer.Id }, new { success = true, data = MapToDto(transfer) });
    }

    [HttpPut("{id:long}/status")]
    public async Task<IActionResult> UpdateTransferStatus(long id, [FromBody] UpdateTransferStatusDto dto)
    {
        try
        {
            var updated = await _inventoryRepository.UpdateTransferStatusAsync(id, dto.Status, GetCurrentUserId());
            return updated
                ? Ok(new { success = true, message = $"Transfer status updated to {dto.Status}." })
                : NotFound(new { success = false, message = $"Inventory transfer {id} not found." });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { success = false, message = ex.Message });
        }
    }

    private long? GetCurrentUserId()
    {
        var value = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return long.TryParse(value, out var id) ? id : null;
    }

    private static InventoryTransferDto MapToDto(InventoryTransferEntity transfer)
        => new(
            transfer.Id,
            transfer.TransferCode,
            transfer.FromWarehouseId,
            transfer.FromWarehouse?.Name ?? string.Empty,
            transfer.ToWarehouseId,
            transfer.ToWarehouse?.Name ?? string.Empty,
            transfer.Status,
            transfer.RequestedBy,
            transfer.ApprovedBy,
            transfer.CreatedAt,
            transfer.ReceivedAt,
            transfer.Items.Select(item => new InventoryTransferItemDto(
                item.Id,
                item.VariantId,
                item.Variant?.Sku ?? string.Empty,
                item.Variant?.Name ?? string.Empty,
                item.QuantityRequested,
                item.QuantityReceived)).ToList());
}

public record UpdateTransferStatusDto(string Status);