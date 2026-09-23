-- MIGRATION 020: Product module demo data
-- Idempotent seed for catalog, variants, suppliers, stock orders, FIFO lots and prices.


INSERT INTO postcodes (postcode, suburb_name, state, description) VALUES
    ('4102', 'Woolloongabba', 'QLD', 'Brisbane warehouse area')
ON CONFLICT (postcode) DO NOTHING;

INSERT INTO addresses (street_address, suburb, city, state, postcode_id, latitude, longitude)
SELECT '123 Murray Street', 'Perth', 'Perth', 'WA', p.id, -31.9523, 115.8613
FROM postcodes p WHERE p.postcode = '6000'
  AND NOT EXISTS (SELECT 1 FROM addresses a WHERE a.street_address = '123 Murray Street');
INSERT INTO addresses (street_address, suburb, city, state, postcode_id, latitude, longitude)
SELECT '164 Kewdale Road', 'Kewdale', 'Perth', 'WA', p.id, -31.9751, 115.9416
FROM postcodes p WHERE p.postcode = '6100'
  AND NOT EXISTS (SELECT 1 FROM addresses a WHERE a.street_address = '164 Kewdale Road');
INSERT INTO addresses (street_address, suburb, city, state, postcode_id, latitude, longitude)
SELECT '171 Logan Road', 'Woolloongabba', 'Brisbane', 'QLD', p.id, -27.4975, 153.0356
FROM postcodes p WHERE p.postcode = '4102'
  AND NOT EXISTS (SELECT 1 FROM addresses a WHERE a.street_address = '171 Logan Road');

INSERT INTO stores (code, name, address_id, phone, email)
SELECT 'STR-PER-01', 'VissSoft Furniture Perth Showroom', a.id, '+61 8 9000 0001', 'perth@visssoft.com.au'
FROM addresses a WHERE a.street_address = '123 Murray Street'
ON CONFLICT (code) DO NOTHING;

INSERT INTO warehouses (code, name, address_id, is_store, total_capacity_cbm)
SELECT 'WH-164', 'Kho Tổng Perth 164 Kewdale', a.id, FALSE, 2000.0000
FROM addresses a WHERE a.street_address = '164 Kewdale Road'
ON CONFLICT (code) DO NOTHING;

INSERT INTO store_warehouses (store_id, warehouse_id, priority)
SELECT s.id, w.id, 1 FROM stores s, warehouses w
WHERE s.code = 'STR-PER-01' AND w.code = 'WH-164'
ON CONFLICT DO NOTHING;
INSERT INTO postcode_stores (postcode_id, store_id)
SELECT p.id, s.id FROM postcodes p, stores s
WHERE p.postcode = '6000' AND s.code = 'STR-PER-01'
ON CONFLICT DO NOTHING;

INSERT INTO suppliers (name, country, contact_name, contact_email, lead_time_days, payment_terms)
SELECT seed.name, seed.country, seed.contact_name, seed.contact_email, seed.lead_time_days, seed.payment_terms
FROM (VALUES
  ('Pacific Home Imports', 'Australia', 'Mia Carter', 'orders@pacifichome.example', 28, '30 days'),
  ('Oak & Co Manufacturing', 'Vietnam', 'Minh Tran', 'export@oakco.example', 45, '50% deposit')
) AS seed(name, country, contact_name, contact_email, lead_time_days, payment_terms)
WHERE NOT EXISTS (
  SELECT 1 FROM suppliers s WHERE s.name = seed.name AND s.country = seed.country
);

INSERT INTO product_categories (name, slug, description, display_order)
VALUES
    ('Living Room', 'living-room', 'Sofa and living room furniture', 1),
    ('Bedroom', 'bedroom', 'Beds and bedroom furniture', 2),
    ('Dining Room', 'dining-room', 'Dining tables and chairs', 3)
ON CONFLICT (slug) DO NOTHING;

INSERT INTO products (sku, name, slug, category_id, description, materials_summary, is_active)
SELECT 'SOFA-LUNA', 'Luna 3-Seater Sofa', 'luna-3-seater-sofa', c.id, 'Contemporary sofa for everyday living.', 'Performance fabric, kiln-dried timber', TRUE
FROM product_categories c WHERE c.slug = 'living-room'
ON CONFLICT (sku) DO NOTHING;
INSERT INTO products (sku, name, slug, category_id, description, materials_summary, is_active)
SELECT 'BED-PRADO', 'Prado King Bed Frame', 'prado-king-bed-frame', c.id, 'King bed frame with upholstered headboard.', 'Engineered timber, linen fabric', TRUE
FROM product_categories c WHERE c.slug = 'bedroom'
ON CONFLICT (sku) DO NOTHING;
INSERT INTO products (sku, name, slug, category_id, description, materials_summary, is_active)
SELECT 'DIN-MARLOW', 'Marlow Dining Set', 'marlow-dining-set', c.id, 'Dining table with four matching chairs.', 'Solid oak veneer, powder-coated steel', TRUE
FROM product_categories c WHERE c.slug = 'dining-room'
ON CONFLICT (sku) DO NOTHING;

