-- ================================================================
-- SEED DATA: Default Roles, Admin User & Sample Data
-- NOTE: Password hash là bcrypt hash của 'Admin123!'
-- ================================================================

-- ----------------------------------------------------------------
-- 1. Seed Roles (RBAC)
-- ----------------------------------------------------------------
INSERT INTO roles (code, name, description) VALUES
    ('SUPER_ADMIN',     'Super Administrator',      'Quyền toàn hệ thống, không giới hạn store'),
    ('STORE_MANAGER',   'Store Manager',            'Quản lý cửa hàng và nhân viên tại chi nhánh'),
    ('SALESPERSON',     'Salesperson / POS Cashier','Nhân viên bán hàng tại quầy POS'),
    ('WAREHOUSE_STAFF', 'Warehouse Staff',          'Nhân viên quản lý kho hàng'),
    ('DISPATCHER',      'Delivery Dispatcher',      'Điều phối viên giao hàng'),
    ('MARKETER',        'Marketing Staff',          'Nhân viên marketing & promotion'),
    ('ACCOUNTANT',      'Accountant',               'Kế toán, xem báo cáo tài chính'),
    ('CS_AGENT',        'Customer Service Agent',   'Nhân viên CSKH, trực live chat')
ON CONFLICT (code) DO NOTHING;

-- ----------------------------------------------------------------
-- 2. Seed Postcodes mẫu (Úc - các bang chính)
-- ----------------------------------------------------------------
INSERT INTO postcodes (postcode, suburb_name, state, description) VALUES
    ('6000', 'Perth CBD',       'WA',  'Perth CBD - Cửa hàng Perth phục vụ'),
    ('6001', 'Perth GPO',       'WA',  'Perth GPO Area'),
    ('6005', 'West Perth',      'WA',  'West Perth'),
    ('6100', 'Burswood',        'WA',  'Burswood / East Perth'),
    ('2000', 'Sydney CBD',      'NSW', 'Sydney CBD - Cửa hàng Sydney phục vụ'),
    ('2010', 'Surry Hills',     'NSW', 'Surry Hills'),
    ('2060', 'North Sydney',    'NSW', 'North Sydney'),
    ('3000', 'Melbourne CBD',   'VIC', 'Melbourne CBD - Cửa hàng Melbourne phục vụ'),
    ('3004', 'St Kilda Road',   'VIC', 'St Kilda Road / South Yarra'),
    ('4102', 'Woolloongabba',   'QLD', 'Brisbane warehouse area')
ON CONFLICT (postcode) DO NOTHING;

-- ----------------------------------------------------------------
-- 3. Seed Addresses mẫu
-- ----------------------------------------------------------------
INSERT INTO addresses (street_address, suburb, city, state, postcode_id, latitude, longitude)
SELECT seed.street_address, seed.suburb, seed.city, seed.state, p.id, seed.latitude, seed.longitude
FROM (VALUES
    ('123 Murray Street', 'Perth', 'Perth', 'WA', '6000', -31.9523::numeric, 115.8613::numeric),
    ('456 George Street', 'Sydney', 'Sydney', 'NSW', '2000', -33.8688::numeric, 151.2093::numeric),
    ('789 Collins Street', 'Melbourne', 'Melbourne', 'VIC', '3000', -37.8136::numeric, 144.9631::numeric),
    ('164 Kewdale Road', 'Kewdale', 'Perth', 'WA', '6100', -31.9751::numeric, 115.9416::numeric),
    ('171 Logan Road', 'Woolloongabba', 'Brisbane', 'QLD', '4102', -27.4975::numeric, 153.0356::numeric)
) AS seed(street_address, suburb, city, state, postcode, latitude, longitude)
JOIN postcodes p ON p.postcode = seed.postcode
WHERE NOT EXISTS (
    SELECT 1 FROM addresses a
    WHERE a.street_address = seed.street_address
      AND a.postcode_id = p.id
);

-- ----------------------------------------------------------------
-- 4. Seed Stores mẫu
-- ----------------------------------------------------------------
INSERT INTO stores (code, name, address_id, phone, email) VALUES
    ('STR-PER-01', 'VissSoft Furniture Perth Showroom',
        (SELECT id FROM addresses WHERE street_address = '123 Murray Street' LIMIT 1),
        '+61 8 9000 0001', 'perth@visssoft.com.au'),
    ('STR-SYD-01', 'VissSoft Furniture Sydney Showroom',
        (SELECT id FROM addresses WHERE street_address = '456 George Street' LIMIT 1),
        '+61 2 9000 0001', 'sydney@visssoft.com.au'),
    ('STR-MEL-01', 'VissSoft Furniture Melbourne Showroom',
        (SELECT id FROM addresses WHERE street_address = '789 Collins Street' LIMIT 1),
        '+61 3 9000 0001', 'melbourne@visssoft.com.au')
