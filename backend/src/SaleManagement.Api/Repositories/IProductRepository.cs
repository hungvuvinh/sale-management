using SaleManagement.Api.Entities;

namespace SaleManagement.Api.Repositories;

public interface IProductRepository
{
    Task<IEnumerable<ProductCategoryEntity>> GetActiveCategoriesAsync();
    Task<ProductCategoryEntity> CreateCategoryAsync(ProductCategoryEntity entity);
    Task<IEnumerable<ProductEntity>> GetProductsAsync(string? keyword, long? categoryId, int? stage, long? storeId);
    Task<IEnumerable<WarehouseInventoryEntity>> GetWarehouseInventoryAsync(long variantId, long? warehouseId);
    Task<ProductEntity?> GetProductByIdAsync(long id);
    Task<ProductEntity> CreateProductAsync(ProductEntity entity);
    Task<bool> UpdateProductAsync(long id, string? sku, string name, string slug, long? categoryId, string? description, string? materialsSummary, string? mainImageUrl, string? galleryImages, string? videoUrl, bool isActive);
    Task<bool> DeleteProductAsync(long id);
    Task<ProductVariantEntity> CreateVariantAsync(ProductVariantEntity entity);
    Task<bool> UpdateVariantAsync(long id, string sku, string name, string? barcode, string attributesJson, decimal weightKg, decimal cbm, int boxCount, string? dimensionsJson, bool isActive);
    Task<bool> DeleteVariantAsync(long id);
    Task UpdateStagePricesAsync(long lotId, decimal s1, decimal s2, decimal s3, decimal s4, decimal s5, decimal? vipPrice);
    Task<IEnumerable<FifoLotEntity>> GetActiveFifoLotsAsync(long? variantId, long? lotId);
    Task UpdateLotStageAsync(long lotId, short newStage);
    Task AddPriceAuditLogAsync(StagePriceAuditLogEntity auditLog);
    Task<IEnumerable<StagePriceAuditLogEntity>> GetPriceHistoryAsync(long lotId);
    Task<Dictionary<short, decimal>> GetStagePricesByLotIdAsync(long lotId);
    Task<ProductVariantEntity?> GetVariantByIdAsync(long variantId);
    
    // Store-specific Daily Prices
    Task<StoreVariantPriceEntity?> GetStoreVariantPriceAsync(long storeId, long variantId);
    Task<bool> UpdateStoreVariantDailyPriceAsync(long storeId, long variantId, decimal newPrice);
    Task<IEnumerable<StoreVariantPriceEntity>> GetStoreVariantPricesAsync(long? storeId, long? variantId);

    // Pricing Stage Configs (bởi Người Quản Lý)
    Task<PricingStageConfigEntity?> GetPricingStageConfigAsync(long variantId, short fromStage);
    Task<IEnumerable<PricingStageConfigEntity>> GetPricingStageConfigsByVariantIdAsync(long variantId);
    Task SavePricingStageConfigsAsync(long variantId, IEnumerable<PricingStageConfigEntity> configs);
}
