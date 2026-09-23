using Moq;
using SaleManagement.Api.Entities;
using SaleManagement.Api.Models;
using SaleManagement.Api.Repositories;
using SaleManagement.Api.Services;
using Xunit;

namespace SaleManagement.Api.Tests.Services;

public class PricingEngineServiceTests
{
    private readonly Mock<IProductRepository> _productRepoMock;
    private readonly Mock<IStoreRepository> _storeRepoMock;
    private readonly PricingEngineService _pricingEngine;

    public PricingEngineServiceTests()
    {
        _productRepoMock = new Mock<IProductRepository>();
        _storeRepoMock = new Mock<IStoreRepository>();
        _pricingEngine = new PricingEngineService(_productRepoMock.Object, _storeRepoMock.Object);
    }

    [Fact]
    public async Task CheckAndUpdatePriceStage_ManagerConfig_TransitionsWhenStockAndMinDaysMet()
    {
        // Arrange: Lot at Stage 1, 60% remaining (<= 70%), 10 days at stage (>= 7 min days)
        var lot = new FifoLotEntity
        {
            Id = 101,
            LotNumber = "LOT-PRICE-MGR-PCT",
            VariantId = 1,
            InitialQuantity = 20,
            RemainingQuantity = 12,
            CurrentStage = 1,
            ActivatedAt = DateTime.UtcNow.AddDays(-10),
            Status = "ACTIVE"
        };

        var managerConfig = new PricingStageConfigEntity
        {
            VariantId = 1,
            FromStage = 1,
            ToStage = 2,
            TargetStockPct = 70.0m,
            MinDaysAtStage = 7,
            MaxDaysAtStage = 45
        };

        var stagePrices = new Dictionary<short, decimal>
        {
            { 1, 1699m },
            { 2, 1499m }
        };

        _productRepoMock.Setup(r => r.GetActiveFifoLotsAsync(1, 101))
            .ReturnsAsync(new List<FifoLotEntity> { lot });
        _productRepoMock.Setup(r => r.GetPricingStageConfigAsync(1, 1))
            .ReturnsAsync(managerConfig);
        _productRepoMock.Setup(r => r.GetStagePricesByLotIdAsync(101))
            .ReturnsAsync(stagePrices);
        _productRepoMock.Setup(r => r.GetActiveFifoLotsAsync(1, null))
            .ReturnsAsync(new List<FifoLotEntity> { lot });
        _storeRepoMock.Setup(r => r.GetActiveStoresAsync())
            .ReturnsAsync(new List<StoreEntity>());

        // Act
        var results = await _pricingEngine.CheckAndUpdatePriceStageAsync(new PriceStageCheckRequestDto(1, 101));

        // Assert
        Assert.Single(results);
        var res = results[0];
        Assert.True(res.StageChanged);
        Assert.Equal(1, res.OldStage);
        Assert.Equal(2, res.NewStage);
        Assert.Equal(1699m, res.OldPriceAud);
        Assert.Equal(1499m, res.NewPriceAud);
        _productRepoMock.Verify(r => r.UpdateLotStageAsync(101, 2), Times.Once);
        _productRepoMock.Verify(r => r.AddPriceAuditLogAsync(It.IsAny<StagePriceAuditLogEntity>()), Times.Once);
    }

    [Fact]
    public async Task CheckAndUpdatePriceStage_ManagerConfig_DoesNotTransitionWhenMinDaysNotMet()
    {
        // Arrange: Lot at Stage 1, 50% remaining (<= 70%), but only 3 days (< 7 min days)
        var lot = new FifoLotEntity
        {
            Id = 102,
            LotNumber = "LOT-PRICE-MGR-MIN-BLOCKED",
            VariantId = 1,
            InitialQuantity = 20,
            RemainingQuantity = 10,
            CurrentStage = 1,
            ActivatedAt = DateTime.UtcNow.AddDays(-3),
            Status = "ACTIVE"
        };

        var managerConfig = new PricingStageConfigEntity
        {
            VariantId = 1,
            FromStage = 1,
            ToStage = 2,
            TargetStockPct = 70.0m,
            MinDaysAtStage = 7,
            MaxDaysAtStage = 45
        };

        _productRepoMock.Setup(r => r.GetActiveFifoLotsAsync(1, 102))
            .ReturnsAsync(new List<FifoLotEntity> { lot });
        _productRepoMock.Setup(r => r.GetPricingStageConfigAsync(1, 1))
            .ReturnsAsync(managerConfig);
        _productRepoMock.Setup(r => r.GetStagePricesByLotIdAsync(102))
            .ReturnsAsync(new Dictionary<short, decimal> { { 1, 1699m } });
        _productRepoMock.Setup(r => r.GetActiveFifoLotsAsync(1, null))
            .ReturnsAsync(new List<FifoLotEntity> { lot });
        _storeRepoMock.Setup(r => r.GetActiveStoresAsync())
            .ReturnsAsync(new List<StoreEntity>());

        // Act
        var results = await _pricingEngine.CheckAndUpdatePriceStageAsync(new PriceStageCheckRequestDto(1, 102));

        // Assert
        Assert.Single(results);
        var res = results[0];
        Assert.False(res.StageChanged);
        Assert.Equal(1, res.OldStage);
        Assert.Equal(1, res.NewStage);
        _productRepoMock.Verify(r => r.UpdateLotStageAsync(It.IsAny<long>(), It.IsAny<short>()), Times.Never);
    }

