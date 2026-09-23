using SaleManagement.Api.Data;
using SaleManagement.Api.Entities;
using SaleManagement.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace SaleManagement.Api.Services;

/// <summary>
/// Marketing Service — manages Promotions, Product Combos (cross-sell),
/// and Ad Spend tracking.
/// </summary>
public class MarketingService
{
    private readonly AppDbContext _db;

    public MarketingService(AppDbContext db)
    {
        _db = db;
    }

    // ─────────────────────────────────────────────
    //  PROMOTIONS
    // ─────────────────────────────────────────────

    public async Task<IEnumerable<PromotionDto>> GetPromotionsAsync(bool? activeOnly)
    {
        var query = _db.Promotions
            .Include(p => p.PromotionProducts)
            .AsQueryable();

        if (activeOnly == true)
            query = query.Where(p => p.IsActive && p.EndDate >= DateTime.UtcNow);

        var promotions = await query.OrderByDescending(p => p.StartDate).ToListAsync();
        return promotions.Select(MapPromotion);
    }

    public async Task<PromotionDto?> GetPromotionByIdAsync(long id)
    {
        var p = await _db.Promotions
            .Include(x => x.PromotionProducts)
            .FirstOrDefaultAsync(x => x.Id == id);
        return p != null ? MapPromotion(p) : null;
    }

    public async Task<PromotionDto> CreatePromotionAsync(CreatePromotionDto dto)
    {
        if (dto.StartDate > dto.EndDate)
            throw new ArgumentException("Ngày bắt đầu phải trước ngày kết thúc.");

        var entity = new PromotionEntity
        {
            Name = dto.Name,
            BadgeLabel = dto.BadgeLabel,
            DiscountType = dto.DiscountType,
            DiscountValue = dto.DiscountValue,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            IsActive = dto.IsActive
        };

        _db.Promotions.Add(entity);
        await _db.SaveChangesAsync();

        // Link products
        if (dto.ProductIds?.Count > 0)
        {
            foreach (var productId in dto.ProductIds.Distinct())
            {
                _db.PromotionProducts.Add(new PromotionProductEntity
                {
                    PromotionId = entity.Id,
                    ProductId = productId
                });
            }
            await _db.SaveChangesAsync();
        }

        await _db.Entry(entity).Collection(e => e.PromotionProducts).LoadAsync();
        return MapPromotion(entity);
    }

