-- ================================================================
-- MIGRATION 001: Core & Infrastructure Tables
-- Tables: postcodes, addresses, stores, warehouses,
--         store_warehouses, postcode_stores, store_shipping_rates
-- ================================================================

CREATE EXTENSION IF NOT EXISTS pg_trgm;
CREATE EXTENSION IF NOT EXISTS btree_gist;

-- ----------------------------------------------------------------
-- 1. postcodes — Danh mục Bưu chính Úc (Postcode-Gated Pricing)
-- ----------------------------------------------------------------
CREATE TABLE IF NOT EXISTS postcodes (
    id                  BIGSERIAL PRIMARY KEY,
    postcode            VARCHAR(10) NOT NULL UNIQUE,
    suburb_name         VARCHAR(100) NOT NULL,
    state               VARCHAR(10) NOT NULL,
    description         TEXT
);

-- ----------------------------------------------------------------
-- 2. addresses — Địa chỉ chuẩn hóa (tham chiếu postcodes)
-- ----------------------------------------------------------------
CREATE TABLE IF NOT EXISTS addresses (
    id                  BIGSERIAL PRIMARY KEY,
    street_address      TEXT NOT NULL,
    address_detail      TEXT,
    suburb              VARCHAR(100) NOT NULL,
    city                VARCHAR(100) NOT NULL,
    state               VARCHAR(10) NOT NULL,
    postcode_id         BIGINT NOT NULL REFERENCES postcodes(id) ON DELETE RESTRICT,
    latitude            NUMERIC(10, 7),
    longitude           NUMERIC(10, 7),
    created_at          TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    updated_at          TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP
);
CREATE INDEX IF NOT EXISTS idx_addresses_postcode ON addresses(postcode_id);
CREATE INDEX IF NOT EXISTS idx_addresses_coords ON addresses(latitude, longitude);

-- ----------------------------------------------------------------
-- 3. stores — Cửa hàng / Showroom trưng bày
-- ----------------------------------------------------------------
CREATE TABLE IF NOT EXISTS stores (
    id                  BIGSERIAL PRIMARY KEY,
    code                VARCHAR(30) NOT NULL UNIQUE,
    name                VARCHAR(150) NOT NULL,
    address_id          BIGINT NOT NULL REFERENCES addresses(id) ON DELETE RESTRICT,
    phone               VARCHAR(30),
    email               VARCHAR(100),
    is_active           BOOLEAN DEFAULT TRUE,
    created_at          TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    updated_at          TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP
);

-- ----------------------------------------------------------------
-- 4. warehouses — Kho hàng (Kho tổng & Kho tại Showroom)
-- ----------------------------------------------------------------
CREATE TABLE IF NOT EXISTS warehouses (
    id                  BIGSERIAL PRIMARY KEY,
    code                VARCHAR(30) NOT NULL UNIQUE,
    name                VARCHAR(150) NOT NULL,
    address_id          BIGINT NOT NULL REFERENCES addresses(id) ON DELETE RESTRICT,
    is_store            BOOLEAN DEFAULT FALSE,
    total_capacity_cbm  NUMERIC(12, 4) CHECK (total_capacity_cbm >= 0),
    is_active           BOOLEAN DEFAULT TRUE,
    created_at          TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    updated_at          TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP
);

-- ----------------------------------------------------------------
-- 5. store_warehouses — Quan hệ Nhiều-Nhiều Cửa hàng <-> Kho
-- ----------------------------------------------------------------
CREATE TABLE IF NOT EXISTS store_warehouses (
    store_id            BIGINT NOT NULL REFERENCES stores(id) ON DELETE CASCADE,
    warehouse_id        BIGINT NOT NULL REFERENCES warehouses(id) ON DELETE CASCADE,
    priority            SMALLINT DEFAULT 1 CHECK (priority > 0),
    PRIMARY KEY (store_id, warehouse_id)
);

-- ----------------------------------------------------------------
-- 6. postcode_stores — Gán Postcode cho Cửa hàng phục vụ
-- ----------------------------------------------------------------
CREATE TABLE IF NOT EXISTS postcode_stores (
    postcode_id         BIGINT NOT NULL REFERENCES postcodes(id) ON DELETE CASCADE,
    store_id            BIGINT NOT NULL REFERENCES stores(id) ON DELETE CASCADE,
    PRIMARY KEY (postcode_id, store_id)
);
CREATE INDEX IF NOT EXISTS idx_postcode_stores_lookup ON postcode_stores(postcode_id, store_id);

-- ----------------------------------------------------------------
-- 7. store_shipping_rates — Cước vận chuyển theo bậc thang km
-- ----------------------------------------------------------------
CREATE TABLE IF NOT EXISTS store_shipping_rates (
    id                  BIGSERIAL PRIMARY KEY,
    store_id            BIGINT NOT NULL REFERENCES stores(id) ON DELETE CASCADE,
    min_distance_km     NUMERIC(8, 2) NOT NULL CHECK (min_distance_km >= 0),
    max_distance_km     NUMERIC(8, 2) NOT NULL,
    shipping_fee_aud    NUMERIC(15, 2) NOT NULL CHECK (shipping_fee_aud >= 0),
    created_at          TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT chk_distance_range CHECK (max_distance_km > min_distance_km),
    CONSTRAINT uq_shipping_rate_no_overlap EXCLUDE USING gist (
        store_id WITH =,
        numrange(min_distance_km, max_distance_km, '[)') WITH &&
    )
);
