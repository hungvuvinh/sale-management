using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SaleManagement.Api.Entities;

// ----------------------------------------------------------------
// 1. customers
// ----------------------------------------------------------------
[Table("customers")]
public class CustomerEntity
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Required]
    [Column("customer_code")]
    public string CustomerCode { get; set; } = null!;

    [Required]
    [Column("first_name")]
    public string FirstName { get; set; } = null!;

    [Required]
    [Column("last_name")]
    public string LastName { get; set; } = null!;

    [Column("email")]
    public string? Email { get; set; }

    [Required]
    [Column("phone")]
    public string Phone { get; set; } = null!;

    [Column("address_id")]
    public long? AddressId { get; set; }

    [ForeignKey(nameof(AddressId))]
    public AddressEntity? Address { get; set; }

    [Column("is_vip")]
    public bool IsVip { get; set; } = false;

    [Column("loyalty_points")]
    public int LoyaltyPoints { get; set; } = 0;

    [Column("vip_granted_at")]
    public DateTime? VipGrantedAt { get; set; }

    [Column("notes")]
    public string? Notes { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<OrderEntity> Orders { get; set; } = new List<OrderEntity>();
}

// ----------------------------------------------------------------
// 2. orders
// ----------------------------------------------------------------
[Table("orders")]
public class OrderEntity
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Required]
    [Column("order_code")]
    public string OrderCode { get; set; } = null!;

    [Column("customer_id")]
    public long? CustomerId { get; set; }

    [ForeignKey(nameof(CustomerId))]
    public CustomerEntity? Customer { get; set; }

    [Column("store_id")]
    public long StoreId { get; set; }

    [ForeignKey(nameof(StoreId))]
    public StoreEntity Store { get; set; } = null!;

    [Column("salesperson_user_id")]
    public long? SalespersonUserId { get; set; }

    [ForeignKey(nameof(SalespersonUserId))]
    public UserEntity? Salesperson { get; set; }

    [Required]
    [Column("order_channel")]
    public string OrderChannel { get; set; } = "POS"; // POS, WEB, PHONE

    [Required]
    [Column("order_type")]
    public string OrderType { get; set; } = "ORDER_NOW"; // ORDER_NOW, PRE_ORDER

    [Required]
    [Column("status")]
    public string Status { get; set; } = "PENDING";
    // PENDING, CONFIRMED, PROCESSING, READY_TO_SHIP, SHIPPING, COMPLETED, CANCELLED, COMEBACK

    [Required]
    [Column("payment_status")]
    public string PaymentStatus { get; set; } = "UNPAID";
    // UNPAID, PARTIAL, PAID, REFUNDED

    [Column("subtotal_aud")]
    public decimal SubtotalAud { get; set; }

    [Column("discount_aud")]
    public decimal DiscountAud { get; set; }

    [Column("shipping_fee_aud")]
    public decimal ShippingFeeAud { get; set; }

    [Column("total_aud")]
    public decimal TotalAud { get; set; }

    [Column("paid_amount_aud")]
    public decimal PaidAmountAud { get; set; }

    [Column("expected_profit_aud")]
    public decimal ExpectedProfitAud { get; set; }

    [Column("actual_profit_aud")]
    public decimal? ActualProfitAud { get; set; }

    [Column("target_delivery_date")]
    public DateOnly? TargetDeliveryDate { get; set; }

    [Column("notes")]
    public string? Notes { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<OrderItemEntity> Items { get; set; } = new List<OrderItemEntity>();
    public ICollection<OrderPaymentEntity> Payments { get; set; } = new List<OrderPaymentEntity>();
}

