using SaleManagement.Api.Entities;
using SaleManagement.Api.Models;
using SaleManagement.Api.Repositories;

namespace SaleManagement.Api.Services;

public class DeliveryService
{
    private readonly IDeliveryRepository _deliveryRepository;

    public DeliveryService(IDeliveryRepository deliveryRepository)
    {
        _deliveryRepository = deliveryRepository;
    }

    public async Task<IEnumerable<DeliveryServiceRateDto>> GetActiveServiceRatesAsync()
    {
        var rates = await _deliveryRepository.GetActiveServiceRatesAsync();
        return rates.Select(r => new DeliveryServiceRateDto(
            Id: r.Id,
            Code: r.Code,
            Name: r.Name,
            FeeType: r.FeeType,
            FeeAud: r.FeeAud,
            IsActive: r.IsActive
        ));
    }

    public async Task<bool> UpdateServiceRateAsync(long id, UpdateDeliveryServiceRateDto dto)
    {
        return await _deliveryRepository.UpdateServiceRateAsync(id, dto.Name, dto.FeeAud, dto.IsActive);
    }

    public async Task<ShippingFeeResultDto> CalculateShippingFeeAsync(CalculateShippingFeeDto dto)
    {
        long targetStoreId = dto.StoreId ?? 1; // Default to Store 1 Perth if omitted

        // 1. Calculate Base Shipping Fee by Distance
        var storeRate = await _deliveryRepository.GetStoreShippingRateAsync(targetStoreId, dto.DistanceKm);
        decimal baseShippingFee = storeRate?.ShippingFeeAud ?? (dto.DistanceKm switch
        {
            <= 20 => 45.00m,
            <= 50 => 75.00m,
            <= 100 => 120.00m,
            _ => 180.00m
        });

        decimal upstairsSurcharge = 0;
        decimal assemblySurcharge = 0;
        var surcharges = new List<ShippingSurchargeBreakdownDto>();

        // 2. Upstairs Carry Surcharge (Dynamic DB rate query)
        if (dto.IsUpstairs && dto.StairsFloorCount > 0)
        {
            var upstairsRate = await _deliveryRepository.GetServiceRateByCodeAsync("UPSTAIRS_CARRY");
            if (upstairsRate != null && upstairsRate.IsActive)
            {
                decimal unitFee = upstairsRate.FeeAud;
                int qty = dto.StairsFloorCount;
                upstairsSurcharge = Math.Round(unitFee * qty, 2);

                surcharges.Add(new ShippingSurchargeBreakdownDto(
                    ServiceRateId: upstairsRate.Id,
                    ServiceCode: upstairsRate.Code,
                    ServiceName: upstairsRate.Name,
                    Quantity: qty,
                    UnitFeeAud: unitFee,
                    LineSurchargeAud: upstairsSurcharge
                ));
            }
        }

        // 3. Furniture Assembly Surcharge (Dynamic DB rate query)
        if (dto.IsAssembling)
        {
            var assemblyRate = await _deliveryRepository.GetServiceRateByCodeAsync("FURNITURE_ASSEMBLY");
            if (assemblyRate != null && assemblyRate.IsActive)
            {
                decimal unitFee = assemblyRate.FeeAud;
                int qty = 1;
                assemblySurcharge = Math.Round(unitFee * qty, 2);

                surcharges.Add(new ShippingSurchargeBreakdownDto(
                    ServiceRateId: assemblyRate.Id,
                    ServiceCode: assemblyRate.Code,
                    ServiceName: assemblyRate.Name,
                    Quantity: qty,
                    UnitFeeAud: unitFee,
                    LineSurchargeAud: assemblySurcharge
                ));
            }
        }

        decimal totalShippingFee = baseShippingFee + upstairsSurcharge + assemblySurcharge;

        return new ShippingFeeResultDto(
            DistanceKm: dto.DistanceKm,
            BaseShippingFee: baseShippingFee,
            UpstairsSurcharge: upstairsSurcharge,
            AssemblySurcharge: assemblySurcharge,
            TotalShippingFee: totalShippingFee,
            Surcharges: surcharges
        );
    }

