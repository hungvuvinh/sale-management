using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SaleManagement.Api.Entities;

[Table("suppliers")]
public class SupplierEntity
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Required]
    [Column("name")]
    public string Name { get; set; } = null!;

    [Required]
    [Column("country")]
    public string Country { get; set; } = null!;

    [Column("contact_name")]
    public string? ContactName { get; set; }

    [Column("contact_phone")]
    public string? ContactPhone { get; set; }

    [Column("contact_email")]
    public string? ContactEmail { get; set; }

    [Column("lead_time_days")]
    public int LeadTimeDays { get; set; } = 30;

    [Column("payment_terms")]
    public string? PaymentTerms { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

[Table("stock_orders")]
public class StockOrderEntity
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Required]
    [Column("po_number")]
    public string PoNumber { get; set; } = null!;

    [Column("supplier_id")]
    public long SupplierId { get; set; }

    [ForeignKey(nameof(SupplierId))]
    public SupplierEntity Supplier { get; set; } = null!;

    [Column("destination_warehouse_id")]
    public long DestinationWarehouseId { get; set; }

    [ForeignKey(nameof(DestinationWarehouseId))]
    public WarehouseEntity DestinationWarehouse { get; set; } = null!;

    [Column("container_code")]
    public string? ContainerCode { get; set; }

    [Column("eta_date")]
    public DateOnly EtaDate { get; set; }

    [Column("actual_arrival_date")]
    public DateOnly? ActualArrivalDate { get; set; }

    [Required]
    [Column("status")]
    public string Status { get; set; } = "PENDING";

    [Column("total_cbm")]
    public decimal TotalCbm { get; set; }

    [Column("container_freight_aud")]
    public decimal ContainerFreightAud { get; set; }

    [Column("customs_tax_aud")]
    public decimal CustomsTaxAud { get; set; }

    [Column("currency_code")]
    public string CurrencyCode { get; set; } = "USD";

    [Column("exchange_rate")]
    public decimal ExchangeRate { get; set; } = 1.50m;

    [Column("notes")]
    public string? Notes { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<StockOrderItemEntity> Items { get; set; } = new List<StockOrderItemEntity>();
}

[Table("stock_order_items")]
public class StockOrderItemEntity
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("stock_order_id")]
    public long StockOrderId { get; set; }

    [ForeignKey(nameof(StockOrderId))]
    public StockOrderEntity StockOrder { get; set; } = null!;

    [Column("variant_id")]
    public long VariantId { get; set; }

    [ForeignKey(nameof(VariantId))]
    public ProductVariantEntity Variant { get; set; } = null!;

    [Column("quantity_ordered")]
    public int QuantityOrdered { get; set; }

    [Column("quantity_received")]
    public int QuantityReceived { get; set; }

    [Column("unit_cost_foreign")]
    public decimal UnitCostForeign { get; set; }

    [Column("unit_cost_aud")]
    public decimal UnitCostAud { get; set; }

    [Column("unit_cbm")]
    public decimal UnitCbm { get; set; }

    [Column("unit_freight_aud")]
    public decimal UnitFreightAud { get; set; }

    [Column("calculated_landed_cost_aud")]
    public decimal CalculatedLandedCostAud { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

[Table("fifo_lots")]
public class FifoLotEntity
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Required]
    [Column("lot_number")]
    public string LotNumber { get; set; } = null!;

    [Column("variant_id")]
    public long VariantId { get; set; }

    [ForeignKey(nameof(VariantId))]
    public ProductVariantEntity Variant { get; set; } = null!;

    [Column("warehouse_id")]
    public long WarehouseId { get; set; }

    [ForeignKey(nameof(WarehouseId))]
    public WarehouseEntity Warehouse { get; set; } = null!;

    [Column("stock_order_item_id")]
    public long StockOrderItemId { get; set; }

    [ForeignKey(nameof(StockOrderItemId))]
    public StockOrderItemEntity StockOrderItem { get; set; } = null!;

    [Column("initial_quantity")]
    public int InitialQuantity { get; set; }

    [Column("remaining_quantity")]
    public int RemainingQuantity { get; set; }

    [Column("unit_landed_cost_aud")]
    public decimal UnitLandedCostAud { get; set; }

    [Column("current_stage")]
    public short CurrentStage { get; set; } = 1;

    [Column("received_date")]
    public DateOnly ReceivedDate { get; set; }

    [Required]
    [Column("status")]
    public string Status { get; set; } = "ACTIVE";

    [Column("activated_at")]
    public DateTime? ActivatedAt { get; set; }

    [Column("exhausted_at")]
    public DateTime? ExhaustedAt { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
