using SaleManagement.Api.Entities;
using SaleManagement.Api.Models;
using SaleManagement.Api.Repositories;

namespace SaleManagement.Api.Services;

public class StoresService
{
    private readonly IStoreRepository _storeRepository;

    public StoresService(IStoreRepository storeRepository)
    {
        _storeRepository = storeRepository;
    }

    public async Task<IEnumerable<StoreDto>> GetStoresAsync()
    {
        var stores = await _storeRepository.GetActiveStoresAsync();
        return stores.Select(s => new StoreDto(
            Id: s.Id,
            Code: s.Code,
            Name: s.Name,
            Phone: s.Phone ?? string.Empty,
            Email: s.Email ?? string.Empty,
            IsActive: s.IsActive,
            Address: new AddressDto(
                Id: s.Address.Id,
                StreetAddress: s.Address.StreetAddress,
                AddressDetail: s.Address.AddressDetail,
                Suburb: s.Address.Suburb,
                City: s.Address.City,
                State: s.Address.State,
                PostcodeId: s.Address.PostcodeId,
                Postcode: s.Address.Postcode?.Postcode ?? string.Empty,
                Latitude: s.Address.Latitude,
                Longitude: s.Address.Longitude
            )
        ));
    }

    public async Task<IEnumerable<WarehouseDto>> GetWarehousesAsync()
    {
        var warehouses = await _storeRepository.GetActiveWarehousesAsync();
        return warehouses.Select(w => new WarehouseDto(
            Id: w.Id,
            Code: w.Code,
            Name: w.Name,
            IsStore: w.IsStore,
            TotalCapacityCbm: w.TotalCapacityCbm,
            IsActive: w.IsActive,
            Address: new AddressDto(
                Id: w.Address.Id,
                StreetAddress: w.Address.StreetAddress,
                AddressDetail: w.Address.AddressDetail,
                Suburb: w.Address.Suburb,
                City: w.Address.City,
                State: w.Address.State,
                PostcodeId: w.Address.PostcodeId,
                Postcode: w.Address.Postcode?.Postcode ?? string.Empty,
                Latitude: w.Address.Latitude,
                Longitude: w.Address.Longitude
            )
        ));
    }

    public async Task<PostcodeLookupDto?> LookupPostcodeAsync(string code)
    {
        var postcode = await _storeRepository.GetPostcodeByCodeAsync(code.Trim());
        if (postcode == null) return null;

        var assignedStoreEntities = await _storeRepository.GetActiveStoresByPostcodeIdAsync(postcode.Id);
        var assignedStoreEntity = assignedStoreEntities.Count == 1 ? assignedStoreEntities[0] : null;

        StoreDto? assignedStoreDto = null;
        ShippingRateDto? rateDto = null;

        if (assignedStoreEntity != null)
        {
            assignedStoreDto = new StoreDto(
                Id: assignedStoreEntity.Id,
                Code: assignedStoreEntity.Code,
                Name: assignedStoreEntity.Name,
                Phone: assignedStoreEntity.Phone ?? string.Empty,
                Email: assignedStoreEntity.Email ?? string.Empty,
                IsActive: assignedStoreEntity.IsActive,
                Address: new AddressDto(
                    Id: assignedStoreEntity.Address.Id,
                    StreetAddress: assignedStoreEntity.Address.StreetAddress,
                    AddressDetail: assignedStoreEntity.Address.AddressDetail,
                    Suburb: assignedStoreEntity.Address.Suburb,
                    City: assignedStoreEntity.Address.City,
                    State: assignedStoreEntity.Address.State,
                    PostcodeId: assignedStoreEntity.Address.PostcodeId,
                    Postcode: assignedStoreEntity.Address.Postcode?.Postcode ?? string.Empty,
                    Latitude: assignedStoreEntity.Address.Latitude,
                    Longitude: assignedStoreEntity.Address.Longitude
                )
            );

            var rate = await _storeRepository.GetShippingRateByStoreIdAsync(assignedStoreEntity.Id);
            if (rate != null)
            {
                rateDto = new ShippingRateDto(
                    Id: rate.Id,
                    StoreId: rate.StoreId,
                    MinDistanceKm: rate.MinDistanceKm,
                    MaxDistanceKm: rate.MaxDistanceKm,
                    ShippingFeeAud: rate.ShippingFeeAud
                );
            }
        }

        var availableStores = assignedStoreEntities
            .Select(MapStore)
            .ToArray();

        return new PostcodeLookupDto(
            Postcode: postcode.Postcode,
            Suburb: postcode.SuburbName,
            State: postcode.State,
            AssignedStore: assignedStoreDto,
            AvailableStores: availableStores,
            PriceUnlocked: assignedStoreDto != null,
            EstimatedShippingRate: rateDto
        );
    }

    public async Task<PostcodeLookupDto> SelectStoreForPostcodeAsync(string code, long storeId)
    {
        var postcode = await _storeRepository.GetPostcodeByCodeAsync(code.Trim())
            ?? throw new KeyNotFoundException($"Postcode '{code}' not found.");
        var stores = await _storeRepository.GetActiveStoresByPostcodeIdAsync(postcode.Id);
        if (!stores.Any(store => store.Id == storeId))
            throw new ArgumentException("Store không phục vụ postcode đã chọn.");

        var selectedStore = stores.First(store => store.Id == storeId);
        var rate = await _storeRepository.GetShippingRateByStoreIdAsync(storeId);
        return new PostcodeLookupDto(
            postcode.Postcode,
            postcode.SuburbName,
            postcode.State,
            MapStore(selectedStore),
            stores.Select(MapStore).ToArray(),
            true,
            rate == null ? null : new ShippingRateDto(
                rate.Id,
                rate.StoreId,
                rate.MinDistanceKm,
                rate.MaxDistanceKm,
                rate.ShippingFeeAud));
    }

    private static StoreDto MapStore(StoreEntity store) => new(
        Id: store.Id,
        Code: store.Code,
        Name: store.Name,
        Phone: store.Phone ?? string.Empty,
        Email: store.Email ?? string.Empty,
        IsActive: store.IsActive,
        Address: new AddressDto(
            Id: store.Address.Id,
            StreetAddress: store.Address.StreetAddress,
            AddressDetail: store.Address.AddressDetail,
            Suburb: store.Address.Suburb,
            City: store.Address.City,
            State: store.Address.State,
            PostcodeId: store.Address.PostcodeId,
            Postcode: store.Address.Postcode?.Postcode ?? string.Empty,
            Latitude: store.Address.Latitude,
            Longitude: store.Address.Longitude));

    public async Task<bool> AssignPostcodeToStoreAsync(long postcodeId, long storeId)
    {
        await _storeRepository.AssignPostcodeToStoreAsync(postcodeId, storeId);
        return true;
    }
}
