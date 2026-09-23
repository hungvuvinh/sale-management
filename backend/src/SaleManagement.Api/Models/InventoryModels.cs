namespace SaleManagement.Api.Models;

public record ReceiveStockOrderDto(
    DateOnly? ReceivedDate,
    List<ReceiveStockOrderItemDto> Items
);

public record ReceiveStockOrderItemDto(
    long StockOrderItemId,
    int QuantityReceived
);

public record CreateInventoryTransferDto(
    long FromWarehouseId,
    long ToWarehouseId,
    List<CreateInventoryTransferItemDto> Items
);

public record CreateInventoryTransferItemDto(
    long VariantId,
    int QuantityRequested
);

public record InventoryTransferDto(
    long Id,
    string TransferCode,
    long FromWarehouseId,
    string FromWarehouseName,
    long ToWarehouseId,
    string ToWarehouseName,
    string Status,
    long? RequestedBy,
    long? ApprovedBy,
    DateTime CreatedAt,
    DateTime? ReceivedAt,
    List<InventoryTransferItemDto> Items
);

public record InventoryTransferItemDto(
    long Id,
    long VariantId,
    string VariantSku,
    string VariantName,
    int QuantityRequested,
    int QuantityReceived
);