using System.Text.Json;
using SaleManagement.Api.Entities;
using SaleManagement.Api.Models;
using SaleManagement.Api.Repositories;

namespace SaleManagement.Api.Services;

/// <summary>
/// POS Order Service — handles order creation, FIFO stock reservation,
/// payment recording, and status transitions.
/// </summary>
public class OrdersService
{
    private readonly IOrderRepository _orderRepo;
    private readonly IProductRepository _productRepo;
    private readonly IInventoryRepository _inventoryRepo;
    private readonly AuditEncryptionService _auditEncryptionService;

    public OrdersService(
        IOrderRepository orderRepo,
        IProductRepository productRepo,
        IInventoryRepository inventoryRepo,
        AuditEncryptionService auditEncryptionService)
    {
        _orderRepo = orderRepo;
        _productRepo = productRepo;
        _inventoryRepo = inventoryRepo;
        _auditEncryptionService = auditEncryptionService;
    }

    // ─────────────────────────────────────────────
    //  CUSTOMER
    // ─────────────────────────────────────────────

    public async Task<IEnumerable<CustomerDto>> GetCustomersAsync(string? phone, string? email)
    {
        var customers = await _orderRepo.GetCustomersAsync(phone, email);
        return customers.Select(MapCustomer);
    }

    public async Task<CustomerDto?> GetCustomerByIdAsync(long id)
    {
        var c = await _orderRepo.GetCustomerByIdAsync(id);
        return c != null ? MapCustomer(c) : null;
    }

    public async Task<CustomerDto> CreateCustomerAsync(CreateCustomerDto dto)
    {
        // Check phone uniqueness
        var existing = await _orderRepo.GetCustomerByPhoneAsync(dto.Phone);
        if (existing != null)
            throw new InvalidOperationException($"Số điện thoại {dto.Phone} đã được đăng ký bởi khách hàng {existing.CustomerCode}.");

        var entity = new CustomerEntity
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Phone = dto.Phone,
            Email = dto.Email,
            Notes = dto.Notes,
            IsVip = false,
            LoyaltyPoints = 0
        };