    [Fact]
    public async Task CheckAndUpdatePriceStage_ManagerConfig_TransitionsWhenMaxDaysExceeded()
    {
        // Arrange: Lot at Stage 2, 90% remaining (> 50%), but 50 days (>= 45 max days)
        var lot = new FifoLotEntity
        {
            Id = 103,
            LotNumber = "LOT-PRICE-MGR-MAXDAYS",
            VariantId = 2,
            InitialQuantity = 10,
            RemainingQuantity = 9,
            CurrentStage = 2,
            ActivatedAt = DateTime.UtcNow.AddDays(-50),
            Status = "ACTIVE"
        };

        var managerConfig = new PricingStageConfigEntity
        {
            VariantId = 2,
            FromStage = 2,
            ToStage = 3,
            TargetStockPct = 50.0m,
            MinDaysAtStage = 7,
            MaxDaysAtStage = 45
        };

        var stagePrices = new Dictionary<short, decimal>
        {
            { 2, 1249m },
            { 3, 1099m }
        };

        _productRepoMock.Setup(r => r.GetActiveFifoLotsAsync(2, 103))
            .ReturnsAsync(new List<FifoLotEntity> { lot });
        _productRepoMock.Setup(r => r.GetPricingStageConfigAsync(2, 2))
            .ReturnsAsync(managerConfig);
        _productRepoMock.Setup(r => r.GetStagePricesByLotIdAsync(103))
            .ReturnsAsync(stagePrices);
        _productRepoMock.Setup(r => r.GetActiveFifoLotsAsync(2, null))
            .ReturnsAsync(new List<FifoLotEntity> { lot });
        _storeRepoMock.Setup(r => r.GetActiveStoresAsync())
            .ReturnsAsync(new List<StoreEntity>());

        // Act
        var results = await _pricingEngine.CheckAndUpdatePriceStageAsync(new PriceStageCheckRequestDto(2, 103));

        // Assert
        Assert.Single(results);
        var res = results[0];
        Assert.True(res.StageChanged);
        Assert.Equal(2, res.OldStage);
        Assert.Equal(3, res.NewStage);
        Assert.Equal(1249m, res.OldPriceAud);
        Assert.Equal(1099m, res.NewPriceAud);
        _productRepoMock.Verify(r => r.UpdateLotStageAsync(103, 3), Times.Once);
    }

