-- ================================================================
-- MIGRATION 005: Warehouse Inventory & Transactions
-- Tables: warehouse_inventory, inventory_transactions,
--         inventory_transfers, inventory_transfer_items
-- ================================================================

-- ----------------------------------------------------------------
-- 1. warehouse_inventory — Số dư tồn kho tổng hợp tại từng kho
-- ----------------------------------------------------------------
CREATE TABLE IF NOT EXISTS warehouse_inventory (
    warehouse_id        BIGINT NOT NULL REFERENCES warehouses(id) ON DELETE CASCADE,
    variant_id          BIGINT NOT NULL REFERENCES product_variants(id) ON DELETE RESTRICT,
    on_hand_quantity    INTEGER NOT NULL DEFAULT 0 CHECK (on_hand_quantity >= 0),
    reserved_quantity   INTEGER NOT NULL DEFAULT 0 CHECK (reserved_quantity >= 0),
    available_quantity  INTEGER GENERATED ALWAYS AS (on_hand_quantity - reserved_quantity) STORED,
    low_stock_threshold INTEGER NOT NULL DEFAULT 5 CHECK (low_stock_threshold >= 0),
    PRIMARY KEY (warehouse_id, variant_id),
    CONSTRAINT chk_reserved_lte_on_hand CHECK (reserved_quantity <= on_hand_quantity),
    CONSTRAINT chk_available_non_negative CHECK (on_hand_quantity - reserved_quantity >= 0)
);
CREATE INDEX IF NOT EXISTS idx_wh_inventory_lookup ON warehouse_inventory(variant_id, warehouse_id);

-- ----------------------------------------------------------------
-- 2. inventory_transactions — Sổ cái giao dịch kho
-- ----------------------------------------------------------------
CREATE TABLE IF NOT EXISTS inventory_transactions (
    id                  BIGSERIAL PRIMARY KEY,
    transaction_code    VARCHAR(50) NOT NULL UNIQUE,
    warehouse_id        BIGINT NOT NULL REFERENCES warehouses(id) ON DELETE RESTRICT,
    variant_id          BIGINT NOT NULL REFERENCES product_variants(id) ON DELETE RESTRICT,
    fifo_lot_id         BIGINT REFERENCES fifo_lots(id) ON DELETE SET NULL,
    transaction_type    VARCHAR(30) NOT NULL,
    -- STOCK_IN, SALE_OUT, TRANSFER_OUT, TRANSFER_IN, ADJUSTMENT, RETURN_IN
    change_quantity     INTEGER NOT NULL,
    reference_id        BIGINT,
    reference_type      VARCHAR(50),
    performed_by_user   BIGINT REFERENCES users(id) ON DELETE SET NULL,
    notes               TEXT,
    created_at          TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP
);
CREATE INDEX IF NOT EXISTS idx_inv_tx_lookup ON inventory_transactions(warehouse_id, variant_id, created_at);

-- ----------------------------------------------------------------
-- 3. inventory_transfers — Phiếu điều chuyển kho
-- ----------------------------------------------------------------
CREATE TABLE IF NOT EXISTS inventory_transfers (
    id                  BIGSERIAL PRIMARY KEY,
    transfer_code       VARCHAR(50) NOT NULL UNIQUE,
    from_warehouse_id   BIGINT NOT NULL REFERENCES warehouses(id) ON DELETE RESTRICT,
    to_warehouse_id     BIGINT NOT NULL REFERENCES warehouses(id) ON DELETE RESTRICT,
    status              VARCHAR(30) NOT NULL DEFAULT 'REQUESTED',
    -- REQUESTED, APPROVED, IN_TRANSIT, RECEIVED, CANCELLED
    requested_by        BIGINT REFERENCES users(id) ON DELETE SET NULL,
    approved_by         BIGINT REFERENCES users(id) ON DELETE SET NULL,
    created_at          TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    received_at         TIMESTAMPTZ
);

-- ----------------------------------------------------------------
-- 4. inventory_transfer_items — Chi tiết hàng hóa trong phiếu chuyển kho
-- ----------------------------------------------------------------
CREATE TABLE IF NOT EXISTS inventory_transfer_items (
    id                  BIGSERIAL PRIMARY KEY,
    transfer_id         BIGINT NOT NULL REFERENCES inventory_transfers(id) ON DELETE CASCADE,
    variant_id          BIGINT NOT NULL REFERENCES product_variants(id) ON DELETE RESTRICT,
    quantity_requested  INTEGER NOT NULL CHECK (quantity_requested > 0),
    quantity_received   INTEGER DEFAULT 0 CHECK (quantity_received >= 0)
);
