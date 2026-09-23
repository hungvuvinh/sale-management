using Microsoft.EntityFrameworkCore;
using SaleManagement.Api.Data;
using SaleManagement.Api.Entities;
using System.Data;

namespace SaleManagement.Api.Repositories;

public class InventoryRepository : IInventoryRepository
{
    private readonly AppDbContext _db;

    public InventoryRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<WarehouseInventoryEntity>> GetInventoryAsync(long? warehouseId, long? variantId)
    {
        var query = _db.WarehouseInventory
            .Include(wi => wi.Warehouse)
            .Include(wi => wi.Variant)
            .AsQueryable();

        if (warehouseId.HasValue)
            query = query.Where(wi => wi.WarehouseId == warehouseId.Value);
        if (variantId.HasValue)
            query = query.Where(wi => wi.VariantId == variantId.Value);

        return await query
            .OrderBy(wi => wi.Warehouse.Name)
            .ThenBy(wi => wi.Variant.Sku)
            .ToListAsync();
    }

    public async Task<WarehouseInventoryEntity?> GetInventoryAsync(long warehouseId, long variantId)
        => await _db.WarehouseInventory
            .Include(wi => wi.Warehouse)
            .Include(wi => wi.Variant)
            .FirstOrDefaultAsync(wi => wi.WarehouseId == warehouseId && wi.VariantId == variantId);

    public async Task<InventoryTransactionEntity> AddTransactionAsync(InventoryTransactionEntity tx)
    {
        _db.InventoryTransactions.Add(tx);
        await _db.SaveChangesAsync();
        return tx;
    }

    public async Task<bool> UpsertOnHandAsync(long warehouseId, long variantId, int deltaOnHand)
    {
        // Try update first
        var affected = await _db.WarehouseInventory
            .Where(wi => wi.WarehouseId == warehouseId && wi.VariantId == variantId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(wi => wi.OnHandQuantity, wi => wi.OnHandQuantity + deltaOnHand));

        if (affected == 0)
        {
            // Row doesn't exist yet — create it
            var row = new WarehouseInventoryEntity
            {
                WarehouseId = warehouseId,
                VariantId = variantId,
                OnHandQuantity = deltaOnHand,
                ReservedQuantity = 0,
                LowStockThreshold = 5
            };
            _db.WarehouseInventory.Add(row);
            await _db.SaveChangesAsync();
        }
        return true;
    }

    public async Task<bool> ConfirmSaleOutAsync(long warehouseId, long variantId, int qty)
    {
        // Atomically decrements both on_hand and reserved
        var affected = await _db.WarehouseInventory
            .Where(wi => wi.WarehouseId == warehouseId
                      && wi.VariantId == variantId
                      && wi.OnHandQuantity >= qty
                      && wi.ReservedQuantity >= qty)
            .ExecuteUpdateAsync(s => s
                .SetProperty(wi => wi.OnHandQuantity, wi => wi.OnHandQuantity - qty)
                .SetProperty(wi => wi.ReservedQuantity, wi => wi.ReservedQuantity - qty));
        return affected > 0;
    }

    public async Task<InventoryTransferEntity> CreateTransferAsync(InventoryTransferEntity entity, List<InventoryTransferItemEntity> items)
    {
        if (entity.FromWarehouseId == entity.ToWarehouseId)
            throw new ArgumentException("Source and destination warehouses must be different.");
        if (items.Count == 0)
            throw new ArgumentException("At least one transfer item is required.");

        foreach (var item in items)
        {
            entity.Items.Add(item);
        }
        await using var createTransaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        _db.InventoryTransfers.Add(entity);
        await _db.SaveChangesAsync();
        await createTransaction.CommitAsync();
        return (await GetTransferByIdAsync(entity.Id))!;
    }

    public async Task<IEnumerable<InventoryTransferEntity>> GetTransfersAsync(string? status)
    {
        var query = _db.InventoryTransfers
            .Include(transfer => transfer.FromWarehouse)
            .Include(transfer => transfer.ToWarehouse)
            .Include(transfer => transfer.Items)
                .ThenInclude(item => item.Variant)
            .AsQueryable();
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(transfer => transfer.Status == status);
        return await query.OrderByDescending(transfer => transfer.Id).ToListAsync();
    }

