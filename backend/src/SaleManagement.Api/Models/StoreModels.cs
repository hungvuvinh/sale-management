namespace SaleManagement.Api.Models;

public record AddressDto(
    long Id,
    string StreetAddress,
    string? AddressDetail,
    string Suburb,
    string City,
    string State,
    long PostcodeId,
    string Postcode,
    decimal? Latitude,
    decimal? Longitude
);

public record StoreDto(
    long Id,
    string Code,
    string Name,
    string Phone,
    string Email,
    bool IsActive,
    AddressDto Address
);

public record WarehouseDto(
    long Id,
    string Code,
    string Name,
    bool IsStore,
    decimal? TotalCapacityCbm,
    bool IsActive,
    AddressDto Address
);

public record ShippingRateDto(
    long Id,
    long StoreId,
    decimal MinDistanceKm,
    decimal MaxDistanceKm,
    decimal ShippingFeeAud
);

public record PostcodeLookupDto(
    string Postcode,
    string Suburb,
    string State,
    StoreDto? AssignedStore,
    IReadOnlyList<StoreDto> AvailableStores,
    bool PriceUnlocked,
    ShippingRateDto? EstimatedShippingRate
);

public record AssignPostcodeDto(
    long PostcodeId,
    long StoreId
);

public record SelectPostcodeStoreDto(
    string Postcode,
    long StoreId
);
