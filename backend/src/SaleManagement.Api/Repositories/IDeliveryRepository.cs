using SaleManagement.Api.Entities;

namespace SaleManagement.Api.Repositories;

public interface IDeliveryRepository
{
    Task<IEnumerable<DeliveryServiceRateEntity>> GetActiveServiceRatesAsync();
    Task<DeliveryServiceRateEntity?> GetServiceRateByCodeAsync(string code);
    Task<DeliveryServiceRateEntity?> GetServiceRateByIdAsync(long id);
    Task<bool> UpdateServiceRateAsync(long id, string name, decimal feeAud, bool isActive);

    Task<StoreShippingRateEntity?> GetStoreShippingRateAsync(long storeId, decimal distanceKm);

    Task<DeliveryBookingEntity> CreateBookingAsync(DeliveryBookingEntity booking, List<DeliveryBookingServiceEntity> services);
    Task<DeliveryBookingEntity?> GetBookingByIdAsync(long bookingId);
    Task<IEnumerable<DeliveryBookingEntity>> GetBookingsByDateRangeAsync(DateTime startDate, DateTime endDate, long? storeId);
    Task<bool> UpdateBookingStatusAsync(long bookingId, string newStatus);
}
