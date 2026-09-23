using SaleManagement.Api.Entities;

namespace SaleManagement.Api.Repositories;

public interface IStoreRepository
{
    Task<IEnumerable<StoreEntity>> GetActiveStoresAsync();
    Task<IEnumerable<WarehouseEntity>> GetActiveWarehousesAsync();
    Task<PostcodeEntity?> GetPostcodeByCodeAsync(string postcode);
    Task<StoreEntity?> GetAssignedStoreByPostcodeIdAsync(long postcodeId);
    Task<IReadOnlyList<StoreEntity>> GetActiveStoresByPostcodeIdAsync(long postcodeId);
    Task<StoreEntity?> GetActiveStoreByIdAsync(long storeId);
    Task<bool> IsStoreAssignedToPostcodeAsync(long postcodeId, long storeId);
    Task<StoreShippingRateEntity?> GetShippingRateByStoreIdAsync(long storeId);
    Task AssignPostcodeToStoreAsync(long postcodeId, long storeId);
}