    public async Task<DeliveryBookingDto> CreateBookingAsync(CreateDeliveryBookingDto dto)
    {
        var servicesEntities = new List<DeliveryBookingServiceEntity>();
        decimal totalSurcharge = 0;

        // 1. Calculate Upstairs Surcharge if requested
        if (dto.IsUpstairs && dto.StairsFloorCount > 0)
        {
            var upstairsRate = await _deliveryRepository.GetServiceRateByCodeAsync("UPSTAIRS_CARRY");
            if (upstairsRate != null)
            {
                decimal unitFee = upstairsRate.FeeAud;
                int qty = dto.StairsFloorCount;
                decimal lineTotal = Math.Round(unitFee * qty, 2);
                totalSurcharge += lineTotal;

                servicesEntities.Add(new DeliveryBookingServiceEntity
                {
                    ServiceRateId = upstairsRate.Id,
                    Quantity = qty,
                    UnitFeeAud = unitFee,
                    LineSurchargeAud = lineTotal
                });
            }
        }

        // 2. Calculate Assembly Surcharge if requested
        if (dto.IsAssembling)
        {
            var assemblyRate = await _deliveryRepository.GetServiceRateByCodeAsync("FURNITURE_ASSEMBLY");
            if (assemblyRate != null)
            {
                decimal unitFee = assemblyRate.FeeAud;
                int qty = 1;
                decimal lineTotal = Math.Round(unitFee * qty, 2);
                totalSurcharge += lineTotal;

                servicesEntities.Add(new DeliveryBookingServiceEntity
                {
                    ServiceRateId = assemblyRate.Id,
                    Quantity = qty,
                    UnitFeeAud = unitFee,
                    LineSurchargeAud = lineTotal
                });
            }
        }

        var booking = new DeliveryBookingEntity
        {
            OrderId = dto.OrderId,
            DeliveryAddressId = dto.DeliveryAddressId,
            ScheduledDate = dto.ScheduledDate.Date,
            TimeSlot = dto.TimeSlot ?? "FLEXIBLE",
            IsAssembling = dto.IsAssembling,
            IsUpstairs = dto.IsUpstairs,
            StairsFloorCount = dto.StairsFloorCount,
            SpecialNotes = dto.SpecialNotes,
            SurchargeAud = totalSurcharge,
            Status = "BOOKED"
        };

        var created = await _deliveryRepository.CreateBookingAsync(booking, servicesEntities);
        return MapToDto(created);
    }

    public async Task<DeliveryBookingDto?> GetBookingByIdAsync(long bookingId)
    {
        var booking = await _deliveryRepository.GetBookingByIdAsync(bookingId);
        return booking == null ? null : MapToDto(booking);
    }

    public async Task<IEnumerable<DeliveryCalendarDayDto>> GetDeliveryCalendarAsync(DateTime startDate, DateTime endDate, long? storeId)
    {
        var bookings = await _deliveryRepository.GetBookingsByDateRangeAsync(startDate, endDate, storeId);
        var groupedByDate = bookings.GroupBy(b => b.ScheduledDate.Date);

        var calendarDays = new List<DeliveryCalendarDayDto>();
        for (var dt = startDate.Date; dt <= endDate.Date; dt = dt.AddDays(1))
        {
            var dayBookings = groupedByDate.FirstOrDefault(g => g.Key == dt)?.ToList() ?? new List<DeliveryBookingEntity>();

            calendarDays.Add(new DeliveryCalendarDayDto(
                Date: dt,
                TotalBookings: dayBookings.Count,
                MorningCount: dayBookings.Count(b => b.TimeSlot == "MORNING"),
                AfternoonCount: dayBookings.Count(b => b.TimeSlot == "AFTERNOON"),
                EveningCount: dayBookings.Count(b => b.TimeSlot == "EVENING"),
                FlexibleCount: dayBookings.Count(b => b.TimeSlot == "FLEXIBLE"),
                Bookings: dayBookings.Select(MapToDto).ToList()
            ));
        }

        return calendarDays;
    }

    public async Task<bool> UpdateBookingStatusAsync(long bookingId, string newStatus)
    {
        return await _deliveryRepository.UpdateBookingStatusAsync(bookingId, newStatus);
    }

    private static DeliveryBookingDto MapToDto(DeliveryBookingEntity b)
    {
        string addrText = b.DeliveryAddress != null
            ? $"{b.DeliveryAddress.StreetAddress}, {b.DeliveryAddress.Suburb}, {b.DeliveryAddress.City} {b.DeliveryAddress.State}"
            : string.Empty;

        return new DeliveryBookingDto(
            Id: b.Id,
            OrderId: b.OrderId,
            DeliveryAddressId: b.DeliveryAddressId,
            AddressText: addrText,
            ScheduledDate: b.ScheduledDate,
            TimeSlot: b.TimeSlot,
            IsAssembling: b.IsAssembling,
            IsUpstairs: b.IsUpstairs,
            StairsFloorCount: b.StairsFloorCount,
            SpecialNotes: b.SpecialNotes,
            SurchargeAud: b.SurchargeAud,
            Status: b.Status,
            CreatedAt: b.CreatedAt,
            Services: b.Services.Select(s => new DeliveryBookingServiceDto(
                Id: s.Id,
                ServiceRateId: s.ServiceRateId,
                ServiceCode: s.ServiceRate?.Code ?? string.Empty,
                ServiceName: s.ServiceRate?.Name ?? string.Empty,
                Quantity: s.Quantity,
                UnitFeeAud: s.UnitFeeAud,
                LineSurchargeAud: s.LineSurchargeAud
            )).ToList()
        );
    }
}
