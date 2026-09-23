using Microsoft.EntityFrameworkCore;
using SaleManagement.Api.Data;
using SaleManagement.Api.Entities;
using SaleManagement.Api.Models;
using System.Data;

namespace SaleManagement.Api.Repositories;

public class StockOrderRepository : IStockOrderRepository
{
    private readonly AppDbContext _context;

    public StockOrderRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<SupplierEntity>> GetActiveSuppliersAsync()
    {
        return await _context.Suppliers
            .Where(s => s.IsActive)
            .OrderBy(s => s.Name)
            .ToListAsync();
    }

    public async Task<SupplierEntity> CreateSupplierAsync(SupplierEntity entity)
    {
        _context.Suppliers.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<IEnumerable<StockOrderEntity>> GetStockOrdersAsync()
    {
        return await _context.StockOrders
            .Include(so => so.Supplier)
            .Include(so => so.DestinationWarehouse)
            .Include(so => so.Items)
                .ThenInclude(i => i.Variant)
            .OrderByDescending(so => so.Id)
            .ToListAsync();
    }

    public async Task<StockOrderEntity?> GetStockOrderByIdAsync(long id)
    {
        return await _context.StockOrders
            .Include(so => so.Supplier)
            .Include(so => so.DestinationWarehouse)
            .Include(so => so.Items)
                .ThenInclude(i => i.Variant)
            .FirstOrDefaultAsync(so => so.Id == id);
    }

    public async Task<StockOrderEntity> CreateStockOrderAsync(StockOrderEntity entity, List<StockOrderItemEntity> items)
    {
        using var tx = await _context.Database.BeginTransactionAsync();
        try
        {
            _context.StockOrders.Add(entity);
            await _context.SaveChangesAsync();

            foreach (var item in items)
            {
                item.StockOrderId = entity.Id;
                _context.StockOrderItems.Add(item);
            }
            await _context.SaveChangesAsync();

            await tx.CommitAsync();
            return (await GetStockOrderByIdAsync(entity.Id))!;
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> UpdateStockOrderStatusAsync(long id, string status)
    {
        var order = await _context.StockOrders.FindAsync(id);
        if (order == null) return false;

        order.Status = status;
        order.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<StockOrderEntity?> ReceiveStockOrderAsync(long id, ReceiveStockOrderDto request, long? actorUserId)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var order = await _context.StockOrders
            .Include(stockOrder => stockOrder.Items)
            .FirstOrDefaultAsync(stockOrder => stockOrder.Id == id);

        if (order == null)
            return null;
        if (order.Status is not ("PENDING" or "SAILING" or "CUSTOMS_CLEARING" or "ARRIVED" or "PARTIALLY_RECEIVED"))
            throw new InvalidOperationException($"Stock order cannot be received from status {order.Status}.");
        if (request.Items.Count == 0)
            throw new ArgumentException("At least one stock order item is required.");

        var receivedDate = request.ReceivedDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        foreach (var receivedItem in request.Items)
        {
            if (receivedItem.QuantityReceived <= 0)
                throw new ArgumentException("Received quantity must be greater than zero.");

            var orderItem = order.Items.FirstOrDefault(item => item.Id == receivedItem.StockOrderItemId);
            if (orderItem == null)
                throw new KeyNotFoundException($"Stock order item {receivedItem.StockOrderItemId} was not found.");

            var remaining = orderItem.QuantityOrdered - orderItem.QuantityReceived;
            if (receivedItem.QuantityReceived > remaining)
                throw new InvalidOperationException($"Received quantity for item {orderItem.Id} exceeds the outstanding quantity.");

            orderItem.QuantityReceived += receivedItem.QuantityReceived;
            var lotNumber = $"LOT-{order.PoNumber}-{orderItem.Id}-{Guid.NewGuid():N}";
            var lot = new FifoLotEntity
            {
                LotNumber = lotNumber[..Math.Min(60, lotNumber.Length)],
                VariantId = orderItem.VariantId,
                WarehouseId = order.DestinationWarehouseId,
                StockOrderItemId = orderItem.Id,
                InitialQuantity = receivedItem.QuantityReceived,
                RemainingQuantity = receivedItem.QuantityReceived,
                UnitLandedCostAud = orderItem.CalculatedLandedCostAud,
                CurrentStage = 1,
                ReceivedDate = receivedDate,
                Status = "ACTIVE",
                ActivatedAt = DateTime.UtcNow
            };
            _context.FifoLots.Add(lot);
            await _context.SaveChangesAsync();

            var stageOne = orderItem.CalculatedLandedCostAud <= 0 ? 0 : Math.Round(orderItem.CalculatedLandedCostAud / .4m, 2);
            var stagePrices = new[] { stageOne, stageOne * .9m, stageOne * .75m, stageOne * .65m, stageOne * .5m };
            _context.FifoLotStagePrices.AddRange(stagePrices.Select((price, index) => new FifoLotStagePriceEntity
            {
                FifoLotId = lot.Id,
                Stage = (short)(index + 1),
                PriceAud = Math.Round(price, 2),
                UpdatedAt = DateTime.UtcNow
            }));

            var inventory = await _context.WarehouseInventory
                .FirstOrDefaultAsync(row => row.WarehouseId == order.DestinationWarehouseId && row.VariantId == orderItem.VariantId);
            if (inventory == null)
            {
                _context.WarehouseInventory.Add(new WarehouseInventoryEntity
                {
                    WarehouseId = order.DestinationWarehouseId,
                    VariantId = orderItem.VariantId,
                    OnHandQuantity = receivedItem.QuantityReceived,
                    ReservedQuantity = 0,
                    LowStockThreshold = 5
                });
            }
            else
            {
                inventory.OnHandQuantity += receivedItem.QuantityReceived;
            }

            _context.InventoryTransactions.Add(new InventoryTransactionEntity
            {
                TransactionCode = $"STOCK-IN-{Guid.NewGuid():N}",
                WarehouseId = order.DestinationWarehouseId,
                VariantId = orderItem.VariantId,
                FifoLotId = lot.Id,
                TransactionType = "STOCK_IN",
                ChangeQuantity = receivedItem.QuantityReceived,
                ReferenceId = order.Id,
                ReferenceType = "STOCK_ORDER",
                PerformedByUser = actorUserId,
                Notes = $"Received from PO {order.PoNumber}"
            });
        }

        if (order.Items.All(item => item.QuantityReceived == item.QuantityOrdered))
            order.Status = "RECEIVED";
        else
            order.Status = "PARTIALLY_RECEIVED";
        order.ActualArrivalDate = receivedDate;
        order.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
        return await GetStockOrderByIdAsync(order.Id);
    }
}
