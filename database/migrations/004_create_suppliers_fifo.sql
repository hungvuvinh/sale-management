-- ================================================================
-- MIGRATION 004: Suppliers, Stock Orders & FIFO Lots
-- Tables: suppliers, stock_orders, stock_order_items, fifo_lots
--         + FK constraints for fifo_lot_stage_prices,
--           stage_price_audit_logs
-- ================================================================

-- ----------------------------------------------------------------
-- 1. suppliers — Nhà cung cấp & Lead-Time
-- ----------------------------------------------------------------
CREATE TABLE IF NOT EXISTS suppliers (
    id                  BIGSERIAL PRIMARY KEY,
    name                VARCHAR(200) NOT NULL,
    country             VARCHAR(100) NOT NULL,
    contact_name        VARCHAR(100),
    contact_phone       VARCHAR(30),
    contact_email       VARCHAR(100),
    lead_time_days      INTEGER NOT NULL DEFAULT 30 CHECK (lead_time_days >= 0),
    payment_terms       TEXT,
    is_active           BOOLEAN DEFAULT TRUE,
    created_at          TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP
);

-- ----------------------------------------------------------------
-- 2. stock_orders — Đơn mua hàng Container từ NCC
-- ----------------------------------------------------------------
CREATE TABLE IF NOT EXISTS stock_orders (
    id                          BIGSERIAL PRIMARY KEY,
    po_number                   VARCHAR(50) NOT NULL UNIQUE,
    supplier_id                 BIGINT NOT NULL REFERENCES suppliers(id) ON DELETE RESTRICT,
    destination_warehouse_id    BIGINT NOT NULL REFERENCES warehouses(id) ON DELETE RESTRICT,
    container_code              VARCHAR(50),
    eta_date                    DATE NOT NULL,
    actual_arrival_date         DATE,
    status                      VARCHAR(30) NOT NULL DEFAULT 'PENDING',
    -- PENDING, SAILING, CUSTOMS_CLEARING, ARRIVED, RECEIVED
    total_cbm                   NUMERIC(12, 4) NOT NULL DEFAULT 0.0000 CHECK (total_cbm >= 0),
    container_freight_aud       NUMERIC(15, 2) NOT NULL DEFAULT 0.00 CHECK (container_freight_aud >= 0),
    customs_tax_aud             NUMERIC(15, 2) NOT NULL DEFAULT 0.00 CHECK (customs_tax_aud >= 0),
    currency_code               VARCHAR(3) NOT NULL DEFAULT 'USD',
    exchange_rate               NUMERIC(12, 6) NOT NULL DEFAULT 1.500000 CHECK (exchange_rate > 0),
    notes                       TEXT,
    created_at                  TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    updated_at                  TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP
);
CREATE INDEX IF NOT EXISTS idx_stock_orders_eta ON stock_orders(eta_date);
CREATE INDEX IF NOT EXISTS idx_stock_orders_status ON stock_orders(status);

-- ----------------------------------------------------------------
-- 3. stock_order_items — Chi tiết SKU trong Container & Landed Cost
-- ----------------------------------------------------------------
CREATE TABLE IF NOT EXISTS stock_order_items (
    id                              BIGSERIAL PRIMARY KEY,
    stock_order_id                  BIGINT NOT NULL REFERENCES stock_orders(id) ON DELETE CASCADE,
    variant_id                      BIGINT NOT NULL REFERENCES product_variants(id) ON DELETE RESTRICT,
    quantity_ordered                INTEGER NOT NULL CHECK (quantity_ordered > 0),
    quantity_received               INTEGER NOT NULL DEFAULT 0 CHECK (quantity_received >= 0),
    unit_cost_foreign               NUMERIC(15, 2) NOT NULL CHECK (unit_cost_foreign >= 0),
    unit_cost_aud                   NUMERIC(15, 2) NOT NULL CHECK (unit_cost_aud >= 0),
    unit_cbm                        NUMERIC(10, 4) NOT NULL CHECK (unit_cbm > 0),
    unit_freight_aud                NUMERIC(15, 2) NOT NULL DEFAULT 0.00 CHECK (unit_freight_aud >= 0),
    calculated_landed_cost_aud      NUMERIC(15, 2) NOT NULL CHECK (calculated_landed_cost_aud >= 0),
    created_at                      TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP
);
CREATE INDEX IF NOT EXISTS idx_stock_order_items_order ON stock_order_items(stock_order_id);
CREATE INDEX IF NOT EXISTS idx_stock_order_items_variant ON stock_order_items(variant_id);

