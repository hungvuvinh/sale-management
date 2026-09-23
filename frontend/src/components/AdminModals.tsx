import { Image, Plus, Trash2, X } from 'lucide-react';
import { useState } from 'react';
import type { FifoLot, PricingStageConfig, Product, ProductVariant, StockOrder, Supplier, Warehouse } from '../types/domain';

interface ModalBaseProps {
  isOpen: boolean;
  onClose: () => void;
  title: string;
  maxWidth?: string;
  children: React.ReactNode;
}

export function ModalBase({ isOpen, onClose, title, maxWidth = '580px', children }: ModalBaseProps) {
  if (!isOpen) return null;

  return (
    <div className="modal-backdrop" onClick={onClose} role="dialog" aria-modal="true">
      <div className="modal-container" style={{ maxWidth }} onClick={(e) => e.stopPropagation()}>
        <div className="modal-header">
          <h3>{title}</h3>
          <button type="button" className="icon-button" onClick={onClose} aria-label="Đóng modal">
            <X className="h-5 w-5" />
          </button>
        </div>
        <div className="modal-body">{children}</div>
      </div>
    </div>
  );
}

// 1. PRODUCT MODAL
interface ProductModalProps {
  isOpen: boolean;
  onClose: () => void;
  initialData?: Product | null;
  onSubmit: (data: { sku: string; name: string; slug: string; categoryId?: number; description?: string; isActive: boolean }) => Promise<void>;
  isLoading: boolean;
}

export function ProductModal({ isOpen, onClose, initialData, onSubmit, isLoading }: ProductModalProps) {
  const [sku, setSku] = useState(initialData?.sku ?? '');
  const [name, setName] = useState(initialData?.name ?? '');
  const [slug, setSlug] = useState(initialData?.slug ?? '');
  const [description, setDescription] = useState('');
  const [isActive, setIsActive] = useState(initialData?.isActive ?? true);

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (!sku.trim() || !name.trim()) return;
    const finalSlug = slug.trim() || sku.trim().toLowerCase().replace(/\s+/g, '-');
    await onSubmit({ sku: sku.trim(), name: name.trim(), slug: finalSlug, description: description.trim() || undefined, isActive });
    onClose();
  }

  return (
    <ModalBase isOpen={isOpen} onClose={onClose} title={initialData ? 'Chỉnh sửa Sản Phẩm' : 'Thêm Sản Phẩm Mới'}>
      <form onSubmit={handleSubmit} className="modal-form">
        <div className="image-placeholder-box">
          <Image className="h-10 w-10 text-gray-400" />
          <span>Hình ảnh đại diện sản phẩm (Placeholder)</span>
        </div>

        <div className="form-group">
          <label htmlFor="prod-sku">Mã SKU *</label>
          <input id="prod-sku" className="field-control" required value={sku} onChange={(e) => setSku(e.target.value)} placeholder="VD: SOFA-LUNA" />
        </div>

        <div className="form-group">
          <label htmlFor="prod-name">Tên sản phẩm *</label>
          <input id="prod-name" className="field-control" required value={name} onChange={(e) => setName(e.target.value)} placeholder="VD: Luna 3-Seater Sofa" />
        </div>

        <div className="form-group">
          <label htmlFor="prod-slug">URL Slug</label>
          <input id="prod-slug" className="field-control" value={slug} onChange={(e) => setSlug(e.target.value)} placeholder="Tự động tạo từ SKU/Tên nếu bỏ trống" />
        </div>

        <div className="form-group">
          <label htmlFor="prod-desc">Mô tả sản phẩm</label>
          <textarea id="prod-desc" className="field-control" rows={3} value={description} onChange={(e) => setDescription(e.target.value)} placeholder="Mô tả chi tiết sản phẩm..." />
        </div>

        {initialData && (
          <label className="form-group">
            <span>Trạng thái</span>
            <select className="field-control" value={isActive ? 'active' : 'inactive'} onChange={(e) => setIsActive(e.target.value === 'active')}>
              <option value="active">Đang hoạt động</option>
              <option value="inactive">Tạm ẩn</option>
            </select>
          </label>
        )}

        <div className="modal-footer">
          <button type="button" className="outline-action" onClick={onClose} disabled={isLoading}>
            Hủy
          </button>
          <button type="submit" className="primary-action" disabled={isLoading || !sku.trim() || !name.trim()}>
            {isLoading ? 'Đang xử lý...' : initialData ? 'Lưu cập nhật' : 'Tạo sản phẩm'}
          </button>
        </div>
      </form>
    </ModalBase>
  );
}

