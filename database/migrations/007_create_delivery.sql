-- ================================================================
-- MIGRATION 007: Delivery System
-- Tables: carriers, drivers, delivery_bookings,
--         delivery_routes, delivery_stops
-- ================================================================

-- ----------------------------------------------------------------
-- 1. carriers — Đối tác Vận tải (TNT, Startrack, nội bộ)
-- ----------------------------------------------------------------
CREATE TABLE IF NOT EXISTS carriers (
    id                  BIGSERIAL PRIMARY KEY,
    name                VARCHAR(150) NOT NULL,
    code                VARCHAR(50) NOT NULL UNIQUE,
    phone               VARCHAR(30),
    email               VARCHAR(100),
    is_internal         BOOLEAN DEFAULT FALSE,
    rate_card_json      JSONB,
    is_active           BOOLEAN DEFAULT TRUE,
    created_at          TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP
);

-- ----------------------------------------------------------------
-- 2. drivers — Tài xế giao hàng
-- ----------------------------------------------------------------
CREATE TABLE IF NOT EXISTS drivers (
    id                  BIGSERIAL PRIMARY KEY,
    carrier_id          BIGINT NOT NULL REFERENCES carriers(id) ON DELETE CASCADE,
    user_id             BIGINT REFERENCES users(id) ON DELETE SET NULL,
    full_name           VARCHAR(100) NOT NULL,
    phone               VARCHAR(30) NOT NULL,
    license_number      VARCHAR(50),
    is_active           BOOLEAN DEFAULT TRUE,
    created_at          TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP
);

-- ----------------------------------------------------------------
-- 3. delivery_bookings — Đặt lịch giao hàng & Yêu Cầu Đặc Biệt
-- ----------------------------------------------------------------
CREATE TABLE IF NOT EXISTS delivery_bookings (
    id                      BIGSERIAL PRIMARY KEY,
    order_id                BIGINT NOT NULL REFERENCES orders(id) ON DELETE CASCADE,
    delivery_address_id     BIGINT NOT NULL REFERENCES addresses(id) ON DELETE RESTRICT,
    scheduled_date          DATE NOT NULL,
    time_slot               VARCHAR(30) NOT NULL DEFAULT 'FLEXIBLE',
    -- MORNING (7am-12pm), AFTERNOON (12pm-5pm), EVENING (5pm-8pm), FLEXIBLE
    is_assembling           BOOLEAN NOT NULL DEFAULT FALSE,
    is_upstairs             BOOLEAN NOT NULL DEFAULT FALSE,
    stairs_floor_count      INTEGER DEFAULT 0 CHECK (stairs_floor_count >= 0),
    special_notes           TEXT,
    surcharge_aud           NUMERIC(15, 2) NOT NULL DEFAULT 0.00 CHECK (surcharge_aud >= 0),
    status                  VARCHAR(30) NOT NULL DEFAULT 'BOOKED',
    -- BOOKED, ASSIGNED, IN_TRANSIT, DELIVERED, COMEBACK, CANCELLED
    created_at              TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    updated_at              TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP
);
CREATE INDEX IF NOT EXISTS idx_delivery_bookings_date ON delivery_bookings(scheduled_date);
CREATE INDEX IF NOT EXISTS idx_delivery_bookings_order ON delivery_bookings(order_id);

-- ----------------------------------------------------------------
-- 4. delivery_routes — Lộ trình xe tải trong ngày
-- ----------------------------------------------------------------
CREATE TABLE IF NOT EXISTS delivery_routes (
    id                      BIGSERIAL PRIMARY KEY,
    route_code              VARCHAR(50) NOT NULL UNIQUE,
    carrier_id              BIGINT NOT NULL REFERENCES carriers(id) ON DELETE RESTRICT,
    driver_id               BIGINT REFERENCES drivers(id) ON DELETE SET NULL,
    origin_warehouse_id     BIGINT NOT NULL REFERENCES warehouses(id) ON DELETE RESTRICT,
    delivery_date           DATE NOT NULL,
    total_distance_km       NUMERIC(8, 2) DEFAULT 0.00 CHECK (total_distance_km >= 0),
    total_duration_mins     INTEGER DEFAULT 0 CHECK (total_duration_mins >= 0),
    status                  VARCHAR(30) NOT NULL DEFAULT 'PLANNED',
    -- PLANNED, IN_PROGRESS, COMPLETED, CANCELLED
    created_at              TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP
);
CREATE INDEX IF NOT EXISTS idx_delivery_routes_date ON delivery_routes(delivery_date, status);

-- ----------------------------------------------------------------
-- 5. delivery_stops — Điểm dừng trong lộ trình giao hàng
-- ----------------------------------------------------------------
CREATE TABLE IF NOT EXISTS delivery_stops (
    id                      BIGSERIAL PRIMARY KEY,
    route_id                BIGINT NOT NULL REFERENCES delivery_routes(id) ON DELETE CASCADE,
    booking_id              BIGINT NOT NULL REFERENCES delivery_bookings(id) ON DELETE RESTRICT,
    sequence_order          INTEGER NOT NULL CHECK (sequence_order > 0),
    estimated_arrival       TIMESTAMPTZ,
    actual_arrival          TIMESTAMPTZ,
    status                  VARCHAR(30) NOT NULL DEFAULT 'PENDING',
    -- PENDING, DONE, COMEBACK, FAILED
    proof_of_delivery_url   TEXT,
    customer_feedback       TEXT,
    failure_reason          TEXT
);
CREATE INDEX IF NOT EXISTS idx_delivery_stops_route ON delivery_stops(route_id, sequence_order);

-- Special delivery services and their per-booking charges.
CREATE TABLE IF NOT EXISTS delivery_service_rates (
    id                  BIGSERIAL PRIMARY KEY,
    code                VARCHAR(50) NOT NULL UNIQUE,
    name                VARCHAR(100) NOT NULL,
    fee_type            VARCHAR(30) NOT NULL DEFAULT 'FLAT',
    fee_aud             NUMERIC(15, 2) NOT NULL DEFAULT 0.00 CHECK (fee_aud >= 0),
    is_active           BOOLEAN DEFAULT TRUE,
    created_at          TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    updated_at          TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP
);

INSERT INTO delivery_service_rates (code, name, fee_type, fee_aud) VALUES
    ('UPSTAIRS_CARRY', 'Phụ phí vác lầu (per floor)', 'PER_FLOOR', 30.00),
    ('FURNITURE_ASSEMBLY', 'Phụ phí lắp ráp nội thất', 'FLAT', 50.00)
ON CONFLICT (code) DO NOTHING;

CREATE TABLE IF NOT EXISTS delivery_booking_services (
    id                  BIGSERIAL PRIMARY KEY,
    booking_id          BIGINT NOT NULL REFERENCES delivery_bookings(id) ON DELETE CASCADE,
    service_rate_id     BIGINT NOT NULL REFERENCES delivery_service_rates(id) ON DELETE RESTRICT,
    quantity            INTEGER NOT NULL DEFAULT 1 CHECK (quantity > 0),
    unit_fee_aud        NUMERIC(15, 2) NOT NULL CHECK (unit_fee_aud >= 0),
    line_surcharge_aud  NUMERIC(15, 2) NOT NULL CHECK (line_surcharge_aud >= 0),
    created_at          TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP
);
CREATE INDEX IF NOT EXISTS idx_booking_services_booking ON delivery_booking_services(booking_id);
CREATE INDEX IF NOT EXISTS idx_booking_services_rate ON delivery_booking_services(service_rate_id);
