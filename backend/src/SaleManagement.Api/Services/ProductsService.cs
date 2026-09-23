using System.Text.Json.Nodes;
using SaleManagement.Api.Entities;
using SaleManagement.Api.Models;
using SaleManagement.Api.Repositories;

namespace SaleManagement.Api.Services;

public class ProductsService
{
    private readonly IProductRepository _productRepository;

    public ProductsService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<IEnumerable<CategoryDto>> GetCategoriesAsync()
    {
        var list = await _productRepository.GetActiveCategoriesAsync();
        return list.Select(c => new CategoryDto(
            Id: c.Id,
            ParentId: c.ParentId,
            Name: c.Name,
            Slug: c.Slug,
            Description: c.Description,
            DisplayOrder: c.DisplayOrder,
            IsActive: c.IsActive
        ));
    }

    public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto dto)
    {
        var entity = new ProductCategoryEntity
        {
            ParentId = dto.ParentId,
            Name = dto.Name,
            Slug = dto.Slug,
            Description = dto.Description,
            DisplayOrder = dto.DisplayOrder,
            IsActive = true
        };

        var created = await _productRepository.CreateCategoryAsync(entity);
        return new CategoryDto(
            Id: created.Id,
            ParentId: created.ParentId,
            Name: created.Name,
            Slug: created.Slug,
            Description: created.Description,
            DisplayOrder: created.DisplayOrder,
            IsActive: created.IsActive
        );
    }

    public async Task<IEnumerable<ProductListDto>> GetProductsAsync(string? keyword, long? categoryId, int? stage, long? storeId, long? warehouseId)
    {
        var products = await _productRepository.GetProductsAsync(keyword, categoryId, stage, storeId);
        var resultList = new List<ProductListDto>();

        foreach (var p in products)
        {
            var variantsList = new List<VariantSummaryDto>();

            foreach (var v in p.Variants.Where(x => x.IsActive))
            {
                JsonNode attrNode = JsonNode.Parse(v.AttributesJson ?? "{}") ?? new JsonObject();
                
                var activeLots = await _productRepository.GetActiveFifoLotsAsync(v.Id, null);
                if (warehouseId.HasValue)
                {
                    activeLots = activeLots.Where(lot => lot.WarehouseId == warehouseId.Value);
                }
                var activeLot = activeLots.FirstOrDefault();

                int currentStage = activeLot != null ? activeLot.CurrentStage : 1;
                var inventory = await _productRepository.GetWarehouseInventoryAsync(v.Id, warehouseId);
                int onHand = inventory.Sum(item => item.OnHandQuantity);
                int reserved = inventory.Sum(item => item.ReservedQuantity);
                int remainingQty = inventory.Any() ? onHand : activeLot?.RemainingQuantity ?? 0;
                var stagePrices = activeLot != null ? await _productRepository.GetStagePricesByLotIdAsync(activeLot.Id) : new Dictionary<short, decimal>();

                var stagesDict = stagePrices.ToDictionary(k => $"stage{k.Key}", v => v.Value);
                decimal currentPrice = storeId.HasValue
                    ? stagePrices.GetValueOrDefault((short)currentStage, 0m)
                    : 0m;
                var visibleStagePrices = storeId.HasValue ? stagesDict : new Dictionary<string, decimal>();

                variantsList.Add(new VariantSummaryDto(
                    VariantId: v.Id,
                    Sku: v.Sku,
                    Name: v.Name,
                    Barcode: v.Barcode,
                    Attributes: attrNode,
                    WeightKg: v.WeightKg,
                    Cbm: v.Cbm,
                    BoxCount: v.BoxCount,
                    CurrentPriceAud: currentPrice,
                    VipPriceAud: null,
                    StagePrices: visibleStagePrices,
                    Stock: new StockSummaryDto(OnHand: remainingQty, Reserved: inventory.Any() ? reserved : 0, Available: inventory.Any() ? remainingQty - reserved : remainingQty)
                ));
            }

            resultList.Add(new ProductListDto(
                Id: p.Id,
                Sku: p.Sku,
                Name: p.Name,
                Slug: p.Slug,
                CategoryId: p.CategoryId,
                CategoryName: p.Category?.Name,
                MainImageUrl: p.MainImageUrl,
                IsActive: p.IsActive,
                CurrentStage: 1,
                Variants: variantsList
            ));
        }

        return resultList;
    }

    public async Task<ProductDetailDto?> GetProductByIdAsync(long id, long? storeId = null)
    {
        var p = await _productRepository.GetProductByIdAsync(id);
        if (p == null) return null;

        JsonNode galleryNode = JsonNode.Parse(p.GalleryImages ?? "[]") ?? new JsonArray();
        var variantsList = new List<VariantDetailDto>();

        foreach (var v in p.Variants)
        {
            JsonNode attrNode = JsonNode.Parse(v.AttributesJson ?? "{}") ?? new JsonObject();
            JsonNode? dimNode = v.DimensionsCm != null ? JsonNode.Parse(v.DimensionsCm) : null;

            var activeLots = await _productRepository.GetActiveFifoLotsAsync(v.Id, null);
            var activeLot = activeLots.FirstOrDefault();

            int currentStage = activeLot != null ? activeLot.CurrentStage : 1;
            int remainingQty = activeLot != null ? activeLot.RemainingQuantity : 0;
            var stagePrices = activeLot != null ? await _productRepository.GetStagePricesByLotIdAsync(activeLot.Id) : new Dictionary<short, decimal>();

            var stagesDict = stagePrices.ToDictionary(k => $"stage{k.Key}", v => v.Value);
            decimal currentPrice = storeId.HasValue
                ? stagePrices.GetValueOrDefault((short)currentStage, 0m)
                : 0m;
            var visibleStagePrices = storeId.HasValue ? stagesDict : new Dictionary<string, decimal>();

            variantsList.Add(new VariantDetailDto(
                Id: v.Id,
                ProductId: v.ProductId,
                Sku: v.Sku,
                Name: v.Name,
                Barcode: v.Barcode,
                AttributesJson: attrNode,
                WeightKg: v.WeightKg,
                Cbm: v.Cbm,
                BoxCount: v.BoxCount,
                DimensionsCm: dimNode,
                IsActive: v.IsActive,
                Stock: new StockSummaryDto(OnHand: remainingQty, Reserved: 0, Available: remainingQty),
                CurrentStage: currentStage,
                CurrentPriceAud: currentPrice,
                VipPriceAud: null,
                StagePrices: visibleStagePrices
            ));
        }

        return new ProductDetailDto(
            Id: p.Id,
            Sku: p.Sku,
            Name: p.Name,
            Slug: p.Slug,
            CategoryId: p.CategoryId,
            CategoryName: p.Category?.Name,
            Description: p.Description,
            MaterialsSummary: p.MaterialsSummary,
            MainImageUrl: p.MainImageUrl,
            GalleryImages: galleryNode,
            VideoUrl: p.VideoUrl,
            IsActive: p.IsActive,
            CreatedAt: p.CreatedAt,
            Variants: variantsList
        );
    }

    public async Task<ProductDetailDto> CreateProductAsync(CreateProductDto dto)
    {
        string galleryJson = dto.GalleryImages?.ToJsonString() ?? "[]";

        var entity = new ProductEntity
        {
            Sku = dto.Sku,
            Name = dto.Name,
            Slug = dto.Slug,
            CategoryId = dto.CategoryId,
            Description = dto.Description,
            MaterialsSummary = dto.MaterialsSummary,
            MainImageUrl = dto.MainImageUrl,
            GalleryImages = galleryJson,
            VideoUrl = dto.VideoUrl,
            IsActive = true
        };

        var created = await _productRepository.CreateProductAsync(entity);
        return (await GetProductByIdAsync(created.Id))!;
    }

    public async Task<bool> UpdateProductAsync(long id, UpdateProductDto dto)
    {
        string? galleryJson = dto.GalleryImages?.ToJsonString();

        return await _productRepository.UpdateProductAsync(
            id, dto.Sku, dto.Name, dto.Slug, dto.CategoryId, dto.Description,
            dto.MaterialsSummary, dto.MainImageUrl, galleryJson, dto.VideoUrl, dto.IsActive
        );
    }

    public async Task<bool> DeleteProductAsync(long id)
    {
        return await _productRepository.DeleteProductAsync(id);
    }

    public async Task<VariantDetailDto> CreateVariantAsync(long productId, CreateVariantDto dto)
    {
        string attrJson = dto.AttributesJson.ToJsonString();
        string? dimJson = dto.DimensionsCm?.ToJsonString();

        var entity = new ProductVariantEntity
        {
            ProductId = productId,
            Sku = dto.Sku,
            Name = dto.Name,
            Barcode = dto.Barcode,
            AttributesJson = attrJson,
            WeightKg = dto.WeightKg,
            Cbm = dto.Cbm,
            BoxCount = dto.BoxCount,
            DimensionsCm = dimJson,
            IsActive = true
        };

        var created = await _productRepository.CreateVariantAsync(entity);

        return new VariantDetailDto(
            Id: created.Id,
            ProductId: productId,
            Sku: created.Sku,
            Name: created.Name,
            Barcode: created.Barcode,
            AttributesJson: dto.AttributesJson,
            WeightKg: created.WeightKg,
            Cbm: created.Cbm,
            BoxCount: created.BoxCount,
            DimensionsCm: dto.DimensionsCm,
            IsActive: true,
            Stock: new StockSummaryDto(0, 0, 0),
            CurrentStage: 1,
            CurrentPriceAud: 0.0m,
            VipPriceAud: null,
            StagePrices: new Dictionary<string, decimal>()
        );
    }

    public async Task<bool> UpdateVariantAsync(long id, UpdateVariantDto dto)
    {
        return await _productRepository.UpdateVariantAsync(
            id,
            dto.Sku,
            dto.Name,
            dto.Barcode,
            dto.AttributesJson.ToJsonString(),
            dto.WeightKg,
            dto.Cbm,
            dto.BoxCount,
            dto.DimensionsCm?.ToJsonString(),
            dto.IsActive);
    }

    public async Task<bool> DeleteVariantAsync(long id)
    {
        return await _productRepository.DeleteVariantAsync(id);
    }

    public async Task<bool> UpdateStagePricesAsync(UpdateStagePricesDto dto)
    {
        await _productRepository.UpdateStagePricesAsync(
            dto.FifoLotId, dto.Stage1Price, dto.Stage2Price, dto.Stage3Price, dto.Stage4Price, dto.Stage5Price, dto.VipPrice
        );
        return true;
    }

    public async Task<IEnumerable<PriceAuditLogDto>> GetPriceHistoryAsync(long variantId, long lotId)
    {
        var logs = await _productRepository.GetPriceHistoryAsync(lotId);
        return logs.Select(l => new PriceAuditLogDto(
            Id: l.Id,
            FifoLotId: l.FifoLotId,
            OldStage: l.OldStage,
            NewStage: l.NewStage,
            OldPriceAud: l.OldPriceAud,
            NewPriceAud: l.NewPriceAud,
            TriggeredBy: l.TriggeredBy,
            TriggeredByUser: l.TriggeredByUser,
            RemainingStockQty: l.RemainingStockQty,
            RemainingStockPct: l.RemainingStockPct,
            DaysAtOldStage: l.DaysAtOldStage,
            Reason: l.Reason,
            ChangedAt: l.ChangedAt
        ));
    }

    public async Task<IEnumerable<PricingStageConfigDto>> GetPricingStageConfigsAsync(long variantId)
    {
        var configs = await _productRepository.GetPricingStageConfigsByVariantIdAsync(variantId);
        return configs.Select(c => new PricingStageConfigDto(
            Id: c.Id,
            VariantId: c.VariantId,
            FromStage: c.FromStage,
            ToStage: c.ToStage,
            TargetStockPct: c.TargetStockPct,
            MinDaysAtStage: c.MinDaysAtStage,
            MaxDaysAtStage: c.MaxDaysAtStage
        ));
    }

    public async Task SavePricingStageConfigsAsync(long variantId, SavePricingStageConfigsDto dto)
    {
        var entities = dto.Configs.Select(c => new PricingStageConfigEntity
        {
            VariantId = variantId,
            FromStage = c.FromStage,
            ToStage = c.ToStage,
            TargetStockPct = c.TargetStockPct,
            MinDaysAtStage = c.MinDaysAtStage,
            MaxDaysAtStage = c.MaxDaysAtStage
        });

        await _productRepository.SavePricingStageConfigsAsync(variantId, entities);
    }

    public async Task<bool> UpdateStoreVariantDailyPriceAsync(long storeId, long variantId, decimal newPrice)
    {
        return await _productRepository.UpdateStoreVariantDailyPriceAsync(storeId, variantId, newPrice);
    }

    public async Task<IEnumerable<StoreVariantPriceDto>> GetStoreVariantPricesAsync(long? storeId, long? variantId)
    {
        var list = await _productRepository.GetStoreVariantPricesAsync(storeId, variantId);
        return list.Select(p => new StoreVariantPriceDto(
            Id: p.Id,
            StoreId: p.StoreId,
            VariantId: p.VariantId,
            CurrentDailyPrice: p.CurrentDailyPrice,
            UpdatedAt: p.UpdatedAt
        ));
    }
}