// 2. VARIANT MODAL — Lưu ý: Không nhập tồn kho ở đây, số lượng biến thể quản lý qua kho & PO
interface VariantModalProps {
  isOpen: boolean;
  onClose: () => void;
  productId: number;
  productSku: string;
  initialData?: ProductVariant | null;
  onSubmit: (productId: number, data: { sku: string; name: string; weightKg: number; cbm: number; boxCount: number; isActive: boolean }, variantId?: number) => Promise<void>;
  isLoading: boolean;
}

export function VariantModal({ isOpen, onClose, productId, productSku, initialData, onSubmit, isLoading }: VariantModalProps) {
  const [sku, setSku] = useState(initialData?.sku ?? `${productSku}-VAR`);
  const [name, setName] = useState(initialData?.name ?? '');
  const [weightKg, setWeightKg] = useState(initialData?.weightKg?.toString() ?? '10');
  const [cbm, setCbm] = useState(initialData?.cbm?.toString() ?? '0.5');
  const [boxCount, setBoxCount] = useState(initialData?.boxCount?.toString() ?? '1');
  const [isActive, setIsActive] = useState(initialData?.isActive ?? true);

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (!sku.trim() || !name.trim()) return;
    await onSubmit(productId, {
      sku: sku.trim(),
      name: name.trim(),
      weightKg: Number(weightKg) || 1,
      cbm: Number(cbm) || 0.1,
      boxCount: Number(boxCount) || 1,
      isActive,
    }, initialData?.variantId);
    onClose();
  }

  return (
    <ModalBase isOpen={isOpen} onClose={onClose} title={`${initialData ? 'Chỉnh sửa' : 'Thêm'} Biến Thể Cho ${productSku}`}>
      <form onSubmit={handleSubmit} className="modal-form">
        <div className="image-placeholder-box">
          <Image className="h-10 w-10 text-gray-400" />
          <span>Hình ảnh Biến thể (Variant Placeholder)</span>
        </div>

        <div className="form-group">
          <label htmlFor="var-sku">Mã SKU Biến Thể *</label>
          <input id="var-sku" className="field-control" required value={sku} onChange={(e) => setSku(e.target.value)} placeholder="VD: SOFA-LUNA-GREY" />
        </div>

        <div className="form-group">
          <label htmlFor="var-name">Tên Biến Thể *</label>
          <input id="var-name" className="field-control" required value={name} onChange={(e) => setName(e.target.value)} placeholder="VD: Luna Grey Performance Fabric" />
        </div>

        {initialData && (
          <label className="form-group">
            <span>Trạng thái</span>
            <select className="field-control" value={isActive ? 'active' : 'inactive'} onChange={(e) => setIsActive(e.target.value === 'active')}>
              <option value="active">Đang hoạt động</option>
              <option value="inactive">Tạm ẩn</option>
            </select>
          </label>
        )}

        <div className="form-row">
          <div className="form-group">
            <label htmlFor="var-cbm">Thể tích CBM *</label>
            <input id="var-cbm" className="field-control" type="number" step="0.01" min="0.01" required value={cbm} onChange={(e) => setCbm(e.target.value)} />
          </div>
          <div className="form-group">
            <label htmlFor="var-weight">Trọng lượng (kg)</label>
            <input id="var-weight" className="field-control" type="number" step="0.5" min="0.1" required value={weightKg} onChange={(e) => setWeightKg(e.target.value)} />
          </div>
          <div className="form-group">
            <label htmlFor="var-boxes">Số thùng (Box Count)</label>
            <input id="var-boxes" className="field-control" type="number" min="1" required value={boxCount} onChange={(e) => setBoxCount(e.target.value)} />
          </div>
        </div>

        <p style={{ margin: '4px 0 0', fontSize: '11px', color: 'var(--ink-muted)' }}>
          * Lưu ý: Tồn kho biến thể được tự động tích lũy khi nhận hàng (Stock Order / PO) tại kho đích, không nhập thủ công tại đây.
        </p>

        <div className="modal-footer">
          <button type="button" className="outline-action" onClick={onClose} disabled={isLoading}>
            Hủy
          </button>
          <button type="submit" className="primary-action" disabled={isLoading || !sku.trim() || !name.trim()}>
            {isLoading ? 'Đang lưu...' : initialData ? 'Lưu cập nhật' : 'Tạo biến thể'}
          </button>
        </div>
      </form>
    </ModalBase>
  );
}