INSERT INTO product_variants (product_id, sku, name, barcode, attributes_json, weight_kg, cbm, box_count, dimensions_cm)
SELECT p.id, 'SOFA-LUNA-GREY', 'Luna Grey', '931000000001', '{"color":"Grey","seats":3}', 62.000, 1.2500, 3, '{"width":220,"depth":95,"height":85}'
FROM products p WHERE p.sku = 'SOFA-LUNA'
  AND NOT EXISTS (SELECT 1 FROM product_variants v WHERE v.sku = 'SOFA-LUNA-GREY');
INSERT INTO product_variants (product_id, sku, name, barcode, attributes_json, weight_kg, cbm, box_count, dimensions_cm)
SELECT p.id, 'BED-PRADO-KING-BLK', 'Prado King Black', '931000000002', '{"size":"King","color":"Black"}', 78.000, 0.8500, 3, '{"width":195,"depth":215,"height":115}'
FROM products p WHERE p.sku = 'BED-PRADO'
  AND NOT EXISTS (SELECT 1 FROM product_variants v WHERE v.sku = 'BED-PRADO-KING-BLK');
INSERT INTO product_variants (product_id, sku, name, barcode, attributes_json, weight_kg, cbm, box_count, dimensions_cm)
SELECT p.id, 'DIN-MARLOW-OAK', 'Marlow Oak Set', '931000000003', '{"seats":4,"color":"Natural Oak"}', 88.000, 1.1000, 5, '{"width":180,"depth":90,"height":76}'
FROM products p WHERE p.sku = 'DIN-MARLOW'
  AND NOT EXISTS (SELECT 1 FROM product_variants v WHERE v.sku = 'DIN-MARLOW-OAK');

INSERT INTO stock_orders (po_number, supplier_id, destination_warehouse_id, container_code, eta_date, actual_arrival_date, status, total_cbm, container_freight_aud, customs_tax_aud, currency_code, exchange_rate, notes)
SELECT 'PO-DEMO-001', s.id, w.id, 'CONT-DEMO-SEP', CURRENT_DATE - 12, CURRENT_DATE - 2, 'RECEIVED', 18.4000, 1850.00, 420.00, 'USD', 1.52, 'Demo received container'
FROM suppliers s, warehouses w WHERE s.name = 'Pacific Home Imports' AND w.code = 'WH-164'
  AND NOT EXISTS (SELECT 1 FROM stock_orders so WHERE so.po_number = 'PO-DEMO-001');
INSERT INTO stock_orders (po_number, supplier_id, destination_warehouse_id, container_code, eta_date, status, total_cbm, container_freight_aud, customs_tax_aud, currency_code, exchange_rate, notes)
SELECT 'PO-DEMO-002', s.id, w.id, 'CONT-DEMO-OCT', CURRENT_DATE + 21, 'SAILING', 12.7000, 1650.00, 350.00, 'USD', 1.52, 'Demo incoming container'
FROM suppliers s, warehouses w WHERE s.name = 'Oak & Co Manufacturing' AND w.code = 'WH-164'
  AND NOT EXISTS (SELECT 1 FROM stock_orders so WHERE so.po_number = 'PO-DEMO-002');

INSERT INTO stock_order_items (stock_order_id, variant_id, quantity_ordered, quantity_received, unit_cost_foreign, unit_cost_aud, unit_cbm, unit_freight_aud, calculated_landed_cost_aud)
SELECT so.id, v.id, 12, 12, 520.00, 790.40, v.cbm, 100.54, 930.94
FROM stock_orders so, product_variants v WHERE so.po_number = 'PO-DEMO-001' AND v.sku = 'SOFA-LUNA-GREY'
  AND NOT EXISTS (SELECT 1 FROM stock_order_items i WHERE i.stock_order_id = so.id AND i.variant_id = v.id);
INSERT INTO stock_order_items (stock_order_id, variant_id, quantity_ordered, quantity_received, unit_cost_foreign, unit_cost_aud, unit_cbm, unit_freight_aud, calculated_landed_cost_aud)
SELECT so.id, v.id, 10, 10, 410.00, 623.20, v.cbm, 68.37, 745.57
FROM stock_orders so, product_variants v WHERE so.po_number = 'PO-DEMO-001' AND v.sku = 'BED-PRADO-KING-BLK'
  AND NOT EXISTS (SELECT 1 FROM stock_order_items i WHERE i.stock_order_id = so.id AND i.variant_id = v.id);
INSERT INTO stock_order_items (stock_order_id, variant_id, quantity_ordered, quantity_received, unit_cost_foreign, unit_cost_aud, unit_cbm, unit_freight_aud, calculated_landed_cost_aud)
SELECT so.id, v.id, 8, 0, 290.00, 440.80, v.cbm, 58.00, 535.80
FROM stock_orders so, product_variants v WHERE so.po_number = 'PO-DEMO-002' AND v.sku = 'DIN-MARLOW-OAK'
  AND NOT EXISTS (SELECT 1 FROM stock_order_items i WHERE i.stock_order_id = so.id AND i.variant_id = v.id);

