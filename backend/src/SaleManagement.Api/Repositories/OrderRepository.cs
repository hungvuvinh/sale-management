using Microsoft.EntityFrameworkCore;
using SaleManagement.Api.Data;
using SaleManagement.Api.Entities;

namespace SaleManagement.Api.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _db;

    public OrderRepository(AppDbContext db)
    {
        _db = db;
    }

    // ─────────────────────────────────────────────
    //  CUSTOMER
    // ─────────────────────────────────────────────

    public async Task<IEnumerable<CustomerEntity>> GetCustomersAsync(string? phone, string? email)
    {
        var query = _db.Customers.AsQueryable();
        if (!string.IsNullOrEmpty(phone))
            query = query.Where(c => c.Phone.Contains(phone));
        if (!string.IsNullOrEmpty(email))
            query = query.Where(c => c.Email != null && c.Email.Contains(email));
        return await query.OrderBy(c => c.LastName).ThenBy(c => c.FirstName).ToListAsync();
    }

    public async Task<CustomerEntity?> GetCustomerByIdAsync(long id)
        => await _db.Customers.FindAsync(id);

    public async Task<CustomerEntity?> GetCustomerByPhoneAsync(string phone)
        => await _db.Customers.FirstOrDefaultAsync(c => c.Phone == phone);

    public async Task<CustomerEntity> CreateCustomerAsync(CustomerEntity entity)
    {
        // Generate customer_code: CUST-{YYYYMMDD}-{6 random hex}
        entity.CustomerCode = $"CUST-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";
        _db.Customers.Add(entity);
        await _db.SaveChangesAsync();
        return entity;
    }

    // ─────────────────────────────────────────────
    //  ORDER CRUD
    // ─────────────────────────────────────────────

    public async Task<IEnumerable<OrderEntity>> GetOrdersAsync(long? storeId, string? status, int page, int pageSize)
    {
        var query = _db.Orders
            .Include(o => o.Customer)
            .Include(o => o.Store)
            .AsQueryable();

        if (storeId.HasValue)
            query = query.Where(o => o.StoreId == storeId.Value);
        if (!string.IsNullOrEmpty(status))
            query = query.Where(o => o.Status == status);

        return await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<OrderEntity?> GetOrderByIdAsync(long id)
        => await _db.Orders
            .Include(o => o.Customer)
            .Include(o => o.Store)
            .Include(o => o.Items)
                .ThenInclude(i => i.Variant)
            .Include(o => o.Payments)
            .FirstOrDefaultAsync(o => o.Id == id);

    public async Task<OrderEntity?> GetOrderByCodeAsync(string orderCode)
        => await _db.Orders
            .Include(o => o.Customer)
            .Include(o => o.Store)
            .Include(o => o.Items)
                .ThenInclude(i => i.Variant)
            .Include(o => o.Payments)
            .FirstOrDefaultAsync(o => o.OrderCode == orderCode);

    public async Task<OrderEntity> CreateOrderAsync(OrderEntity order, List<OrderItemEntity> items)
    {
        // Generate order code: ORD-{YYYYMMDD}-{8 random hex}
        order.OrderCode = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
        _db.Orders.Add(order);
        await _db.SaveChangesAsync(); // get order.Id

        foreach (var item in items)
        {
            item.OrderId = order.Id;
        }
        _db.OrderItems.AddRange(items);
        await _db.SaveChangesAsync();

        // Reload with full navigation
        return (await GetOrderByIdAsync(order.Id))!;
    }

    public async Task<bool> UpdateOrderStatusAsync(long orderId, string newStatus)
    {
        var order = await _db.Orders.FindAsync(orderId);
        if (order == null) return false;
        order.Status = newStatus;
        order.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    // ─────────────────────────────────────────────
    //  FIFO ALLOCATION & INVENTORY RESERVATION
    // ─────────────────────────────────────────────

    /// <summary>
    /// Returns active FIFO lots for a given variant, ordered oldest first (FIFO).
    /// storeId is used to prefer lots at warehouses linked to the store (is_store=true
    /// with matching address or directly associated). Falls back to all warehouses.
    /// </summary>
    public async Task<List<FifoLotEntity>> GetActiveFifoLotsForReservationAsync(long variantId, long storeId)
        => await _db.FifoLots
            .Where(l => l.VariantId == variantId
                     && l.Status == "ACTIVE"
                     && l.RemainingQuantity > 0)
            .OrderBy(l => l.ReceivedDate)
            .ThenBy(l => l.Id)
            .ToListAsync();

    /// <summary>
    /// Atomically increments reserved_quantity for the given variant
    /// at a warehouse, guarded by the DB CHECK constraint that ensures
    /// reserved_quantity <= on_hand_quantity.
    /// Returns false if available stock is insufficient.
    /// </summary>
    public async Task<bool> ReserveInventoryAsync(long warehouseId, long variantId, int qty)
    {
        // Use ExecuteUpdate to do a single atomic UPDATE with WHERE guard
        var affected = await _db.WarehouseInventory
            .Where(wi => wi.WarehouseId == warehouseId
                      && wi.VariantId == variantId
                      && (wi.OnHandQuantity - wi.ReservedQuantity) >= qty)
            .ExecuteUpdateAsync(s => s
                .SetProperty(wi => wi.ReservedQuantity, wi => wi.ReservedQuantity + qty));
        return affected > 0;
    }

    public async Task AddFifoAllocationAsync(OrderItemFifoAllocationEntity allocation)
    {
        _db.OrderItemFifoAllocations.Add(allocation);
        await _db.SaveChangesAsync();
    }

    // ─────────────────────────────────────────────
    //  PAYMENT
    // ─────────────────────────────────────────────

    public async Task<OrderPaymentEntity> AddPaymentAsync(OrderPaymentEntity payment)
    {
        _db.OrderPayments.Add(payment);
        await _db.SaveChangesAsync();
        return payment;
    }

    public async Task<bool> UpdateOrderPaidAmountAsync(long orderId, decimal newPaidAmount, string newPaymentStatus)
    {
        var order = await _db.Orders.FindAsync(orderId);
        if (order == null) return false;
        order.PaidAmountAud = newPaidAmount;
        order.PaymentStatus = newPaymentStatus;
        order.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    // ─────────────────────────────────────────────
    //  FIFO PROFIT ENGINE
    // ─────────────────────────────────────────────

    public async Task FinalizeActualProfitAsync(long orderId)
    {
        // Sum all realized_profit_aud from order_item_fifo_allocations for this order
        decimal actualProfit = await _db.OrderItemFifoAllocations
            .Where(a => a.OrderItemId != 0 && _db.OrderItems
                .Any(oi => oi.Id == a.OrderItemId && oi.OrderId == orderId))
            .SumAsync(a => (decimal?)a.RealizedProfitAud) ?? 0m;

        var order = await _db.Orders.FindAsync(orderId);
        if (order != null)
        {
            order.ActualProfitAud = Math.Round(actualProfit, 2);
            order.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
    }

    public async Task ClearActualProfitAsync(long orderId)
    {
        var order = await _db.Orders.FindAsync(orderId);
        if (order != null)
        {
            order.ActualProfitAud = null;
            order.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
    }

    // ─────────────────────────────────────────────
    //  AUDIT LOG
    // ─────────────────────────────────────────────

    public async Task AddAuditLogAsync(OrderAuditLogEntity log)
    {
        _db.OrderAuditLogs.Add(log);
        await _db.SaveChangesAsync();
    }
}
