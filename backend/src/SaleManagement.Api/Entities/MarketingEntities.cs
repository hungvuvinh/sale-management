using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SaleManagement.Api.Entities;

[Table("promotions")]
public class PromotionEntity
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Required]
    [Column("name")]
    public string Name { get; set; } = null!;

    [Column("badge_label")]
    public string? BadgeLabel { get; set; }

    [Required]
    [Column("discount_type")]
    public string DiscountType { get; set; } = "PERCENTAGE"; // PERCENTAGE, FIXED_AMOUNT

    [Column("discount_value")]
    public decimal DiscountValue { get; set; }

    [Column("start_date")]
    public DateTime StartDate { get; set; }

    [Column("end_date")]
    public DateTime EndDate { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<PromotionProductEntity> PromotionProducts { get; set; } = new();
}

[Table("promotion_products")]
public class PromotionProductEntity
{
    [Column("promotion_id")]
    public long PromotionId { get; set; }

    [ForeignKey(nameof(PromotionId))]
    public PromotionEntity Promotion { get; set; } = null!;

    [Column("product_id")]
    public long ProductId { get; set; }

    [ForeignKey(nameof(ProductId))]
    public ProductEntity Product { get; set; } = null!;
}

[Table("ad_spend_logs")]
public class AdSpendLogEntity
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Required]
    [Column("campaign_name")]
    public string CampaignName { get; set; } = null!;

    [Required]
    [Column("platform")]
    public string Platform { get; set; } = "FACEBOOK"; // FACEBOOK, GOOGLE, TIKTOK, OTHER

    [Column("spend_aud")]
    public decimal SpendAud { get; set; }

    [Column("start_date")]
    public DateTime StartDate { get; set; }

    [Column("end_date")]
    public DateTime EndDate { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

[Table("sales_shifts")]
public class SalesShiftEntity
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("user_id")]
    public long UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public UserEntity User { get; set; } = null!;

    [Column("store_id")]
    public long StoreId { get; set; }

    [ForeignKey(nameof(StoreId))]
    public StoreEntity Store { get; set; } = null!;

    [Column("work_date")]
    public DateTime WorkDate { get; set; }

    [Column("hours_worked")]
    public decimal HoursWorked { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

[Table("sales_commissions")]
public class SalesCommissionEntity
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("user_id")]
    public long UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public UserEntity User { get; set; } = null!;

    [Column("period_start")]
    public DateTime PeriodStart { get; set; }

    [Column("period_end")]
    public DateTime PeriodEnd { get; set; }

    [Column("total_sales_aud")]
    public decimal TotalSalesAud { get; set; }

    [Column("total_hours_worked")]
    public decimal TotalHoursWorked { get; set; }

    [Column("sales_per_hour_aud")]
    public decimal SalesPerHourAud { get; set; }

    [Column("kpi_threshold_aud")]
    public decimal KpiThresholdAud { get; set; }

    [Column("target_sales_aud")]
    public decimal TargetSalesAud { get; set; }

    [Column("excess_sales_aud")]
    public decimal ExcessSalesAud { get; set; }

    [Column("commission_rate_pct")]
    public decimal CommissionRatePct { get; set; }

    [Column("gross_commission_aud")]
    public decimal GrossCommissionAud { get; set; }

    [Column("superannuation_pct")]
    public decimal SuperannuationPct { get; set; } = 9.50m; // 9.5% Australian Superannuation

    [Column("superannuation_aud")]
    public decimal SuperannuationAud { get; set; }

    [Column("net_commission_aud")]
    public decimal NetCommissionAud { get; set; }

    [Column("status")]
    public string Status { get; set; } = "DRAFT"; // DRAFT, FINALIZED, PAID

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

[Table("product_combos")]
public class ProductComboEntity
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Required]
    [Column("name")]
    public string Name { get; set; } = null!;

    [Required]
    [Column("code")]
    public string Code { get; set; } = null!;

    [Column("description")]
    public string? Description { get; set; }

    [Column("combo_price_aud")]
    public decimal ComboPriceAud { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<ComboItemEntity> ComboItems { get; set; } = new();
}

[Table("combo_items")]
public class ComboItemEntity
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("combo_id")]
    public long ComboId { get; set; }

    [ForeignKey(nameof(ComboId))]
    public ProductComboEntity Combo { get; set; } = null!;

    [Column("variant_id")]
    public long VariantId { get; set; }

    [ForeignKey(nameof(VariantId))]
    public ProductVariantEntity Variant { get; set; } = null!;

    [Column("quantity")]
    public int Quantity { get; set; } = 1;
}