INSERT INTO fifo_lots (lot_number, variant_id, warehouse_id, stock_order_item_id, initial_quantity, remaining_quantity, unit_landed_cost_aud, current_stage, received_date, status, activated_at)
SELECT 'LOT-DEMO-LUNA-001', v.id, w.id, i.id, 12, 9, i.calculated_landed_cost_aud, 2, CURRENT_DATE - 2, 'ACTIVE', CURRENT_TIMESTAMP
FROM product_variants v, warehouses w, stock_order_items i, stock_orders so
WHERE v.sku = 'SOFA-LUNA-GREY' AND w.code = 'WH-164' AND so.po_number = 'PO-DEMO-001' AND i.stock_order_id = so.id AND i.variant_id = v.id
ON CONFLICT (lot_number) DO NOTHING;
INSERT INTO fifo_lots (lot_number, variant_id, warehouse_id, stock_order_item_id, initial_quantity, remaining_quantity, unit_landed_cost_aud, current_stage, received_date, status, activated_at)
SELECT 'LOT-DEMO-PRADO-001', v.id, w.id, i.id, 10, 10, i.calculated_landed_cost_aud, 1, CURRENT_DATE - 2, 'ACTIVE', CURRENT_TIMESTAMP
FROM product_variants v, warehouses w, stock_order_items i, stock_orders so
WHERE v.sku = 'BED-PRADO-KING-BLK' AND w.code = 'WH-164' AND so.po_number = 'PO-DEMO-001' AND i.stock_order_id = so.id AND i.variant_id = v.id
ON CONFLICT (lot_number) DO NOTHING;

INSERT INTO fifo_lot_stage_prices (fifo_lot_id, stage, price_aud, vip_price_aud)
SELECT l.id, p.stage, p.price, p.vip FROM fifo_lots l
CROSS JOIN (VALUES (1, 1699.00, 1599.00), (2, 1499.00, 1399.00), (3, 1299.00, 1199.00), (4, 1099.00, 999.00), (5, 899.00, 799.00)) p(stage, price, vip)
WHERE l.lot_number = 'LOT-DEMO-LUNA-001'
ON CONFLICT (fifo_lot_id, stage) DO NOTHING;
INSERT INTO fifo_lot_stage_prices (fifo_lot_id, stage, price_aud, vip_price_aud)
SELECT l.id, p.stage, p.price, p.vip FROM fifo_lots l
CROSS JOIN (VALUES (1, 1399.00, 1299.00), (2, 1249.00, 1149.00), (3, 1099.00, 999.00), (4, 899.00, 799.00), (5, 749.00, 649.00)) p(stage, price, vip)
WHERE l.lot_number = 'LOT-DEMO-PRADO-001'
ON CONFLICT (fifo_lot_id, stage) DO NOTHING;

INSERT INTO warehouse_inventory (warehouse_id, variant_id, on_hand_quantity, reserved_quantity, low_stock_threshold)
SELECT w.id, v.id, 12, 3, 4 FROM warehouses w, product_variants v
WHERE w.code = 'WH-164' AND v.sku = 'SOFA-LUNA-GREY'
ON CONFLICT (warehouse_id, variant_id) DO UPDATE SET on_hand_quantity = EXCLUDED.on_hand_quantity, reserved_quantity = EXCLUDED.reserved_quantity;
INSERT INTO warehouse_inventory (warehouse_id, variant_id, on_hand_quantity, reserved_quantity, low_stock_threshold)
SELECT w.id, v.id, 10, 0, 3 FROM warehouses w, product_variants v
WHERE w.code = 'WH-164' AND v.sku = 'BED-PRADO-KING-BLK'
ON CONFLICT (warehouse_id, variant_id) DO UPDATE SET on_hand_quantity = EXCLUDED.on_hand_quantity, reserved_quantity = EXCLUDED.reserved_quantity;

INSERT INTO store_variant_prices (store_id, variant_id, current_daily_price)
SELECT s.id, v.id, p.price FROM stores s, product_variants v
CROSS JOIN (VALUES ('SOFA-LUNA-GREY', 1499.00), ('BED-PRADO-KING-BLK', 1399.00)) p(sku, price)
WHERE s.code = 'STR-PER-01' AND v.sku = p.sku
ON CONFLICT (store_id, variant_id) DO UPDATE SET current_daily_price = EXCLUDED.current_daily_price, updated_at = CURRENT_TIMESTAMP;

INSERT INTO pricing_stage_configs (variant_id, from_stage, to_stage, target_stock_pct, min_days_at_stage, max_days_at_stage)
SELECT v.id, c.from_stage, c.to_stage, c.target_pct, 7, 45
FROM product_variants v
CROSS JOIN (VALUES (1, 2, 70.0), (2, 3, 50.0), (3, 4, 30.0), (4, 5, 15.0)) c(from_stage, to_stage, target_pct)
WHERE v.sku IN ('SOFA-LUNA-GREY', 'BED-PRADO-KING-BLK')
ON CONFLICT (variant_id, from_stage) DO NOTHING;
