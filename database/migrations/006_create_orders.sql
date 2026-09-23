-- ================================================================
-- MIGRATION 006: Customers, Orders & Audit Logs
-- Tables: customers, customer_loyalty_transactions, orders,
--         order_items, order_audit_logs,
--         order_item_fifo_allocations, order_payments
-- ================================================================

-- ----------------------------------------------------------------
-- 1. customers — Hồ sơ khách hàng & Thẻ thành viên VIP Loyalty
-- ----------------------------------------------------------------
CREATE TABLE IF NOT EXISTS customers (
    id                  BIGSERIAL PRIMARY KEY,
    customer_code       VARCHAR(50) NOT NULL UNIQUE,
    first_name          VARCHAR(100) NOT NULL,
    last_name           VARCHAR(100) NOT NULL,
    email               VARCHAR(255) UNIQUE,
    phone               VARCHAR(30) NOT NULL,
    address_id          BIGINT REFERENCES addresses(id) ON DELETE SET NULL,
    is_vip              BOOLEAN DEFAULT FALSE,
    loyalty_points      INTEGER NOT NULL DEFAULT 0 CHECK (loyalty_points >= 0),
    vip_granted_at      TIMESTAMPTZ,
    notes               TEXT,
    created_at          TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    updated_at          TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP
);
CREATE INDEX IF NOT EXISTS idx_customers_phone ON customers(phone);
CREATE INDEX IF NOT EXISTS idx_customers_email ON customers(email);

-- ----------------------------------------------------------------
-- ----------------------------------------------------------------
-- 2. orders — Đơn hàng Bán lẻ & POS đa kênh
-- ----------------------------------------------------------------
CREATE TABLE IF NOT EXISTS orders (
    id                      BIGSERIAL PRIMARY KEY,
    order_code              VARCHAR(60) NOT NULL UNIQUE,
    customer_id             BIGINT REFERENCES customers(id) ON DELETE SET NULL,
    store_id                BIGINT NOT NULL REFERENCES stores(id) ON DELETE RESTRICT,
    salesperson_user_id     BIGINT REFERENCES users(id) ON DELETE SET NULL,
    order_channel           VARCHAR(30) NOT NULL DEFAULT 'POS',
    -- POS, WEB, PHONE
    order_type              VARCHAR(30) NOT NULL DEFAULT 'ORDER_NOW',
    -- ORDER_NOW, PRE_ORDER
    status                  VARCHAR(30) NOT NULL DEFAULT 'PENDING',
    -- PENDING, CONFIRMED, PROCESSING, READY_TO_SHIP, SHIPPING, COMPLETED, CANCELLED, COMEBACK
    payment_status          VARCHAR(30) NOT NULL DEFAULT 'UNPAID',
    -- UNPAID, PARTIAL, PAID, REFUNDED
    subtotal_aud            NUMERIC(15, 2) NOT NULL DEFAULT 0.00 CHECK (subtotal_aud >= 0),
    discount_aud            NUMERIC(15, 2) NOT NULL DEFAULT 0.00 CHECK (discount_aud >= 0),
    shipping_fee_aud        NUMERIC(15, 2) NOT NULL DEFAULT 0.00 CHECK (shipping_fee_aud >= 0),
    total_aud               NUMERIC(15, 2) NOT NULL DEFAULT 0.00 CHECK (total_aud >= 0),
    paid_amount_aud         NUMERIC(15, 2) NOT NULL DEFAULT 0.00 CHECK (paid_amount_aud >= 0),
    expected_profit_aud     NUMERIC(15, 2) NOT NULL DEFAULT 0.00,
    actual_profit_aud       NUMERIC(15, 2) DEFAULT NULL,
    target_delivery_date    DATE,
    notes                   TEXT,
    created_at              TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    updated_at              TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP
);
CREATE INDEX IF NOT EXISTS idx_orders_code ON orders(order_code);
CREATE INDEX IF NOT EXISTS idx_orders_customer ON orders(customer_id);
CREATE INDEX IF NOT EXISTS idx_orders_status ON orders(status);
CREATE INDEX IF NOT EXISTS idx_orders_created ON orders(created_at);
CREATE INDEX IF NOT EXISTS idx_orders_store ON orders(store_id);

-- Loyalty transactions are created after orders so the FK is declared inline.
CREATE TABLE IF NOT EXISTS customer_loyalty_transactions (
    id                  BIGSERIAL PRIMARY KEY,
    customer_id         BIGINT NOT NULL REFERENCES customers(id) ON DELETE CASCADE,
    order_id            BIGINT REFERENCES orders(id) ON DELETE SET NULL,
    points_delta        INTEGER NOT NULL,
    reason              VARCHAR(150) NOT NULL,
    created_at          TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP
);

