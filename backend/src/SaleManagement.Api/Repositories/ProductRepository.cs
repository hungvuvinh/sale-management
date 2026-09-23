using Microsoft.EntityFrameworkCore;
using SaleManagement.Api.Data;
using SaleManagement.Api.Entities;

namespace SaleManagement.Api.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _context;

    public ProductRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ProductCategoryEntity>> GetActiveCategoriesAsync()
    {
        return await _context.ProductCategories
            .Where(c => c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<ProductCategoryEntity> CreateCategoryAsync(ProductCategoryEntity entity)
    {
        _context.ProductCategories.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<IEnumerable<ProductEntity>> GetProductsAsync(string? keyword, long? categoryId, int? stage, long? storeId)
    {
        var query = _context.Products
            .Include(p => p.Category)
            .Include(p => p.Variants)
            .Where(p => p.IsActive);

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            string kw = keyword.ToLower();
            query = query.Where(p => 
                p.Name.ToLower().Contains(kw) || 
                p.Sku.ToLower().Contains(kw) || 
                p.Variants.Any(v => v.Name.ToLower().Contains(kw) || v.Sku.ToLower().Contains(kw))
            );
        }

        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        return await query.OrderByDescending(p => p.Id).ToListAsync();
    }

    public async Task<IEnumerable<WarehouseInventoryEntity>> GetWarehouseInventoryAsync(long variantId, long? warehouseId)
    {
        var query = _context.WarehouseInventory
            .Where(inventory => inventory.VariantId == variantId);

        if (warehouseId.HasValue)
        {
            query = query.Where(inventory => inventory.WarehouseId == warehouseId.Value);
        }

        return await query.ToListAsync();
    }

    public async Task<ProductEntity?> GetProductByIdAsync(long id)
    {
        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Variants)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<ProductEntity> CreateProductAsync(ProductEntity entity)
    {
        _context.Products.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<bool> UpdateProductAsync(long id, string? sku, string name, string slug, long? categoryId, string? description, string? materialsSummary, string? mainImageUrl, string? galleryImages, string? videoUrl, bool isActive)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) return false;

        if (!string.IsNullOrWhiteSpace(sku)) product.Sku = sku;
        product.Name = name;
        product.Slug = slug;
        product.CategoryId = categoryId;
        if (description is not null) product.Description = description;
        if (materialsSummary is not null) product.MaterialsSummary = materialsSummary;
        if (mainImageUrl is not null) product.MainImageUrl = mainImageUrl;
        if (galleryImages is not null) product.GalleryImages = galleryImages;
        if (videoUrl is not null) product.VideoUrl = videoUrl;
        product.IsActive = isActive;
        product.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteProductAsync(long id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) return false;

        product.IsActive = false;
        product.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<ProductVariantEntity> CreateVariantAsync(ProductVariantEntity entity)
    {
        _context.ProductVariants.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<bool> UpdateVariantAsync(long id, string sku, string name, string? barcode, string attributesJson, decimal weightKg, decimal cbm, int boxCount, string? dimensionsJson, bool isActive)
    {
        var variant = await _context.ProductVariants.FindAsync(id);
        if (variant == null) return false;

        variant.Sku = sku;
        variant.Name = name;
        variant.Barcode = barcode;
        variant.AttributesJson = attributesJson;
        variant.WeightKg = weightKg;
        variant.Cbm = cbm;
        variant.BoxCount = boxCount;
        variant.DimensionsCm = dimensionsJson;
        variant.IsActive = isActive;
        variant.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteVariantAsync(long id)
    {
        var variant = await _context.ProductVariants.FindAsync(id);
        if (variant == null) return false;
        variant.IsActive = false;
        variant.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task UpdateStagePricesAsync(long lotId, decimal s1, decimal s2, decimal s3, decimal s4, decimal s5, decimal? vipPrice)
    {
        var existing = await _context.FifoLotStagePrices
            .Where(p => p.FifoLotId == lotId)
            .ToListAsync();

        var stages = new (short Stage, decimal Price)[] { (1, s1), (2, s2), (3, s3), (4, s4), (5, s5) };

        foreach (var (stg, prc) in stages)
        {
            var item = existing.FirstOrDefault(x => x.Stage == stg);
            if (item != null)
            {
                item.PriceAud = prc;
                item.VipPriceAud = vipPrice;
                item.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                _context.FifoLotStagePrices.Add(new FifoLotStagePriceEntity
                {
                    FifoLotId = lotId,
                    Stage = stg,
                    PriceAud = prc,
                    VipPriceAud = vipPrice,
                    UpdatedAt = DateTime.UtcNow
                });
            }
        }

        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<FifoLotEntity>> GetActiveFifoLotsAsync(long? variantId, long? lotId)
    {
        var query = _context.FifoLots
            .Include(f => f.Variant)
            .Where(f => f.Status == "ACTIVE");

        if (lotId.HasValue)
        {
            query = query.Where(f => f.Id == lotId.Value);
        }
        else if (variantId.HasValue)
        {
            query = query.Where(f => f.VariantId == variantId.Value);
        }

        return await query.ToListAsync();
    }

    public async Task UpdateLotStageAsync(long lotId, short newStage)
    {
        var lot = await _context.FifoLots.FindAsync(lotId);
        if (lot != null)
        {
            lot.CurrentStage = newStage;
            await _context.SaveChangesAsync();
        }
    }

    public async Task AddPriceAuditLogAsync(StagePriceAuditLogEntity auditLog)
    {
        _context.StagePriceAuditLogs.Add(auditLog);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<StagePriceAuditLogEntity>> GetPriceHistoryAsync(long lotId)
    {
        return await _context.StagePriceAuditLogs
            .Where(l => l.FifoLotId == lotId)
            .OrderByDescending(l => l.ChangedAt)
            .ToListAsync();
    }

    public async Task<Dictionary<short, decimal>> GetStagePricesByLotIdAsync(long lotId)
    {
        return await _context.FifoLotStagePrices
            .Where(p => p.FifoLotId == lotId)
            .ToDictionaryAsync(p => p.Stage, p => p.PriceAud);
    }

    public async Task<PricingStageConfigEntity?> GetPricingStageConfigAsync(long variantId, short fromStage)
    {
        return await _context.PricingStageConfigs
            .FirstOrDefaultAsync(c => c.VariantId == variantId && c.FromStage == fromStage);
    }

    public async Task<IEnumerable<PricingStageConfigEntity>> GetPricingStageConfigsByVariantIdAsync(long variantId)
    {
        return await _context.PricingStageConfigs
            .Where(c => c.VariantId == variantId)
            .OrderBy(c => c.FromStage)
            .ToListAsync();
    }

    public async Task SavePricingStageConfigsAsync(long variantId, IEnumerable<PricingStageConfigEntity> configs)
    {
        var existing = await _context.PricingStageConfigs
            .Where(c => c.VariantId == variantId)
            .ToListAsync();

        _context.PricingStageConfigs.RemoveRange(existing);
        _context.PricingStageConfigs.AddRange(configs);
        await _context.SaveChangesAsync();
    }

    public async Task<ProductVariantEntity?> GetVariantByIdAsync(long variantId)
    {
        return await _context.ProductVariants.FirstOrDefaultAsync(v => v.Id == variantId);
    }

    public async Task<StoreVariantPriceEntity?> GetStoreVariantPriceAsync(long storeId, long variantId)
    {
        return await _context.StoreVariantPrices
            .FirstOrDefaultAsync(p => p.StoreId == storeId && p.VariantId == variantId);
    }

    public async Task<bool> UpdateStoreVariantDailyPriceAsync(long storeId, long variantId, decimal newPrice)
    {
        var record = await _context.StoreVariantPrices
            .FirstOrDefaultAsync(p => p.StoreId == storeId && p.VariantId == variantId);

        if (record == null)
        {
            record = new StoreVariantPriceEntity
            {
                StoreId = storeId,
                VariantId = variantId,
                CurrentDailyPrice = newPrice,
                UpdatedAt = DateTime.UtcNow
            };
            _context.StoreVariantPrices.Add(record);
        }
        else
        {
            record.CurrentDailyPrice = newPrice;
            record.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<StoreVariantPriceEntity>> GetStoreVariantPricesAsync(long? storeId, long? variantId)
    {
        var query = _context.StoreVariantPrices.AsQueryable();

        if (storeId.HasValue)
            query = query.Where(p => p.StoreId == storeId.Value);

        if (variantId.HasValue)
            query = query.Where(p => p.VariantId == variantId.Value);

        return await query.ToListAsync();
    }
}