// 3. STOCK ORDER MODAL — HỖ TRỢ NHIỀU BIẾN THỂ TRÊN 1 ĐƠN NHẬN HÀNG
interface StockOrderItemInput {
  variantId: number;
  quantityOrdered: number;
  unitCostForeign: number;
  unitCbm: number;
}

interface StockOrderModalProps {
  isOpen: boolean;
  onClose: () => void;
  suppliers: Supplier[];
  warehouses: Warehouse[];
  products: Product[];
  onSubmit: (data: {
    poNumber: string;
    supplierId: number;
    destinationWarehouseId: number;
    etaDate: string;
    containerFreightAud: number;
    customsTaxAud: number;
    items: StockOrderItemInput[];
  }) => Promise<void>;
  isLoading: boolean;
}

export function StockOrderModal({ isOpen, onClose, suppliers, warehouses, products, onSubmit, isLoading }: StockOrderModalProps) {
  const [poNumber, setPoNumber] = useState(`PO-${Date.now().toString().slice(-6)}`);
  const [supplierId, setSupplierId] = useState(suppliers[0]?.id?.toString() ?? '');
  const [warehouseId, setWarehouseId] = useState(warehouses[0]?.id?.toString() ?? '');
  const [etaDate, setEtaDate] = useState(new Date(Date.now() + 14 * 86400000).toISOString().slice(0, 10));
  const [containerFreight, setContainerFreight] = useState('1500');
  const [customsTax, setCustomsTax] = useState('300');

  const allVariants = products.flatMap((p) => p.variants);
  const [items, setItems] = useState<StockOrderItemInput[]>([
    {
      variantId: allVariants[0]?.variantId ?? 1,
      quantityOrdered: 10,
      unitCostForeign: 450,
      unitCbm: 1.0,
    },
  ]);

  function handleAddItem() {
    setItems((prev) => [
      ...prev,
      {
        variantId: allVariants[0]?.variantId ?? 1,
        quantityOrdered: 5,
        unitCostForeign: 300,
        unitCbm: 1.0,
      },
    ]);
  }

  function handleRemoveItem(index: number) {
    if (items.length <= 1) return;
    setItems((prev) => prev.filter((_, i) => i !== index));
  }

  function handleItemChange(index: number, field: keyof StockOrderItemInput, value: number) {
    setItems((prev) => {
      const updated = [...prev];
      updated[index] = { ...updated[index], [field]: value };
      return updated;
    });
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (!supplierId || !warehouseId || items.length === 0) return;
    await onSubmit({
      poNumber,
      supplierId: Number(supplierId),
      destinationWarehouseId: Number(warehouseId),
      etaDate,
      containerFreightAud: Number(containerFreight) || 0,
      customsTaxAud: Number(customsTax) || 0,
      items,
    });
    onClose();
  }

  return (
    <ModalBase isOpen={isOpen} onClose={onClose} maxWidth="720px" title="Tạo Đơn Nhập Hàng (Stock Order / PO)">
      <form onSubmit={handleSubmit} className="modal-form">
        <div className="form-group">
          <label htmlFor="po-num">Số PO *</label>
          <input id="po-num" className="field-control" required value={poNumber} onChange={(e) => setPoNumber(e.target.value)} />
        </div>

        <div className="form-row">
          <div className="form-group">
            <label htmlFor="po-supp">Nhà cung cấp *</label>
            <select id="po-supp" className="field-control" required value={supplierId} onChange={(e) => setSupplierId(e.target.value)}>
              <option value="">Chọn nhà cung cấp</option>
              {suppliers.map((s) => (
                <option key={s.id} value={s.id}>
                  {s.name} ({s.country})
                </option>
              ))}
            </select>
          </div>

          <div className="form-group">
            <label htmlFor="po-wh">Kho đích nhận *</label>
            <select id="po-wh" className="field-control" required value={warehouseId} onChange={(e) => setWarehouseId(e.target.value)}>
              <option value="">Chọn kho nhận</option>
              {warehouses.map((w) => (
                <option key={w.id} value={w.id}>
                  {w.name}
                </option>
              ))}
            </select>
          </div>
        </div>

        <div className="form-row">
          <div className="form-group">
            <label htmlFor="po-eta">Ngày ETA dự kiến</label>
            <input id="po-eta" className="field-control" type="date" value={etaDate} onChange={(e) => setEtaDate(e.target.value)} />
          </div>
          <div className="form-group">
            <label htmlFor="po-freight">Phí Container (AUD)</label>
            <input id="po-freight" className="field-control" type="number" value={containerFreight} onChange={(e) => setContainerFreight(e.target.value)} />
          </div>
          <div className="form-group">
            <label htmlFor="po-tax">Thuế hải quan (AUD)</label>
            <input id="po-tax" className="field-control" type="number" value={customsTax} onChange={(e) => setCustomsTax(e.target.value)} />
          </div>
        </div>

        <fieldset className="modal-fieldset">
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <legend style={{ fontWeight: 'bold' }}>Danh sách Mặt Hàng Nhập ({items.length} items)</legend>
            <button type="button" className="text-action" onClick={handleAddItem}>
              <Plus className="h-4 w-4" /> + Thêm mặt hàng
            </button>
          </div>

          {items.map((item, idx) => (
            <div key={idx} style={{ display: 'grid', gridTemplateColumns: '2fr 1fr 1fr 1fr auto', gap: '8px', alignItems: 'end', paddingBottom: '10px', borderBottom: '1px dashed #e0e0e0' }}>
              <div className="form-group">
                <label>Biến thể #{idx + 1} *</label>
                <select
                  className="field-control"
                  required
                  value={item.variantId}
                  onChange={(e) => handleItemChange(idx, 'variantId', Number(e.target.value))}
                >
                  {allVariants.map((v) => (
                    <option key={v.variantId} value={v.variantId}>
                      {v.sku} - {v.name}
                    </option>
                  ))}
                </select>
              </div>

              <div className="form-group">
                <label>Số lượng *</label>
                <input
                  className="field-control"
                  type="number"
                  min="1"
                  required
                  value={item.quantityOrdered}
                  onChange={(e) => handleItemChange(idx, 'quantityOrdered', Number(e.target.value))}
                />
              </div>

              <div className="form-group">
                <label>Giá vốn (USD)</label>
                <input
                  className="field-control"
                  type="number"
                  min="0"
                  required
                  value={item.unitCostForeign}
                  onChange={(e) => handleItemChange(idx, 'unitCostForeign', Number(e.target.value))}
                />
              </div>

              <div className="form-group">
                <label>CBM/unit</label>
                <input
                  className="field-control"
                  type="number"
                  step="0.01"
                  min="0.01"
                  required
                  value={item.unitCbm}
                  onChange={(e) => handleItemChange(idx, 'unitCbm', Number(e.target.value))}
                />
              </div>

              {items.length > 1 && (
                <button type="button" className="icon-button" style={{ color: 'var(--coral)', marginBottom: '4px' }} onClick={() => handleRemoveItem(idx)} title="Xóa mặt hàng này">
                  <Trash2 className="h-4 w-4" />
                </button>
              )}
            </div>
          ))}
        </fieldset>

        <div className="modal-footer">
          <button type="button" className="outline-action" onClick={onClose} disabled={isLoading}>
            Hủy
          </button>
          <button type="submit" className="primary-action" disabled={isLoading || !supplierId || !warehouseId || items.length === 0}>
            {isLoading ? 'Đang tạo...' : 'Tạo PO Container'}
          </button>
        </div>
      </form>
    </ModalBase>
  );
}

