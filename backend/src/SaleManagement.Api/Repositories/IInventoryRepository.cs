using SaleManagement.Api.Entities;

namespace SaleManagement.Api.Repositories;

public interface IInventoryRepository
{
    Task<IEnumerable<WarehouseInventoryEntity>> GetInventoryAsync(long? warehouseId, long? variantId);
    Task<WarehouseInventoryEntity?> GetInventoryAsync(long warehouseId, long variantId);
    Task<InventoryTransactionEntity> AddTransactionAsync(InventoryTransactionEntity tx);

    /// <summary>
    /// Updates on_hand_quantity + reserved_quantity when stock arrives (STOCK_IN).
    /// Creates warehouse_inventory row if not exists.
    /// </summary>
    Task<bool> UpsertOnHandAsync(long warehouseId, long variantId, int deltaOnHand);

    /// <summary>
    /// Decrements reserved_quantity AND on_hand_quantity simultaneously (SALE_OUT — confirmed).
    /// </summary>
    Task<bool> ConfirmSaleOutAsync(long warehouseId, long variantId, int qty);
    Task<InventoryTransferEntity> CreateTransferAsync(InventoryTransferEntity entity, List<InventoryTransferItemEntity> items);
    Task<IEnumerable<InventoryTransferEntity>> GetTransfersAsync(string? status);
    Task<InventoryTransferEntity?> GetTransferByIdAsync(long id);
    Task<bool> UpdateTransferStatusAsync(long id, string status, long? actorUserId);
}
