namespace SaleManagement.Api.Models;

// ─────────────────────────────────────────────
//  CUSTOMER DTOs
// ─────────────────────────────────────────────

public record CustomerDto(
    long Id,
    string CustomerCode,
    string FirstName,
    string LastName,
    string? Email,
    string Phone,
    bool IsVip,
    int LoyaltyPoints,
    string? Notes,
    DateTime CreatedAt
);

public record CreateCustomerDto(
    string FirstName,
    string LastName,
    string Phone,
    string? Email = null,
    string? Notes = null
);

// ─────────────────────────────────────────────
//  ORDER DTOs
// ─────────────────────────────────────────────

public record CreateOrderItemDto(
    long VariantId,
    int Quantity,
    bool UseVipPrice = false
);

public record CreateOrderDto(
    long StoreId,
    long? CustomerId,
    string OrderType,               // ORDER_NOW | PRE_ORDER
    string OrderChannel,            // POS | WEB | PHONE
    decimal ShippingFeeAud,
    decimal DiscountAud,
    DateOnly? TargetDeliveryDate,
    string? Notes,
    List<CreateOrderItemDto> Items
);

public record AddPaymentDto(
    string PaymentMethod,           // CASH, EFTPOS, VISA, MASTERCARD, PAYPAL, BANK_TRANSFER
    decimal AmountAud,
    decimal? CashTenderedAud,
    string? TransactionRef,
    string IdempotencyKey
);

public record OrderItemDto(
    long Id,
    long VariantId,
    string VariantSku,
    string VariantName,
    int Quantity,
    decimal UnitPriceAud,
    int StageApplied,
    bool IsVipPrice,
    decimal DiscountAud,
    decimal LineTotalAud
);

public record FifoAllocationDto(
    long Id,
    long FifoLotId,
    string LotNumber,
    int QuantityAllocated,
    decimal UnitLandedCostAud,
    decimal UnitSellingPriceAud,
    decimal RealizedProfitAud,
    string Status
);

public record OrderPaymentDto(
    long Id,
    string PaymentMethod,
    decimal AmountAud,
    decimal? CashTenderedAud,
    decimal? ChangeGivenAud,
    string? TransactionRef,
    DateTime PaidAt
);

public record OrderDto(
    long Id,
    string OrderCode,
    long? CustomerId,
    string? CustomerName,
    long StoreId,
    string StoreName,
    string OrderChannel,
    string OrderType,
    string Status,
    string PaymentStatus,
    decimal SubtotalAud,
    decimal DiscountAud,
    decimal ShippingFeeAud,
    decimal TotalAud,
    decimal PaidAmountAud,
    decimal ExpectedProfitAud,
    decimal? ActualProfitAud,
    DateOnly? TargetDeliveryDate,
    string? Notes,
    DateTime CreatedAt,
    List<OrderItemDto> Items,
    List<OrderPaymentDto> Payments
);

public record OrderListItemDto(
    long Id,
    string OrderCode,
    string? CustomerName,
    string StoreName,
    string OrderChannel,
    string OrderType,
    string Status,
    string PaymentStatus,
    decimal TotalAud,
    decimal PaidAmountAud,
    DateOnly? TargetDeliveryDate,
    DateTime CreatedAt
);

public record UpdateOrderStatusDto(string Status);

// ─────────────────────────────────────────────
//  INVENTORY DTOs
// ─────────────────────────────────────────────

public record WarehouseInventoryDto(
    long WarehouseId,
    string WarehouseName,
    long VariantId,
    string VariantSku,
    string VariantName,
    int OnHandQuantity,
    int ReservedQuantity,
    int AvailableQuantity,
    int LowStockThreshold,
    bool IsLowStock
);
