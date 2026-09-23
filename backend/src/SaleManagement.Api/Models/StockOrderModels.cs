namespace SaleManagement.Api.Models;

public record SupplierDto(
    long Id,
    string Name,
    string Country,
    string? ContactName,
    string? ContactPhone,
    string? ContactEmail,
    int LeadTimeDays,
    string? PaymentTerms,
    bool IsActive,
    DateTime CreatedAt
);

public record CreateSupplierDto(
    string Name,
    string Country,
    string? ContactName,
    string? ContactPhone,
    string? ContactEmail,
    int LeadTimeDays = 30,
    string? PaymentTerms = null
);

public record StockOrderDto(
    long Id,
    string PoNumber,
    long SupplierId,
    string SupplierName,
    long DestinationWarehouseId,
    string WarehouseName,
    string? ContainerCode,
    DateOnly EtaDate,
    DateOnly? ActualArrivalDate,
    string Status,
    decimal TotalCbm,
    decimal ContainerFreightAud,
    decimal CustomsTaxAud,
    string CurrencyCode,
    decimal ExchangeRate,
    string? Notes,
    DateTime CreatedAt,
    List<StockOrderItemDto> Items
);

public record StockOrderItemDto(
    long Id,
    long StockOrderId,
    long VariantId,
    string VariantSku,
    string VariantName,
    int QuantityOrdered,
    int QuantityReceived,
    decimal UnitCostForeign,
    decimal UnitCostAud,
    decimal UnitCbm,
    decimal UnitFreightAud,
    decimal CalculatedLandedCostAud
);

public record CreateStockOrderDto(
    string PoNumber,
    long SupplierId,
    long DestinationWarehouseId,
    string? ContainerCode,
    DateOnly EtaDate,
    decimal ContainerFreightAud,
    decimal CustomsTaxAud,
    string CurrencyCode = "USD",
    decimal ExchangeRate = 1.50m,
    string? Notes = null,
    List<CreateStockOrderItemDto>? Items = null
);

public record CreateStockOrderItemDto(
    long VariantId,
    int QuantityOrdered,
    decimal UnitCostForeign,
    decimal UnitCbm
);

public record CalculateLandedCostRequestDto(
    decimal ContainerFreightAud,
    decimal CustomsTaxAud,
    decimal ExchangeRateUsdAud,
    List<LandedCostItemInputDto> Items
);

public record LandedCostItemInputDto(
    long VariantId,
    int Quantity,
    decimal UnitCostUsd,
    decimal CbmPerUnit,
    decimal TargetMarginPct = 60.0m
);

public record LandedCostCalculationResultDto(
    decimal TotalCbm,
    decimal FreightPerCbmAud,
    List<LandedCostItemResultDto> Calculations
);

public record LandedCostItemResultDto(
    long VariantId,
    decimal UnitCostAud,
    decimal FreightAud,
    decimal TaxAud,
    decimal LandedCostAud,
    decimal SuggestedStage1Price,
    Dictionary<string, decimal> Generated5Stages
);
