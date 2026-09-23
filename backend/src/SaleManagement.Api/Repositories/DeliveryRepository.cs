using Microsoft.EntityFrameworkCore;
using SaleManagement.Api.Data;
using SaleManagement.Api.Entities;

namespace SaleManagement.Api.Repositories;

public class DeliveryRepository : IDeliveryRepository
{
    private readonly AppDbContext _context;

    public DeliveryRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<DeliveryServiceRateEntity>> GetActiveServiceRatesAsync()
    {
        return await _context.DeliveryServiceRates
            .OrderBy(r => r.Id)
            .ToListAsync();
    }

    public async Task<DeliveryServiceRateEntity?> GetServiceRateByCodeAsync(string code)
    {
        return await _context.DeliveryServiceRates
            .FirstOrDefaultAsync(r => r.Code == code && r.IsActive);
    }

    public async Task<DeliveryServiceRateEntity?> GetServiceRateByIdAsync(long id)
    {
        return await _context.DeliveryServiceRates
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<bool> UpdateServiceRateAsync(long id, string name, decimal feeAud, bool isActive)
    {
        var rate = await _context.DeliveryServiceRates.FindAsync(id);
        if (rate == null) return false;

        rate.Name = name;
        rate.FeeAud = feeAud;
        rate.IsActive = isActive;
        rate.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<StoreShippingRateEntity?> GetStoreShippingRateAsync(long storeId, decimal distanceKm)
    {
        return await _context.StoreShippingRates
            .Where(r => r.StoreId == storeId && distanceKm >= r.MinDistanceKm && distanceKm <= r.MaxDistanceKm)
            .FirstOrDefaultAsync();
    }

    public async Task<DeliveryBookingEntity> CreateBookingAsync(DeliveryBookingEntity booking, List<DeliveryBookingServiceEntity> services)
    {
        _context.DeliveryBookings.Add(booking);
        await _context.SaveChangesAsync();

        foreach (var svc in services)
        {
            svc.BookingId = booking.Id;
            _context.DeliveryBookingServices.Add(svc);
        }

        await _context.SaveChangesAsync();

        return (await GetBookingByIdAsync(booking.Id))!;
    }

    public async Task<DeliveryBookingEntity?> GetBookingByIdAsync(long bookingId)
    {
        return await _context.DeliveryBookings
            .Include(b => b.DeliveryAddress)
            .Include(b => b.Services)
                .ThenInclude(s => s.ServiceRate)
            .FirstOrDefaultAsync(b => b.Id == bookingId);
    }

    public async Task<IEnumerable<DeliveryBookingEntity>> GetBookingsByDateRangeAsync(DateTime startDate, DateTime endDate, long? storeId)
    {
        var query = _context.DeliveryBookings
            .Include(b => b.DeliveryAddress)
            .Include(b => b.Services)
                .ThenInclude(s => s.ServiceRate)
            .Where(b => b.ScheduledDate.Date >= startDate.Date && b.ScheduledDate.Date <= endDate.Date);

        if (storeId.HasValue)
        {
            query = query.Where(b => b.Order.StoreId == storeId.Value);
        }

        return await query
            .OrderBy(b => b.ScheduledDate)
            .ThenBy(b => b.Id)
            .ToListAsync();
    }

    public async Task<bool> UpdateBookingStatusAsync(long bookingId, string newStatus)
    {
        var booking = await _context.DeliveryBookings.FindAsync(bookingId);
        if (booking == null) return false;

        booking.Status = newStatus;
        booking.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }
}
