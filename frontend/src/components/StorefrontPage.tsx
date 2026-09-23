import { ArrowLeft, CheckCircle2, Image as ImageIcon, MapPin, Search, ShoppingBag, Truck, X } from 'lucide-react';
import { useEffect, useState } from 'react';
import { getProducts, getStoreVariantPrices, lookupPostcode, selectStore } from '../api/catalog';
import type { PostcodeLookup, Product, ProductVariant } from '../types/domain';

interface CartItem {
  product: Product;
  variant: ProductVariant;
  quantity: number;
  priceAud: number;
}

export function StorefrontPage() {
  const [postcode, setPostcode] = useState('');
  const [location, setLocation] = useState<PostcodeLookup | null>(null);
  const [products, setProducts] = useState<Product[]>([]);
  const [search, setSearch] = useState('');
  const [selectedCategory, setSelectedCategory] = useState<string>('ALL');
  const [selectedProduct, setSelectedProduct] = useState<Product | null>(null);
  const [cart, setCart] = useState<CartItem[]>([]);
  const [isCartOpen, setIsCartOpen] = useState(false);
  const [isCartPage, setIsCartPage] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [orderSuccess, setOrderSuccess] = useState<string | null>(null);

  useEffect(() => {
    async function loadInitialCatalog(): Promise<void> {
      try {
        const initialProducts = await getProducts();
        setProducts(initialProducts);
      } catch {
        setError('Không thể kết nối catalog sản phẩm.');
      }
    }

    void loadInitialCatalog();
  }, []);

  async function handleLookup(value: string) {
    if (!value.trim()) return;
    setIsLoading(true);
    setError(null);
    try {
      const result = await lookupPostcode(value.trim());
      setLocation(result);
      const activeStore = result.assignedStore ?? result.availableStores[0];
      if (activeStore) {
        setProducts(await loadStoreCatalog(activeStore.id));
      }
    } catch {
      setLocation(null);
      setError('Không tìm thấy thông tin phục vụ cho Postcode này. Vui lòng nhập postcode hợp lệ (VD: 6000, 4102, 2000).');
    } finally {
      setIsLoading(false);
    }
  }

  async function handleStoreChange(storeId: number) {
    setIsLoading(true);
    try {
      const result = await selectStore(postcode, storeId);
      setLocation(result);
      setProducts(await loadStoreCatalog(storeId));
    } catch {
      setError('Không thể chọn cửa hàng này.');
    } finally {
      setIsLoading(false);
    }
  }

  async function loadStoreCatalog(storeId?: number): Promise<Product[]> {
    const catalog = await getProducts(storeId);
    if (!storeId) return catalog;

    try {
      const prices = await getStoreVariantPrices(storeId);
      const pricesByVariant = new Map(prices.map((p) => [p.variantId, p.currentDailyPrice]));
      return catalog.map((product) => ({
        ...product,
        variants: product.variants.map((variant) => ({
          ...variant,
          currentPriceAud: pricesByVariant.get(variant.variantId) ?? variant.currentPriceAud,
        })),
      }));
    } catch {
      return catalog;
    }
  }

  function handleAddToCart(product: Product, variant: ProductVariant, quantity: number) {
    if (!location?.priceUnlocked) {
      setError('Vui lòng nhập Postcode trước khi thêm sản phẩm vào giỏ hàng.');
      return;
    }
    const price = variant.currentPriceAud;
    setCart((prev) => {
      const existingIndex = prev.findIndex((item) => item.variant.variantId === variant.variantId);
      if (existingIndex >= 0) {
        const updated = [...prev];
        updated[existingIndex].quantity += quantity;
        return updated;
      }
      return [...prev, { product, variant, quantity, priceAud: price }];
    });
  }

  const categories = ['ALL', 'Living Room', 'Bedroom', 'Dining Room'];

  const filteredProducts = products.filter((product) => {
    const matchesSearch = `${product.name} ${product.sku}`.toLowerCase().includes(search.toLowerCase());
    const matchesCategory = selectedCategory === 'ALL' || (product.categoryName && product.categoryName.toLowerCase() === selectedCategory.toLowerCase());
    return matchesSearch && matchesCategory;
  });

  const selectedStore = location?.assignedStore ?? location?.availableStores[0];
  const cartSubtotal = cart.reduce((sum, item) => sum + item.priceAud * item.quantity, 0);
  const shippingFee = location?.estimatedShippingRate?.shippingFeeAud ?? 45.0;
  const grandTotal = cartSubtotal > 0 ? cartSubtotal + shippingFee : 0;

  return (
    <section className="storefront-page" aria-labelledby="storefront-title">
      {/* HEADER / NAVIGATION BAR */}
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '18px' }}>
        <div>
          <p className="eyebrow">Online Storefront</p>
          <h1 id="storefront-title" style={{ margin: '4px 0 0', fontFamily: 'Georgia, serif', fontSize: '28px' }}>
            VissSoft Furniture Store
          </h1>
        </div>
        <button type="button" className="outline-action" onClick={() => setIsCartPage(true)} style={{ position: 'relative' }}>
          <ShoppingBag className="h-4 w-4" /> Giỏ hàng ({cart.reduce((a, b) => a + b.quantity, 0)})
        </button>
      </div>

      {orderSuccess && (
        <div className="alert alert-info" role="status" style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
          <CheckCircle2 className="h-5 w-5 text-teal-600" />
          <span>{orderSuccess}</span>
        </div>
      )}

      {isCartPage ? (
        <CartPage
          cart={cart}
          subtotal={cartSubtotal}
          shippingFee={shippingFee}
          onBack={() => setIsCartPage(false)}
          onChangeQuantity={(variantId, quantity) => setCart((items) => items.map((item) => item.variant.variantId === variantId ? { ...item, quantity } : item).filter((item) => item.quantity > 0))}
          onRemove={(variantId) => setCart((items) => items.filter((item) => item.variant.variantId !== variantId))}
        />
      ) : selectedProduct ? (
        <ProductDetail
          product={selectedProduct}
          hasUnlockedPrice={Boolean(location?.priceUnlocked)}
          onBack={() => setSelectedProduct(null)}
          onAddToCart={handleAddToCart}
          estimatedShippingFee={shippingFee}
        />
      ) : (
        <>
          {/* POSTCODE LOOKUP BANNER */}
          <div className="storefront-hero">
            <div>
              <p className="eyebrow">Định vị giá &amp; tồn kho</p>
              <h2>Bảng giá theo khu vực Postcode</h2>
              <p>Nhập Postcode nhận hàng để hệ thống tự động gán cửa hàng phục vụ và mở khóa giá theo từng chi nhánh.</p>
            </div>
            <form
              className="postcode-search"
              onSubmit={(e) => {
                e.preventDefault();
                void handleLookup(postcode);
              }}
            >
              <label htmlFor="postcode">Postcode giao hàng</label>
              <div className="search-row">
                <MapPin className="h-4 w-4" aria-hidden="true" />
                <input id="postcode" inputMode="numeric" value={postcode} onChange={(e) => setPostcode(e.target.value)} placeholder="Nhập postcode (VD: 6000)" />
                <button type="button" onClick={() => void handleLookup(postcode)} disabled={isLoading}>
                  <Search className="h-4 w-4" /> Tra cứu
                </button>
              </div>
            </form>
          </div>

          {error && <div className="alert alert-error" role="alert">{error}</div>}

          {location && (
            <div className="location-bar">
              <div>
                <span className="eyebrow">Khu vực mở giá thành công</span>
                <strong>
                  {location.suburb}, {location.state} · Postcode {location.postcode}
                </strong>
              </div>
              <div className="location-controls">
                {location.availableStores.length > 1 && (
                  <label>
                    Cửa hàng phục vụ:
                    <select value={selectedStore?.id ?? ''} onChange={(e) => void handleStoreChange(Number(e.target.value))}>
                      {location.availableStores.map((s) => (
                        <option key={s.id} value={s.id}>
                          {s.name}
                        </option>
                      ))}
                    </select>
                  </label>
                )}
                <span className="shipping-note">
                  <Truck className="h-4 w-4" /> Ship từ {selectedStore?.name ?? 'Cửa hàng gần nhất'} · Phí ship: ${shippingFee.toFixed(2)} AUD
                </span>
              </div>
            </div>
          )}

          {/* CATEGORY & SEARCH TOOLBAR */}
          <div className="catalog-toolbar" style={{ marginTop: '24px' }}>
            <div>
              <div style={{ display: 'flex', gap: '8px', marginBottom: '8px' }}>
                {categories.map((cat) => (
                  <button
                    key={cat}
                    type="button"
                    className={selectedCategory === cat ? 'workspace-tab active' : 'workspace-tab'}
                    onClick={() => setSelectedCategory(cat)}
                  >
                    {cat === 'ALL' ? 'Tất cả sản phẩm' : cat}
                  </button>
                ))}
              </div>
              <h2>{location?.priceUnlocked ? 'Sản phẩm đang bán' : 'Danh mục sản phẩm'}</h2>
              {!location?.priceUnlocked && (
                <p className="catalog-hint">Giá sản phẩm tạm ẩn. Vui lòng nhập Postcode phía trên để mở khóa giá theo chi nhánh.</p>
              )}
            </div>
            <label className="catalog-search">
              <Search className="h-4 w-4" />
              <input aria-label="Tìm sản phẩm" placeholder="Tìm tên hoặc SKU..." value={search} onChange={(e) => setSearch(e.target.value)} />
            </label>
          </div>

          {/* PRODUCT CARDS GRID */}
          <div className="product-grid">
            {filteredProducts.map((product) => {
              const defaultVariant = product.variants[0];
              return (
                <article className="product-card" key={product.id}>
                  <button type="button" className="product-card-button" onClick={() => setSelectedProduct(product)}>
                    <div className="product-art">
                      <ImageIcon className="h-10 w-10 text-teal-600" />
                    </div>
                    <div className="product-card-body">
                      <span className="product-category">{product.categoryName ?? 'Furniture'}</span>
                      <h3>{product.name}</h3>
                      <p>
                        SKU: {product.sku}
                      </p>
                      <div className="product-price-row">
                        {location?.priceUnlocked ? (
                          <strong>${defaultVariant?.currentPriceAud?.toFixed(2) ?? '--'} AUD</strong>
                        ) : (
                          <strong className="price-locked">Nhập Postcode để xem giá</strong>
                        )}
                        <span>{defaultVariant?.stock?.available ?? 0} có sẵn</span>
                      </div>
                    </div>
                  </button>
                </article>
              );
            })}
            {!isLoading && filteredProducts.length === 0 && (
              <div className="empty-state" style={{ gridColumn: '1 / -1' }}>
                Chưa có sản phẩm phù hợp với từ khóa hoặc danh mục đã chọn.
              </div>
            )}
          </div>
        </>
      )}

      {/* MY CART DRAWER / MODAL */}
      {isCartOpen && (
        <div className="modal-backdrop" onClick={() => setIsCartOpen(false)}>
          <div className="modal-container" onClick={(e) => e.stopPropagation()} style={{ maxWidth: '480px' }}>
            <div className="modal-header">
              <h3>Giỏ Hàng Của Bạn</h3>
              <button type="button" className="icon-button" onClick={() => setIsCartOpen(false)}>
                <X className="h-5 w-5" />
              </button>
            </div>
            <div className="modal-body">
              {cart.length === 0 ? (
                <div className="empty-state">Giỏ hàng đang trống. Hãy chọn sản phẩm để mua.</div>
              ) : (
                <div style={{ display: 'grid', gap: '14px' }}>
                  {cart.map((item, idx) => (
                    <div key={idx} style={{ display: 'flex', justifyContent: 'space-between', paddingBottom: '10px', borderBottom: '1px solid #edf0ed' }}>
                      <div>
                        <strong>{item.product.name}</strong>
                        <div style={{ fontSize: '11px', color: 'var(--ink-muted)' }}>
                          Variant: {item.variant.name} (x{item.quantity})
                        </div>
                      </div>
                      <div style={{ textAlign: 'right' }}>
                        <strong>${(item.priceAud * item.quantity).toFixed(2)} AUD</strong>
                      </div>
                    </div>
                  ))}

                  <div style={{ marginTop: '12px', paddingTop: '12px', borderTop: '2px solid var(--line)', display: 'grid', gap: '6px', fontSize: '12px' }}>
                    <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                      <span>Tạm tính (Subtotal):</span>
                      <strong>${cartSubtotal.toFixed(2)} AUD</strong>
                    </div>
                    <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                      <span>Phí giao hàng dự kiến:</span>
                      <span>${shippingFee.toFixed(2)} AUD</span>
                    </div>
                    <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: '15px', color: 'var(--teal)', fontWeight: 'bold', marginTop: '6px' }}>
                      <span>Tổng cộng:</span>
                      <span>${grandTotal.toFixed(2)} AUD</span>
                    </div>
                  </div>

                  <button
                    type="button"
                    className="primary-action"
                    style={{ width: '100%', marginTop: '14px' }}
                    onClick={() => {
                      setCart([]);
                      setIsCartOpen(false);
                      setOrderSuccess('Đặt hàng thành công! Đơn hàng trực tuyến đã được chuyển sang hệ thống Admin xử lý.');
                    }}
                  >
                    Thanh toán &amp; Đặt hàng
                  </button>
                </div>
              )}
            </div>
          </div>
        </div>
      )}
    </section>
  );
}

