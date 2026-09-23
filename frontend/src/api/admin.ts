import { apiClient } from './client';
import type { FifoLot, InventoryTransfer, Order, PriceStageResult, PricingStageConfig, Product, StockOrder, WarehouseInventory } from '../types/domain';

interface ApiEnvelope<T> {
  data: T;
}

export async function getAdminProducts(warehouseId?: number): Promise<Product[]> {
  const response = await apiClient.get<ApiEnvelope<Product[]>>('/products', { params: warehouseId ? { warehouseId } : undefined });
  return response.data.data;
}

export async function createProduct(payload: { sku: string; name: string; slug: string; categoryId?: number; description?: string }): Promise<void> {
  await apiClient.post('/products', { ...payload, galleryImages: [], materialsSummary: null, mainImageUrl: null, videoUrl: null });
}

export async function updateProduct(id: number, payload: { sku: string; name: string; slug: string; isActive: boolean }): Promise<void> {
  await apiClient.put(`/products/${id}`, payload);
}

export async function deleteProduct(id: number): Promise<void> {
  await apiClient.delete(`/products/${id}`);
}

export async function createVariant(productId: number, payload: { sku: string; name: string; weightKg: number; cbm: number; boxCount: number }): Promise<void> {
  await apiClient.post(`/products/${productId}/variants`, { ...payload, barcode: null, attributesJson: {}, dimensionsCm: null });
}

export async function updateVariant(id: number, payload: { sku: string; name: string; weightKg: number; cbm: number; boxCount: number; isActive: boolean }): Promise<void> {
  await apiClient.put(`/products/variants/${id}`, { ...payload, barcode: null, attributesJson: {}, dimensionsCm: null });
}

export async function saveVariantStageConfigs(variantId: number, configs: Array<{ fromStage: number; toStage: number; targetStockPct: number; minDaysAtStage: number; maxDaysAtStage: number }>): Promise<void> {
  await apiClient.post(`/products/variants/${variantId}/stage-configs`, { configs });
}

export async function deleteVariant(id: number): Promise<void> {
  await apiClient.delete(`/products/variants/${id}`);
}

export async function getStockOrders(): Promise<StockOrder[]> {
  const response = await apiClient.get<ApiEnvelope<StockOrder[]>>('/stock-orders');
  return response.data.data;
}

export async function runPriceStageCheck(variantId?: number): Promise<PriceStageResult[]> {
  const response = await apiClient.post<ApiEnvelope<PriceStageResult[]>>('/products/price-stage-check', {
    variantId: variantId || null,
    fifoLotId: null,
  });
  return response.data.data;
}

export async function createStockOrder(payload: {
  poNumber: string;
  supplierId: number;
  destinationWarehouseId: number;
  etaDate: string;
  containerFreightAud: number;
  customsTaxAud: number;
  items: Array<{ variantId: number; quantityOrdered: number; unitCostForeign: number; unitCbm: number }>;
}): Promise<StockOrder> {
  const response = await apiClient.post<ApiEnvelope<StockOrder>>('/stock-orders', payload);
  return response.data.data;
}

export async function receiveStockOrder(order: StockOrder): Promise<StockOrder> {
  const items = order.items
    .filter((item) => item.quantityOrdered > item.quantityReceived)
    .map((item) => ({
      stockOrderItemId: item.id,
      quantityReceived: item.quantityOrdered - item.quantityReceived,
    }));
  const response = await apiClient.post<ApiEnvelope<StockOrder>>(`/stock-orders/${order.id}/receive`, {
    receivedDate: new Date().toISOString().slice(0, 10),
    items,
  });
  return response.data.data;
}

export async function getInventoryTransfers(): Promise<InventoryTransfer[]> {
  const response = await apiClient.get<ApiEnvelope<InventoryTransfer[]>>('/inventory/transfers');
  return response.data.data;
}

export async function createInventoryTransfer(payload: {
  fromWarehouseId: number;
  toWarehouseId: number;
  items: Array<{ variantId: number; quantityRequested: number }>;
}): Promise<InventoryTransfer> {
  const response = await apiClient.post<ApiEnvelope<InventoryTransfer>>('/inventory/transfers', payload);
  return response.data.data;
}

export async function updateInventoryTransferStatus(id: number, status: string): Promise<void> {
  await apiClient.put(`/inventory/transfers/${id}/status`, { status });
}

export async function getOrders(status?: string, storeId?: number): Promise<Order[]> {
  const response = await apiClient.get<ApiEnvelope<Order[]>>('/orders', { params: { status, storeId } });
  return response.data.data;
}

export async function createOrder(payload: {
  storeId: number;
  customerId?: number;
  orderType: string;
  deliveryType: string;
  shippingAddressText?: string;
  deliveryDate?: string;
  notes?: string;
  items: Array<{ variantId: number; quantity: number; unitPriceAud: number }>;
}): Promise<Order> {
  const response = await apiClient.post<ApiEnvelope<Order>>('/orders', payload);
  return response.data.data;
}

export async function updateOrderStatus(id: number, status: string): Promise<void> {
  await apiClient.patch(`/orders/${id}/status`, { status });
}

export async function getFifoLots(variantId?: number): Promise<FifoLot[]> {
  const response = await apiClient.get<ApiEnvelope<FifoLot[]>>('/inventory/fifo-lots', { params: { variantId } });
  return response.data.data;
}

export async function updateStoreVariantDailyPrice(storeId: number, variantId: number, currentDailyPrice: number): Promise<void> {
  await apiClient.put(`/stores/${storeId}/variants/${variantId}/daily-price`, { currentDailyPrice });
}

export async function updateFifoLotStagePrices(
  variantId: number,
  lotId: number,
  payload: {
    stage1Price: number;
    stage2Price: number;
    stage3Price: number;
    stage4Price: number;
    stage5Price: number;
    vipPrice?: number;
  }
): Promise<void> {
  await apiClient.put(`/products/variants/${variantId}/lots/${lotId}/stage-prices`, payload);
}

export async function getWarehouseInventories(warehouseId?: number, variantId?: number): Promise<WarehouseInventory[]> {
  const response = await apiClient.get<WarehouseInventory[]>('/inventory', {
    params: { warehouseId, variantId },
  });
  return response.data;
}

export async function getPricingStageConfigs(variantId: number): Promise<PricingStageConfig[]> {
  const response = await apiClient.get<ApiEnvelope<PricingStageConfig[]>>(`/products/variants/${variantId}/stage-configs`);
  return response.data.data;
}

export async function savePricingStageConfigs(variantId: number, configs: PricingStageConfig[]): Promise<void> {
  await apiClient.post(`/products/variants/${variantId}/stage-configs`, { configs });
}