-- ----------------------------------------------------------------
-- 4. fifo_lots — Quản lý Lô Hàng FIFO
-- ----------------------------------------------------------------
CREATE TABLE IF NOT EXISTS fifo_lots (
    id                      BIGSERIAL PRIMARY KEY,
    lot_number              VARCHAR(60) NOT NULL UNIQUE,
    variant_id              BIGINT NOT NULL REFERENCES product_variants(id) ON DELETE RESTRICT,
    warehouse_id            BIGINT NOT NULL REFERENCES warehouses(id) ON DELETE RESTRICT,
    stock_order_item_id     BIGINT NOT NULL REFERENCES stock_order_items(id) ON DELETE RESTRICT,
    initial_quantity        INTEGER NOT NULL CHECK (initial_quantity > 0),
    remaining_quantity      INTEGER NOT NULL CHECK (remaining_quantity >= 0),
    unit_landed_cost_aud    NUMERIC(15, 2) NOT NULL CHECK (unit_landed_cost_aud >= 0),
    current_stage           SMALLINT NOT NULL DEFAULT 1 CHECK (current_stage BETWEEN 1 AND 5),
    received_date           DATE NOT NULL,
    status                  VARCHAR(20) NOT NULL DEFAULT 'ACTIVE',
    -- 'QUEUED', 'ACTIVE', 'EXHAUSTED'
    activated_at            TIMESTAMPTZ,
    exhausted_at            TIMESTAMPTZ,
    created_at              TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT chk_remaining_lte_initial CHECK (remaining_quantity <= initial_quantity)
);
CREATE INDEX IF NOT EXISTS idx_fifo_lots_active ON fifo_lots(variant_id, warehouse_id, status);
CREATE INDEX IF NOT EXISTS idx_fifo_lots_order_item ON fifo_lots(stock_order_item_id, received_date ASC);

-- FIFO-dependent price tables are created after fifo_lots so their foreign keys
-- can be declared inline in the owning table definitions.
CREATE TABLE IF NOT EXISTS fifo_lot_stage_prices (
    id                  BIGSERIAL PRIMARY KEY,
    fifo_lot_id         BIGINT NOT NULL REFERENCES fifo_lots(id) ON DELETE CASCADE,
    stage               SMALLINT NOT NULL CHECK (stage BETWEEN 1 AND 5),
    price_aud           NUMERIC(15, 2) NOT NULL CHECK (price_aud >= 0),
    vip_price_aud       NUMERIC(15, 2) CHECK (vip_price_aud >= 0),
    updated_at          TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT uq_lot_stage_price UNIQUE (fifo_lot_id, stage)
);
CREATE INDEX IF NOT EXISTS idx_stage_prices_lookup ON fifo_lot_stage_prices(fifo_lot_id, stage);

CREATE TABLE IF NOT EXISTS stage_price_audit_logs (
    id                  BIGSERIAL PRIMARY KEY,
    fifo_lot_id         BIGINT NOT NULL REFERENCES fifo_lots(id) ON DELETE CASCADE,
    old_stage           SMALLINT NOT NULL,
    new_stage           SMALLINT NOT NULL,
    old_price_aud       NUMERIC(15, 2) NOT NULL,
    new_price_aud       NUMERIC(15, 2) NOT NULL,
    triggered_by        VARCHAR(30) NOT NULL,
    triggered_by_user   BIGINT REFERENCES users(id) ON DELETE SET NULL,
    remaining_stock_qty INTEGER NOT NULL,
    remaining_stock_pct NUMERIC(5, 2) NOT NULL,
    days_at_old_stage   INTEGER NOT NULL,
    reason              TEXT,
    changed_at          TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP
);
CREATE INDEX IF NOT EXISTS idx_price_audit_lot ON stage_price_audit_logs(fifo_lot_id, changed_at);