    [Theory]
    [InlineData(1, 10, 7, 3, 2, true)]   // Stage 1 -> 2: 70% stock (<= 80%)
    [InlineData(1, 10, 9, 18, 2, true)]  // Stage 1 -> 2: 18 days (>= 14 days)
    [InlineData(2, 10, 5, 8, 3, true)]   // Stage 2 -> 3: 50% stock (<= 60%)
    [InlineData(2, 10, 8, 35, 3, true)]  // Stage 2 -> 3: 35 days (>= 30 days)
    [InlineData(3, 10, 3, 10, 4, true)]  // Stage 3 -> 4: 30% stock (<= 40%)
    [InlineData(3, 10, 7, 48, 4, true)]  // Stage 3 -> 4: 48 days (>= 45 days)
    [InlineData(4, 10, 1, 15, 5, true)]  // Stage 4 -> 5: 10% stock (<= 20%)
    [InlineData(4, 10, 5, 65, 5, true)]  // Stage 4 -> 5: 65 days (>= 60 days)
    [InlineData(1, 10, 10, 2, 1, false)] // Stable Stage 1: 100% stock, 2 days -> No change
    [InlineData(5, 10, 2, 90, 5, false)] // Terminal Stage 5: No change
    public async Task CheckAndUpdatePriceStage_DefaultRules_HandlesTransitionsCorrectly(
        int currentStage, int initialQty, int remQty, int daysAtStage, int expectedNewStage, bool expectedChange)
    {
        // Arrange
        var lot = new FifoLotEntity
        {
            Id = 200 + currentStage,
            LotNumber = $"LOT-TEST-{currentStage}",
            VariantId = 3,
            InitialQuantity = initialQty,
            RemainingQuantity = remQty,
            CurrentStage = (short)currentStage,
            ActivatedAt = DateTime.UtcNow.AddDays(-daysAtStage),
            Status = "ACTIVE"
        };

        var stagePrices = new Dictionary<short, decimal>
        {
            { 1, 999m },
            { 2, 899m },
            { 3, 799m },
            { 4, 699m },
            { 5, 599m }
        };

        _productRepoMock.Setup(r => r.GetActiveFifoLotsAsync(3, lot.Id))
            .ReturnsAsync(new List<FifoLotEntity> { lot });
        _productRepoMock.Setup(r => r.GetPricingStageConfigAsync(3, (short)currentStage))
            .ReturnsAsync((PricingStageConfigEntity?)null); // No manager config -> Fallback to default rules
        _productRepoMock.Setup(r => r.GetStagePricesByLotIdAsync(lot.Id))
            .ReturnsAsync(stagePrices);
        _productRepoMock.Setup(r => r.GetActiveFifoLotsAsync(3, null))
            .ReturnsAsync(new List<FifoLotEntity> { lot });
        _storeRepoMock.Setup(r => r.GetActiveStoresAsync())
            .ReturnsAsync(new List<StoreEntity>());

        // Act
        var results = await _pricingEngine.CheckAndUpdatePriceStageAsync(new PriceStageCheckRequestDto(3, lot.Id));

        // Assert
        Assert.Single(results);
        var res = results[0];
        Assert.Equal(expectedChange, res.StageChanged);
        Assert.Equal(currentStage, res.OldStage);
        Assert.Equal(expectedNewStage, res.NewStage);

        if (expectedChange)
        {
            _productRepoMock.Verify(r => r.UpdateLotStageAsync(lot.Id, (short)expectedNewStage), Times.Once);
            Assert.DoesNotContain("hoặc", res.Reason);
        }
        else
        {
            _productRepoMock.Verify(r => r.UpdateLotStageAsync(lot.Id, It.IsAny<short>()), Times.Never);
        }
    }

    [Fact]
    public async Task CheckAndUpdatePriceStage_BothConditionsMet_ContainsAndClauseNotOr()
    {
        // Arrange: Lot at Stage 1, 50% remaining (<= 80%), 20 days at stage (>= 14 days) -> Both met
        var lot = new FifoLotEntity
        {
            Id = 301,
            LotNumber = "LOT-TEST-DUAL",
            VariantId = 3,
            InitialQuantity = 10,
            RemainingQuantity = 5,
            CurrentStage = 1,
            ActivatedAt = DateTime.UtcNow.AddDays(-20),
            Status = "ACTIVE"
        };

        var stagePrices = new Dictionary<short, decimal>
        {
            { 1, 999m },
            { 2, 899m }
        };

        _productRepoMock.Setup(r => r.GetActiveFifoLotsAsync(3, 301))
            .ReturnsAsync(new List<FifoLotEntity> { lot });
        _productRepoMock.Setup(r => r.GetPricingStageConfigAsync(3, 1))
            .ReturnsAsync((PricingStageConfigEntity?)null);
        _productRepoMock.Setup(r => r.GetStagePricesByLotIdAsync(301))
            .ReturnsAsync(stagePrices);
        _productRepoMock.Setup(r => r.GetActiveFifoLotsAsync(3, null))
            .ReturnsAsync(new List<FifoLotEntity> { lot });
        _storeRepoMock.Setup(r => r.GetActiveStoresAsync())
            .ReturnsAsync(new List<StoreEntity>());

        // Act
        var results = await _pricingEngine.CheckAndUpdatePriceStageAsync(new PriceStageCheckRequestDto(3, 301));

        // Assert
        Assert.Single(results);
        var res = results[0];
        Assert.True(res.StageChanged);
        Assert.Contains(" và ", res.Reason);
        Assert.DoesNotContain(" hoặc ", res.Reason);
    }
}

