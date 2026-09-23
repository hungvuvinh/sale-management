using SaleManagement.Api.Entities;
using SaleManagement.Api.Models;
using SaleManagement.Api.Repositories;

namespace SaleManagement.Api.Services;

public class StockOrdersService
{
    private readonly IStockOrderRepository _stockOrderRepository;

    public StockOrdersService(IStockOrderRepository stockOrderRepository)
    {
        _stockOrderRepository = stockOrderRepository;
    }

    public LandedCostCalculationResultDto CalculateLandedCost(CalculateLandedCostRequestDto req)
    {
        decimal totalCbm = req.Items.Sum(x => x.Quantity * x.CbmPerUnit);
        if (totalCbm <= 0)
        {
            throw new ArgumentException("Total CBM of container must be greater than zero.");
        }

        decimal freightPerCbmAud = req.ContainerFreightAud / totalCbm;
        decimal taxPerCbmAud = req.CustomsTaxAud / totalCbm;

        var itemResults = new List<LandedCostItemResultDto>();

        foreach (var item in req.Items)
        {
            decimal unitCostAud = Math.Round(item.UnitCostUsd * req.ExchangeRateUsdAud, 2);
            decimal freightAud = Math.Round(item.CbmPerUnit * freightPerCbmAud, 2);
            decimal taxAud = Math.Round(item.CbmPerUnit * taxPerCbmAud, 2);
            decimal landedCostAud = Math.Round(unitCostAud + freightAud + taxAud, 2);

            decimal marginDivisor = (1.0m - (item.TargetMarginPct / 100.0m));
            if (marginDivisor <= 0) marginDivisor = 0.40m;

            decimal stage1Price = Math.Round(landedCostAud / marginDivisor, 2);
            decimal stage2Price = Math.Round(stage1Price * 0.90m, 2);
            decimal stage3Price = Math.Round(stage1Price * 0.75m, 2);
            decimal stage4Price = Math.Round(stage1Price * 0.65m, 2);
            decimal stage5Price = Math.Round(stage1Price * 0.50m, 2);

            var stagesDict = new Dictionary<string, decimal>
            {
                ["stage1"] = stage1Price,
                ["stage2"] = stage2Price,
                ["stage3"] = stage3Price,
                ["stage4"] = stage4Price,
                ["stage5"] = stage5Price
            };

            itemResults.Add(new LandedCostItemResultDto(
                VariantId: item.VariantId,
                UnitCostAud: unitCostAud,
                FreightAud: freightAud,
                TaxAud: taxAud,
                LandedCostAud: landedCostAud,
                SuggestedStage1Price: stage1Price,
                Generated5Stages: stagesDict
            ));
        }

        return new LandedCostCalculationResultDto(
            TotalCbm: totalCbm,
            FreightPerCbmAud: freightPerCbmAud,
            Calculations: itemResults
        );
    }

    public async Task<IEnumerable<SupplierDto>> GetSuppliersAsync()
    {
        var suppliers = await _stockOrderRepository.GetActiveSuppliersAsync();
        return suppliers.Select(s => new SupplierDto(
            Id: s.Id,
            Name: s.Name,
            Country: s.Country,
            ContactName: s.ContactName,
            ContactPhone: s.ContactPhone,
            ContactEmail: s.ContactEmail,
            LeadTimeDays: s.LeadTimeDays,
            PaymentTerms: s.PaymentTerms,
            IsActive: s.IsActive,
            CreatedAt: s.CreatedAt
        ));
    }

    public async Task<SupplierDto> CreateSupplierAsync(CreateSupplierDto dto)
    {
        var entity = new SupplierEntity
        {
            Name = dto.Name,
            Country = dto.Country,
            ContactName = dto.ContactName,
            ContactPhone = dto.ContactPhone,
            ContactEmail = dto.ContactEmail,
            LeadTimeDays = dto.LeadTimeDays,
            PaymentTerms = dto.PaymentTerms,
            IsActive = true
        };

        var created = await _stockOrderRepository.CreateSupplierAsync(entity);
        return new SupplierDto(
            Id: created.Id,
            Name: created.Name,
            Country: created.Country,
            ContactName: created.ContactName,
            ContactPhone: created.ContactPhone,
            ContactEmail: created.ContactEmail,
            LeadTimeDays: created.LeadTimeDays,
            PaymentTerms: created.PaymentTerms,
            IsActive: created.IsActive,
            CreatedAt: created.CreatedAt
        );
    }

    public async Task<IEnumerable<StockOrderDto>> GetStockOrdersAsync()
    {
        var orders = await _stockOrderRepository.GetStockOrdersAsync();
        return orders.Select(MapToDto);
    }