-- ----------------------------------------------------------------
-- 4. order_items — Chi tiết mặt hàng trong đơn
-- ----------------------------------------------------------------
CREATE TABLE IF NOT EXISTS order_items (
    id                          BIGSERIAL PRIMARY KEY,
    order_id                    BIGINT NOT NULL REFERENCES orders(id) ON DELETE CASCADE,
    variant_id                  BIGINT NOT NULL REFERENCES product_variants(id) ON DELETE RESTRICT,
    quantity                    INTEGER NOT NULL CHECK (quantity > 0),
    unit_price_aud              NUMERIC(15, 2) NOT NULL CHECK (unit_price_aud >= 0),
    stage_applied               SMALLINT NOT NULL CHECK (stage_applied BETWEEN 1 AND 5),
    is_vip_price                BOOLEAN DEFAULT FALSE,
    discount_aud                NUMERIC(15, 2) NOT NULL DEFAULT 0.00 CHECK (discount_aud >= 0),
    line_total_aud              NUMERIC(15, 2) NOT NULL CHECK (line_total_aud >= 0),
    created_at                  TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP
);
CREATE INDEX IF NOT EXISTS idx_order_items_order ON order_items(order_id);
CREATE INDEX IF NOT EXISTS idx_order_items_variant ON order_items(variant_id);

-- ----------------------------------------------------------------
-- 5. order_audit_logs — Nhật ký kiểm toán đơn hàng (AES-256-GCM)
-- ----------------------------------------------------------------
CREATE TABLE IF NOT EXISTS order_audit_logs (
    id                  BIGSERIAL PRIMARY KEY,
    order_id            BIGINT NOT NULL REFERENCES orders(id) ON DELETE CASCADE,
    actor_id            BIGINT REFERENCES users(id) ON DELETE SET NULL,
    action              VARCHAR(50) NOT NULL,
    change_summary      TEXT NOT NULL,
    old_data_encrypted  TEXT,           -- Mã hóa AES-256-GCM dạng Base64
    new_data_encrypted  TEXT,           -- Mã hóa AES-256-GCM dạng Base64
    key_version         VARCHAR(10) NOT NULL DEFAULT 'v1',
    ip_address          VARCHAR(45),
    user_agent          TEXT,
    created_at          TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP
);
CREATE INDEX IF NOT EXISTS idx_order_audit_lookup ON order_audit_logs(order_id, created_at DESC);
CREATE INDEX IF NOT EXISTS idx_order_audit_user ON order_audit_logs(actor_id);

-- ----------------------------------------------------------------
-- 6. order_item_fifo_allocations — Lợi Nhuận Thực Tế theo Lô FIFO
-- ----------------------------------------------------------------
CREATE TABLE IF NOT EXISTS order_item_fifo_allocations (
    id                      BIGSERIAL PRIMARY KEY,
    order_item_id           BIGINT NOT NULL REFERENCES order_items(id) ON DELETE CASCADE,
    fifo_lot_id             BIGINT NOT NULL REFERENCES fifo_lots(id) ON DELETE RESTRICT,
    quantity_allocated      INTEGER NOT NULL CHECK (quantity_allocated > 0),
    unit_landed_cost_aud    NUMERIC(15, 2) NOT NULL CHECK (unit_landed_cost_aud >= 0),
    unit_selling_price_aud  NUMERIC(15, 2) NOT NULL CHECK (unit_selling_price_aud >= 0),
    realized_profit_aud     NUMERIC(15, 2) NOT NULL,
    status                  VARCHAR(20) NOT NULL DEFAULT 'ACTIVE',
    -- ACTIVE, CANCELLED, RETURNED
    created_at              TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP
);
CREATE INDEX IF NOT EXISTS idx_fifo_allocations_item ON order_item_fifo_allocations(order_item_id);
CREATE INDEX IF NOT EXISTS idx_fifo_allocations_lot ON order_item_fifo_allocations(fifo_lot_id);

-- ----------------------------------------------------------------
-- 7. order_payments — Quản lý nhiều đợt thanh toán
-- ----------------------------------------------------------------
CREATE TABLE IF NOT EXISTS order_payments (
    id                  BIGSERIAL PRIMARY KEY,
    order_id            BIGINT NOT NULL REFERENCES orders(id) ON DELETE CASCADE,
    payment_method      VARCHAR(30) NOT NULL,
    -- CASH, EFTPOS, VISA, MASTERCARD, PAYPAL, BANK_TRANSFER
    amount_aud          NUMERIC(15, 2) NOT NULL CHECK (amount_aud > 0),
    cash_tendered_aud   NUMERIC(15, 2) CHECK (cash_tendered_aud >= 0),
    change_given_aud    NUMERIC(15, 2) CHECK (change_given_aud >= 0),
    transaction_ref     VARCHAR(100),
    idempotency_key     VARCHAR(100) NOT NULL,
    cashier_user_id     BIGINT REFERENCES users(id) ON DELETE SET NULL,
    paid_at             TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP
);
CREATE INDEX IF NOT EXISTS idx_order_payments_order ON order_payments(order_id);
CREATE UNIQUE INDEX IF NOT EXISTS uq_order_payments_idempotency ON order_payments(idempotency_key);
CREATE UNIQUE INDEX IF NOT EXISTS uq_order_payments_transaction_ref
    ON order_payments(transaction_ref)
    WHERE transaction_ref IS NOT NULL;