    public async Task<bool> UpdatePromotionAsync(long id, UpdatePromotionDto dto)
    {
        var promotion = await _db.Promotions.FindAsync(id);
        if (promotion == null) return false;

        if (dto.Name != null) promotion.Name = dto.Name;
        if (dto.BadgeLabel != null) promotion.BadgeLabel = dto.BadgeLabel;
        if (dto.DiscountValue.HasValue) promotion.DiscountValue = dto.DiscountValue.Value;
        if (dto.StartDate.HasValue) promotion.StartDate = dto.StartDate.Value;
        if (dto.EndDate.HasValue) promotion.EndDate = dto.EndDate.Value;
        if (dto.IsActive.HasValue) promotion.IsActive = dto.IsActive.Value;

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeletePromotionAsync(long id)
    {
        var promotion = await _db.Promotions.FindAsync(id);
        if (promotion == null) return false;

        _db.Promotions.Remove(promotion);
        await _db.SaveChangesAsync();
        return true;
    }

    // ─────────────────────────────────────────────
    //  PRODUCT COMBOS
    // ─────────────────────────────────────────────

    public async Task<IEnumerable<ProductComboDto>> GetCombosAsync(bool? activeOnly)
    {
        var query = _db.ProductCombos
            .Include(c => c.ComboItems)
                .ThenInclude(ci => ci.Variant)
            .AsQueryable();

        if (activeOnly == true)
            query = query.Where(c => c.IsActive);

        var combos = await query.OrderBy(c => c.Name).ToListAsync();
        return combos.Select(MapCombo);
    }

    public async Task<ProductComboDto?> GetComboByIdAsync(long id)
    {
        var combo = await _db.ProductCombos
            .Include(c => c.ComboItems)
                .ThenInclude(ci => ci.Variant)
            .FirstOrDefaultAsync(c => c.Id == id);
        return combo != null ? MapCombo(combo) : null;
    }

    public async Task<ProductComboDto> CreateComboAsync(CreateProductComboDto dto)
    {
        // Check code uniqueness
        var existing = await _db.ProductCombos.FirstOrDefaultAsync(c => c.Code == dto.Code);
        if (existing != null)
            throw new InvalidOperationException($"Mã Combo '{dto.Code}' đã tồn tại.");

        var entity = new ProductComboEntity
        {
            Name = dto.Name,
            Code = dto.Code,
            Description = dto.Description,
            ComboPriceAud = dto.ComboPriceAud,
            IsActive = true
        };

        _db.ProductCombos.Add(entity);
        await _db.SaveChangesAsync();

        // Add combo items
        if (dto.Items?.Count > 0)
        {
            foreach (var item in dto.Items)
            {
                _db.ComboItems.Add(new ComboItemEntity
                {
                    ComboId = entity.Id,
                    VariantId = item.VariantId,
                    Quantity = item.Quantity > 0 ? item.Quantity : 1
                });
            }
            await _db.SaveChangesAsync();
        }

        // Reload
        await _db.Entry(entity).Collection(e => e.ComboItems).Query()
            .Include(ci => ci.Variant)
            .LoadAsync();

        return MapCombo(entity);
    }

    public async Task<bool> UpdateComboAsync(long id, UpdateProductComboDto dto)
    {
        var combo = await _db.ProductCombos.FindAsync(id);
        if (combo == null) return false;

        if (dto.Name != null) combo.Name = dto.Name;
        if (dto.Description != null) combo.Description = dto.Description;
        if (dto.ComboPriceAud.HasValue) combo.ComboPriceAud = dto.ComboPriceAud.Value;
        if (dto.IsActive.HasValue) combo.IsActive = dto.IsActive.Value;

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteComboAsync(long id)
    {
        var combo = await _db.ProductCombos.FindAsync(id);
        if (combo == null) return false;

        _db.ProductCombos.Remove(combo);
        await _db.SaveChangesAsync();
        return true;
    }

    // ─────────────────────────────────────────────
    //  AD SPEND LOGS
    // ─────────────────────────────────────────────

    public async Task<IEnumerable<AdSpendLogDto>> GetAdSpendLogsAsync(string? platform, DateTime? fromDate, DateTime? toDate)
    {
        var query = _db.AdSpendLogs.AsQueryable();

        if (!string.IsNullOrEmpty(platform)) query = query.Where(a => a.Platform == platform);
        if (fromDate.HasValue) query = query.Where(a => a.StartDate >= fromDate.Value.Date);
        if (toDate.HasValue) query = query.Where(a => a.EndDate <= toDate.Value.Date);

        var logs = await query.OrderByDescending(a => a.StartDate).ToListAsync();
        return logs.Select(a => new AdSpendLogDto(
            Id: a.Id,
            CampaignName: a.CampaignName,
            Platform: a.Platform,
            SpendAud: a.SpendAud,
            StartDate: a.StartDate,
            EndDate: a.EndDate,
            CreatedAt: a.CreatedAt
        ));
    }

    public async Task<AdSpendLogDto> CreateAdSpendLogAsync(CreateAdSpendLogDto dto)
    {
        var entity = new AdSpendLogEntity
        {
            CampaignName = dto.CampaignName,
            Platform = dto.Platform,
            SpendAud = dto.SpendAud,
            StartDate = dto.StartDate.Date,
            EndDate = dto.EndDate.Date
        };

        _db.AdSpendLogs.Add(entity);
        await _db.SaveChangesAsync();

        return new AdSpendLogDto(
            Id: entity.Id,
            CampaignName: entity.CampaignName,
            Platform: entity.Platform,
            SpendAud: entity.SpendAud,
            StartDate: entity.StartDate,
            EndDate: entity.EndDate,
            CreatedAt: entity.CreatedAt
        );
    }

    // ─────────────────────────────────────────────
    //  MAPPING HELPERS
    // ─────────────────────────────────────────────

    private static PromotionDto MapPromotion(PromotionEntity p) => new(
        Id: p.Id,
        Name: p.Name,
        BadgeLabel: p.BadgeLabel,
        DiscountType: p.DiscountType,
        DiscountValue: p.DiscountValue,
        StartDate: p.StartDate,
        EndDate: p.EndDate,
        IsActive: p.IsActive,
        ProductIds: p.PromotionProducts.Select(pp => pp.ProductId).ToList(),
        CreatedAt: p.CreatedAt
    );

    private static ProductComboDto MapCombo(ProductComboEntity c) => new(
        Id: c.Id,
        Name: c.Name,
        Code: c.Code,
        Description: c.Description,
        ComboPriceAud: c.ComboPriceAud,
        IsActive: c.IsActive,
        Items: c.ComboItems.Select(ci => new ComboItemDto(
            Id: ci.Id,
            VariantId: ci.VariantId,
            VariantSku: ci.Variant?.Sku ?? string.Empty,
            VariantName: ci.Variant?.Name ?? string.Empty,
            Quantity: ci.Quantity
        )).ToList(),
        CreatedAt: c.CreatedAt
    );
}
