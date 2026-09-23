import { ArrowRightLeft, Boxes, Eye, Filter, Layers, PackagePlus, Play, Plus, RefreshCw, ShoppingCart, Sliders, Trash2 } from 'lucide-react';
import { useEffect, useState } from 'react';
import {
  createInventoryTransfer,
  createOrder,
  createProduct,
  createStockOrder,
  createVariant,
  deleteProduct,
  deleteVariant,
  getAdminProducts,
  getFifoLots,
  getInventoryTransfers,
  getOrders,
  getStockOrders,
  receiveStockOrder,
  runPriceStageCheck,
  saveVariantStageConfigs,
  updateFifoLotStagePrices,
  updateInventoryTransferStatus,
  updateOrderStatus,
  updateProduct,
  updateVariant,
} from '../api/admin';
import { getStores, getSuppliers, getWarehouses } from '../api/catalog';
import type { FifoLot, InventoryTransfer, Order, PriceStageResult, Product, StockOrder, Supplier, Warehouse } from '../types/domain';
import {
  CreateOrderModal,
  FifoLotStagePricesModal,
  ProductModal,
  StockOrderDetailModal,
  StockOrderModal,
  VariantModal,
  VariantStageConfigModal,
} from './AdminModals';

type AdminTab = 'orders' | 'products' | 'receiving' | 'fifolots' | 'pricing' | 'transfers';

interface AdminInventoryPageProps {
  initialTab?: AdminTab;
}

