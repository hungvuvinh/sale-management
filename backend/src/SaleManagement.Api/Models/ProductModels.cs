using System.Text.Json;
using System.Text.Json.Nodes;

namespace SaleManagement.Api.Models;

public record CategoryDto(
    long Id,
    long? ParentId,
    string Name,
    string Slug,
    string? Description,
    int DisplayOrder,
    bool IsActive
);

public record CreateCategoryDto(
    long? ParentId,
    string Name,
    string Slug,
    string? Description,
    int DisplayOrder = 0
);

public record ProductListDto(
    long Id,
    string Sku,
    string Name,
    string Slug,
    long? CategoryId,
    string? CategoryName,
    string? MainImageUrl,
    bool IsActive,
    int CurrentStage,
    List<VariantSummaryDto> Variants
);

public record VariantSummaryDto(
    long VariantId,
    string Sku,
    string Name,
    string? Barcode,
    JsonNode Attributes,
    decimal WeightKg,
    decimal Cbm,
    int BoxCount,
    decimal CurrentPriceAud,
    decimal? VipPriceAud,
    Dictionary<string, decimal> StagePrices,
    StockSummaryDto Stock
);

public record StockSummaryDto(
    int OnHand,
    int Reserved,
    int Available
);

public record ProductDetailDto(
    long Id,
    string Sku,
    string Name,
    string Slug,
    long? CategoryId,
    string? CategoryName,
    string? Description,
    string? MaterialsSummary,
    string? MainImageUrl,
    JsonNode GalleryImages,
    string? VideoUrl,
    bool IsActive,
    DateTime CreatedAt,
    List<VariantDetailDto> Variants
);

public record VariantDetailDto(
    long Id,
    long ProductId,
    string Sku,
    string Name,
    string? Barcode,
    JsonNode AttributesJson,
    decimal WeightKg,
    decimal Cbm,
    int BoxCount,
    JsonNode? DimensionsCm,
    bool IsActive,
    StockSummaryDto Stock,
    int CurrentStage,
    decimal CurrentPriceAud,
    decimal? VipPriceAud,
    Dictionary<string, decimal> StagePrices
);

public record CreateProductDto(
    string Sku,
    string Name,
    string Slug,
    long? CategoryId,
    string? Description,
    string? MaterialsSummary,
    string? MainImageUrl,
    JsonNode? GalleryImages,
    string? VideoUrl
);

public record UpdateProductDto(
    string? Sku,
    string Name,
    string Slug,
    long? CategoryId,
    string? Description,
    string? MaterialsSummary,
    string? MainImageUrl,
    JsonNode? GalleryImages,
    string? VideoUrl,
    bool IsActive
);

public record CreateVariantDto(
    string Sku,
    string Name,
    string? Barcode,
    JsonNode AttributesJson,
    decimal WeightKg,
    decimal Cbm,
    int BoxCount = 1,
    JsonNode? DimensionsCm = null
);

public record UpdateVariantDto(
    string Sku,
    string Name,
    string? Barcode,
    JsonNode AttributesJson,
    decimal WeightKg,
    decimal Cbm,
    int BoxCount,
    JsonNode? DimensionsCm,
    bool IsActive
);

public record UpdateStagePricesDto(
    long FifoLotId,
    decimal Stage1Price,
    decimal Stage2Price,
    decimal Stage3Price,
    decimal Stage4Price,
    decimal Stage5Price,
    decimal? VipPrice
);

public record PriceStageCheckRequestDto(
    long? VariantId,
    long? FifoLotId
);

public record PriceStageCheckResultDto(
    long VariantId,
    string Sku,
    long FifoLotId,
    bool StageChanged,
    int OldStage,
    int NewStage,
    decimal OldPriceAud,
    decimal NewPriceAud,
    string Reason
);

public record PriceAuditLogDto(
    long Id,
    long FifoLotId,
    int OldStage,
    int NewStage,
    decimal OldPriceAud,
    decimal NewPriceAud,
    string TriggeredBy,
    long? TriggeredByUser,
    int RemainingStockQty,
    decimal RemainingStockPct,
    int DaysAtOldStage,
    string? Reason,
    DateTime ChangedAt
);

public record PricingStageConfigDto(
    long Id,
    long VariantId,
    short FromStage,
    short ToStage,
    decimal TargetStockPct,
    int MinDaysAtStage,
    int MaxDaysAtStage
);

public record SavePricingStageConfigsDto(
    List<StageConfigItemInputDto> Configs
);

public record StageConfigItemInputDto(
    short FromStage,
    short ToStage,
    decimal TargetStockPct,
    int MinDaysAtStage = 14,
    int MaxDaysAtStage = 60
);

public record UpdateVariantDailyPriceDto(
    decimal CurrentDailyPrice
);

public record StoreVariantPriceDto(
    long Id,
    long StoreId,
    long VariantId,
    decimal CurrentDailyPrice,
    DateTime UpdatedAt
);

public record UpdateStoreVariantDailyPriceDto(
    long StoreId,
    long VariantId,
    decimal CurrentDailyPrice
);

