-- MIGRATION 021: Seed data for Frontend operations (Postcodes, Customers, Orders & Order Items)

-- Seed additional postcodes if missing
INSERT INTO postcodes (postcode, suburb_name, state, description) VALUES
    ('2000', 'Sydney CBD', 'NSW', 'Central Sydney area'),
    ('3000', 'Melbourne CBD', 'VIC', 'Central Melbourne area')
ON CONFLICT (postcode) DO NOTHING;

-- Seed additional Customers
INSERT INTO customers (customer_code, first_name, last_name, phone, email, is_vip, loyalty_points)
VALUES
    ('CUST-JOHN-001', 'John', 'Smith', '+61 400 123 456', 'john.smith@example.com', TRUE, 250),
    ('CUST-SARAH-002', 'Sarah', 'Jenkins', '+61 411 987 654', 'sarah.j@example.com', FALSE, 50)
ON CONFLICT (customer_code) DO NOTHING;

-- Seed Orders
INSERT INTO orders (order_code, customer_id, store_id, order_channel, order_type, status, payment_status, subtotal_aud, shipping_fee_aud, total_aud, notes)
SELECT 'ORD-2026-001', c.id, s.id, 'POS', 'ORDER_NOW', 'CONFIRMED', 'PAID', 1499.00, 45.00, 1544.00, 'Deliver to 2nd floor, elevator available'
FROM stores s, customers c
WHERE s.code = 'STR-PER-01' AND c.customer_code = 'CUST-JOHN-001'
ON CONFLICT (order_code) DO NOTHING;

INSERT INTO orders (order_code, customer_id, store_id, order_channel, order_type, status, payment_status, subtotal_aud, shipping_fee_aud, total_aud, notes)
SELECT 'ORD-2026-002', c.id, s.id, 'POS', 'PRE_ORDER', 'PENDING', 'UNPAID', 1399.00, 50.00, 1449.00, 'Customer requested morning delivery'
FROM stores s, customers c
WHERE s.code = 'STR-PER-01' AND c.customer_code = 'CUST-SARAH-002'
ON CONFLICT (order_code) DO NOTHING;

-- Seed Order Items
INSERT INTO order_items (order_id, variant_id, quantity, unit_price_aud, stage_applied, is_vip_price, discount_aud, line_total_aud)
SELECT o.id, v.id, 1, 1499.00, 2, FALSE, 0.00, 1499.00
FROM orders o, product_variants v
WHERE o.order_code = 'ORD-2026-001' AND v.sku = 'SOFA-LUNA-GREY'
AND NOT EXISTS (SELECT 1 FROM order_items oi WHERE oi.order_id = o.id AND oi.variant_id = v.id);

INSERT INTO order_items (order_id, variant_id, quantity, unit_price_aud, stage_applied, is_vip_price, discount_aud, line_total_aud)
SELECT o.id, v.id, 1, 1399.00, 1, FALSE, 0.00, 1399.00
FROM orders o, product_variants v
WHERE o.order_code = 'ORD-2026-002' AND v.sku = 'BED-PRADO-KING-BLK'
AND NOT EXISTS (SELECT 1 FROM order_items oi WHERE oi.order_id = o.id AND oi.variant_id = v.id);