function CartPage({
  cart,
  subtotal,
  shippingFee,
  onBack,
  onChangeQuantity,
  onRemove,
}: {
  cart: CartItem[];
  subtotal: number;
  shippingFee: number;
  onBack: () => void;
  onChangeQuantity: (variantId: number, quantity: number) => void;
  onRemove: (variantId: number) => void;
}) {
  const total = subtotal > 0 ? subtotal + shippingFee : 0;

  return (
    <article className="product-detail">
      <button type="button" className="text-action" onClick={onBack}>
        <ArrowLeft className="h-4 w-4" /> Tiếp tục mua sắm
      </button>
      <div className="panel-heading">
        <div>
          <p className="eyebrow">Shopping cart</p>
          <h2>Giỏ hàng của bạn</h2>
        </div>
        <strong>{cart.reduce((sum, item) => sum + item.quantity, 0)} sản phẩm</strong>
      </div>
      {cart.length === 0 ? (
        <div className="empty-state">Giỏ hàng đang trống.</div>
      ) : (
        <div style={{ display: 'grid', gap: '12px' }}>
          {cart.map((item) => (
            <div key={item.variant.variantId} className="operation-row">
              <div>
                <strong>{item.product.name}</strong>
                <span>{item.variant.name} · {item.variant.sku}</span>
              </div>
              <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                <input aria-label={`Số lượng ${item.variant.name}`} className="field-control" type="number" min="1" value={item.quantity} onChange={(event) => onChangeQuantity(item.variant.variantId, Number(event.target.value) || 1)} style={{ width: '72px' }} />
                <strong>${(item.priceAud * item.quantity).toFixed(2)} AUD</strong>
                <button type="button" className="text-action danger-action" onClick={() => onRemove(item.variant.variantId)}>Xóa</button>
              </div>
            </div>
          ))}
          <div style={{ display: 'grid', gap: '6px', maxWidth: '360px', marginLeft: 'auto', width: '100%' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between' }}><span>Tạm tính</span><strong>${subtotal.toFixed(2)} AUD</strong></div>
            <div style={{ display: 'flex', justifyContent: 'space-between' }}><span>Phí giao hàng dự kiến</span><span>${shippingFee.toFixed(2)} AUD</span></div>
            <div style={{ display: 'flex', justifyContent: 'space-between', fontWeight: 'bold' }}><span>Tổng cộng</span><strong>${total.toFixed(2)} AUD</strong></div>
            <button type="button" className="primary-action" disabled={cart.length === 0}>Tiến hành thanh toán</button>
          </div>
        </div>
      )}
    </article>
  );
}

function ProductDetail({
  product,
  hasUnlockedPrice,
  onBack,
  onAddToCart,
  estimatedShippingFee,
}: {
  product: Product;
  hasUnlockedPrice: boolean;
  onBack: () => void;
  onAddToCart: (product: Product, variant: ProductVariant, quantity: number) => void;
  estimatedShippingFee: number;
}) {
  const [selectedVariantId, setSelectedVariantId] = useState<number>(product.variants[0]?.variantId ?? 0);
  const [quantity, setQuantity] = useState(1);

  const activeVariant = product.variants.find((v) => v.variantId === selectedVariantId) ?? product.variants[0];

  return (
    <article className="product-detail">
      <button type="button" className="text-action" onClick={onBack}>
        <ArrowLeft className="h-4 w-4" /> Quay lại danh mục
      </button>

      <div className="product-detail-grid">
        {/* IMAGE PLACEHOLDER BOX */}
        <div className="product-detail-art" style={{ flexDirection: 'column', gap: '10px' }}>
          <ImageIcon className="h-16 w-16 text-teal-600" />
          <span style={{ fontSize: '12px', color: 'var(--ink-muted)' }}>Hình ảnh sản phẩm (Placeholder)</span>
        </div>

        <div>
          <p className="eyebrow">{product.categoryName ?? 'Furniture'}</p>
          <h2>{product.name}</h2>
          <p className="product-detail-sku">Mã SKU: {product.sku}</p>

          {hasUnlockedPrice ? (
            <strong className="product-detail-price">${activeVariant?.currentPriceAud?.toFixed(2) ?? '--'} AUD</strong>
          ) : (
            <p className="price-locked-detail">Vui lòng nhập Postcode giao hàng để mở khóa giá chính xác.</p>
          )}

          <p className="product-detail-copy">
            Sản phẩm nội thất cao cấp phân phối chính hãng. Giá hiển thị và tồn kho khả dụng được tính toán tự động dựa trên kho phục vụ chi nhánh gần nhất.
          </p>

          {/* VARIANT SELECTOR */}
          <div style={{ marginBottom: '18px' }}>
            <label className="field-label" htmlFor="variant-select">
              Chọn biến thể (Color / Size):
            </label>
            <select
              id="variant-select"
              className="field-control"
              style={{ width: '100%', marginTop: '6px' }}
              value={selectedVariantId}
              onChange={(e) => setSelectedVariantId(Number(e.target.value))}
            >
              {product.variants.map((v) => (
                <option key={v.variantId} value={v.variantId}>
                  {v.name} ({v.sku}) {hasUnlockedPrice ? `- $${v.currentPriceAud} AUD` : ''}
                </option>
              ))}
            </select>
          </div>

          <dl className="product-specs">
            <div>
              <dt>Tên Biến thể</dt>
              <dd>{activeVariant?.name ?? 'Mặc định'}</dd>
            </div>
            <div>
              <dt>Tình trạng kho</dt>
              <dd>{(activeVariant?.stock?.available ?? 0) > 0 ? `${activeVariant?.stock?.available} có sẵn` : 'Đặt hàng trước'}</dd>
            </div>
            <div>
              <dt>Quy cách CBM</dt>
              <dd>{activeVariant?.stock ? 'Tiêu chuẩn' : 'Chính hãng'}</dd>
            </div>
          </dl>

          <div style={{ marginTop: '24px', display: 'flex', gap: '12px', alignItems: 'center' }}>
            <div style={{ display: 'flex', alignItems: 'center', gap: '6px' }}>
              <button type="button" className="outline-action" onClick={() => setQuantity((q) => Math.max(1, q - 1))}>
                -
              </button>
              <span style={{ fontWeight: 'bold', padding: '0 8px' }}>{quantity}</span>
              <button type="button" className="outline-action" onClick={() => setQuantity((q) => q + 1)}>
                +
              </button>
            </div>

            <button
              type="button"
              className="primary-action"
              style={{ flex: 1 }}
              disabled={!hasUnlockedPrice || !activeVariant}
              onClick={() => activeVariant && onAddToCart(product, activeVariant, quantity)}
            >
              <ShoppingBag className="h-4 w-4" /> Thêm vào Giỏ Hàng
            </button>
          </div>

          <div style={{ marginTop: '14px', fontSize: '11px', color: 'var(--ink-muted)', display: 'flex', alignItems: 'center', gap: '6px' }}>
            <Truck className="h-4 w-4 text-teal-600" />
            <span>Phí giao hàng ước tính theo Postcode: ${estimatedShippingFee.toFixed(2)} AUD</span>
          </div>
        </div>
      </div>
    </article>
  );
}