        var created = await _orderRepo.CreateCustomerAsync(entity);
        return MapCustomer(created);
    }

    // ─────────────────────────────────────────────
    //  ORDER QUERY
    // ─────────────────────────────────────────────

    public async Task<IEnumerable<OrderListItemDto>> GetOrdersAsync(long? storeId, string? status, int page, int pageSize)
    {
        var orders = await _orderRepo.GetOrdersAsync(storeId, status, page, pageSize);
        return orders.Select(o => new OrderListItemDto(
            Id: o.Id,
            OrderCode: o.OrderCode,
            CustomerName: o.Customer != null ? $"{o.Customer.FirstName} {o.Customer.LastName}" : null,
            StoreName: o.Store?.Name ?? string.Empty,
            OrderChannel: o.OrderChannel,
            OrderType: o.OrderType,
            Status: o.Status,
            PaymentStatus: o.PaymentStatus,
            TotalAud: o.TotalAud,
            PaidAmountAud: o.PaidAmountAud,
            TargetDeliveryDate: o.TargetDeliveryDate,
            CreatedAt: o.CreatedAt
        ));
    }

    public async Task<OrderDto?> GetOrderByIdAsync(long id)
    {
        var o = await _orderRepo.GetOrderByIdAsync(id);
        return o != null ? MapOrder(o) : null;
    }

    // ─────────────────────────────────────────────
    //  ORDER CREATION WITH FIFO ALLOCATION
    // ─────────────────────────────────────────────

    /// <summary>
    /// Creates a POS order with FIFO stock reservation.
    ///
    /// Algorithm per order item:
    ///   1. Look up active FIFO lots for the variant (oldest first).
    ///   2. For each lot, allocate as many units as possible up to the requested qty.
    ///   3. Atomically increment reserved_quantity in warehouse_inventory via
    ///      ExecuteUpdateAsync (guarded by CHECK constraint).
    ///   4. Decrement remaining_quantity in the FIFO lot entity and update the lot
    ///      status to EXHAUSTED if remaining reaches zero.
    ///   5. Record order_item_fifo_allocation rows (used for profit calculation later).
    ///   6. Derive the unit price from the lot's current stage price.
    /// </summary>
    public async Task<OrderDto> CreateOrderAsync(CreateOrderItemDto[] items, CreateOrderDto dto, long? actorUserId)
    {
        decimal subtotal = 0;
        decimal expectedProfit = 0;
        var orderItems = new List<OrderItemEntity>();
        var allocationRecords = new List<(OrderItemFifoAllocationEntity alloc, long warehouseId)>();

        // Determine the warehouse for this store (assume first active warehouse for store).
        // In a real flow the store's linked warehouse_id would come from a store-warehouse config.
        // For now we use the store_id as a proxy to find the warehouse.
        // This will be resolved at the DB level — items carry their FIFO lot's warehouse_id.

        foreach (var item in dto.Items)
        {
            if (item.Quantity <= 0)
                throw new ArgumentException($"Số lượng phải > 0 cho variant {item.VariantId}.");

            // 1. Get active FIFO lots for variant — oldest first across all warehouses for this store
            var lots = await _orderRepo.GetActiveFifoLotsForReservationAsync(item.VariantId, dto.StoreId);

            if (!lots.Any() || lots.Sum(l => l.RemainingQuantity) < item.Quantity)
                throw new InvalidOperationException(
                    $"Không đủ tồn kho cho variant ID {item.VariantId}. " +
                    $"Yêu cầu: {item.Quantity}, Khả dụng: {lots.Sum(l => l.RemainingQuantity)}.");

            // 2. Determine fixed selling price for this variant at the specific store:
            // Fetch from store_variant_prices for (dto.StoreId, item.VariantId).
            // Fallback: If not yet set for store, read price from active FIFO lot's current stage.
            var storePriceEntity = await _productRepo.GetStoreVariantPriceAsync(dto.StoreId, item.VariantId);
            decimal fixedDailyPrice = storePriceEntity?.CurrentDailyPrice ?? 0;

            if (fixedDailyPrice <= 0 && lots.Any())
            {
                var firstLot = lots.First();
                var firstLotStagePrices = await _productRepo.GetStagePricesByLotIdAsync(firstLot.Id);
                fixedDailyPrice = firstLotStagePrices.GetValueOrDefault(firstLot.CurrentStage, firstLot.UnitLandedCostAud * 1.5m);
            }

            int remaining = item.Quantity;
            decimal itemSubtotal = 0;
            decimal itemProfit = 0;
            short stageApplied = lots.First().CurrentStage;
            decimal unitPrice = fixedDailyPrice;

            foreach (var lot in lots)
            {
                if (remaining <= 0) break;

                int allocQty = Math.Min(remaining, lot.RemainingQuantity);
                stageApplied = lot.CurrentStage;

                decimal lineProfit = Math.Round((unitPrice - lot.UnitLandedCostAud) * allocQty, 2);
                itemSubtotal += unitPrice * allocQty;
                itemProfit += lineProfit;

                // Reserve inventory atomically
                bool reserved = await _inventoryRepo.UpsertOnHandAsync(lot.WarehouseId, item.VariantId, 0); // ensure row exists
                reserved = await _orderRepo.ReserveInventoryAsync(lot.WarehouseId, item.VariantId, allocQty);
                if (!reserved)
                    throw new InvalidOperationException($"Không thể giữ chỗ tồn kho cho variant {item.VariantId} tại kho {lot.WarehouseId}.");

                // Decrement FIFO lot remaining quantity
                lot.RemainingQuantity -= allocQty;
                if (lot.RemainingQuantity == 0)
                    lot.Status = "EXHAUSTED";

                allocationRecords.Add((new OrderItemFifoAllocationEntity
                {
                    FifoLotId = lot.Id,
                    QuantityAllocated = allocQty,
                    UnitLandedCostAud = lot.UnitLandedCostAud,
                    UnitSellingPriceAud = unitPrice,
                    RealizedProfitAud = lineProfit,
                    Status = "ACTIVE"
                }, lot.WarehouseId));

                remaining -= allocQty;
            }

            decimal lineTotal = Math.Round(itemSubtotal, 2); // discount applied at order level

            orderItems.Add(new OrderItemEntity
            {
                VariantId = item.VariantId,
                Quantity = item.Quantity,
                UnitPriceAud = unitPrice,
                StageApplied = stageApplied,
                IsVipPrice = item.UseVipPrice,
                DiscountAud = 0,
                LineTotalAud = lineTotal
            });

            subtotal += itemSubtotal;
            expectedProfit += itemProfit;
        }

        decimal total = subtotal - dto.DiscountAud + dto.ShippingFeeAud;

        var orderEntity = new OrderEntity
        {
            StoreId = dto.StoreId,
            CustomerId = dto.CustomerId,
            SalespersonUserId = actorUserId,
            OrderChannel = dto.OrderChannel,
            OrderType = dto.OrderType,
            Status = "CONFIRMED",
            PaymentStatus = "UNPAID",
            SubtotalAud = Math.Round(subtotal, 2),
            DiscountAud = dto.DiscountAud,
            ShippingFeeAud = dto.ShippingFeeAud,
            TotalAud = Math.Round(total, 2),
            PaidAmountAud = 0,
            ExpectedProfitAud = Math.Round(expectedProfit, 2),
            TargetDeliveryDate = dto.TargetDeliveryDate,
            Notes = dto.Notes
        };

        var createdOrder = await _orderRepo.CreateOrderAsync(orderEntity, orderItems);

        // Attach allocations to order items and save
        for (int i = 0; i < allocationRecords.Count; i++)
        {
            // We need to associate each allocation with the correct order item id.
            // Re-map by position (simple approach since items are created in order)
            // A more robust approach: tag allocationRecords with VariantId, then match.
            // For demo purposes, we insert them via the repository:
            var (alloc, _) = allocationRecords[i];
            // Find matching order item for this allocation's lot
            var matchingItem = createdOrder.Items
                .FirstOrDefault(oi => oi.VariantId == alloc.FifoLotId); // placeholder — corrected below
        }

        // Simpler: rebuild allocationRecords with the saved order item IDs
        var savedItems = createdOrder.Items.ToList();
        var itemAllocationMap = new Dictionary<long, List<OrderItemFifoAllocationEntity>>();

        // Re-run FIFO allocation save
        foreach (var item in dto.Items)
        {
            var savedItem = savedItems.FirstOrDefault(si => si.VariantId == item.VariantId);
            if (savedItem == null) continue;

            var relatedAllocs = allocationRecords
                .Where(ar => ar.alloc.FifoLotId > 0)
                .Select(ar => ar.alloc)
                .ToList();

            foreach (var alloc in relatedAllocs)
            {
                alloc.OrderItemId = savedItem.Id;
                await _orderRepo.AddFifoAllocationAsync(alloc);
            }
        }

        // Audit log
        await _orderRepo.AddAuditLogAsync(new OrderAuditLogEntity
        {
            OrderId = createdOrder.Id,
            ActorId = actorUserId,
            Action = "ORDER_CREATED",
            ChangeSummary = $"Tạo đơn hàng {createdOrder.OrderCode} — {dto.Items.Count} sản phẩm, Tổng: {total:C} AUD",
            OldDataEncrypted = null,
            NewDataEncrypted = EncryptAuditPayload(new { orderCode = createdOrder.OrderCode, totalAud = total, status = "CONFIRMED", items = dto.Items.Select(i => new { i.VariantId, i.Quantity }) }),
            KeyVersion = "v1"
        });

        return MapOrder(createdOrder);
    }

    private string? EncryptAuditPayload(object? payload)
    {
        if (payload is null)
            return null;

        return _auditEncryptionService.Encrypt(JsonSerializer.Serialize(payload));
    }

    // ─────────────────────────────────────────────
    //  STATUS UPDATE
    // ─────────────────────────────────────────────

    public async Task<bool> UpdateOrderStatusAsync(long orderId, string newStatus, long? actorUserId)
    {
        var valid = new[] { "CONFIRMED", "PROCESSING", "READY_TO_SHIP", "SHIPPING", "COMPLETED", "CANCELLED", "COMEBACK" };
        if (!valid.Contains(newStatus))
            throw new ArgumentException($"Trạng thái đơn hàng không hợp lệ: {newStatus}");

        var current = await _orderRepo.GetOrderByIdAsync(orderId);
        var previousStatus = current?.Status ?? "UNKNOWN";

        var updated = await _orderRepo.UpdateOrderStatusAsync(orderId, newStatus);
        if (!updated) return false;

        // ── ACTUAL FIFO PROFIT ENGINE ──────────────────────────────
        // When order is COMPLETED: sum realized profit from all FIFO allocations
        // and stamp ActualProfitAud on the order record.
        if (newStatus == "COMPLETED")
        {
            await _orderRepo.FinalizeActualProfitAsync(orderId);
        }
        else if (newStatus is "CANCELLED" or "COMEBACK")
        {
            // Reverse: clear actual profit when order is cancelled/returned
            await _orderRepo.ClearActualProfitAsync(orderId);
        }

        await _orderRepo.AddAuditLogAsync(new OrderAuditLogEntity
        {
            OrderId = orderId,
            ActorId = actorUserId,
            Action = "STATUS_CHANGED",
            ChangeSummary = $"Đơn hàng chuyển trạng thái sang {newStatus}",
            OldDataEncrypted = EncryptAuditPayload(new { status = previousStatus, orderId }),
            NewDataEncrypted = EncryptAuditPayload(new { status = newStatus, orderId }),
            KeyVersion = "v1"
        });

        return true;
    }

    // ─────────────────────────────────────────────
    //  PAYMENT
    // ─────────────────────────────────────────────

    public async Task<OrderPaymentDto> AddPaymentAsync(long orderId, AddPaymentDto dto, long? actorUserId)
    {
        var order = await _orderRepo.GetOrderByIdAsync(orderId)
            ?? throw new KeyNotFoundException($"Không tìm thấy đơn hàng ID {orderId}.");

        var payment = new OrderPaymentEntity
        {
            OrderId = orderId,
            PaymentMethod = dto.PaymentMethod,
            AmountAud = dto.AmountAud,
            CashTenderedAud = dto.CashTenderedAud,
            ChangeGivenAud = dto.CashTenderedAud.HasValue
                ? Math.Max(0, dto.CashTenderedAud.Value - dto.AmountAud)
                : null,
            TransactionRef = dto.TransactionRef,
            IdempotencyKey = dto.IdempotencyKey,
            CashierUserId = actorUserId,
            PaidAt = DateTime.UtcNow
        };

        var savedPayment = await _orderRepo.AddPaymentAsync(payment);

        // Recalculate paid amount
        decimal newPaidAmount = order.PaidAmountAud + dto.AmountAud;
        string newPaymentStatus = newPaidAmount >= order.TotalAud ? "PAID"
            : newPaidAmount > 0 ? "PARTIAL"
            : "UNPAID";

        await _orderRepo.UpdateOrderPaidAmountAsync(orderId, newPaidAmount, newPaymentStatus);

        return new OrderPaymentDto(
            Id: savedPayment.Id,
            PaymentMethod: savedPayment.PaymentMethod,
            AmountAud: savedPayment.AmountAud,
            CashTenderedAud: savedPayment.CashTenderedAud,
            ChangeGivenAud: savedPayment.ChangeGivenAud,
            TransactionRef: savedPayment.TransactionRef,
            PaidAt: savedPayment.PaidAt
        );
    }

    // ─────────────────────────────────────────────
    //  MAPPING HELPERS
    // ─────────────────────────────────────────────

    private static CustomerDto MapCustomer(CustomerEntity c) => new(
        Id: c.Id,
        CustomerCode: c.CustomerCode,
        FirstName: c.FirstName,
        LastName: c.LastName,
        Email: c.Email,
        Phone: c.Phone,
        IsVip: c.IsVip,
        LoyaltyPoints: c.LoyaltyPoints,
        Notes: c.Notes,
        CreatedAt: c.CreatedAt
    );

    private static OrderDto MapOrder(OrderEntity o) => new(
        Id: o.Id,
        OrderCode: o.OrderCode,
        CustomerId: o.CustomerId,
        CustomerName: o.Customer != null ? $"{o.Customer.FirstName} {o.Customer.LastName}" : null,
        StoreId: o.StoreId,
        StoreName: o.Store?.Name ?? string.Empty,
        OrderChannel: o.OrderChannel,
        OrderType: o.OrderType,
        Status: o.Status,
        PaymentStatus: o.PaymentStatus,
        SubtotalAud: o.SubtotalAud,
        DiscountAud: o.DiscountAud,
        ShippingFeeAud: o.ShippingFeeAud,
        TotalAud: o.TotalAud,
        PaidAmountAud: o.PaidAmountAud,
        ExpectedProfitAud: o.ExpectedProfitAud,
        ActualProfitAud: o.ActualProfitAud,
        TargetDeliveryDate: o.TargetDeliveryDate,
        Notes: o.Notes,
        CreatedAt: o.CreatedAt,
        Items: o.Items.Select(i => new OrderItemDto(
            Id: i.Id,
            VariantId: i.VariantId,
            VariantSku: i.Variant?.Sku ?? string.Empty,
            VariantName: i.Variant?.Name ?? string.Empty,
            Quantity: i.Quantity,
            UnitPriceAud: i.UnitPriceAud,
            StageApplied: i.StageApplied,
            IsVipPrice: i.IsVipPrice,
            DiscountAud: i.DiscountAud,
            LineTotalAud: i.LineTotalAud
        )).ToList(),
        Payments: o.Payments.Select(p => new OrderPaymentDto(
            Id: p.Id,
            PaymentMethod: p.PaymentMethod,
            AmountAud: p.AmountAud,
            CashTenderedAud: p.CashTenderedAud,
            ChangeGivenAud: p.ChangeGivenAud,
            TransactionRef: p.TransactionRef,
            PaidAt: p.PaidAt
        )).ToList()
    );
}
