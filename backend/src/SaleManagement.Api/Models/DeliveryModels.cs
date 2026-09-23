namespace SaleManagement.Api.Models;

public record DeliveryServiceRateDto(
    long Id,
    string Code,
    string Name,
    string FeeType,
    decimal FeeAud,
    bool IsActive
);

public record UpdateDeliveryServiceRateDto(
    string Name,
    decimal FeeAud,
    bool IsActive
);

public record CalculateShippingFeeDto(
    string Postcode,
    decimal DistanceKm,
    bool IsUpstairs,
    int StairsFloorCount,
    bool IsAssembling,
    long? StoreId
);

public record ShippingFeeResultDto(
    decimal DistanceKm,
    decimal BaseShippingFee,
    decimal UpstairsSurcharge,
    decimal AssemblySurcharge,
    decimal TotalShippingFee,
    List<ShippingSurchargeBreakdownDto> Surcharges
);

public record ShippingSurchargeBreakdownDto(
    long ServiceRateId,
    string ServiceCode,
    string ServiceName,
    int Quantity,
    decimal UnitFeeAud,
    decimal LineSurchargeAud
);

public record CreateDeliveryBookingDto(
    long OrderId,
    long DeliveryAddressId,
    DateTime ScheduledDate,
    string TimeSlot, // MORNING, AFTERNOON, EVENING, FLEXIBLE
    bool IsAssembling,
    bool IsUpstairs,
    int StairsFloorCount,
    string? SpecialNotes
);

public record DeliveryBookingServiceDto(
    long Id,
    long ServiceRateId,
    string ServiceCode,
    string ServiceName,
    int Quantity,
    decimal UnitFeeAud,
    decimal LineSurchargeAud
);

public record DeliveryBookingDto(
    long Id,
    long OrderId,
    long DeliveryAddressId,
    string AddressText,
    DateTime ScheduledDate,
    string TimeSlot,
    bool IsAssembling,
    bool IsUpstairs,
    int StairsFloorCount,
    string? SpecialNotes,
    decimal SurchargeAud,
    string Status,
    DateTime CreatedAt,
    List<DeliveryBookingServiceDto> Services
);

public record DeliveryCalendarDayDto(
    DateTime Date,
    int TotalBookings,
    int MorningCount,
    int AfternoonCount,
    int EveningCount,
    int FlexibleCount,
    List<DeliveryBookingDto> Bookings
);
