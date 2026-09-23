import { apiClient } from './client';
import type { PostcodeLookup, Product, Store, StoreVariantPrice, Supplier, Warehouse } from '../types/domain';

interface ApiEnvelope<T> {
  data: T;
}

export async function lookupPostcode(postcode: string): Promise<PostcodeLookup> {
  const response = await apiClient.get<ApiEnvelope<PostcodeLookup>>('/postcodes/lookup', {
    params: { code: postcode },
  });
  return response.data.data;
}

export async function selectStore(postcode: string, storeId: number): Promise<PostcodeLookup> {
  const response = await apiClient.post<ApiEnvelope<PostcodeLookup>>('/postcodes/select-store', {
    postcode,
    storeId,
  });
  return response.data.data;
}

export async function getProducts(storeId?: number, warehouseId?: number): Promise<Product[]> {
  const params = {
    ...(storeId ? { storeId } : {}),
    ...(warehouseId ? { warehouseId } : {}),
  };
  const response = await apiClient.get<ApiEnvelope<Product[]>>('/products', {
    params: Object.keys(params).length > 0 ? params : undefined,
  });
  return response.data.data;
}

export async function getStoreVariantPrices(storeId: number): Promise<StoreVariantPrice[]> {
  const response = await apiClient.get<ApiEnvelope<StoreVariantPrice[]>>(`/stores/${storeId}/variant-prices`);
  return response.data.data;
}

export async function getStores(): Promise<Store[]> {
  const response = await apiClient.get<ApiEnvelope<Store[]>>('/stores');
  return response.data.data;
}

export async function getWarehouses(): Promise<Warehouse[]> {
  const response = await apiClient.get<ApiEnvelope<Warehouse[]>>('/warehouses');
  return response.data.data;
}

export async function getSuppliers(): Promise<Supplier[]> {
  const response = await apiClient.get<ApiEnvelope<Supplier[]>>('/suppliers');
  return response.data.data;
}