    public async Task<InventoryTransferEntity?> GetTransferByIdAsync(long id)
        => await _db.InventoryTransfers
            .Include(transfer => transfer.FromWarehouse)
            .Include(transfer => transfer.ToWarehouse)
            .Include(transfer => transfer.Items)
                .ThenInclude(item => item.Variant)
            .FirstOrDefaultAsync(transfer => transfer.Id == id);

    public async Task<bool> UpdateTransferStatusAsync(long id, string status, long? actorUserId)
    {
        var allowedStatuses = new[] { "REQUESTED", "IN_TRANSIT", "RECEIVED", "CANCELLED" };
        if (!allowedStatuses.Contains(status, StringComparer.OrdinalIgnoreCase))
            throw new ArgumentException($"Unsupported transfer status '{status}'.");

        await using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var transfer = await _db.InventoryTransfers
            .Include(item => item.Items)
            .FirstOrDefaultAsync(item => item.Id == id);
        if (transfer == null)
            return false;
        if (transfer.Status == status)
            return true;

        var normalizedStatus = status.ToUpperInvariant();
        if (normalizedStatus == "IN_TRANSIT")
        {
            if (transfer.Status != "REQUESTED")
                throw new InvalidOperationException($"Transfer cannot move from {transfer.Status} to IN_TRANSIT.");

            foreach (var item in transfer.Items)
            {
                var affected = await _db.WarehouseInventory
                    .Where(row => row.WarehouseId == transfer.FromWarehouseId
                        && row.VariantId == item.VariantId
                        && row.OnHandQuantity - row.ReservedQuantity >= item.QuantityRequested)
                    .ExecuteUpdateAsync(update => update.SetProperty(row => row.OnHandQuantity, row => row.OnHandQuantity - item.QuantityRequested));
                if (affected == 0)
                    throw new InvalidOperationException($"Insufficient available inventory for variant {item.VariantId}.");

                _db.InventoryTransactions.Add(new InventoryTransactionEntity
                {
                    TransactionCode = $"TRANSFER-OUT-{Guid.NewGuid():N}",
                    WarehouseId = transfer.FromWarehouseId,
                    VariantId = item.VariantId,
                    TransactionType = "TRANSFER_OUT",
                    ChangeQuantity = -item.QuantityRequested,
                    ReferenceId = transfer.Id,
                    ReferenceType = "INVENTORY_TRANSFER",
                    PerformedByUser = actorUserId
                });
            }
            transfer.Status = "IN_TRANSIT";
            transfer.ApprovedBy = actorUserId;
        }
        else if (normalizedStatus == "RECEIVED")
        {
            if (transfer.Status != "IN_TRANSIT")
                throw new InvalidOperationException($"Transfer cannot move from {transfer.Status} to RECEIVED.");

            foreach (var item in transfer.Items)
            {
                var destination = await _db.WarehouseInventory
                    .FirstOrDefaultAsync(row => row.WarehouseId == transfer.ToWarehouseId && row.VariantId == item.VariantId);
                if (destination == null)
                {
                    _db.WarehouseInventory.Add(new WarehouseInventoryEntity
                    {
                        WarehouseId = transfer.ToWarehouseId,
                        VariantId = item.VariantId,
                        OnHandQuantity = item.QuantityRequested,
                        ReservedQuantity = 0,
                        LowStockThreshold = 5
                    });
                }
                else
                {
                    destination.OnHandQuantity += item.QuantityRequested;
                }
                item.QuantityReceived = item.QuantityRequested;
                _db.InventoryTransactions.Add(new InventoryTransactionEntity
                {
                    TransactionCode = $"TRANSFER-IN-{Guid.NewGuid():N}",
                    WarehouseId = transfer.ToWarehouseId,
                    VariantId = item.VariantId,
                    TransactionType = "TRANSFER_IN",
                    ChangeQuantity = item.QuantityRequested,
                    ReferenceId = transfer.Id,
                    ReferenceType = "INVENTORY_TRANSFER",
                    PerformedByUser = actorUserId
                });
            }
            transfer.Status = "RECEIVED";
            transfer.ReceivedAt = DateTime.UtcNow;
        }
        else if (normalizedStatus == "CANCELLED")
        {
            if (transfer.Status != "REQUESTED")
                throw new InvalidOperationException($"Transfer cannot be cancelled from {transfer.Status}.");
            transfer.Status = "CANCELLED";
        }
        else
        {
            throw new InvalidOperationException($"Transfer cannot move from {transfer.Status} to {normalizedStatus}.");
        }

        await _db.SaveChangesAsync();
        await transaction.CommitAsync();
        return true;
    }
}
