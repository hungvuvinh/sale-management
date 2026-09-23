export interface UserProfile {
  id: string;
  email: string;
  fullName: string;
  role: string;
  storeId?: string;
  isActive: boolean;
}

export interface HealthStatus {
  status: string;
  timestamp: string;
  environment: string;
  databaseConnected: boolean;
}

export interface Store {
  id: number;
  code: string;
  name: string;
  phone: string;
  email: string;
  isActive: boolean;
  address?: {
    suburb: string;
    city: string;
    state: string;
    postcode: string;
  };
}

export interface Warehouse {
  id: number;
  code: string;
  name: string;
  isStore: boolean;
  isActive: boolean;
}

export interface Supplier {
  id: number;
  name: string;
  country: string;
  leadTimeDays: number;
  isActive: boolean;
}

export interface PostcodeLookup {
  postcode: string;
  suburb: string;
  state: string;
  assignedStore?: Store;
  availableStores: Store[];
  priceUnlocked: boolean;
  estimatedShippingRate?: {
    shippingFeeAud: number;
  };
}

export interface ProductVariant {
  variantId: number;
  sku: string;
  name: string;
  weightKg: number;
  cbm: number;
  boxCount: number;
  isActive: boolean;
  currentPriceAud: number;
  vipPriceAud?: number;
  currentStage?: number;
  stagePrices: Record<string, number>;
  stock: {
    onHand: number;
    reserved: number;
    available: number;
  };
}

export interface Product {
  id: number;
  sku: string;
  name: string;
  slug: string;
  categoryName?: string;
  mainImageUrl?: string;
  isActive: boolean;
  currentStage: number;
  variants: ProductVariant[];
}

export interface StoreVariantPrice {
  id: number;
  storeId: number;
  variantId: number;
  currentDailyPrice: number;
  updatedAt: string;
}

export interface StockOrder {
  id: number;
  poNumber: string;
  supplierId?: number;
  supplierName: string;
  destinationWarehouseId: number;
  warehouseName: string;
  containerCode?: string;
  etaDate: string;
  actualArrivalDate?: string;
  status: string;
  totalCbm: number;
  containerFreightAud: number;
  customsTaxAud?: number;
  currencyCode?: string;
  exchangeRate?: number;
  notes?: string;
  createdAt?: string;
  items: Array<{
    id: number;
    stockOrderId?: number;
    variantId: number;
    variantSku: string;
    variantName: string;
    quantityOrdered: number;
    quantityReceived: number;
    unitCostForeign: number;
    unitCostAud?: number;
    unitCbm: number;
    unitFreightAud?: number;
    calculatedLandedCostAud: number;
  }>;
}

export interface InventoryTransfer {
  id: number;
  transferCode: string;
  fromWarehouseId: number;
  fromWarehouseName: string;
  toWarehouseId: number;
  toWarehouseName: string;
  status: string;
  createdAt: string;
  receivedAt?: string;
  items: Array<{
    id: number;
    variantId: number;
    variantSku: string;
    variantName: string;
    quantityRequested: number;
    quantityReceived: number;
  }>;
}

export interface PriceStageResult {
  variantId: number;
  sku: string;
  fifoLotId: number;
  stageChanged: boolean;
  oldStage: number;
  newStage: number;
  oldPriceAud: number;
  newPriceAud: number;
  reason: string;
}

export interface OrderItem {
  id: number;
  orderId: number;
  variantId: number;
  variantSku?: string;
  variantName?: string;
  fifoLotId?: number;
  quantity: number;
  unitPriceAud: number;
  totalPriceAud: number;
  itemStatus: string;
}

export interface Order {
  id: number;
  orderNumber: string;
  orderCode?: string;
  orderChannel?: string;
  storeId: number;
  storeName?: string;
  customerId?: number;
  customerName?: string;
  orderType: string;
  deliveryType: string;
  status: string;
  paymentStatus: string;
  subtotalAud: number;
  deliveryFeeAud: number;
  taxAud: number;
  totalAud: number;
  shippingAddressText?: string;
  deliveryDate?: string;
  createdAt?: string;
  notes?: string;
  items: OrderItem[];
}

export interface WarehouseInventory {
  warehouseId: number;
  warehouseName: string;
  variantId: number;
  variantSku: string;
  variantName: string;
  onHandQuantity: number;
  reservedQuantity: number;
  availableQuantity: number;
  lowStockThreshold: number;
  isLowStock: boolean;
}

export interface FifoLot {
  id: number;
  lotNumber: string;
  variantId: number;
  variantSku?: string;
  variantName?: string;
  warehouseId: number;
  warehouseName?: string;
  initialQuantity: number;
  remainingQuantity: number;
  unitLandedCostAud: number;
  currentStage: number;
  receivedDate: string;
  status: string;
  stagePrices?: Record<string | number, number>;
}

export interface PricingStageConfig {
  id?: number;
  variantId?: number;
  fromStage: number;
  toStage: number;
  targetStockPct: number;
  minDaysAtStage: number;
  maxDaysAtStage: number;
}



