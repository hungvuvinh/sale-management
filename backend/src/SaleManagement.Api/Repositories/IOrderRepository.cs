using SaleManagement.Api.Entities;

namespace SaleManagement.Api.Repositories;

public interface IOrderRepository
{
    // Customer
    Task<IEnumerable<CustomerEntity>> GetCustomersAsync(string? phone, string? email);
    Task<CustomerEntity?> GetCustomerByIdAsync(long id);
    Task<CustomerEntity?> GetCustomerByPhoneAsync(string phone);
    Task<CustomerEntity> CreateCustomerAsync(CustomerEntity entity);

    // Order CRUD
    Task<IEnumerable<OrderEntity>> GetOrdersAsync(long? storeId, string? status, int page, int pageSize);
    Task<OrderEntity?> GetOrderByIdAsync(long id);
    Task<OrderEntity?> GetOrderByCodeAsync(string orderCode);
    Task<OrderEntity> CreateOrderAsync(OrderEntity order, List<OrderItemEntity> items);
    Task<bool> UpdateOrderStatusAsync(long orderId, string newStatus);

    // FIFO Allocation (used during order creation)
    /// <summary>Gets active FIFO lots for a variant servicing a given store (via warehouse).</summary>
    Task<List<FifoLotEntity>> GetActiveFifoLotsForReservationAsync(long variantId, long storeId);
    Task<bool> ReserveInventoryAsync(long warehouseId, long variantId, int qty);
    Task AddFifoAllocationAsync(OrderItemFifoAllocationEntity allocation);

    // FIFO Profit Engine
    /// <summary>Sum RealizedProfitAud from all allocations and stamp ActualProfitAud on the order (called on COMPLETED).</summary>
    Task FinalizeActualProfitAsync(long orderId);
    /// <summary>Clears ActualProfitAud when an order is CANCELLED or COMEBACK.</summary>
    Task ClearActualProfitAsync(long orderId);

    // Payment
    Task<OrderPaymentEntity> AddPaymentAsync(OrderPaymentEntity payment);
    Task<bool> UpdateOrderPaidAmountAsync(long orderId, decimal newPaidAmount, string newPaymentStatus);

    // Audit
    Task AddAuditLogAsync(OrderAuditLogEntity log);
}