export function AdminInventoryPage({ initialTab = 'orders' }: AdminInventoryPageProps) {
  const [tab, setTab] = useState<AdminTab>(initialTab);
  const [orders, setOrders] = useState<Order[]>([]);
  const [products, setProducts] = useState<Product[]>([]);
  const [stockOrders, setStockOrders] = useState<StockOrder[]>([]);
  const [fifoLots, setFifoLots] = useState<FifoLot[]>([]);
  const [warehouses, setWarehouses] = useState<Warehouse[]>([]);
  const [stores, setStores] = useState<{ id: number; name: string }[]>([]);
  const [suppliers, setSuppliers] = useState<Supplier[]>([]);
  const [transfers, setTransfers] = useState<InventoryTransfer[]>([]);
  const [priceResults, setPriceResults] = useState<PriceStageResult[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [message, setMessage] = useState<string | null>(null);

  // Filter States
  const [fifoWarehouseFilter, setFifoWarehouseFilter] = useState<string>('ALL');
  const [poWarehouseFilter, setPoWarehouseFilter] = useState<string>('ALL');
  const [productWarehouseFilter, setProductWarehouseFilter] = useState<string>('ALL');
    const [orderStoreFilter, setOrderStoreFilter] = useState<string>('ALL');
  const [posStoreId, setPosStoreId] = useState(1);

  // Modal Visibility States
  const [isOrderModalOpen, setIsOrderModalOpen] = useState(false);
  const [isStockOrderModalOpen, setIsStockOrderModalOpen] = useState(false);
  const [isProductModalOpen, setIsProductModalOpen] = useState(false);
  const [selectedProductForEdit, setSelectedProductForEdit] = useState<Product | null>(null);
  const [variantModalTarget, setVariantModalTarget] = useState<{ productId: number; sku: string; variant?: Product['variants'][number] } | null>(null);
  const [selectedStockOrderForDetail, setSelectedStockOrderForDetail] = useState<StockOrder | null>(null);
  const [selectedLotForPricing, setSelectedLotForPricing] = useState<FifoLot | null>(null);
  const [selectedVariantForStageConfig, setSelectedVariantForStageConfig] = useState<{ variantId: number; sku: string; name: string } | null>(null);

  useEffect(() => {
    void loadData();
  }, [productWarehouseFilter, orderStoreFilter]);

  async function loadData(): Promise<boolean> {
    setIsLoading(true);
    setMessage(null);
    try {
      const results = await Promise.allSettled([
        getOrders(undefined, orderStoreFilter === 'ALL' ? undefined : Number(orderStoreFilter)),
        getAdminProducts(productWarehouseFilter === 'ALL' ? undefined : Number(productWarehouseFilter)),
        getStockOrders(),
        getFifoLots(),
        getWarehouses(),
        getStores(),
        getSuppliers(),
        getInventoryTransfers(),
      ]);

      const [orderRes, prodRes, stockRes, fifoRes, whRes, storeRes, suppRes, transRes] = results;
      const failures: string[] = [];

      if (orderRes.status === 'fulfilled') setOrders(orderRes.value);
      else failures.push('Đơn hàng');
      if (prodRes.status === 'fulfilled') setProducts(prodRes.value);
      else failures.push('Sản phẩm');
      if (stockRes.status === 'fulfilled') setStockOrders(stockRes.value);
      else failures.push('Đơn nhập PO');
      if (fifoRes.status === 'fulfilled') setFifoLots(fifoRes.value);
      else failures.push('Lô FIFO');
      if (whRes.status === 'fulfilled') setWarehouses(whRes.value);
      else failures.push('Kho hàng');
      if (storeRes.status === 'fulfilled') setStores(storeRes.value);
      else failures.push('Cửa hàng');
      if (suppRes.status === 'fulfilled') setSuppliers(suppRes.value);
      else failures.push('Nhà cung cấp');
      if (transRes.status === 'fulfilled') setTransfers(transRes.value);
      else failures.push('Chuyển kho');

      if (failures.length > 0) {
        setMessage(`Chưa tải được: ${failures.join(', ')}.`);
      }
      return failures.length === 0;
    } finally {
      setIsLoading(false);
    }
  }

  // --- ACTIONS ---
  async function runAdminAction(action: () => Promise<void>, successMessage: string, failureMessage: string): Promise<void> {
    setIsLoading(true);
    setMessage(null);
    try {
      await action();
      const refreshed = await loadData();
      if (refreshed) setMessage(successMessage);
    } catch {
      setMessage(failureMessage);
    } finally {
      setIsLoading(false);
    }
  }

  async function handleReceive(order: StockOrder) {
    setIsLoading(true);
    setMessage(null);
    try {
      await receiveStockOrder(order);
      const refreshed = await loadData();
      if (refreshed) setMessage(`Đã nhập kho thành công cho PO ${order.poNumber}.`);
    } catch {
      setMessage(`Không thể nhập kho cho ${order.poNumber}.`);
    } finally {
      setIsLoading(false);
    }
  }

  async function handlePriceCheck() {
    setIsLoading(true);
    setMessage(null);
    try {
      setPriceResults(await runPriceStageCheck());
      await loadData();
      setMessage('Đã chạy kiểm tra và tự động cập nhật stage giá 5-stage.');
    } catch {
      setMessage('Không thể chạy engine chuyển stage giá.');
    } finally {
      setIsLoading(false);
    }
  }

  const tabs = [
    { id: 'orders' as const, label: 'Đặt hàng (Orders)', icon: ShoppingCart },
    { id: 'receiving' as const, label: 'Nhận hàng (PO)', icon: PackagePlus },
    { id: 'products' as const, label: 'Sản phẩm & Biến thể', icon: Boxes },
    { id: 'fifolots' as const, label: 'Lô hàng FIFO & Stage', icon: Layers },
    { id: 'pricing' as const, label: 'Engine định giá', icon: Play },
    { id: 'transfers' as const, label: 'Chuyển kho', icon: ArrowRightLeft },
  ];

  // Filtering Logic
  const filteredFifoLots = fifoLots.filter((lot) => {
    if (fifoWarehouseFilter === 'ALL') return true;
    return lot.warehouseId.toString() === fifoWarehouseFilter;
  });

  const filteredStockOrders = stockOrders.filter((po) => {
    if (poWarehouseFilter === 'ALL') return true;
    return po.destinationWarehouseId?.toString() === poWarehouseFilter;
  });

  return (
    <section className="admin-page" aria-labelledby="admin-title">
      <div className="admin-heading">
        <div>
          <p className="eyebrow">Admin operations</p>
          <h1 id="admin-title">Quản trị Hệ Thống Bán Hàng</h1>
          <p>Quản lý Đặt hàng, Nhận hàng Container PO, Sản phẩm/Biến thể, Lô FIFO và Engine Giá 5-Stage.</p>
        </div>
        <button className="outline-action" type="button" onClick={() => void loadData()} disabled={isLoading}>
          <RefreshCw className="h-4 w-4" /> Làm mới
        </button>
      </div>

      <div className="admin-tabs" aria-label="Phân hệ quản trị">
        {tabs.map(({ id, label, icon: Icon }) => (
          <button
            type="button"
            key={id}
            aria-pressed={tab === id}
            className={tab === id ? 'admin-tab active' : 'admin-tab'}
            onClick={() => setTab(id)}
          >
            <Icon className="h-4 w-4" /> {label}
          </button>
        ))}
      </div>

      {message && <div className="alert alert-info" role="status">{message}</div>}

      {/* 1. ORDERS TAB */}
      {tab === 'orders' && (
        <div className="admin-panel">
          <div className="panel-heading">
            <div>
              <p className="eyebrow">Order management</p>
              <h2>Danh sách Đặt hàng (POS & Online)</h2>
            </div>
            <div style={{ display: 'flex', gap: '10px', alignItems: 'center' }}>
              <select className="field-control" style={{ width: 'auto', padding: '6px 10px' }} value={orderStoreFilter} onChange={(e) => setOrderStoreFilter(e.target.value)}>
                <option value="ALL">Tất cả cửa hàng</option>
                {stores.map((store) => <option key={store.id} value={store.id}>{store.name}</option>)}
              </select>
              <button className="primary-action" type="button" onClick={() => setIsOrderModalOpen(true)}>
                <Plus className="h-4 w-4" /> Tạo Đơn Mới (Modal)
              </button>
            </div>
          </div>
          <div className="data-table-wrap">
            <table className="data-table">
              <thead>
                <tr>
                  <th>Mã đơn hàng</th>
                  <th>Loại đơn / Nhận hàng</th>
                  <th>Tổng tiền (AUD)</th>
                  <th>Trạng thái đơn</th>
                  <th>Thanh toán</th>
                  <th>Thao tác</th>
                </tr>
              </thead>
              <tbody>
                {orders.map((ord) => (
                  <tr key={ord.id}>
                    <td>
                      <strong>{ord.orderCode ?? ord.orderNumber}</strong>
                      <span className="table-subtitle">{ord.customerName ?? 'Khách vãng lai'} · {ord.storeName ?? 'Chi nhánh Perth'}</span>
                    </td>
                    <td>
                      <span>{ord.orderChannel}</span>
                      <span className="table-subtitle">{ord.orderType}</span>
                    </td>
                    <td>
                      <strong>${ord.totalAud?.toFixed(2)}</strong>
                    </td>
                    <td>
                      <span className={`status-pill ${ord.status === 'COMPLETED' ? 'health-online' : ''}`}>{ord.status}</span>
                    </td>
                    <td>
                      <span className="table-subtitle">{ord.paymentStatus}</span>
                    </td>
                    <td className="table-actions">
                      {ord.status === 'DRAFT' && (
                        <button
                          type="button"
                          className="text-action"
                          disabled={isLoading}
                          onClick={() => void runAdminAction(() => updateOrderStatus(ord.id, 'CONFIRMED'), 'Đã duyệt đơn hàng.', 'Không thể duyệt đơn.')}
                        >
                          Duyệt đơn
                        </button>
                      )}
                      {ord.status === 'CONFIRMED' && (
                        <button
                          type="button"
                          className="text-action"
                          disabled={isLoading}
                          onClick={() => void runAdminAction(() => updateOrderStatus(ord.id, 'COMPLETED'), 'Đã hoàn tất đơn hàng & chốt lợi nhuận FIFO.', 'Không thể hoàn tất đơn.')}
                        >
                          Hoàn tất (Complete)
                        </button>
                      )}
                      {ord.status !== 'CANCELLED' && ord.status !== 'COMPLETED' && (
                        <button
                          type="button"
                          className="text-action danger-action"
                          disabled={isLoading}
                          onClick={() => void runAdminAction(() => updateOrderStatus(ord.id, 'CANCELLED'), 'Đã hủy đơn hàng.', 'Không thể hủy đơn.')}
                        >
                          Hủy đơn
                        </button>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
            {orders.length === 0 && <div className="empty-state">Chưa có đơn hàng nào được tạo. Bấm nút "Tạo Đơn Mới" để bắt đầu.</div>}
          </div>
        </div>
      )}

      {/* 2. RECEIVING TAB (PO) */}
      {tab === 'receiving' && (
        <div className="admin-panel">
          <div className="panel-heading">
            <div>
              <p className="eyebrow">Inbound Container PO</p>
              <h2>Quản lý Nhận hàng (Stock Orders)</h2>
            </div>
            <div style={{ display: 'flex', gap: '10px', alignItems: 'center' }}>
              <div style={{ display: 'flex', alignItems: 'center', gap: '6px' }}>
                <Filter className="h-4 w-4 text-gray-500" />
                <select
                  className="field-control"
                  style={{ width: 'auto', padding: '6px 10px' }}
                  value={poWarehouseFilter}
                  onChange={(e) => setPoWarehouseFilter(e.target.value)}
                >
                  <option value="ALL">Tất cả kho nhận</option>
                  {warehouses.map((w) => (
                    <option key={w.id} value={w.id.toString()}>
                      Kho: {w.name}
                    </option>
                  ))}
                </select>
              </div>
              <button className="primary-action" type="button" onClick={() => setIsStockOrderModalOpen(true)}>
                <Plus className="h-4 w-4" /> Tạo PO Nhập Hàng (Modal)
              </button>
            </div>
          </div>
          <div className="data-table-wrap">
            <table className="data-table">
              <thead>
                <tr>
                  <th>Số PO</th>
                  <th>Nhà cung cấp & Kho nhận</th>
                  <th>Ngày ETA</th>
                  <th>Tổng CBM & Cước Container</th>
                  <th>Trạng thái</th>
                  <th>Thao tác</th>
                </tr>
              </thead>
              <tbody>
                {filteredStockOrders.map((order) => (
                  <tr key={order.id}>
                    <td>
                      <strong>{order.poNumber}</strong>
                    </td>
                    <td>
                      {order.supplierName}
                      <span className="table-subtitle">Kho đích: <strong>{order.warehouseName}</strong></span>
                    </td>
                    <td>{order.etaDate?.slice(0, 10)}</td>
                    <td>
                      {order.totalCbm} CBM · {order.items?.length ?? 1} mặt hàng
                      <span className="table-subtitle">Cước: ${order.containerFreightAud} AUD</span>
                    </td>
                    <td>
                      <span className={`status-pill ${order.status === 'RECEIVED' ? 'health-online' : ''}`}>{order.status}</span>
                    </td>
                    <td className="table-actions">
                      <button
                        type="button"
                        className="text-action"
                        onClick={() => setSelectedStockOrderForDetail(order)}
                        style={{ marginRight: '8px' }}
                      >
                        <Eye className="h-4 w-4" /> Chi tiết PO
                      </button>
                      {order.status !== 'RECEIVED' ? (
                        <button type="button" className="text-action" onClick={() => void handleReceive(order)} disabled={isLoading}>
                          Nhận container
                        </button>
                      ) : (
                        <span className="table-subtitle">Đã nhập kho</span>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
            {filteredStockOrders.length === 0 && <div className="empty-state">Không có Stock Order nào phù hợp với bộ lọc kho.</div>}
          </div>
        </div>
      )}

      {/* 3. PRODUCTS & VARIANTS TAB */}
      {tab === 'products' && (
        <div className="admin-panel">
          <div className="panel-heading">
            <div>
              <p className="eyebrow">Product Master</p>
              <h2>Danh mục Sản phẩm & Biến thể</h2>
            </div>
            <div style={{ display: 'flex', gap: '10px', alignItems: 'center' }}>
              <div style={{ display: 'flex', alignItems: 'center', gap: '6px' }}>
                <Filter className="h-4 w-4 text-gray-500" />
                <select className="field-control" style={{ width: 'auto', padding: '6px 10px' }} value={productWarehouseFilter} onChange={(e) => setProductWarehouseFilter(e.target.value)}>
                  <option value="ALL">Tất cả kho</option>
                  {warehouses.map((warehouse) => <option key={warehouse.id} value={warehouse.id}>{warehouse.name}</option>)}
                </select>
              </div>
              <button className="primary-action" type="button" onClick={() => { setSelectedProductForEdit(null); setIsProductModalOpen(true); }}>
                <Plus className="h-4 w-4" /> Thêm Sản Phẩm (Modal)
              </button>
            </div>
          </div>

          <div className="product-grid" style={{ marginTop: '20px' }}>
            {products.map((prod) => (
              <article key={prod.id} className="product-card">
                <div className="product-art">
                  <Boxes className="h-10 w-10 text-teal-600" />
                </div>
                <div className="product-card-body">
                  <span className="product-category">{prod.categoryName ?? 'Furniture'}</span>
                  <h3>{prod.name}</h3>
                  <p>Mã SKU: <strong>{prod.sku}</strong></p>
                  <div className="product-price-row">
                    <span className="stage-badge">Stage {prod.currentStage}</span>
                    <span>{prod.variants.length} biến thể</span>
                  </div>

                  <div className="table-actions" style={{ marginTop: '14px', paddingTop: '10px', borderTop: '1px solid #edf0ed' }}>
                    <button
                      type="button"
                      className="text-action"
                      onClick={() => {
                        setVariantModalTarget({ productId: prod.id, sku: prod.sku });
                      }}
                    >
                      + Thêm Variant
                    </button>
                    <button type="button" className="text-action" onClick={() => { setSelectedProductForEdit(prod); setIsProductModalOpen(true); }}>
                      Chỉnh sửa sản phẩm
                    </button>
                    <button
                      type="button"
                      className="text-action danger-action"
                      onClick={() => {
                        if (window.confirm(`Bạn có chắc chắn muốn ẩn/xóa sản phẩm ${prod.name}?`)) {
                          void runAdminAction(() => deleteProduct(prod.id), 'Đã ẩn sản phẩm.', 'Không thể ẩn sản phẩm.');
                        }
                      }}
                    >
                      Xóa sản phẩm
                    </button>
                  </div>

                  {prod.variants.length > 0 && (
                    <div style={{ marginTop: '12px', fontSize: '11px', background: '#f9fbf9', padding: '8px', borderRadius: '6px' }}>
                      <strong style={{ display: 'block', color: 'var(--ink-muted)', marginBottom: '4px' }}>Các biến thể & Tồn kho:</strong>
                      {prod.variants.map((v) => (
                        <div key={v.variantId} style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', padding: '5px 0', borderBottom: '1px dashed #e0e0e0' }}>
                          <div>
                            <strong>{v.sku}</strong>
                            <span style={{ display: 'block', color: 'var(--ink-muted)' }}>{v.name} · Tồn: {v.stock?.available ?? 0}</span>
                          </div>
                          <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                            <span>${v.currentPriceAud}</span>
                            <button
                              type="button"
                              className="text-action"
                              style={{ color: 'var(--teal)', fontWeight: 600 }}
                              title="Cài đặt ngưỡng % tồn kho & ngày chuyển stage"
                              onClick={() => setSelectedVariantForStageConfig({ variantId: v.variantId, sku: v.sku, name: v.name })}
                            >
                              ⚙️ Cài Stage (%)
                            </button>
                            <button type="button" className="text-action" onClick={() => setVariantModalTarget({ productId: prod.id, sku: prod.sku, variant: v })}>Sửa</button>
                            <button
                              type="button"
                              className="icon-button"
                              style={{ color: 'var(--coral)', padding: '2px' }}
                              title="Xóa biến thể này"
                              onClick={() => {
                                if (window.confirm(`Bạn có chắc chắn muốn xóa/ẩn biến thể ${v.sku}?`)) {
                                  void runAdminAction(() => deleteVariant(v.variantId), 'Đã xóa biến thể thành công.', 'Không thể xóa biến thể.');
                                }
                              }}
                            >
                              <Trash2 className="h-3.5 w-3.5" />
                            </button>
                          </div>
                        </div>
                      ))}
                    </div>
                  )}
                </div>
              </article>
            ))}
          </div>
          {products.length === 0 && <div className="empty-state">Chưa có sản phẩm nào. Bấm nút "Thêm Sản Phẩm" để tạo.</div>}
        </div>
      )}

      {/* 4. FIFO LOTS TAB */}
      {tab === 'fifolots' && (
        <div className="admin-panel">
          <div className="panel-heading">
            <div>
              <p className="eyebrow">FIFO Engine Inventory</p>
              <h2>Lô Hàng FIFO & Cài Đặt Giá 5 Stage</h2>
            </div>
            <div style={{ display: 'flex', alignItems: 'center', gap: '6px' }}>
              <Filter className="h-4 w-4 text-gray-500" />
              <select
                className="field-control"
                style={{ width: 'auto', padding: '6px 10px' }}
                value={fifoWarehouseFilter}
                onChange={(e) => setFifoWarehouseFilter(e.target.value)}
              >
                <option value="ALL">Tất cả các kho lưu giữ</option>
                {warehouses.map((w) => (
                  <option key={w.id} value={w.id.toString()}>
                    Kho: {w.name}
                  </option>
                ))}
              </select>
            </div>
          </div>
          <div className="data-table-wrap">
            <table className="data-table">
              <thead>
                <tr>
                  <th>Mã Lô FIFO</th>
                  <th>Mặt hàng (SKU)</th>
                  <th>Kho lưu giữ</th>
                  <th>SL Ban đầu / Còn lại</th>
                  <th>Giá Vốn Landed (AUD)</th>
                  <th>Stage Hiện tại</th>
                  <th>Thao tác</th>
                </tr>
              </thead>
              <tbody>
                {filteredFifoLots.map((lot) => (
                  <tr key={lot.id}>
                    <td>
                      <strong>{lot.lotNumber}</strong>
                      <span className="table-subtitle">Ngày nhập: {lot.receivedDate?.slice(0, 10)}</span>
                    </td>
                    <td>
                      <strong>{lot.variantSku ?? 'SKU'}</strong>
                      <span className="table-subtitle">{lot.variantName}</span>
                    </td>
                    <td><strong>{lot.warehouseName ?? 'Kho Perth'}</strong></td>
                    <td>
                      <strong>{lot.remainingQuantity}</strong> / {lot.initialQuantity} SP
                    </td>
                    <td>
                      <strong style={{ color: 'var(--teal)' }}>${lot.unitLandedCostAud?.toFixed(2)} AUD</strong>
                    </td>
                    <td>
                      <span className="stage-badge">Stage {lot.currentStage}</span>
                    </td>
                    <td className="table-actions">
                      <button
                        type="button"
                        className="text-action"
                        onClick={() => setSelectedLotForPricing(lot)}
                      >
                        <Sliders className="h-4 w-4" /> Cài đặt giá 5-Stage
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
            {filteredFifoLots.length === 0 && <div className="empty-state">Không có lô FIFO nào phù hợp với bộ lọc kho.</div>}
          </div>
        </div>
      )}

      {/* 5. PRICING ENGINE TAB */}
      {tab === 'pricing' && (
        <div className="admin-panel">
          <div className="panel-heading">
            <div>
              <p className="eyebrow">Pricing Automation</p>
              <h2>Engine Chuyển Giá 5 Stage Tự Động</h2>
              <p className="panel-copy">Quét tồn kho &amp; số ngày lưu kho để tự động nâng stage giá (Stage 1 sang Stage 5) theo quy định.</p>
            </div>
            <button type="button" className="primary-action compact-action" onClick={() => void handlePriceCheck()} disabled={isLoading}>
              <Play className="h-4 w-4" /> {isLoading ? 'Đang quét...' : 'Chạy Engine Giá'}
            </button>
          </div>
          <div className="price-results">
            {priceResults.map((res) => (
              <div className="operation-row" key={`${res.variantId}-${res.fifoLotId}`}>
                <div>
                  <strong>{res.sku}</strong>
                  <span>{res.reason}</span>
                </div>
                <div>
                  <span className="stage-badge">Stage {res.oldStage} → Stage {res.newStage}</span>
                  <strong>${res.newPriceAud?.toFixed(2)}</strong>
                </div>
              </div>
            ))}
            {priceResults.length === 0 && <div className="empty-state">Bấm nút "Chạy Engine Giá" để kích hoạt thuật toán chuyển stage giá tự động.</div>}
          </div>
        </div>
      )}

      {/* 6. TRANSFERS TAB */}
      {tab === 'transfers' && (
        <TransferPanel
          products={products}
          warehouses={warehouses}
          transfers={transfers}
          onCreate={async (from, to, varId, qty) => {
            await createInventoryTransfer({ fromWarehouseId: from, toWarehouseId: to, items: [{ variantId: varId, quantityRequested: qty }] });
            await loadData();
          }}
          onStatus={async (id, status) => {
            await updateInventoryTransferStatus(id, status);
            await loadData();
          }}
        />
      )}

      {/* --- MODALS RENDER --- */}
      <CreateOrderModal
        isOpen={isOrderModalOpen}
        onClose={() => setIsOrderModalOpen(false)}
        products={products}
        stores={stores}
        selectedStoreId={posStoreId}
        onStoreChange={setPosStoreId}
        isLoading={isLoading}
        onSubmit={async (dto) => {
          await runAdminAction(async () => { await createOrder(dto); }, 'Tạo đơn hàng thành công!', 'Không thể tạo đơn hàng.');
        }}
      />

      <StockOrderModal
        isOpen={isStockOrderModalOpen}
        onClose={() => setIsStockOrderModalOpen(false)}
        suppliers={suppliers}
        warehouses={warehouses}
        products={products}
        isLoading={isLoading}
        onSubmit={async (dto) => {
          await runAdminAction(async () => { await createStockOrder(dto); }, 'Tạo đơn PO thành công!', 'Không thể tạo PO.');
        }}
      />

      <StockOrderDetailModal
        isOpen={Boolean(selectedStockOrderForDetail)}
        onClose={() => setSelectedStockOrderForDetail(null)}
        order={selectedStockOrderForDetail}
      />

      <FifoLotStagePricesModal
        isOpen={Boolean(selectedLotForPricing)}
        onClose={() => setSelectedLotForPricing(null)}
        lot={selectedLotForPricing}
        isLoading={isLoading}
        onSaveStageConfigs={saveVariantStageConfigs}
        onSubmit={async (vId, lId, prices) => {
          await runAdminAction(
            async () => { await updateFifoLotStagePrices(vId, lId, prices); },
            'Đã cập nhật bảng giá 5-stage cho lô thành công!',
            'Không thể cập nhật giá stage.'
          );
        }}
      />

      <ProductModal
        isOpen={isProductModalOpen}
        onClose={() => setIsProductModalOpen(false)}
        initialData={selectedProductForEdit}
        isLoading={isLoading}
        onSubmit={async (dto) => {
          if (selectedProductForEdit) {
            await runAdminAction(() => updateProduct(selectedProductForEdit.id, { ...dto, isActive: true }), 'Đã cập nhật sản phẩm.', 'Không thể cập nhật sản phẩm.');
          } else {
            await runAdminAction(() => createProduct(dto), 'Đã tạo sản phẩm thành công.', 'Không thể tạo sản phẩm.');
          }
        }}
      />

      {variantModalTarget && (
        <VariantModal
          isOpen={Boolean(variantModalTarget)}
          onClose={() => setVariantModalTarget(null)}
          productId={variantModalTarget.productId}
          productSku={variantModalTarget.sku}
          initialData={variantModalTarget.variant}
          isLoading={isLoading}
          onSubmit={async (pId, dto, variantId) => {
            await runAdminAction(() => variantId ? updateVariant(variantId, dto) : createVariant(pId, dto), variantId ? 'Đã cập nhật biến thể.' : 'Đã tạo biến thể thành công.', 'Không thể lưu biến thể.');
          }}
        />
      )}

      {selectedVariantForStageConfig && (
        <VariantStageConfigModal
          isOpen={Boolean(selectedVariantForStageConfig)}
          onClose={() => setSelectedVariantForStageConfig(null)}
          variantId={selectedVariantForStageConfig.variantId}
          variantSku={selectedVariantForStageConfig.sku}
          variantName={selectedVariantForStageConfig.name}
          isLoading={isLoading}
          onSubmit={async (vId, configs) => {
            await runAdminAction(
              async () => { await saveVariantStageConfigs(vId, configs); },
              'Đã lưu cấu hình ngưỡng % tồn kho và số ngày chuyển stage cho biến thể!',
              'Không thể lưu cấu hình stage.'
            );
          }}
        />
      )}
    </section>
  );
}

function TransferPanel({
  products,
  warehouses,
  transfers,
  onCreate,
  onStatus,
}: {
  products: Product[];
  warehouses: Warehouse[];
  transfers: InventoryTransfer[];
  onCreate: (from: number, to: number, variant: number, quantity: number) => Promise<void>;
  onStatus: (id: number, status: string) => Promise<void>;
}) {
  const [fromWarehouse, setFromWarehouse] = useState('');
  const [toWarehouse, setToWarehouse] = useState('');
  const [variantId, setVariantId] = useState('');
  const [quantity, setQuantity] = useState('1');

  return (
    <div className="admin-grid">
      <div className="admin-panel form-panel">
        <ArrowRightLeft className="h-8 w-8 text-teal-600" />
        <p className="eyebrow">Warehouse transfers</p>
        <h2>Tạo phiếu chuyển kho</h2>
        <label className="field-label" htmlFor="transfer-from">Kho xuất</label>
        <select id="transfer-from" className="field-control" value={fromWarehouse} onChange={(e) => setFromWarehouse(e.target.value)}>
          <option value="">Chọn kho xuất</option>
          {warehouses.map((w) => (
            <option key={w.id} value={w.id}>{w.name}</option>
          ))}
        </select>
        <label className="field-label" htmlFor="transfer-to">Kho nhận</label>
        <select id="transfer-to" className="field-control" value={toWarehouse} onChange={(e) => setToWarehouse(e.target.value)}>
          <option value="">Chọn kho nhận</option>
          {warehouses.map((w) => (
            <option key={w.id} value={w.id}>{w.name}</option>
          ))}
        </select>
        <label className="field-label" htmlFor="transfer-variant">Sản phẩm</label>
        <select id="transfer-variant" className="field-control" value={variantId} onChange={(e) => setVariantId(e.target.value)}>
          <option value="">Chọn variant</option>
          {products.flatMap((p) => p.variants.map((v) => (
            <option key={v.variantId} value={v.variantId}>{v.sku} · {v.name}</option>
          )))}
        </select>
        <label className="field-label" htmlFor="transfer-quantity">Số lượng</label>
        <input id="transfer-quantity" className="field-control" type="number" min="1" value={quantity} onChange={(e) => setQuantity(e.target.value)} />
        <button
          type="button"
          className="primary-action"
          disabled={!fromWarehouse || !toWarehouse || !variantId || Number(quantity) <= 0 || fromWarehouse === toWarehouse}
          onClick={() => void onCreate(Number(fromWarehouse), Number(toWarehouse), Number(variantId), Number(quantity))}
        >
          Tạo phiếu chuyển
        </button>
      </div>

      <div className="admin-panel">
        <div className="panel-heading">
          <div>
            <p className="eyebrow">Transfer queue</p>
            <h2>Phiếu chuyển đang xử lý</h2>
          </div>
          <span className="count-badge">{transfers.length} phiếu</span>
        </div>
        {transfers.map((t) => (
          <div className="operation-row" key={t.id}>
            <div>
              <strong>{t.transferCode}</strong>
              <span>{t.fromWarehouseName} → {t.toWarehouseName}</span>
            </div>
            <div>
              <span className="status-pill">{t.status}</span>
              {t.status === 'REQUESTED' && (
                <button type="button" className="text-action" onClick={() => void onStatus(t.id, 'IN_TRANSIT')}>Xuất kho</button>
              )}
              {t.status === 'IN_TRANSIT' && (
                <button type="button" className="text-action" onClick={() => void onStatus(t.id, 'RECEIVED')}>Xác nhận nhận</button>
              )}
            </div>
          </div>
        ))}
        {transfers.length === 0 && <div className="empty-state">Chưa có phiếu chuyển kho.</div>}
      </div>
    </div>
  );
}
