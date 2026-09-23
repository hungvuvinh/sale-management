using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SaleManagement.Api.Entities;

// ----------------------------------------------------------------
// 1. warehouse_inventory — Số dư tồn kho tổng hợp
// NOTE: available_quantity is a GENERATED column in DB (on_hand - reserved),
//       we map it as a read-only property.
// ----------------------------------------------------------------
[Table("warehouse_inventory")]
public class WarehouseInventoryEntity
{
    [Column("warehouse_id")]
    public long WarehouseId { get; set; }

    [ForeignKey(nameof(WarehouseId))]
    public WarehouseEntity Warehouse { get; set; } = null!;

    [Column("variant_id")]
    public long VariantId { get; set; }

    [ForeignKey(nameof(VariantId))]
    public ProductVariantEntity Variant { get; set; } = null!;

    [Column("on_hand_quantity")]
    public int OnHandQuantity { get; set; }

    [Column("reserved_quantity")]
    public int ReservedQuantity { get; set; }

    // Computed column — DB generated
    [Column("available_quantity")]
    public int AvailableQuantity { get; private set; }

    [Column("low_stock_threshold")]
    public int LowStockThreshold { get; set; } = 5;
}

// ----------------------------------------------------------------
// 2. inventory_transactions — Sổ cái giao dịch kho
// ----------------------------------------------------------------
[Table("inventory_transactions")]
public class InventoryTransactionEntity
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Required]
    [Column("transaction_code")]
    public string TransactionCode { get; set; } = null!;

    [Column("warehouse_id")]
    public long WarehouseId { get; set; }

    [ForeignKey(nameof(WarehouseId))]
    public WarehouseEntity Warehouse { get; set; } = null!;

    [Column("variant_id")]
    public long VariantId { get; set; }

    [ForeignKey(nameof(VariantId))]
    public ProductVariantEntity Variant { get; set; } = null!;

    [Column("fifo_lot_id")]
    public long? FifoLotId { get; set; }

    [ForeignKey(nameof(FifoLotId))]
    public FifoLotEntity? FifoLot { get; set; }

    [Required]
    [Column("transaction_type")]
    public string TransactionType { get; set; } = null!;
    // STOCK_IN, SALE_OUT, TRANSFER_OUT, TRANSFER_IN, ADJUSTMENT, RETURN_IN

    [Column("change_quantity")]
    public int ChangeQuantity { get; set; }

    [Column("reference_id")]
    public long? ReferenceId { get; set; }

    [Column("reference_type")]
    public string? ReferenceType { get; set; }

    [Column("performed_by_user")]
    public long? PerformedByUser { get; set; }

    [ForeignKey(nameof(PerformedByUser))]
    public UserEntity? PerformedBy { get; set; }

    [Column("notes")]
    public string? Notes { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

[Table("inventory_transfers")]
public class InventoryTransferEntity
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Required]
    [Column("transfer_code")]
    public string TransferCode { get; set; } = null!;

    [Column("from_warehouse_id")]
    public long FromWarehouseId { get; set; }

    [ForeignKey(nameof(FromWarehouseId))]
    public WarehouseEntity FromWarehouse { get; set; } = null!;

    [Column("to_warehouse_id")]
    public long ToWarehouseId { get; set; }

    [ForeignKey(nameof(ToWarehouseId))]
    public WarehouseEntity ToWarehouse { get; set; } = null!;

    [Required]
    [Column("status")]
    public string Status { get; set; } = "REQUESTED";

    [Column("requested_by")]
    public long? RequestedBy { get; set; }

    [Column("approved_by")]
    public long? ApprovedBy { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("received_at")]
    public DateTime? ReceivedAt { get; set; }

    public ICollection<InventoryTransferItemEntity> Items { get; set; } = new List<InventoryTransferItemEntity>();
}

[Table("inventory_transfer_items")]
public class InventoryTransferItemEntity
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("transfer_id")]
    public long TransferId { get; set; }

    [ForeignKey(nameof(TransferId))]
    public InventoryTransferEntity Transfer { get; set; } = null!;

    [Column("variant_id")]
    public long VariantId { get; set; }

    [ForeignKey(nameof(VariantId))]
    public ProductVariantEntity Variant { get; set; } = null!;

    [Column("quantity_requested")]
    public int QuantityRequested { get; set; }

    [Column("quantity_received")]
    public int QuantityReceived { get; set; }
}
