using SaleManagement.Api.Entities;
using SaleManagement.Api.Models;
using SaleManagement.Api.Repositories;

namespace SaleManagement.Api.Services;

public class PricingEngineService
{
    private readonly IProductRepository _productRepository;
    private readonly IStoreRepository _storeRepository;

    public PricingEngineService(IProductRepository productRepository, IStoreRepository storeRepository)
    {
        _productRepository = productRepository;
        _storeRepository = storeRepository;
    }

    public async Task<List<PriceStageCheckResultDto>> CheckAndUpdatePriceStageAsync(PriceStageCheckRequestDto req)
    {
        var lots = await _productRepository.GetActiveFifoLotsAsync(req.VariantId, req.FifoLotId);
        var results = new List<PriceStageCheckResultDto>();

        foreach (var lot in lots)
        {
            int initialQty = lot.InitialQuantity;
            int remainingQty = lot.RemainingQuantity;
            int currentStage = lot.CurrentStage;
            DateTime activatedAt = lot.ActivatedAt ?? lot.CreatedAt;

            double remPct = initialQty > 0 ? (remainingQty * 100.0 / initialQty) : 0.0;
            int daysAtStage = (DateTime.UtcNow - activatedAt).Days;

            int targetStage = currentStage;
            string reason = string.Empty;

            // 1. Kiểm tra cấu hình cụ thể do Người Quản Lý thiết lập trong Database (pricing_stage_configs)
            var managerConfig = await _productRepository.GetPricingStageConfigAsync(lot.VariantId, (short)currentStage);

            if (managerConfig != null)
            {
                double targetPct = (double)managerConfig.TargetStockPct;
                int minDays = managerConfig.MinDaysAtStage;
                int maxDays = managerConfig.MaxDaysAtStage;

                bool isStockConditionMet = (remPct <= targetPct && daysAtStage >= minDays);
                bool isMaxDaysConditionMet = (daysAtStage >= maxDays);

                if (isStockConditionMet && isMaxDaysConditionMet)
                {
                    targetStage = managerConfig.ToStage;
                    reason = $"[Manager Config DB] Do tồn kho còn {remPct:F1}% (<= {targetPct}% ngưỡng cấu hình) sau {daysAtStage} ngày (>= {minDays} ngày tối thiểu) và đã lưu Stage {daysAtStage} ngày vượt quá số ngày tối đa ({maxDays} ngày).";
                }
                else if (isStockConditionMet)
                {
                    targetStage = managerConfig.ToStage;
                    reason = $"[Manager Config DB] Do tồn kho còn {remPct:F1}% (<= {targetPct}% ngưỡng cấu hình) và đã lưu Stage {daysAtStage} ngày (>= {minDays} ngày tối thiểu).";
                }
                else if (isMaxDaysConditionMet)
                {
                    targetStage = managerConfig.ToStage;
                    reason = $"[Manager Config DB] Do đã lưu Stage {daysAtStage} ngày vượt quá số ngày tối đa ({maxDays} ngày cấu hình).";
                }
                else if (remPct <= targetPct && daysAtStage < minDays)
                {
                    reason = $"[Manager Config DB] Tồn kho còn {remPct:F1}% (<= {targetPct}%) nhưng chưa đủ thời gian tối thiểu lưu Stage ({daysAtStage}/{minDays} ngày).";
                }
            }
            else
            {
                // 2. Mặc định Fallback theo quy tắc tiêu chuẩn hệ thống nếu Quản lý chưa cấu hình riêng cho Variant
                double defaultStockThreshold = 0.0;
                int defaultTimeThreshold = 0;
                int potentialTargetStage = currentStage;
                bool isClearance = false;

                if (currentStage == 1)
                {
                    potentialTargetStage = 2;
                    defaultStockThreshold = 80.0;
                    defaultTimeThreshold = 14;
                }
                else if (currentStage == 2)
                {
                    potentialTargetStage = 3;
                    defaultStockThreshold = 60.0;
                    defaultTimeThreshold = 30;
                }
                else if (currentStage == 3)
                {
                    potentialTargetStage = 4;
                    defaultStockThreshold = 40.0;
                    defaultTimeThreshold = 45;
                }
                else if (currentStage == 4)
                {
                    potentialTargetStage = 5;
                    defaultStockThreshold = 20.0;
                    defaultTimeThreshold = 60;
                    isClearance = true;
                }

                if (potentialTargetStage > currentStage)
                {
                    bool isStockMet = remPct <= defaultStockThreshold;
                    bool isTimeMet = daysAtStage >= defaultTimeThreshold;

                    if (isStockMet && isTimeMet)
                    {
                        targetStage = potentialTargetStage;
                        string stageLabel = isClearance ? " (Xả hàng)" : string.Empty;
                        reason = $"[Default Config] Do tồn kho còn {remPct:F1}% (<= {defaultStockThreshold}% ngưỡng{stageLabel}) và đã lưu Stage {daysAtStage} ngày (>= {defaultTimeThreshold} ngày).";
                    }
                    else if (isStockMet)
                    {
                        targetStage = potentialTargetStage;
                        string stageLabel = isClearance ? " xả hàng" : string.Empty;
                        reason = $"[Default Config] Do tồn kho còn {remPct:F1}% (<= {defaultStockThreshold}% ngưỡng{stageLabel}).";
                    }
                    else if (isTimeMet)
                    {
                        targetStage = potentialTargetStage;
                        string stageLabel = isClearance ? " xả hàng" : string.Empty;
                        reason = $"[Default Config] Do đã lưu Stage {daysAtStage} ngày (>= {defaultTimeThreshold} ngày{stageLabel}).";
                    }
                }
            }

            if (targetStage > currentStage)
            {
                var stagePrices = await _productRepository.GetStagePricesByLotIdAsync(lot.Id) ?? new Dictionary<short, decimal>();
                decimal oldPrice = stagePrices.GetValueOrDefault((short)currentStage, 0.0m);
                decimal newPrice = stagePrices.GetValueOrDefault((short)targetStage, oldPrice);

                await _productRepository.UpdateLotStageAsync(lot.Id, (short)targetStage);

                var auditLog = new StagePriceAuditLogEntity
                {
                    FifoLotId = lot.Id,
                    OldStage = (short)currentStage,
                    NewStage = (short)targetStage,
                    OldPriceAud = oldPrice,
                    NewPriceAud = newPrice,
                    TriggeredBy = "SYSTEM_WORKER",
                    RemainingStockQty = remainingQty,
                    RemainingStockPct = (decimal)remPct,
                    DaysAtOldStage = daysAtStage,
                    Reason = reason,
                    ChangedAt = DateTime.UtcNow
                };

                await _productRepository.AddPriceAuditLogAsync(auditLog);

                results.Add(new PriceStageCheckResultDto(
                    VariantId: lot.VariantId,
                    Sku: lot.Variant?.Sku ?? string.Empty,
                    FifoLotId: lot.Id,
                    StageChanged: true,
                    OldStage: currentStage,
                    NewStage: targetStage,
                    OldPriceAud: oldPrice,
                    NewPriceAud: newPrice,
                    Reason: reason
                ));
            }
            else
            {
                results.Add(new PriceStageCheckResultDto(
                    VariantId: lot.VariantId,
                    Sku: lot.Variant?.Sku ?? string.Empty,
                    FifoLotId: lot.Id,
                    StageChanged: false,
                    OldStage: currentStage,
                    NewStage: currentStage,
                    OldPriceAud: 0m,
                    NewPriceAud: 0m,
                    Reason: $"Tồn kho còn {remPct:F1}% và thời gian lưu giá {daysAtStage} ngày chưa đạt ngưỡng chuyển Stage tiếp theo."
                ));
            }
        }

        // Đồng bộ giá ngày cho từng Cửa hàng sau khi Engine chạy
        await SyncStoreVariantDailyPricesAsync(req.VariantId);

        return results;
    }

    /// <summary>
    /// Đồng bộ store_variant_prices cho từng cửa hàng dựa trên đơn giá theo Stage hiện tại của lô FIFO active cũ nhất.
    /// </summary>
    public async Task SyncStoreVariantDailyPricesAsync(long? variantId = null)
    {
        var activeStores = await _storeRepository.GetActiveStoresAsync();
        var activeLots = await _productRepository.GetActiveFifoLotsAsync(variantId, null);
        var lotGroups = activeLots.GroupBy(l => l.VariantId);

        foreach (var group in lotGroups)
        {
            var oldestLot = group.OrderBy(l => l.ReceivedDate).ThenBy(l => l.Id).FirstOrDefault();
            if (oldestLot != null)
            {
                var stagePrices = await _productRepository.GetStagePricesByLotIdAsync(oldestLot.Id) ?? new Dictionary<short, decimal>();
                decimal activePrice = stagePrices.GetValueOrDefault((short)oldestLot.CurrentStage, oldestLot.UnitLandedCostAud * 1.5m);

                if (activePrice > 0)
                {
                    foreach (var store in activeStores)
                    {
                        await _productRepository.UpdateStoreVariantDailyPriceAsync(store.Id, group.Key, activePrice);
                    }
                }
            }
        }
    }
}
