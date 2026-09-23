namespace SaleManagement.Api.Models;

// ─────────────────────────────────────────────
//  SALES SHIFTS
// ─────────────────────────────────────────────

public record SalesShiftDto(
    long Id,
    long UserId,
    string UserName,
    long StoreId,
    string StoreName,
    DateTime WorkDate,
    decimal HoursWorked,
    DateTime CreatedAt
);

public record CreateSalesShiftDto(
    long UserId,
    long StoreId,
    DateTime WorkDate,
    decimal HoursWorked
);

// ─────────────────────────────────────────────
//  SALES COMMISSIONS
// ─────────────────────────────────────────────

public record SalesCommissionDto(
    long Id,
    long UserId,
    string UserName,
    DateTime PeriodStart,
    DateTime PeriodEnd,
    decimal TotalSalesAud,
    decimal TotalHoursWorked,
    decimal SalesPerHourAud,
    decimal KpiThresholdAud,
    decimal TargetSalesAud,
    decimal ExcessSalesAud,
    decimal CommissionRatePct,
    decimal GrossCommissionAud,
    decimal SuperannuationPct,
    decimal SuperannuationAud,
    decimal NetCommissionAud,
    string Status,
    DateTime CreatedAt
);

public record CalculateCommissionDto(
    long UserId,
    DateTime PeriodStart,
    DateTime PeriodEnd,
    /// <summary>KPI threshold in AUD/hour (e.g. 1500 AUD/hour target)</summary>
    decimal KpiThresholdAud,
    /// <summary>Commission rate % applied to excess sales (e.g. 10 for 10%)</summary>
    decimal CommissionRatePct,
    /// <summary>Superannuation % — defaults to 9.5% if null (Australian standard)</summary>
    decimal? SuperannuationPct
);

// ─────────────────────────────────────────────
//  PROMOTIONS
// ─────────────────────────────────────────────

public record PromotionDto(
    long Id,
    string Name,
    string? BadgeLabel,
    string DiscountType,
    decimal DiscountValue,
    DateTime StartDate,
    DateTime EndDate,
    bool IsActive,
    List<long> ProductIds,
    DateTime CreatedAt
);

public record CreatePromotionDto(
    string Name,
    string? BadgeLabel,
    string DiscountType,    // PERCENTAGE | FIXED_AMOUNT
    decimal DiscountValue,
    DateTime StartDate,
    DateTime EndDate,
    bool IsActive = true,
    List<long>? ProductIds = null
);

public record UpdatePromotionDto(
    string? Name,
    string? BadgeLabel,
    decimal? DiscountValue,
    DateTime? StartDate,
    DateTime? EndDate,
    bool? IsActive
);

// ─────────────────────────────────────────────
//  PRODUCT COMBOS
// ─────────────────────────────────────────────

public record ProductComboDto(
    long Id,
    string Name,
    string Code,
    string? Description,
    decimal ComboPriceAud,
    bool IsActive,
    List<ComboItemDto> Items,
    DateTime CreatedAt
);

public record ComboItemDto(
    long Id,
    long VariantId,
    string VariantSku,
    string VariantName,
    int Quantity
);

public record CreateProductComboDto(
    string Name,
    string Code,
    string? Description,
    decimal ComboPriceAud,
    List<CreateComboItemDto>? Items
);

public record CreateComboItemDto(
    long VariantId,
    int Quantity = 1
);

public record UpdateProductComboDto(
    string? Name,
    string? Description,
    decimal? ComboPriceAud,
    bool? IsActive
);

// ─────────────────────────────────────────────
//  AD SPEND LOGS
// ─────────────────────────────────────────────

public record AdSpendLogDto(
    long Id,
    string CampaignName,
    string Platform,
    decimal SpendAud,
    DateTime StartDate,
    DateTime EndDate,
    DateTime CreatedAt
);

public record CreateAdSpendLogDto(
    string CampaignName,
    string Platform,        // FACEBOOK | GOOGLE | TIKTOK | OTHER
    decimal SpendAud,
    DateTime StartDate,
    DateTime EndDate
);
