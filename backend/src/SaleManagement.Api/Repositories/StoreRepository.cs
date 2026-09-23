using Microsoft.EntityFrameworkCore;
using SaleManagement.Api.Data;
using SaleManagement.Api.Entities;

namespace SaleManagement.Api.Repositories;

public class StoreRepository : IStoreRepository
{
    private readonly AppDbContext _context;

    public StoreRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<StoreEntity>> GetActiveStoresAsync()
    {
        return await _context.Stores
            .Include(s => s.Address)
                .ThenInclude(a => a.Postcode)
            .Where(s => s.IsActive)
            .OrderBy(s => s.Id)
            .ToListAsync();
    }

    public async Task<IEnumerable<WarehouseEntity>> GetActiveWarehousesAsync()
    {
        return await _context.Warehouses
            .Include(w => w.Address)
                .ThenInclude(a => a.Postcode)
            .Where(w => w.IsActive)
            .OrderBy(w => w.Id)
            .ToListAsync();
    }

    public async Task<PostcodeEntity?> GetPostcodeByCodeAsync(string postcode)
    {
        return await _context.Postcodes
            .FirstOrDefaultAsync(p => p.Postcode == postcode);
    }

    public async Task<StoreEntity?> GetAssignedStoreByPostcodeIdAsync(long postcodeId)
    {
        return (await GetActiveStoresByPostcodeIdAsync(postcodeId)).FirstOrDefault();
    }

    public async Task<IReadOnlyList<StoreEntity>> GetActiveStoresByPostcodeIdAsync(long postcodeId)
    {
        return await _context.PostcodeStores
            .Include(ps => ps.Store)
                .ThenInclude(s => s.Address)
                    .ThenInclude(a => a.Postcode)
            .Where(ps => ps.PostcodeId == postcodeId && ps.Store.IsActive)
            .Select(ps => ps.Store)
            .OrderBy(store => store.Id)
            .ToListAsync();
    }

    public async Task<StoreEntity?> GetActiveStoreByIdAsync(long storeId)
    {
        return await _context.Stores
            .Include(store => store.Address)
                .ThenInclude(address => address.Postcode)
            .FirstOrDefaultAsync(store => store.Id == storeId && store.IsActive);
    }

    public async Task<bool> IsStoreAssignedToPostcodeAsync(long postcodeId, long storeId)
    {
        return await _context.PostcodeStores
            .AnyAsync(mapping => mapping.PostcodeId == postcodeId && mapping.StoreId == storeId && mapping.Store.IsActive);
    }

    public async Task<StoreShippingRateEntity?> GetShippingRateByStoreIdAsync(long storeId)
    {
        return await _context.StoreShippingRates
            .Where(r => r.StoreId == storeId)
            .OrderBy(r => r.MinDistanceKm)
            .FirstOrDefaultAsync();
    }

    public async Task AssignPostcodeToStoreAsync(long postcodeId, long storeId)
    {
        var exists = await _context.PostcodeStores
            .AnyAsync(ps => ps.PostcodeId == postcodeId && ps.StoreId == storeId);

        if (!exists)
        {
            _context.PostcodeStores.Add(new PostcodeStoreEntity
            {
                PostcodeId = postcodeId,
                StoreId = storeId
            });
            await _context.SaveChangesAsync();
        }
    }
}
