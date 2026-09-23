namespace SaleManagement.Api.Services;

public static class CacheKeyFactory
{
    public static string Price(long storeId, long variantId, long lotId) => $"price:{storeId}:{variantId}:{lotId}";

    public static string Stock(long warehouseId, long variantId) => $"stock:{warehouseId}:{variantId}";

    public static string CustomerLoyalty(long customerId) => $"customer:loyalty:{customerId}";

    public static string LotActive(long warehouseId, long variantId) => $"lot:active:{warehouseId}:{variantId}";

    public static string StoreInfo(long storeId) => $"store:info:{storeId}";
}