// 4. STOCK ORDER DETAIL MODAL — XEM CHI TIẾT ĐƠN NHẬN HÀNG (PO)
interface StockOrderDetailModalProps {
  isOpen: boolean;
  onClose: () => void;
  order: StockOrder | null;
}

export function StockOrderDetailModal({ isOpen, onClose, order }: StockOrderDetailModalProps) {
  if (!order) return null;

  return (
    <ModalBase isOpen={isOpen} onClose={onClose} maxWidth="760px" title={`Chi Tiết Đơn Nhận Hàng — ${order.poNumber}`}>
      <div style={{ display: 'grid', gap: '16px' }}>
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(3, 1fr)', gap: '12px', padding: '14px', background: '#f8faf9', borderRadius: '8px', border: '1px solid var(--line)' }}>
          <div>
            <span className="sidebar-label">Nhà cung cấp</span>
            <strong>{order.supplierName}</strong>
          </div>
          <div>
            <span className="sidebar-label">Kho đích</span>
            <strong>{order.warehouseName}</strong>
          </div>
          <div>
            <span className="sidebar-label">Trạng thái</span>
            <span className={`status-pill ${order.status === 'RECEIVED' ? 'health-online' : ''}`}>{order.status}</span>
          </div>
          <div>
            <span className="sidebar-label">Ngày ETA</span>
            <span>{order.etaDate?.slice(0, 10)}</span>
          </div>
          <div>
            <span className="sidebar-label">Tổng thể tích CBM</span>
            <strong>{order.totalCbm} CBM</strong>
          </div>
          <div>
            <span className="sidebar-label">Cước cont & Thuế</span>
            <span>${order.containerFreightAud} AUD (Thuế: ${order.customsTaxAud ?? 0})</span>
          </div>
        </div>

        <div>
          <h4 style={{ margin: '8px 0 10px', fontSize: '14px' }}>Chi tiết các mặt hàng trong Container:</h4>
          <div className="data-table-wrap">
            <table className="data-table">
              <thead>
                <tr>
                  <th>Mã SKU</th>
                  <th>Tên mặt hàng</th>
                  <th>SL Đặt</th>
                  <th>SL Đã nhận</th>
                  <th>Giá mua (USD)</th>
                  <th>Landed Cost (AUD)</th>
                </tr>
              </thead>
              <tbody>
                {order.items?.map((item) => (
                  <tr key={item.id}>
                    <td><strong>{item.variantSku}</strong></td>
                    <td>{item.variantName}</td>
                    <td><strong>{item.quantityOrdered}</strong></td>
                    <td>{item.quantityReceived}</td>
                    <td>${item.unitCostForeign?.toFixed(2)}</td>
                    <td><strong style={{ color: 'var(--teal)' }}>${item.calculatedLandedCostAud?.toFixed(2)} AUD</strong></td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>

        <div className="modal-footer">
          <button type="button" className="primary-action" onClick={onClose}>
            Đóng
          </button>
        </div>
      </div>
    </ModalBase>
  );
}

// 5. FIFO LOT STAGE PRICES MODAL — XEM VÀ CÀI ĐẶT GIÁ 5-STAGE CHO LÔ FIFO
interface FifoLotStagePricesModalProps {
  isOpen: boolean;
  onClose: () => void;
  lot: FifoLot | null;
  onSubmit: (variantId: number, lotId: number, prices: { stage1Price: number; stage2Price: number; stage3Price: number; stage4Price: number; stage5Price: number; vipPrice?: number }) => Promise<void>;
  onSaveStageConfigs: (variantId: number, configs: Array<{ fromStage: number; toStage: number; targetStockPct: number; minDaysAtStage: number; maxDaysAtStage: number }>) => Promise<void>;
  isLoading: boolean;
}

export function FifoLotStagePricesModal({ isOpen, onClose, lot, onSubmit, onSaveStageConfigs, isLoading }: FifoLotStagePricesModalProps) {
  const [s1, setS1] = useState(lot?.stagePrices?.[1]?.toString() ?? (lot?.stagePrices?.['1']?.toString() ?? '1499'));
  const [s2, setS2] = useState(lot?.stagePrices?.[2]?.toString() ?? (lot?.stagePrices?.['2']?.toString() ?? '1299'));
  const [s3, setS3] = useState(lot?.stagePrices?.[3]?.toString() ?? (lot?.stagePrices?.['3']?.toString() ?? '1099'));
  const [s4, setS4] = useState(lot?.stagePrices?.[4]?.toString() ?? (lot?.stagePrices?.['4']?.toString() ?? '899'));
  const [s5, setS5] = useState(lot?.stagePrices?.[5]?.toString() ?? (lot?.stagePrices?.['5']?.toString() ?? '699'));
  const [vip, setVip] = useState('');
  const [maxDays, setMaxDays] = useState(['60', '60', '60', '60']);

  if (!lot) return null;

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (!lot) return;
    await onSubmit(lot.variantId, lot.id, {
      stage1Price: Number(s1) || 0,
      stage2Price: Number(s2) || 0,
      stage3Price: Number(s3) || 0,
      stage4Price: Number(s4) || 0,
      stage5Price: Number(s5) || 0,
      vipPrice: vip ? Number(vip) : undefined,
    });
    await onSaveStageConfigs(lot.variantId, maxDays.map((value, index) => ({
      fromStage: index + 1,
      toStage: index + 2,
      targetStockPct: 50,
      minDaysAtStage: 14,
      maxDaysAtStage: Number(value) || 60,
    })));
    onClose();
  }

  return (
    <ModalBase isOpen={isOpen} onClose={onClose} title={`Cài Đặt Giá 5-Stage — Lô ${lot.lotNumber}`}>
      <form onSubmit={handleSubmit} className="modal-form">
        <div style={{ padding: '12px', background: '#f8faf9', borderRadius: '8px', border: '1px solid var(--line)', display: 'grid', gridTemplateColumns: 'repeat(2, 1fr)', gap: '8px', fontSize: '12px' }}>
          <div>
            <span className="sidebar-label">Mặt hàng</span>
            <strong>{lot.variantSku} - {lot.variantName}</strong>
          </div>
          <div>
            <span className="sidebar-label">Kho lưu giữ</span>
            <strong>{lot.warehouseName ?? 'Kho Perth'}</strong>
          </div>
          <div>
            <span className="sidebar-label">Giá vốn Landed Cost</span>
            <strong style={{ color: 'var(--teal)' }}>${lot.unitLandedCostAud?.toFixed(2)} AUD</strong>
          </div>
          <div>
            <span className="sidebar-label">Tồn kho còn lại</span>
            <strong>{lot.remainingQuantity} / {lot.initialQuantity} SP (Stage {lot.currentStage})</strong>
          </div>
        </div>

        <fieldset className="modal-fieldset">
          <legend style={{ fontWeight: 'bold' }}>Bảng Giá 5 Stage Cho Lô Này</legend>
          <div className="form-row">
            <div className="form-group">
              <label htmlFor="stg-1">Stage 1 (Giá mở bán AUD) *</label>
              <input id="stg-1" className="field-control" type="number" step="0.01" required value={s1} onChange={(e) => setS1(e.target.value)} />
            </div>
            <div className="form-group">
              <label htmlFor="stg-2">Stage 2 (AUD) *</label>
              <input id="stg-2" className="field-control" type="number" step="0.01" required value={s2} onChange={(e) => setS2(e.target.value)} />
            </div>
          </div>

          <div className="form-row">
            <div className="form-group">
              <label htmlFor="stg-3">Stage 3 (AUD) *</label>
              <input id="stg-3" className="field-control" type="number" step="0.01" required value={s3} onChange={(e) => setS3(e.target.value)} />
            </div>
            <div className="form-group">
              <label htmlFor="stg-4">Stage 4 (AUD) *</label>
              <input id="stg-4" className="field-control" type="number" step="0.01" required value={s4} onChange={(e) => setS4(e.target.value)} />
            </div>
          </div>

          <div className="form-row">
            <div className="form-group">
              <label htmlFor="stg-5">Stage 5 (Xả kho AUD) *</label>
              <input id="stg-5" className="field-control" type="number" step="0.01" required value={s5} onChange={(e) => setS5(e.target.value)} />
            </div>
            <div className="form-group">
              <label htmlFor="stg-vip">Giá ưu đãi VIP (AUD)</label>
              <input id="stg-vip" className="field-control" type="number" step="0.01" value={vip} onChange={(e) => setVip(e.target.value)} placeholder="Tùy chọn" />
            </div>
          </div>
        </fieldset>

        <fieldset className="modal-fieldset">
          <legend style={{ fontWeight: 'bold' }}>Số ngày tối đa trước khi tự chuyển stage</legend>
          <div className="form-row">
            {maxDays.map((value, index) => (
              <div className="form-group" key={index}>
                <label htmlFor={`stg-max-days-${index + 1}`}>Stage {index + 1} → {index + 2} (ngày)</label>
                <input id={`stg-max-days-${index + 1}`} className="field-control" type="number" min="0" required value={value} onChange={(e) => setMaxDays((previous) => previous.map((item, itemIndex) => itemIndex === index ? e.target.value : item))} />
              </div>
            ))}
          </div>
        </fieldset>

        <div className="modal-footer">
          <button type="button" className="outline-action" onClick={onClose} disabled={isLoading}>
            Hủy
          </button>
          <button type="submit" className="primary-action" disabled={isLoading}>
            {isLoading ? 'Đang lưu...' : 'Lưu Bảng Giá Stage'}
          </button>
        </div>
      </form>
    </ModalBase>
  );
}

// 6. CREATE ORDER MODAL (ĐẶT HÀNG POS / ONLINE)
interface CreateOrderModalProps {
  isOpen: boolean;
  onClose: () => void;
  products: Product[];
  stores: Array<{ id: number; name: string }>;
  selectedStoreId: number;
  onStoreChange: (storeId: number) => void;
  onSubmit: (data: {
    storeId: number;
    orderType: string;
    deliveryType: string;
    shippingAddressText?: string;
    deliveryDate?: string;
    notes?: string;
    items: Array<{ variantId: number; quantity: number; unitPriceAud: number }>;
  }) => Promise<void>;
  isLoading: boolean;
}

export function CreateOrderModal({ isOpen, onClose, products, stores, selectedStoreId, onStoreChange, onSubmit, isLoading }: CreateOrderModalProps) {
  const allVariants = products.flatMap((p) => p.variants);
  const [selectedVariantId, setSelectedVariantId] = useState(allVariants[0]?.variantId?.toString() ?? '');
  const [quantity, setQuantity] = useState('1');
  const [unitPrice, setUnitPrice] = useState(allVariants[0]?.currentPriceAud?.toString() ?? '1499');
  const [orderType, setOrderType] = useState('POS_ORDER_NOW');
  const [deliveryType, setDeliveryType] = useState('STORE_DELIVERY');
  const [address, setAddress] = useState('123 Murray St, Perth WA 6000');
  const [notes, setNotes] = useState('Khách đặt tại quầy POS');

  function handleVariantSelect(vId: string) {
    setSelectedVariantId(vId);
    const target = allVariants.find((v) => v.variantId.toString() === vId);
    if (target) {
      setUnitPrice(target.currentPriceAud.toString());
    }
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (!selectedVariantId) return;
    await onSubmit({
      storeId: selectedStoreId,
      orderType,
      deliveryType,
      shippingAddressText: address.trim() || undefined,
      deliveryDate: new Date(Date.now() + 3 * 86400000).toISOString().slice(0, 10),
      notes: notes.trim() || undefined,
      items: [
        {
          variantId: Number(selectedVariantId),
          quantity: Number(quantity) || 1,
          unitPriceAud: Number(unitPrice) || 0,
        },
      ],
    });
    onClose();
  }

  return (
    <ModalBase isOpen={isOpen} onClose={onClose} title="Tạo Đơn Hàng Mới (POS / Online)">
      <form onSubmit={handleSubmit} className="modal-form">
        <div className="form-row">
          <div className="form-group">
            <label htmlFor="ord-store">Cửa hàng POS *</label>
            <select id="ord-store" className="field-control" value={selectedStoreId} onChange={(e) => onStoreChange(Number(e.target.value))}>
              {stores.map((store) => <option key={store.id} value={store.id}>{store.name}</option>)}
            </select>
          </div>
          <div className="form-group">
            <label htmlFor="ord-type">Loại đơn *</label>
            <select id="ord-type" className="field-control" value={orderType} onChange={(e) => setOrderType(e.target.value)}>
              <option value="POS_ORDER_NOW">Đơn bán ngay (POS Order Now)</option>
              <option value="PRE_ORDER">Đơn đặt trước (Pre-Order)</option>
              <option value="ONLINE">Đơn hàng Online (Storefront)</option>
            </select>
          </div>

          <div className="form-group">
            <label htmlFor="ord-del">Hình thức nhận hàng *</label>
            <select id="ord-del" className="field-control" value={deliveryType} onChange={(e) => setDeliveryType(e.target.value)}>
              <option value="STORE_DELIVERY">Giao tận nơi (Delivery)</option>
              <option value="PICKUP">Nhận tại cửa hàng (Pickup)</option>
            </select>
          </div>
        </div>

        <fieldset className="modal-fieldset">
          <legend>Sản phẩm chọn mua</legend>
          <div className="form-group">
            <label htmlFor="ord-var">Chọn mặt hàng *</label>
            <select id="ord-var" className="field-control" required value={selectedVariantId} onChange={(e) => handleVariantSelect(e.target.value)}>
              <option value="">Chọn mặt hàng</option>
              {allVariants.map((v) => (
                <option key={v.variantId} value={v.variantId}>
                  {v.sku} - {v.name} (${v.currentPriceAud})
                </option>
              ))}
            </select>
          </div>

          <div className="form-row">
            <div className="form-group">
              <label htmlFor="ord-qty">Số lượng *</label>
              <input id="ord-qty" className="field-control" type="number" min="1" required value={quantity} onChange={(e) => setQuantity(e.target.value)} />
            </div>
            <div className="form-group">
              <label htmlFor="ord-price">Đơn giá bán (AUD)</label>
              <input id="ord-price" className="field-control" type="number" step="0.01" min="0" required value={unitPrice} onChange={(e) => setUnitPrice(e.target.value)} />
            </div>
          </div>
        </fieldset>

        <div className="form-group">
          <label htmlFor="ord-addr">Địa chỉ giao hàng</label>
          <input id="ord-addr" className="field-control" value={address} onChange={(e) => setAddress(e.target.value)} placeholder="Nhập địa chỉ nhận hàng" />
        </div>

        <div className="form-group">
          <label htmlFor="ord-notes">Ghi chú đơn hàng</label>
          <input id="ord-notes" className="field-control" value={notes} onChange={(e) => setNotes(e.target.value)} placeholder="VD: Giao giờ hành chính, gọi trước 15p" />
        </div>

        <div className="modal-footer">
          <button type="button" className="outline-action" onClick={onClose} disabled={isLoading}>
            Hủy
          </button>
          <button type="submit" className="primary-action" disabled={isLoading || !selectedVariantId}>
            {isLoading ? 'Đang tạo...' : 'Tạo đơn hàng'}
          </button>
        </div>
      </form>
    </ModalBase>
  );
}

// 7. VARIANT STAGE CONFIG MODAL — CÀI ĐẶT CÁC NGƯỠNG CHUYỂN STAGE & NGÀY TỐI ĐA Ở STAGE ĐÓ
interface VariantStageConfigModalProps {
  isOpen: boolean;
  onClose: () => void;
  variantId: number;
  variantSku: string;
  variantName: string;
  onSubmit: (variantId: number, configs: PricingStageConfig[]) => Promise<void>;
  isLoading: boolean;
}

export function VariantStageConfigModal({
  isOpen,
  onClose,
  variantId,
  variantSku,
  variantName,
  onSubmit,
  isLoading,
}: VariantStageConfigModalProps) {
  const [configs, setConfigs] = useState<PricingStageConfig[]>([
    { fromStage: 1, toStage: 2, targetStockPct: 70, minDaysAtStage: 7, maxDaysAtStage: 45 },
    { fromStage: 2, toStage: 3, targetStockPct: 50, minDaysAtStage: 7, maxDaysAtStage: 45 },
    { fromStage: 3, toStage: 4, targetStockPct: 30, minDaysAtStage: 7, maxDaysAtStage: 45 },
    { fromStage: 4, toStage: 5, targetStockPct: 15, minDaysAtStage: 7, maxDaysAtStage: 45 },
  ]);

  function handleChange(index: number, field: keyof PricingStageConfig, val: number) {
    setConfigs((prev) => {
      const copy = [...prev];
      copy[index] = { ...copy[index], [field]: val };
      return copy;
    });
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    await onSubmit(variantId, configs);
    onClose();
  }

  return (
    <ModalBase isOpen={isOpen} onClose={onClose} maxWidth="680px" title={`Cài Đặt Chuyển Stage Giá — ${variantSku}`}>
      <form onSubmit={handleSubmit} className="modal-form">
        <p style={{ margin: '0 0 8px', fontSize: '13px', color: 'var(--ink-muted)' }}>
          Biến thể: <strong>{variantName}</strong> ({variantSku}). Cài đặt ngưỡng % tồn kho, số ngày tối thiểu và <strong>số ngày tối đa</strong> để tự động chuyển stage giá.
        </p>

        <div style={{ display: 'grid', gap: '12px' }}>
          {configs.map((cfg, idx) => (
            <fieldset key={idx} className="modal-fieldset">
              <legend style={{ fontWeight: 'bold' }}>
                Chuyển từ Stage {cfg.fromStage} → Stage {cfg.toStage}
              </legend>
              <div className="form-row">
                <div className="form-group">
                  <label htmlFor={`target-pct-${idx}`}>Ngưỡng Tồn Kho (%)</label>
                  <input
                    id={`target-pct-${idx}`}
                    className="field-control"
                    type="number"
                    step="1"
                    min="1"
                    max="100"
                    required
                    value={cfg.targetStockPct}
                    onChange={(e) => handleChange(idx, 'targetStockPct', Number(e.target.value))}
                  />
                </div>
                <div className="form-group">
                  <label htmlFor={`min-days-${idx}`}>Ngày tối thiểu ở stage</label>
                  <input
                    id={`min-days-${idx}`}
                    className="field-control"
                    type="number"
                    min="0"
                    required
                    value={cfg.minDaysAtStage}
                    onChange={(e) => handleChange(idx, 'minDaysAtStage', Number(e.target.value))}
                  />
                </div>
                <div className="form-group">
                  <label htmlFor={`max-days-${idx}`}>Ngày tối đa ở stage *</label>
                  <input
                    id={`max-days-${idx}`}
                    className="field-control"
                    type="number"
                    min="1"
                    required
                    value={cfg.maxDaysAtStage}
                    onChange={(e) => handleChange(idx, 'maxDaysAtStage', Number(e.target.value))}
                  />
                </div>
              </div>
            </fieldset>
          ))}
        </div>

        <div className="modal-footer">
          <button type="button" className="outline-action" onClick={onClose} disabled={isLoading}>
            Hủy
          </button>
          <button type="submit" className="primary-action" disabled={isLoading}>
            {isLoading ? 'Đang lưu...' : 'Lưu Cấu Hình Stage'}
          </button>
        </div>
      </form>
    </ModalBase>
  );
}

