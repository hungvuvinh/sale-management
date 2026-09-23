using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SaleManagement.Api.Entities;

[Table("product_categories")]
public class ProductCategoryEntity
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("parent_id")]
    public long? ParentId { get; set; }

    [ForeignKey(nameof(ParentId))]
    public ProductCategoryEntity? ParentCategory { get; set; }

    [Required]
    [Column("name")]
    public string Name { get; set; } = null!;

    [Required]
    [Column("slug")]
    public string Slug { get; set; } = null!;

    [Column("description")]
    public string? Description { get; set; }

    [Column("display_order")]
    public int DisplayOrder { get; set; } = 0;

    [Column("is_active")]
    public bool IsActive { get; set; } = true;
}

[Table("products")]
public class ProductEntity
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Required]
    [Column("sku")]
    public string Sku { get; set; } = null!;

    [Required]
    [Column("name")]
    public string Name { get; set; } = null!;

    [Required]
    [Column("slug")]
    public string Slug { get; set; } = null!;

    [Column("category_id")]
    public long? CategoryId { get; set; }

    [ForeignKey(nameof(CategoryId))]
    public ProductCategoryEntity? Category { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [Column("materials_summary")]
    public string? MaterialsSummary { get; set; }

    [Column("main_image_url")]
    public string? MainImageUrl { get; set; }

    [Column("gallery_images", TypeName = "jsonb")]
    public string GalleryImages { get; set; } = "[]";

    [Column("video_url")]
    public string? VideoUrl { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ProductVariantEntity> Variants { get; set; } = new List<ProductVariantEntity>();
}

[Table("product_variants")]
public class ProductVariantEntity
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("product_id")]
    public long ProductId { get; set; }

    [ForeignKey(nameof(ProductId))]
    public ProductEntity Product { get; set; } = null!;

    [Required]
    [Column("sku")]
    public string Sku { get; set; } = null!;

    [Required]
    [Column("name")]
    public string Name { get; set; } = null!;

    [Column("barcode")]
    public string? Barcode { get; set; }

    [Column("attributes_json", TypeName = "jsonb")]
    public string AttributesJson { get; set; } = "{}";

    [Column("weight_kg")]
    public decimal WeightKg { get; set; }

    [Column("cbm")]
    public decimal Cbm { get; set; }

    [Column("box_count")]
    public int BoxCount { get; set; } = 1;

    [Column("dimensions_cm", TypeName = "jsonb")]
    public string? DimensionsCm { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

[Table("fifo_lot_stage_prices")]
public class FifoLotStagePriceEntity
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("fifo_lot_id")]
    public long FifoLotId { get; set; }

    [Column("stage")]
    public short Stage { get; set; }

    [Column("price_aud")]
    public decimal PriceAud { get; set; }

    [Column("vip_price_aud")]
    public decimal? VipPriceAud { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

[Table("store_variant_prices")]
public class StoreVariantPriceEntity
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("store_id")]
    public long StoreId { get; set; }

    [Column("variant_id")]
    public long VariantId { get; set; }

    [Column("current_daily_price")]
    public decimal CurrentDailyPrice { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

[Table("pricing_stage_configs")]
public class PricingStageConfigEntity
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("variant_id")]
    public long VariantId { get; set; }

    [Column("from_stage")]
    public short FromStage { get; set; }

    [Column("to_stage")]
    public short ToStage { get; set; }

    [Column("target_stock_pct")]
    public decimal TargetStockPct { get; set; }

    [Column("min_days_at_stage")]
    public int MinDaysAtStage { get; set; } = 14;

    [Column("max_days_at_stage")]
    public int MaxDaysAtStage { get; set; } = 60;
}

[Table("stage_price_audit_logs")]
public class StagePriceAuditLogEntity
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("fifo_lot_id")]
    public long FifoLotId { get; set; }

    [Column("old_stage")]
    public short OldStage { get; set; }

    [Column("new_stage")]
    public short NewStage { get; set; }

    [Column("old_price_aud")]
    public decimal OldPriceAud { get; set; }

    [Column("new_price_aud")]
    public decimal NewPriceAud { get; set; }

    [Required]
    [Column("triggered_by")]
    public string TriggeredBy { get; set; } = null!;

    [Column("triggered_by_user")]
    public long? TriggeredByUser { get; set; }

    [Column("remaining_stock_qty")]
    public int RemainingStockQty { get; set; }

    [Column("remaining_stock_pct")]
    public decimal RemainingStockPct { get; set; }

    [Column("days_at_old_stage")]
    public int DaysAtOldStage { get; set; }

    [Column("reason")]
    public string? Reason { get; set; }

    [Column("changed_at")]
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
}