    public async Task<StockOrderDto?> GetStockOrderByIdAsync(long id)
    {
        var order = await _stockOrderRepository.GetStockOrderByIdAsync(id);
        return order != null ? MapToDto(order) : null;
    }

    public async Task<StockOrderDto> CreateStockOrderAsync(CreateStockOrderDto dto)
    {
        decimal totalCbm = 0;
        if (dto.Items != null && dto.Items.Any())
        {
            totalCbm = dto.Items.Sum(x => x.QuantityOrdered * x.UnitCbm);
        }

        var orderEntity = new StockOrderEntity
        {
            PoNumber = dto.PoNumber,
            SupplierId = dto.SupplierId,
            DestinationWarehouseId = dto.DestinationWarehouseId,
            ContainerCode = dto.ContainerCode,
            EtaDate = dto.EtaDate,
            Status = "PENDING",
            TotalCbm = totalCbm,
            ContainerFreightAud = dto.ContainerFreightAud,
            CustomsTaxAud = dto.CustomsTaxAud,
            CurrencyCode = dto.CurrencyCode,
            ExchangeRate = dto.ExchangeRate,
            Notes = dto.Notes
        };

        var itemEntities = new List<StockOrderItemEntity>();

        if (dto.Items != null && dto.Items.Any() && totalCbm > 0)
        {
            decimal freightPerCbm = dto.ContainerFreightAud / totalCbm;
            decimal taxPerCbm = dto.CustomsTaxAud / totalCbm;

            foreach (var item in dto.Items)
            {
                decimal unitCostAud = Math.Round(item.UnitCostForeign * dto.ExchangeRate, 2);
                decimal unitFreightAud = Math.Round(item.UnitCbm * freightPerCbm, 2);
                decimal unitTaxAud = Math.Round(item.UnitCbm * taxPerCbm, 2);
                decimal landedCostAud = Math.Round(unitCostAud + unitFreightAud + unitTaxAud, 2);

                itemEntities.Add(new StockOrderItemEntity
                {
                    VariantId = item.VariantId,
                    QuantityOrdered = item.QuantityOrdered,
                    QuantityReceived = 0,
                    UnitCostForeign = item.UnitCostForeign,
                    UnitCostAud = unitCostAud,
                    UnitCbm = item.UnitCbm,
                    UnitFreightAud = unitFreightAud,
                    CalculatedLandedCostAud = landedCostAud
                });
            }
        }

        var createdOrder = await _stockOrderRepository.CreateStockOrderAsync(orderEntity, itemEntities);
        return MapToDto(createdOrder);
    }

    public async Task<bool> UpdateStockOrderStatusAsync(long id, string status)
    {
        return await _stockOrderRepository.UpdateStockOrderStatusAsync(id, status);
    }

    public async Task<StockOrderDto?> ReceiveStockOrderAsync(long id, ReceiveStockOrderDto request, long? actorUserId)
    {
        if (request.Items == null || request.Items.Count == 0)
            throw new ArgumentException("At least one stock order item is required.");

        var received = await _stockOrderRepository.ReceiveStockOrderAsync(id, request, actorUserId);
        return received == null ? null : MapToDto(received);
    }

    private static StockOrderDto MapToDto(StockOrderEntity so)
    {
        var itemsDto = so.Items.Select(i => new StockOrderItemDto(
            Id: i.Id,
            StockOrderId: i.StockOrderId,
            VariantId: i.VariantId,
            VariantSku: i.Variant?.Sku ?? string.Empty,
            VariantName: i.Variant?.Name ?? string.Empty,
            QuantityOrdered: i.QuantityOrdered,
            QuantityReceived: i.QuantityReceived,
            UnitCostForeign: i.UnitCostForeign,
            UnitCostAud: i.UnitCostAud,
            UnitCbm: i.UnitCbm,
            UnitFreightAud: i.UnitFreightAud,
            CalculatedLandedCostAud: i.CalculatedLandedCostAud
        )).ToList();

        return new StockOrderDto(
            Id: so.Id,
            PoNumber: so.PoNumber,
            SupplierId: so.SupplierId,
            SupplierName: so.Supplier?.Name ?? string.Empty,
            DestinationWarehouseId: so.DestinationWarehouseId,
            WarehouseName: so.DestinationWarehouse?.Name ?? string.Empty,
            ContainerCode: so.ContainerCode,
            EtaDate: so.EtaDate,
            ActualArrivalDate: so.ActualArrivalDate,
            Status: so.Status,
            TotalCbm: so.TotalCbm,
            ContainerFreightAud: so.ContainerFreightAud,
            CustomsTaxAud: so.CustomsTaxAud,
            CurrencyCode: so.CurrencyCode,
            ExchangeRate: so.ExchangeRate,
            Notes: so.Notes,
            CreatedAt: so.CreatedAt,
            Items: itemsDto
        );
    }
}