ON CONFLICT (code) DO NOTHING;

-- ----------------------------------------------------------------
-- 5. Seed Warehouses mẫu (WH-164, WH-171, WH-53)
-- ----------------------------------------------------------------
INSERT INTO warehouses (code, name, address_id, is_store, total_capacity_cbm) VALUES
    ('WH-164', 'Kho Tổng Perth 164 Kewdale',
        (SELECT id FROM addresses WHERE street_address = '164 Kewdale Road' LIMIT 1),
        FALSE, 2000.0000),
    ('WH-171', 'Kho Tổng Brisbane 171 Logan',
        (SELECT id FROM addresses WHERE street_address = '171 Logan Road' LIMIT 1),
        FALSE, 1500.0000),
    ('WH-STR-PER-01', 'Kho tại Showroom Perth',
        (SELECT id FROM addresses WHERE street_address = '123 Murray Street' LIMIT 1),
        TRUE, 300.0000)
ON CONFLICT (code) DO NOTHING;

-- ----------------------------------------------------------------
-- 6. Seed Store-Warehouse liên kết
-- ----------------------------------------------------------------
INSERT INTO store_warehouses (store_id, warehouse_id, priority) VALUES
    ((SELECT id FROM stores WHERE code = 'STR-PER-01'),
     (SELECT id FROM warehouses WHERE code = 'WH-164'), 1),
    ((SELECT id FROM stores WHERE code = 'STR-PER-01'),
     (SELECT id FROM warehouses WHERE code = 'WH-STR-PER-01'), 2)
ON CONFLICT DO NOTHING;

-- ----------------------------------------------------------------
-- 7. Seed Postcode -> Store mapping mẫu (Perth postcodes -> STR-PER-01)
-- ----------------------------------------------------------------
INSERT INTO postcode_stores (postcode_id, store_id) VALUES
    ((SELECT id FROM postcodes WHERE postcode = '6000'),
     (SELECT id FROM stores WHERE code = 'STR-PER-01')),
    ((SELECT id FROM postcodes WHERE postcode = '6001'),
     (SELECT id FROM stores WHERE code = 'STR-PER-01')),
    ((SELECT id FROM postcodes WHERE postcode = '6005'),
     (SELECT id FROM stores WHERE code = 'STR-PER-01')),
    ((SELECT id FROM postcodes WHERE postcode = '6100'),
     (SELECT id FROM stores WHERE code = 'STR-PER-01')),
    ((SELECT id FROM postcodes WHERE postcode = '2000'),
     (SELECT id FROM stores WHERE code = 'STR-SYD-01')),
    ((SELECT id FROM postcodes WHERE postcode = '3000'),
     (SELECT id FROM stores WHERE code = 'STR-MEL-01'))
ON CONFLICT DO NOTHING;

-- ----------------------------------------------------------------
-- 8. Seed Shipping Rates mẫu (Store Perth)
-- ----------------------------------------------------------------
INSERT INTO store_shipping_rates (store_id, min_distance_km, max_distance_km, shipping_fee_aud)
SELECT s.id, 0, 20, 45.00 FROM stores s WHERE s.code = 'STR-PER-01'
ON CONFLICT DO NOTHING;

INSERT INTO store_shipping_rates (store_id, min_distance_km, max_distance_km, shipping_fee_aud)
SELECT s.id, 20, 50, 75.00 FROM stores s WHERE s.code = 'STR-PER-01'
ON CONFLICT DO NOTHING;

INSERT INTO store_shipping_rates (store_id, min_distance_km, max_distance_km, shipping_fee_aud)
SELECT s.id, 50, 100, 120.00 FROM stores s WHERE s.code = 'STR-PER-01'
ON CONFLICT DO NOTHING;

INSERT INTO store_shipping_rates (store_id, min_distance_km, max_distance_km, shipping_fee_aud)
SELECT s.id, 100, 200, 180.00 FROM stores s WHERE s.code = 'STR-PER-01'
ON CONFLICT DO NOTHING;

-- ----------------------------------------------------------------
-- 9. Seed Product Categories (4 nhóm chính)
-- ----------------------------------------------------------------
INSERT INTO product_categories (name, slug, description, display_order) VALUES
    ('Living Room', 'living-room', 'Nội thất phòng khách: Sofa, Kệ TV, Bàn cà phê', 1),
    ('Dining Room', 'dining-room', 'Nội thất phòng ăn: Bàn ăn, Ghế ăn, Tủ buffet', 2),
    ('Bedroom',     'bedroom',    'Nội thất phòng ngủ: Giường, Tủ quần áo, Bàn đầu giường', 3),
    ('Outdoor',     'outdoor',    'Nội thất sân vườn: Bộ bàn ghế ngoài trời, Ghế bãi biển', 4)
ON CONFLICT (slug) DO NOTHING;
