-- ================================================================
-- MIGRATION 008: Marketing, Commissions & Ad Spend
-- Tables: promotions, promotion_products, ad_spend_logs,
--         sales_shifts, sales_commissions
-- ================================================================

-- ----------------------------------------------------------------
-- 1. promotions & promotion_products — Chiến dịch Giảm giá
-- ----------------------------------------------------------------
CREATE TABLE IF NOT EXISTS promotions (
    id                  BIGSERIAL PRIMARY KEY,
    name                VARCHAR(150) NOT NULL,
    badge_label         VARCHAR(50),
    -- 'Hot Deal', 'Clearance', 'Noel', 'EOFY Sale'
    discount_type       VARCHAR(20) NOT NULL,
    -- PERCENTAGE, FIXED_AMOUNT
    discount_value      NUMERIC(15, 2) NOT NULL CHECK (discount_value > 0),
    start_date          TIMESTAMPTZ NOT NULL,
    end_date            TIMESTAMPTZ NOT NULL,
    is_active           BOOLEAN DEFAULT TRUE,
    created_at          TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT chk_promotion_dates CHECK (start_date <= end_date)
);

CREATE TABLE IF NOT EXISTS promotion_products (
    promotion_id        BIGINT NOT NULL REFERENCES promotions(id) ON DELETE CASCADE,
    product_id          BIGINT NOT NULL REFERENCES products(id) ON DELETE CASCADE,
    PRIMARY KEY (promotion_id, product_id)
);

-- ----------------------------------------------------------------
-- 2. product_combos & combo_items — Gói sản phẩm
-- ----------------------------------------------------------------
CREATE TABLE IF NOT EXISTS product_combos (
    id                  BIGSERIAL PRIMARY KEY,
    name                VARCHAR(150) NOT NULL,
    code                VARCHAR(50) NOT NULL UNIQUE,
    combo_price_aud     NUMERIC(15, 2) NOT NULL CHECK (combo_price_aud >= 0),
    description         TEXT,
    is_active            BOOLEAN DEFAULT TRUE,
    created_at          TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS combo_items (
    id                  BIGSERIAL PRIMARY KEY,
    combo_id            BIGINT NOT NULL REFERENCES product_combos(id) ON DELETE CASCADE,
    variant_id          BIGINT NOT NULL REFERENCES product_variants(id) ON DELETE CASCADE,
    quantity            INTEGER NOT NULL DEFAULT 1 CHECK (quantity > 0)
);
CREATE INDEX IF NOT EXISTS idx_combo_items_combo ON combo_items(combo_id);
CREATE INDEX IF NOT EXISTS idx_combo_items_variant ON combo_items(variant_id);

-- ----------------------------------------------------------------
-- 3. ad_spend_logs — Nhật ký chi phí quảng cáo Facebook / Google
-- ----------------------------------------------------------------
CREATE TABLE IF NOT EXISTS ad_spend_logs (
    id                  BIGSERIAL PRIMARY KEY,
    campaign_name       VARCHAR(200) NOT NULL,
    platform            VARCHAR(50) NOT NULL,
    -- FACEBOOK, GOOGLE, TIKTOK, OTHER
    spend_aud           NUMERIC(15, 2) NOT NULL CHECK (spend_aud >= 0),
    start_date          DATE NOT NULL,
    end_date            DATE NOT NULL,
    created_at          TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT chk_ad_spend_dates CHECK (start_date <= end_date)
);

-- ----------------------------------------------------------------
-- 3. sales_shifts — Chấm công giờ làm việc Nhân viên Bán hàng
-- ----------------------------------------------------------------
CREATE TABLE IF NOT EXISTS sales_shifts (
    id                  BIGSERIAL PRIMARY KEY,
    user_id             BIGINT NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    store_id            BIGINT NOT NULL REFERENCES stores(id) ON DELETE RESTRICT,
    work_date           DATE NOT NULL,
    hours_worked        NUMERIC(6, 2) NOT NULL CHECK (hours_worked >= 0),
    created_at          TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP
);
CREATE INDEX IF NOT EXISTS idx_shifts_user_date ON sales_shifts(user_id, work_date);

-- ----------------------------------------------------------------
-- 4. sales_commissions — Hoa hồng Seller theo KPI & Superannuation 9.5%
-- ----------------------------------------------------------------
CREATE TABLE IF NOT EXISTS sales_commissions (
    id                      BIGSERIAL PRIMARY KEY,
    user_id                 BIGINT NOT NULL REFERENCES users(id) ON DELETE RESTRICT,
    period_start            DATE NOT NULL,
    period_end              DATE NOT NULL,
    total_sales_aud         NUMERIC(15, 2) NOT NULL DEFAULT 0.00 CHECK (total_sales_aud >= 0),
    total_hours_worked      NUMERIC(8, 2) NOT NULL DEFAULT 0.00 CHECK (total_hours_worked >= 0),
    sales_per_hour_aud      NUMERIC(15, 2) NOT NULL DEFAULT 0.00 CHECK (sales_per_hour_aud >= 0),
    kpi_threshold_aud       NUMERIC(15, 2) NOT NULL DEFAULT 0.00 CHECK (kpi_threshold_aud >= 0),
    target_sales_aud        NUMERIC(15, 2) NOT NULL DEFAULT 0.00 CHECK (target_sales_aud >= 0),
    excess_sales_aud        NUMERIC(15, 2) NOT NULL DEFAULT 0.00 CHECK (excess_sales_aud >= 0),
    commission_rate_pct     NUMERIC(5, 2) NOT NULL DEFAULT 0.00 CHECK (commission_rate_pct >= 0),
    gross_commission_aud    NUMERIC(15, 2) NOT NULL DEFAULT 0.00 CHECK (gross_commission_aud >= 0),
    superannuation_pct      NUMERIC(5, 2) NOT NULL DEFAULT 9.50 CHECK (superannuation_pct >= 0),
    superannuation_aud      NUMERIC(15, 2) NOT NULL DEFAULT 0.00 CHECK (superannuation_aud >= 0),
    net_commission_aud      NUMERIC(15, 2) NOT NULL DEFAULT 0.00 CHECK (net_commission_aud >= 0),
    status                  VARCHAR(30) NOT NULL DEFAULT 'DRAFT',
    -- DRAFT, FINALIZED, PAID
    created_at              TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT chk_commission_period CHECK (period_start <= period_end)
);
CREATE INDEX IF NOT EXISTS idx_commissions_user_period ON sales_commissions(user_id, period_start, period_end);
