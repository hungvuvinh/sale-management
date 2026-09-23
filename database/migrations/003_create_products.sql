-- ================================================================
-- MIGRATION 003: Products & 5-Stage Pricing Tables
-- Tables: product_categories, products, product_variants,
--         fifo_lot_stage_prices, pricing_stage_configs,
--         stage_price_audit_logs
-- ================================================================

-- ----------------------------------------------------------------
-- 1. product_categories — Danh mục 4 nhóm (Living/Dining/Bedroom/Outdoor)
-- ----------------------------------------------------------------
CREATE TABLE IF NOT EXISTS product_categories (
    id                  BIGSERIAL PRIMARY KEY,
    parent_id           BIGINT REFERENCES product_categories(id) ON DELETE SET NULL,
    name                VARCHAR(150) NOT NULL,
    slug                VARCHAR(150) NOT NULL UNIQUE,
    description         TEXT,
    display_order       INTEGER DEFAULT 0,
    is_active           BOOLEAN DEFAULT TRUE
);
CREATE INDEX IF NOT EXISTS idx_product_categories_slug ON product_categories(slug);

-- ----------------------------------------------------------------
-- 2. products — Sản phẩm cha (Parent Product)
-- ----------------------------------------------------------------
CREATE TABLE IF NOT EXISTS products (
    id                  BIGSERIAL PRIMARY KEY,
    sku                 VARCHAR(50) NOT NULL UNIQUE,
    name                VARCHAR(255) NOT NULL,
    slug                VARCHAR(255) NOT NULL UNIQUE,
    category_id         BIGINT REFERENCES product_categories(id) ON DELETE SET NULL,
    description         TEXT,
    materials_summary   TEXT,
    main_image_url      TEXT,
    gallery_images      JSONB DEFAULT '[]'::jsonb,
    video_url           TEXT,
    is_active           BOOLEAN DEFAULT TRUE,
    created_at          TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    updated_at          TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP
);
CREATE INDEX IF NOT EXISTS idx_products_sku ON products(sku);
CREATE INDEX IF NOT EXISTS idx_products_search_trgm ON products USING gin (name gin_trgm_ops, sku gin_trgm_ops);

-- ----------------------------------------------------------------
-- 3. product_variants — Biến thể con (SKU con)
-- ----------------------------------------------------------------
CREATE TABLE IF NOT EXISTS product_variants (
    id                  BIGSERIAL PRIMARY KEY,
    product_id          BIGINT NOT NULL REFERENCES products(id) ON DELETE CASCADE,
    sku                 VARCHAR(60) NOT NULL UNIQUE,
    name                VARCHAR(255) NOT NULL,
    barcode             VARCHAR(50) UNIQUE,
    attributes_json     JSONB NOT NULL DEFAULT '{}',
    weight_kg           NUMERIC(10, 3) NOT NULL CHECK (weight_kg > 0),
    cbm                 NUMERIC(10, 4) NOT NULL CHECK (cbm > 0),
    box_count           INTEGER NOT NULL DEFAULT 1 CHECK (box_count >= 1),
    dimensions_cm       JSONB,
    is_active           BOOLEAN DEFAULT TRUE,
    created_at          TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    updated_at          TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP
);
CREATE INDEX IF NOT EXISTS idx_variants_product ON product_variants(product_id);
CREATE INDEX IF NOT EXISTS idx_variants_attrs ON product_variants USING gin(attributes_json);

-- Store-specific daily prices. This is part of the base product schema so a
-- fresh database does not need a later rename/drop migration.
CREATE TABLE IF NOT EXISTS store_variant_prices (
    id                  BIGSERIAL PRIMARY KEY,
    store_id            BIGINT NOT NULL REFERENCES stores(id) ON DELETE CASCADE,
    variant_id          BIGINT NOT NULL REFERENCES product_variants(id) ON DELETE CASCADE,
    current_daily_price NUMERIC(15, 2) NOT NULL DEFAULT 0.00,
    updated_at          TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT uk_store_variant UNIQUE (store_id, variant_id)
);
CREATE INDEX IF NOT EXISTS idx_store_variant_prices ON store_variant_prices(store_id, variant_id);

-- ----------------------------------------------------------------
-- 4. pricing_stage_configs — Cấu hình quy tắc tự động chuyển Stage
-- ----------------------------------------------------------------
CREATE TABLE IF NOT EXISTS pricing_stage_configs (
    id                  BIGSERIAL PRIMARY KEY,
    variant_id          BIGINT NOT NULL REFERENCES product_variants(id) ON DELETE CASCADE,
    from_stage          SMALLINT NOT NULL CHECK (from_stage BETWEEN 1 AND 4),
    to_stage            SMALLINT NOT NULL CHECK (to_stage BETWEEN 2 AND 5),
    target_stock_pct    NUMERIC(5, 2) NOT NULL CHECK (target_stock_pct >= 0 AND target_stock_pct <= 100),
    min_days_at_stage   INTEGER NOT NULL DEFAULT 14 CHECK (min_days_at_stage >= 0),
    max_days_at_stage   INTEGER NOT NULL DEFAULT 60 CHECK (max_days_at_stage >= min_days_at_stage),
    CONSTRAINT uq_variant_stage_transition UNIQUE (variant_id, from_stage)
);

-- ----------------------------------------------------------------
-- ----------------------------------------------------------------
-- 5. product_associations — Gợi ý sản phẩm liên quan
-- ----------------------------------------------------------------
CREATE TABLE IF NOT EXISTS product_associations (
    product_id              BIGINT NOT NULL REFERENCES products(id) ON DELETE CASCADE,
    associated_product_id   BIGINT NOT NULL REFERENCES products(id) ON DELETE CASCADE,
    association_type        VARCHAR(30) NOT NULL DEFAULT 'CROSS_SELL',
    PRIMARY KEY (product_id, associated_product_id)
);