// ----------------------------------------------------------------
// 3. order_items
// ----------------------------------------------------------------
[Table("order_items")]
public class OrderItemEntity
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("order_id")]
    public long OrderId { get; set; }

    [ForeignKey(nameof(OrderId))]
    public OrderEntity Order { get; set; } = null!;

    [Column("variant_id")]
    public long VariantId { get; set; }

    [ForeignKey(nameof(VariantId))]
    public ProductVariantEntity Variant { get; set; } = null!;

    [Column("quantity")]
    public int Quantity { get; set; }

    [Column("unit_price_aud")]
    public decimal UnitPriceAud { get; set; }

    [Column("stage_applied")]
    public short StageApplied { get; set; }

    [Column("is_vip_price")]
    public bool IsVipPrice { get; set; } = false;

    [Column("discount_aud")]
    public decimal DiscountAud { get; set; }

    [Column("line_total_aud")]
    public decimal LineTotalAud { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<OrderItemFifoAllocationEntity> FifoAllocations { get; set; } = new List<OrderItemFifoAllocationEntity>();
}

// ----------------------------------------------------------------
// 4. order_item_fifo_allocations
// ----------------------------------------------------------------
[Table("order_item_fifo_allocations")]
public class OrderItemFifoAllocationEntity
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("order_item_id")]
    public long OrderItemId { get; set; }

    [ForeignKey(nameof(OrderItemId))]
    public OrderItemEntity OrderItem { get; set; } = null!;

    [Column("fifo_lot_id")]
    public long FifoLotId { get; set; }

    [ForeignKey(nameof(FifoLotId))]
    public FifoLotEntity FifoLot { get; set; } = null!;

    [Column("quantity_allocated")]
    public int QuantityAllocated { get; set; }

    [Column("unit_landed_cost_aud")]
    public decimal UnitLandedCostAud { get; set; }

    [Column("unit_selling_price_aud")]
    public decimal UnitSellingPriceAud { get; set; }

    [Column("realized_profit_aud")]
    public decimal RealizedProfitAud { get; set; }

    [Required]
    [Column("status")]
    public string Status { get; set; } = "ACTIVE"; // ACTIVE, CANCELLED, RETURNED

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

// ----------------------------------------------------------------
// 5. order_payments
// ----------------------------------------------------------------
[Table("order_payments")]
public class OrderPaymentEntity
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("order_id")]
    public long OrderId { get; set; }

    [ForeignKey(nameof(OrderId))]
    public OrderEntity Order { get; set; } = null!;

    [Required]
    [Column("payment_method")]
    public string PaymentMethod { get; set; } = null!;
    // CASH, EFTPOS, VISA, MASTERCARD, PAYPAL, BANK_TRANSFER

    [Column("amount_aud")]
    public decimal AmountAud { get; set; }

    [Column("cash_tendered_aud")]
    public decimal? CashTenderedAud { get; set; }

    [Column("change_given_aud")]
    public decimal? ChangeGivenAud { get; set; }

    [Column("transaction_ref")]
    public string? TransactionRef { get; set; }

    [Required]
    [Column("idempotency_key")]
    public string IdempotencyKey { get; set; } = null!;

    [Column("cashier_user_id")]
    public long? CashierUserId { get; set; }

    [ForeignKey(nameof(CashierUserId))]
    public UserEntity? Cashier { get; set; }

    [Column("paid_at")]
    public DateTime PaidAt { get; set; } = DateTime.UtcNow;
}

// ----------------------------------------------------------------
// 6. order_audit_logs
// ----------------------------------------------------------------
[Table("order_audit_logs")]
public class OrderAuditLogEntity
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("order_id")]
    public long OrderId { get; set; }

    [ForeignKey(nameof(OrderId))]
    public OrderEntity Order { get; set; } = null!;

    [Column("actor_id")]
    public long? ActorId { get; set; }

    [ForeignKey(nameof(ActorId))]
    public UserEntity? Actor { get; set; }

    [Required]
    [Column("action")]
    public string Action { get; set; } = null!;

    [Required]
    [Column("change_summary")]
    public string ChangeSummary { get; set; } = null!;

    [Column("old_data_encrypted")]
    public string? OldDataEncrypted { get; set; }

    [Column("new_data_encrypted")]
    public string? NewDataEncrypted { get; set; }

    [Required]
    [Column("key_version")]
    public string KeyVersion { get; set; } = "v1";

    [Column("ip_address")]
    public string? IpAddress { get; set; }

    [Column("user_agent")]
    public string? UserAgent { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
