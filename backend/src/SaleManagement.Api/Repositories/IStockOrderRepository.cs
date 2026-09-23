using SaleManagement.Api.Entities;
using SaleManagement.Api.Models;

namespace SaleManagement.Api.Repositories;

public interface IStockOrderRepository
{
    Task<IEnumerable<SupplierEntity>> GetActiveSuppliersAsync();
    Task<SupplierEntity> CreateSupplierAsync(SupplierEntity entity);
    Task<IEnumerable<StockOrderEntity>> GetStockOrdersAsync();
    Task<StockOrderEntity?> GetStockOrderByIdAsync(long id);
    Task<StockOrderEntity> CreateStockOrderAsync(StockOrderEntity entity, List<StockOrderItemEntity> items);
    Task<bool> UpdateStockOrderStatusAsync(long id, string status);
    Task<StockOrderEntity?> ReceiveStockOrderAsync(long id, ReceiveStockOrderDto request, long? actorUserId);
}
