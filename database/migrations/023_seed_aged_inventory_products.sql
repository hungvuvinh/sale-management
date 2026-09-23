-- MIGRATION 023: Seed multiple products, variants and aged FIFO lots (>30 to 150 days old)
-- Expands catalog with Office, Outdoor, Storage, Dining, Bedroom & Living furniture with full FIFO lots & pricing stages.

-- 1. Ensure Product Categories
INSERT INTO product_categories (name, slug, description, display_order) VALUES
    ('Office & Study', 'office-study', 'Ergonomic standing desks, task chairs and study suites', 4),
    ('Outdoor Furniture', 'outdoor-furniture', 'Weather-resistant teak, wicker and aluminum outdoor sets', 5),
    ('Storage & Organization', 'storage-organization', 'Buffets, sideboards, bookcases, consoles and shelving', 6)
ON CONFLICT (slug) DO NOTHING;

-- 2. Ensure Suppliers
INSERT INTO suppliers (name, country, contact_name, contact_email, lead_time_days, payment_terms)
SELECT seed.name, seed.country, seed.contact_name, seed.contact_email, seed.lead_time_days, seed.payment_terms
FROM (VALUES
  ('Nordic Living Woodworks', 'Sweden', 'Lars Lindqvist', 'b2b@nordicliving.example', 35, '30 days net'),
  ('Artisan Teak & Metal Co', 'Indonesia', 'Budi Santoso', 'export@artisanteak.example', 40, '30% deposit, 70% on BL'),
  ('ErgoDynamics Tech Co', 'Taiwan', 'Chen Wei', 'sales@ergodynamics.example', 25, 'Letter of Credit')
) AS seed(name, country, contact_name, contact_email, lead_time_days, payment_terms)
WHERE NOT EXISTS (
  SELECT 1 FROM suppliers s WHERE s.name = seed.name AND s.country = seed.country
);

-- 3. Insert Products
INSERT INTO products (sku, name, slug, category_id, description, materials_summary, is_active)
SELECT 'ARM-NORDIC', 'Nordic Swivel Recliner Armchair', 'nordic-swivel-recliner-armchair', c.id, 'Ergonomic Scandinavian lounge armchair with 360-degree smooth swivel and matching ottoman.', 'Solid oak frame, high-density foam, premium upholstery', TRUE
FROM product_categories c WHERE c.slug = 'living-room'
ON CONFLICT (sku) DO NOTHING;

INSERT INTO products (sku, name, slug, category_id, description, materials_summary, is_active)
SELECT 'TBL-RIVER', 'Epoxy Resin River Dining Table', 'epoxy-resin-river-dining-table', c.id, 'Handcrafted live-edge walnut slab table with deep translucent epoxy river center and powder-coated steel spider legs.', 'American Walnut, Eco-resin, Matte black steel', TRUE
FROM product_categories c WHERE c.slug = 'dining-room'
ON CONFLICT (sku) DO NOTHING;

INSERT INTO products (sku, name, slug, category_id, description, materials_summary, is_active)
SELECT 'DESK-APEX', 'Apex Dual-Motor Electric Standing Desk', 'apex-dual-motor-electric-standing-desk', c.id, 'Smart sit-stand desk with whisper-quiet dual motors, 4 memory presets, collision avoidance, and cable management tray.', 'Commercial-grade steel frame, FSC-certified solid timber desktop', TRUE
FROM product_categories c WHERE c.slug = 'office-study'
ON CONFLICT (sku) DO NOTHING;

INSERT INTO products (sku, name, slug, category_id, description, materials_summary, is_active)
SELECT 'CHAIR-ERGOPRO', 'ErgoPro High-Back Mesh Task Chair', 'ergopro-high-back-mesh-task-chair', c.id, 'Professional ergonomic executive chair featuring 4D armrests, dynamic lumbar support, and breathable Korean mesh.', 'Polished aluminum base, breathable elastomeric mesh, Class-4 gas lift', TRUE
FROM product_categories c WHERE c.slug = 'office-study'
ON CONFLICT (sku) DO NOTHING;

INSERT INTO products (sku, name, slug, category_id, description, materials_summary, is_active)
SELECT 'SOFA-CHESTER', 'Chesterfield 4-Seater Luxury Velvet Sofa', 'chesterfield-4-seater-luxury-velvet-sofa', c.id, 'Timeless Chesterfield deep button-tufted silhouette with rolled arms and lavish stain-resistant velvet.', 'Kiln-dried hardwood frame, pocket spring cushions, brass caster legs', TRUE
FROM product_categories c WHERE c.slug = 'living-room'
ON CONFLICT (sku) DO NOTHING;

INSERT INTO products (sku, name, slug, category_id, description, materials_summary, is_active)
SELECT 'BED-HAVEN', 'Haven Hydraulic Gas-Lift Storage Bed', 'haven-hydraulic-gas-lift-storage-bed', c.id, 'Upholstered platform bed with heavy-duty hydraulic gas lift mechanism revealing massive under-bed dust-free storage.', 'Reinforced steel subframe, bentwood slats, textured woven fabric', TRUE
FROM product_categories c WHERE c.slug = 'bedroom'
ON CONFLICT (sku) DO NOTHING;

INSERT INTO products (sku, name, slug, category_id, description, materials_summary, is_active)
SELECT 'OUT-BALI', 'Bali Grade-A Teak Outdoor Dining Suite', 'bali-grade-a-teak-outdoor-dining-suite', c.id, 'Sustainably harvested Grade-A teak outdoor dining table with UV and water-resistant Sunbrella cushioned armchairs.', 'SVLK-certified Indonesian Teak, Sunbrella fabric, 316 stainless steel fittings', TRUE
FROM product_categories c WHERE c.slug = 'outdoor-furniture'
ON CONFLICT (sku) DO NOTHING;

INSERT INTO products (sku, name, slug, category_id, description, materials_summary, is_active)
SELECT 'CAB-MODERNO', 'Moderno 6-Drawer Sideboard Credenza', 'moderno-6-drawer-sideboard-credenza', c.id, 'Mid-century credenza featuring fluted acoustic drawer fronts, push-to-open soft closing slides, and brushed brass legs.', 'American Walnut veneer, solid brass hardware, tempered glass shelf', TRUE
FROM product_categories c WHERE c.slug = 'storage-organization'
ON CONFLICT (sku) DO NOTHING;

INSERT INTO products (sku, name, slug, category_id, description, materials_summary, is_active)
SELECT 'SHELF-LOFT', 'Loft 5-Tier Industrial Bookcase', 'loft-5-tier-industrial-bookcase', c.id, 'Open-architecture industrial shelf with thick solid timber shelves and cross-braced architectural iron scaffolding.', 'Reclaimed pine, powder-coated structural steel', TRUE
FROM product_categories c WHERE c.slug = 'storage-organization'
ON CONFLICT (sku) DO NOTHING;

INSERT INTO products (sku, name, slug, category_id, description, materials_summary, is_active)
SELECT 'COFFEE-ORION', 'Orion Italian Carrara Marble Coffee Table', 'orion-italian-carrara-marble-coffee-table', c.id, 'Statement circular coffee table with genuine Italian Carrara white marble slab top and sculptural fluted oak base.', 'Honest honed Carrara marble, solid European oak drum pedestal', TRUE
FROM product_categories c WHERE c.slug = 'living-room'
ON CONFLICT (sku) DO NOTHING;

-- 4. Insert Variants
-- ARM-NORDIC variants
INSERT INTO product_variants (product_id, sku, name, barcode, attributes_json, weight_kg, cbm, box_count, dimensions_cm)
SELECT p.id, 'ARM-NORDIC-OAT', 'Nordic Armchair - Oatmeal Linen', '932000000010', '{"color":"Oatmeal Linen","swivel":true,"ottoman":true}', 28.000, 0.6500, 2, '{"width":85,"depth":90,"height":98}'
FROM products p WHERE p.sku = 'ARM-NORDIC' AND NOT EXISTS (SELECT 1 FROM product_variants WHERE sku = 'ARM-NORDIC-OAT');

INSERT INTO product_variants (product_id, sku, name, barcode, attributes_json, weight_kg, cbm, box_count, dimensions_cm)
SELECT p.id, 'ARM-NORDIC-EMR', 'Nordic Armchair - Emerald Velvet', '932000000011', '{"color":"Emerald Velvet","swivel":true,"ottoman":true}', 28.500, 0.6500, 2, '{"width":85,"depth":90,"height":98}'
FROM products p WHERE p.sku = 'ARM-NORDIC' AND NOT EXISTS (SELECT 1 FROM product_variants WHERE sku = 'ARM-NORDIC-EMR');

INSERT INTO product_variants (product_id, sku, name, barcode, attributes_json, weight_kg, cbm, box_count, dimensions_cm)
SELECT p.id, 'ARM-NORDIC-TAN', 'Nordic Armchair - Tan Leather', '932000000012', '{"color":"Cognac Tan Leather","swivel":true,"ottoman":true}', 31.000, 0.6500, 2, '{"width":85,"depth":90,"height":98}'
FROM products p WHERE p.sku = 'ARM-NORDIC' AND NOT EXISTS (SELECT 1 FROM product_variants WHERE sku = 'ARM-NORDIC-TAN');

-- TBL-RIVER variants
INSERT INTO product_variants (product_id, sku, name, barcode, attributes_json, weight_kg, cbm, box_count, dimensions_cm)
SELECT p.id, 'TBL-RIVER-200', 'River Table 200cm - Blue Resin', '932000000020', '{"size":"200x100cm","resinColor":"Ocean Blue","timber":"American Walnut"}', 95.000, 0.9500, 2, '{"width":200,"depth":100,"height":76}'
FROM products p WHERE p.sku = 'TBL-RIVER' AND NOT EXISTS (SELECT 1 FROM product_variants WHERE sku = 'TBL-RIVER-200');

INSERT INTO product_variants (product_id, sku, name, barcode, attributes_json, weight_kg, cbm, box_count, dimensions_cm)
SELECT p.id, 'TBL-RIVER-240', 'River Table 240cm - Emerald Resin', '932000000021', '{"size":"240x110cm","resinColor":"Deep Emerald","timber":"Smoked Oak"}', 118.000, 1.2000, 2, '{"width":240,"depth":110,"height":76}'
FROM products p WHERE p.sku = 'TBL-RIVER' AND NOT EXISTS (SELECT 1 FROM product_variants WHERE sku = 'TBL-RIVER-240');

-- DESK-APEX variants
INSERT INTO product_variants (product_id, sku, name, barcode, attributes_json, weight_kg, cbm, box_count, dimensions_cm)
SELECT p.id, 'DESK-APEX-WALNUT', 'Apex Standing Desk 150cm - Walnut Top', '932000000030', '{"size":"150x75cm","top":"Solid Walnut","frame":"Matte Black"}', 46.000, 0.4500, 2, '{"width":150,"depth":75,"height":125}'
FROM products p WHERE p.sku = 'DESK-APEX' AND NOT EXISTS (SELECT 1 FROM product_variants WHERE sku = 'DESK-APEX-WALNUT');

INSERT INTO product_variants (product_id, sku, name, barcode, attributes_json, weight_kg, cbm, box_count, dimensions_cm)
SELECT p.id, 'DESK-APEX-OAK', 'Apex Standing Desk 150cm - Oak Top', '932000000031', '{"size":"150x75cm","top":"Natural Oak","frame":"Matte White"}', 45.000, 0.4500, 2, '{"width":150,"depth":75,"height":125}'
FROM products p WHERE p.sku = 'DESK-APEX' AND NOT EXISTS (SELECT 1 FROM product_variants WHERE sku = 'DESK-APEX-OAK');

INSERT INTO product_variants (product_id, sku, name, barcode, attributes_json, weight_kg, cbm, box_count, dimensions_cm)
SELECT p.id, 'DESK-APEX-WHT', 'Apex Standing Desk 120cm - White Top', '932000000032', '{"size":"120x60cm","top":"Matte White","frame":"Matte White"}', 38.000, 0.3500, 2, '{"width":120,"depth":60,"height":125}'
FROM products p WHERE p.sku = 'DESK-APEX' AND NOT EXISTS (SELECT 1 FROM product_variants WHERE sku = 'DESK-APEX-WHT');

-- CHAIR-ERGOPRO variants
INSERT INTO product_variants (product_id, sku, name, barcode, attributes_json, weight_kg, cbm, box_count, dimensions_cm)
SELECT p.id, 'CHAIR-ERGOPRO-BLK', 'ErgoPro Chair - Midnight Black', '932000000040', '{"color":"Black","mesh":"Breathable Elastomer","lumbar":true}', 22.000, 0.3000, 1, '{"width":68,"depth":68,"height":118}'
FROM products p WHERE p.sku = 'CHAIR-ERGOPRO' AND NOT EXISTS (SELECT 1 FROM product_variants WHERE sku = 'CHAIR-ERGOPRO-BLK');

INSERT INTO product_variants (product_id, sku, name, barcode, attributes_json, weight_kg, cbm, box_count, dimensions_cm)
SELECT p.id, 'CHAIR-ERGOPRO-GRY', 'ErgoPro Chair - Platinum Grey', '932000000041', '{"color":"Grey","mesh":"Breathable Elastomer","lumbar":true}', 22.000, 0.3000, 1, '{"width":68,"depth":68,"height":118}'
FROM products p WHERE p.sku = 'CHAIR-ERGOPRO' AND NOT EXISTS (SELECT 1 FROM product_variants WHERE sku = 'CHAIR-ERGOPRO-GRY');

-- SOFA-CHESTER variants
INSERT INTO product_variants (product_id, sku, name, barcode, attributes_json, weight_kg, cbm, box_count, dimensions_cm)
SELECT p.id, 'SOFA-CHESTER-NAVY', 'Chesterfield Sofa - Royal Navy Velvet', '932000000050', '{"seats":4,"fabric":"Stain-Shield Velvet","color":"Royal Navy"}', 76.000, 1.4500, 3, '{"width":240,"depth":98,"height":78}'
FROM products p WHERE p.sku = 'SOFA-CHESTER' AND NOT EXISTS (SELECT 1 FROM product_variants WHERE sku = 'SOFA-CHESTER-NAVY');

INSERT INTO product_variants (product_id, sku, name, barcode, attributes_json, weight_kg, cbm, box_count, dimensions_cm)
SELECT p.id, 'SOFA-CHESTER-RUST', 'Chesterfield Sofa - Burnt Rust Velvet', '932000000051', '{"seats":4,"fabric":"Stain-Shield Velvet","color":"Burnt Rust"}', 76.000, 1.4500, 3, '{"width":240,"depth":98,"height":78}'
FROM products p WHERE p.sku = 'SOFA-CHESTER' AND NOT EXISTS (SELECT 1 FROM product_variants WHERE sku = 'SOFA-CHESTER-RUST');

INSERT INTO product_variants (product_id, sku, name, barcode, attributes_json, weight_kg, cbm, box_count, dimensions_cm)
SELECT p.id, 'SOFA-CHESTER-OLV', 'Chesterfield Sofa - Olive Green Velvet', '932000000052', '{"seats":4,"fabric":"Stain-Shield Velvet","color":"Olive Green"}', 76.000, 1.4500, 3, '{"width":240,"depth":98,"height":78}'
FROM products p WHERE p.sku = 'SOFA-CHESTER' AND NOT EXISTS (SELECT 1 FROM product_variants WHERE sku = 'SOFA-CHESTER-OLV');

-- BED-HAVEN variants
INSERT INTO product_variants (product_id, sku, name, barcode, attributes_json, weight_kg, cbm, box_count, dimensions_cm)
SELECT p.id, 'BED-HAVEN-Q-CHAR', 'Haven Gas-Lift Bed Queen - Charcoal', '932000000060', '{"size":"Queen","storage":"Hydraulic Gas-Lift","color":"Charcoal Grey"}', 82.000, 0.9500, 3, '{"width":165,"depth":218,"height":110}'
FROM products p WHERE p.sku = 'BED-HAVEN' AND NOT EXISTS (SELECT 1 FROM product_variants WHERE sku = 'BED-HAVEN-Q-CHAR');

INSERT INTO product_variants (product_id, sku, name, barcode, attributes_json, weight_kg, cbm, box_count, dimensions_cm)
SELECT p.id, 'BED-HAVEN-K-CHAR', 'Haven Gas-Lift Bed King - Charcoal', '932000000061', '{"size":"King","storage":"Hydraulic Gas-Lift","color":"Charcoal Grey"}', 94.000, 1.1000, 3, '{"width":195,"depth":218,"height":110}'
FROM products p WHERE p.sku = 'BED-HAVEN' AND NOT EXISTS (SELECT 1 FROM product_variants WHERE sku = 'BED-HAVEN-K-CHAR');

INSERT INTO product_variants (product_id, sku, name, barcode, attributes_json, weight_kg, cbm, box_count, dimensions_cm)
SELECT p.id, 'BED-HAVEN-Q-BEIGE', 'Haven Gas-Lift Bed Queen - Natural Beige', '932000000062', '{"size":"Queen","storage":"Hydraulic Gas-Lift","color":"Natural Beige"}', 82.000, 0.9500, 3, '{"width":165,"depth":218,"height":110}'
FROM products p WHERE p.sku = 'BED-HAVEN' AND NOT EXISTS (SELECT 1 FROM product_variants WHERE sku = 'BED-HAVEN-Q-BEIGE');

-- OUT-BALI variants
INSERT INTO product_variants (product_id, sku, name, barcode, attributes_json, weight_kg, cbm, box_count, dimensions_cm)
SELECT p.id, 'OUT-BALI-8S', 'Bali Teak Dining Suite - 8 Seater', '932000000070', '{"seats":8,"material":"Grade-A Teak","cushions":"Sunbrella Sand"}', 135.000, 1.6000, 5, '{"width":240,"depth":105,"height":76}'
FROM products p WHERE p.sku = 'OUT-BALI' AND NOT EXISTS (SELECT 1 FROM product_variants WHERE sku = 'OUT-BALI-8S');

INSERT INTO product_variants (product_id, sku, name, barcode, attributes_json, weight_kg, cbm, box_count, dimensions_cm)
SELECT p.id, 'OUT-BALI-6S', 'Bali Teak Dining Suite - 6 Seater', '932000000071', '{"seats":6,"material":"Grade-A Teak","cushions":"Sunbrella Sand"}', 105.000, 1.2500, 4, '{"width":180,"depth":95,"height":76}'
FROM products p WHERE p.sku = 'OUT-BALI' AND NOT EXISTS (SELECT 1 FROM product_variants WHERE sku = 'OUT-BALI-6S');

-- CAB-MODERNO variants
INSERT INTO product_variants (product_id, sku, name, barcode, attributes_json, weight_kg, cbm, box_count, dimensions_cm)
SELECT p.id, 'CAB-MODERNO-WAL', 'Moderno Sideboard 180cm - Walnut & Brass', '932000000080', '{"drawers":6,"finish":"American Walnut","legs":"Brushed Brass"}', 68.000, 0.7800, 2, '{"width":180,"depth":45,"height":82}'
FROM products p WHERE p.sku = 'CAB-MODERNO' AND NOT EXISTS (SELECT 1 FROM product_variants WHERE sku = 'CAB-MODERNO-WAL');

INSERT INTO product_variants (product_id, sku, name, barcode, attributes_json, weight_kg, cbm, box_count, dimensions_cm)
SELECT p.id, 'CAB-MODERNO-OAK', 'Moderno Sideboard 180cm - Oak & Black', '932000000081', '{"drawers":6,"finish":"Natural Oak","legs":"Matte Black"}', 65.000, 0.7800, 2, '{"width":180,"depth":45,"height":82}'
FROM products p WHERE p.sku = 'CAB-MODERNO' AND NOT EXISTS (SELECT 1 FROM product_variants WHERE sku = 'CAB-MODERNO-OAK');

-- SHELF-LOFT variants
INSERT INTO product_variants (product_id, sku, name, barcode, attributes_json, weight_kg, cbm, box_count, dimensions_cm)
SELECT p.id, 'SHELF-LOFT-TALL', 'Loft Industrial Bookcase - Tall 190cm', '932000000090', '{"tiers":5,"heightCm":190,"finish":"Reclaimed Pine & Steel"}', 42.000, 0.4000, 1, '{"width":90,"depth":38,"height":190}'
FROM products p WHERE p.sku = 'SHELF-LOFT' AND NOT EXISTS (SELECT 1 FROM product_variants WHERE sku = 'SHELF-LOFT-TALL');

INSERT INTO product_variants (product_id, sku, name, barcode, attributes_json, weight_kg, cbm, box_count, dimensions_cm)
SELECT p.id, 'SHELF-LOFT-WIDE', 'Loft Industrial Bookcase - Wide 150cm', '932000000091', '{"tiers":4,"heightCm":140,"finish":"Reclaimed Pine & Steel"}', 48.000, 0.4500, 1, '{"width":150,"depth":38,"height":140}'
FROM products p WHERE p.sku = 'SHELF-LOFT' AND NOT EXISTS (SELECT 1 FROM product_variants WHERE sku = 'SHELF-LOFT-WIDE');

-- COFFEE-ORION variants
INSERT INTO product_variants (product_id, sku, name, barcode, attributes_json, weight_kg, cbm, box_count, dimensions_cm)
SELECT p.id, 'COFFEE-ORION-WHT', 'Orion Marble Coffee Table - Carrara White', '932000000100', '{"top":"Italian Carrara Marble","base":"Fluted Natural Oak","diameterCm":90}', 46.000, 0.3800, 2, '{"width":90,"depth":90,"height":42}'
FROM products p WHERE p.sku = 'COFFEE-ORION' AND NOT EXISTS (SELECT 1 FROM product_variants WHERE sku = 'COFFEE-ORION-WHT');

INSERT INTO product_variants (product_id, sku, name, barcode, attributes_json, weight_kg, cbm, box_count, dimensions_cm)
SELECT p.id, 'COFFEE-ORION-BLK', 'Orion Marble Coffee Table - Marquina Black', '932000000101', '{"top":"Nero Marquina Marble","base":"Fluted Smoked Oak","diameterCm":90}', 48.000, 0.3800, 2, '{"width":90,"depth":90,"height":42}'
FROM products p WHERE p.sku = 'COFFEE-ORION' AND NOT EXISTS (SELECT 1 FROM product_variants WHERE sku = 'COFFEE-ORION-BLK');

-- 5. Insert Aged Stock Orders (Received > 30 to 150 days ago)
INSERT INTO stock_orders (po_number, supplier_id, destination_warehouse_id, container_code, eta_date, actual_arrival_date, status, total_cbm, container_freight_aud, customs_tax_aud, currency_code, exchange_rate, notes)
SELECT 'PO-AGED-150D', s.id, w.id, 'CONT-AGED-150D', CURRENT_DATE - 155, CURRENT_DATE - 150, 'RECEIVED', 42.5000, 3200.00, 850.00, 'USD', 1.52, 'Historical container delivered 150 days ago'
FROM suppliers s, warehouses w WHERE s.name = 'Nordic Living Woodworks' AND w.code = 'WH-164'
ON CONFLICT (po_number) DO NOTHING;

INSERT INTO stock_orders (po_number, supplier_id, destination_warehouse_id, container_code, eta_date, actual_arrival_date, status, total_cbm, container_freight_aud, customs_tax_aud, currency_code, exchange_rate, notes)
SELECT 'PO-AGED-120D', s.id, w.id, 'CONT-AGED-120D', CURRENT_DATE - 125, CURRENT_DATE - 120, 'RECEIVED', 38.0000, 2900.00, 780.00, 'USD', 1.52, 'Historical container delivered 120 days ago'
FROM suppliers s, warehouses w WHERE s.name = 'Artisan Teak & Metal Co' AND w.code = 'WH-164'
ON CONFLICT (po_number) DO NOTHING;

INSERT INTO stock_orders (po_number, supplier_id, destination_warehouse_id, container_code, eta_date, actual_arrival_date, status, total_cbm, container_freight_aud, customs_tax_aud, currency_code, exchange_rate, notes)
SELECT 'PO-AGED-90D', s.id, w.id, 'CONT-AGED-90D', CURRENT_DATE - 95, CURRENT_DATE - 90, 'RECEIVED', 45.2000, 3400.00, 920.00, 'USD', 1.52, 'Historical container delivered 90 days ago'
FROM suppliers s, warehouses w WHERE s.name = 'ErgoDynamics Tech Co' AND w.code = 'WH-164'
ON CONFLICT (po_number) DO NOTHING;

INSERT INTO stock_orders (po_number, supplier_id, destination_warehouse_id, container_code, eta_date, actual_arrival_date, status, total_cbm, container_freight_aud, customs_tax_aud, currency_code, exchange_rate, notes)
SELECT 'PO-AGED-60D', s.id, w.id, 'CONT-AGED-60D', CURRENT_DATE - 65, CURRENT_DATE - 60, 'RECEIVED', 32.8000, 2600.00, 690.00, 'USD', 1.52, 'Historical container delivered 60 days ago'
FROM suppliers s, warehouses w WHERE s.name = 'Pacific Home Imports' AND w.code = 'WH-164'
ON CONFLICT (po_number) DO NOTHING;

INSERT INTO stock_orders (po_number, supplier_id, destination_warehouse_id, container_code, eta_date, actual_arrival_date, status, total_cbm, container_freight_aud, customs_tax_aud, currency_code, exchange_rate, notes)
SELECT 'PO-AGED-45D', s.id, w.id, 'CONT-AGED-45D', CURRENT_DATE - 50, CURRENT_DATE - 45, 'RECEIVED', 29.4000, 2300.00, 580.00, 'USD', 1.52, 'Historical container delivered 45 days ago'
FROM suppliers s, warehouses w WHERE s.name = 'Oak & Co Manufacturing' AND w.code = 'WH-164'
ON CONFLICT (po_number) DO NOTHING;

INSERT INTO stock_orders (po_number, supplier_id, destination_warehouse_id, container_code, eta_date, actual_arrival_date, status, total_cbm, container_freight_aud, customs_tax_aud, currency_code, exchange_rate, notes)
SELECT 'PO-AGED-35D', s.id, w.id, 'CONT-AGED-35D', CURRENT_DATE - 40, CURRENT_DATE - 35, 'RECEIVED', 34.0000, 2700.00, 710.00, 'USD', 1.52, 'Historical container delivered 35 days ago'
FROM suppliers s, warehouses w WHERE s.name = 'Nordic Living Woodworks' AND w.code = 'WH-164'
ON CONFLICT (po_number) DO NOTHING;

-- 6. Insert Stock Order Items
INSERT INTO stock_order_items (stock_order_id, variant_id, quantity_ordered, quantity_received, unit_cost_foreign, unit_cost_aud, unit_cbm, unit_freight_aud, calculated_landed_cost_aud)
SELECT so.id, v.id, 15, 15, 240.00, 364.80, v.cbm, 55.20, 420.00
FROM stock_orders so, product_variants v WHERE so.po_number = 'PO-AGED-150D' AND v.sku = 'ARM-NORDIC-OAT'
AND NOT EXISTS (SELECT 1 FROM stock_order_items WHERE stock_order_id = so.id AND variant_id = v.id);

INSERT INTO stock_order_items (stock_order_id, variant_id, quantity_ordered, quantity_received, unit_cost_foreign, unit_cost_aud, unit_cbm, unit_freight_aud, calculated_landed_cost_aud)
SELECT so.id, v.id, 10, 10, 250.00, 380.00, v.cbm, 60.00, 440.00
FROM stock_orders so, product_variants v WHERE so.po_number = 'PO-AGED-120D' AND v.sku = 'ARM-NORDIC-EMR'
AND NOT EXISTS (SELECT 1 FROM stock_order_items WHERE stock_order_id = so.id AND variant_id = v.id);

INSERT INTO stock_order_items (stock_order_id, variant_id, quantity_ordered, quantity_received, unit_cost_foreign, unit_cost_aud, unit_cbm, unit_freight_aud, calculated_landed_cost_aud)
SELECT so.id, v.id, 12, 12, 300.00, 456.00, v.cbm, 64.00, 520.00
FROM stock_orders so, product_variants v WHERE so.po_number = 'PO-AGED-90D' AND v.sku = 'ARM-NORDIC-TAN'
AND NOT EXISTS (SELECT 1 FROM stock_order_items WHERE stock_order_id = so.id AND variant_id = v.id);

INSERT INTO stock_order_items (stock_order_id, variant_id, quantity_ordered, quantity_received, unit_cost_foreign, unit_cost_aud, unit_cbm, unit_freight_aud, calculated_landed_cost_aud)
SELECT so.id, v.id, 6, 6, 620.00, 942.40, v.cbm, 157.60, 1100.00
FROM stock_orders so, product_variants v WHERE so.po_number = 'PO-AGED-120D' AND v.sku = 'TBL-RIVER-200'
AND NOT EXISTS (SELECT 1 FROM stock_order_items WHERE stock_order_id = so.id AND variant_id = v.id);

INSERT INTO stock_order_items (stock_order_id, variant_id, quantity_ordered, quantity_received, unit_cost_foreign, unit_cost_aud, unit_cbm, unit_freight_aud, calculated_landed_cost_aud)
SELECT so.id, v.id, 8, 8, 760.00, 1155.20, v.cbm, 194.80, 1350.00
FROM stock_orders so, product_variants v WHERE so.po_number = 'PO-AGED-60D' AND v.sku = 'TBL-RIVER-240'
AND NOT EXISTS (SELECT 1 FROM stock_order_items WHERE stock_order_id = so.id AND variant_id = v.id);

INSERT INTO stock_order_items (stock_order_id, variant_id, quantity_ordered, quantity_received, unit_cost_foreign, unit_cost_aud, unit_cbm, unit_freight_aud, calculated_landed_cost_aud)
SELECT so.id, v.id, 20, 20, 270.00, 410.40, v.cbm, 69.60, 480.00
FROM stock_orders so, product_variants v WHERE so.po_number = 'PO-AGED-90D' AND v.sku = 'DESK-APEX-WALNUT'
AND NOT EXISTS (SELECT 1 FROM stock_order_items WHERE stock_order_id = so.id AND variant_id = v.id);

INSERT INTO stock_order_items (stock_order_id, variant_id, quantity_ordered, quantity_received, unit_cost_foreign, unit_cost_aud, unit_cbm, unit_freight_aud, calculated_landed_cost_aud)
SELECT so.id, v.id, 15, 15, 260.00, 395.20, v.cbm, 64.80, 460.00
FROM stock_orders so, product_variants v WHERE so.po_number = 'PO-AGED-60D' AND v.sku = 'DESK-APEX-OAK'
AND NOT EXISTS (SELECT 1 FROM stock_order_items WHERE stock_order_id = so.id AND variant_id = v.id);

INSERT INTO stock_order_items (stock_order_id, variant_id, quantity_ordered, quantity_received, unit_cost_foreign, unit_cost_aud, unit_cbm, unit_freight_aud, calculated_landed_cost_aud)
SELECT so.id, v.id, 12, 12, 215.00, 326.80, v.cbm, 53.20, 380.00
FROM stock_orders so, product_variants v WHERE so.po_number = 'PO-AGED-45D' AND v.sku = 'DESK-APEX-WHT'
AND NOT EXISTS (SELECT 1 FROM stock_order_items WHERE stock_order_id = so.id AND variant_id = v.id);

INSERT INTO stock_order_items (stock_order_id, variant_id, quantity_ordered, quantity_received, unit_cost_foreign, unit_cost_aud, unit_cbm, unit_freight_aud, calculated_landed_cost_aud)
SELECT so.id, v.id, 30, 30, 125.00, 190.00, v.cbm, 30.00, 220.00
FROM stock_orders so, product_variants v WHERE so.po_number = 'PO-AGED-150D' AND v.sku = 'CHAIR-ERGOPRO-BLK'
AND NOT EXISTS (SELECT 1 FROM stock_order_items WHERE stock_order_id = so.id AND variant_id = v.id);

INSERT INTO stock_order_items (stock_order_id, variant_id, quantity_ordered, quantity_received, unit_cost_foreign, unit_cost_aud, unit_cbm, unit_freight_aud, calculated_landed_cost_aud)
SELECT so.id, v.id, 25, 25, 125.00, 190.00, v.cbm, 30.00, 220.00
FROM stock_orders so, product_variants v WHERE so.po_number = 'PO-AGED-90D' AND v.sku = 'CHAIR-ERGOPRO-GRY'
AND NOT EXISTS (SELECT 1 FROM stock_order_items WHERE stock_order_id = so.id AND variant_id = v.id);

INSERT INTO stock_order_items (stock_order_id, variant_id, quantity_ordered, quantity_received, unit_cost_foreign, unit_cost_aud, unit_cbm, unit_freight_aud, calculated_landed_cost_aud)
SELECT so.id, v.id, 8, 8, 590.00, 896.80, v.cbm, 153.20, 1050.00
FROM stock_orders so, product_variants v WHERE so.po_number = 'PO-AGED-90D' AND v.sku = 'SOFA-CHESTER-NAVY'
AND NOT EXISTS (SELECT 1 FROM stock_order_items WHERE stock_order_id = so.id AND variant_id = v.id);

INSERT INTO stock_order_items (stock_order_id, variant_id, quantity_ordered, quantity_received, unit_cost_foreign, unit_cost_aud, unit_cbm, unit_freight_aud, calculated_landed_cost_aud)
SELECT so.id, v.id, 10, 10, 590.00, 896.80, v.cbm, 153.20, 1050.00
FROM stock_orders so, product_variants v WHERE so.po_number = 'PO-AGED-60D' AND v.sku = 'SOFA-CHESTER-RUST'
AND NOT EXISTS (SELECT 1 FROM stock_order_items WHERE stock_order_id = so.id AND variant_id = v.id);

INSERT INTO stock_order_items (stock_order_id, variant_id, quantity_ordered, quantity_received, unit_cost_foreign, unit_cost_aud, unit_cbm, unit_freight_aud, calculated_landed_cost_aud)
SELECT so.id, v.id, 8, 8, 590.00, 896.80, v.cbm, 153.20, 1050.00
FROM stock_orders so, product_variants v WHERE so.po_number = 'PO-AGED-35D' AND v.sku = 'SOFA-CHESTER-OLV'
AND NOT EXISTS (SELECT 1 FROM stock_order_items WHERE stock_order_id = so.id AND variant_id = v.id);

INSERT INTO stock_order_items (stock_order_id, variant_id, quantity_ordered, quantity_received, unit_cost_foreign, unit_cost_aud, unit_cbm, unit_freight_aud, calculated_landed_cost_aud)
SELECT so.id, v.id, 14, 14, 325.00, 494.00, v.cbm, 86.00, 580.00
FROM stock_orders so, product_variants v WHERE so.po_number = 'PO-AGED-120D' AND v.sku = 'BED-HAVEN-Q-CHAR'
AND NOT EXISTS (SELECT 1 FROM stock_order_items WHERE stock_order_id = so.id AND variant_id = v.id);

INSERT INTO stock_order_items (stock_order_id, variant_id, quantity_ordered, quantity_received, unit_cost_foreign, unit_cost_aud, unit_cbm, unit_freight_aud, calculated_landed_cost_aud)
SELECT so.id, v.id, 16, 16, 380.00, 577.60, v.cbm, 102.40, 680.00
FROM stock_orders so, product_variants v WHERE so.po_number = 'PO-AGED-90D' AND v.sku = 'BED-HAVEN-K-CHAR'
AND NOT EXISTS (SELECT 1 FROM stock_order_items WHERE stock_order_id = so.id AND variant_id = v.id);

INSERT INTO stock_order_items (stock_order_id, variant_id, quantity_ordered, quantity_received, unit_cost_foreign, unit_cost_aud, unit_cbm, unit_freight_aud, calculated_landed_cost_aud)
SELECT so.id, v.id, 12, 12, 325.00, 494.00, v.cbm, 86.00, 580.00
FROM stock_orders so, product_variants v WHERE so.po_number = 'PO-AGED-45D' AND v.sku = 'BED-HAVEN-Q-BEIGE'
AND NOT EXISTS (SELECT 1 FROM stock_order_items WHERE stock_order_id = so.id AND variant_id = v.id);

INSERT INTO stock_order_items (stock_order_id, variant_id, quantity_ordered, quantity_received, unit_cost_foreign, unit_cost_aud, unit_cbm, unit_freight_aud, calculated_landed_cost_aud)
SELECT so.id, v.id, 8, 8, 680.00, 1033.60, v.cbm, 166.40, 1200.00
FROM stock_orders so, product_variants v WHERE so.po_number = 'PO-AGED-150D' AND v.sku = 'OUT-BALI-8S'
AND NOT EXISTS (SELECT 1 FROM stock_order_items WHERE stock_order_id = so.id AND variant_id = v.id);

INSERT INTO stock_order_items (stock_order_id, variant_id, quantity_ordered, quantity_received, unit_cost_foreign, unit_cost_aud, unit_cbm, unit_freight_aud, calculated_landed_cost_aud)
SELECT so.id, v.id, 10, 10, 540.00, 820.80, v.cbm, 129.20, 950.00
FROM stock_orders so, product_variants v WHERE so.po_number = 'PO-AGED-90D' AND v.sku = 'OUT-BALI-6S'
AND NOT EXISTS (SELECT 1 FROM stock_order_items WHERE stock_order_id = so.id AND variant_id = v.id);

INSERT INTO stock_order_items (stock_order_id, variant_id, quantity_ordered, quantity_received, unit_cost_foreign, unit_cost_aud, unit_cbm, unit_freight_aud, calculated_landed_cost_aud)
SELECT so.id, v.id, 12, 12, 315.00, 478.80, v.cbm, 81.20, 560.00
FROM stock_orders so, product_variants v WHERE so.po_number = 'PO-AGED-90D' AND v.sku = 'CAB-MODERNO-WAL'
AND NOT EXISTS (SELECT 1 FROM stock_order_items WHERE stock_order_id = so.id AND variant_id = v.id);

INSERT INTO stock_order_items (stock_order_id, variant_id, quantity_ordered, quantity_received, unit_cost_foreign, unit_cost_aud, unit_cbm, unit_freight_aud, calculated_landed_cost_aud)
SELECT so.id, v.id, 10, 10, 295.00, 448.40, v.cbm, 71.60, 520.00
FROM stock_orders so, product_variants v WHERE so.po_number = 'PO-AGED-45D' AND v.sku = 'CAB-MODERNO-OAK'
AND NOT EXISTS (SELECT 1 FROM stock_order_items WHERE stock_order_id = so.id AND variant_id = v.id);

INSERT INTO stock_order_items (stock_order_id, variant_id, quantity_ordered, quantity_received, unit_cost_foreign, unit_cost_aud, unit_cbm, unit_freight_aud, calculated_landed_cost_aud)
SELECT so.id, v.id, 18, 18, 175.00, 266.00, v.cbm, 44.00, 310.00
FROM stock_orders so, product_variants v WHERE so.po_number = 'PO-AGED-90D' AND v.sku = 'SHELF-LOFT-TALL'
AND NOT EXISTS (SELECT 1 FROM stock_order_items WHERE stock_order_id = so.id AND variant_id = v.id);

INSERT INTO stock_order_items (stock_order_id, variant_id, quantity_ordered, quantity_received, unit_cost_foreign, unit_cost_aud, unit_cbm, unit_freight_aud, calculated_landed_cost_aud)
SELECT so.id, v.id, 15, 15, 190.00, 288.80, v.cbm, 51.20, 340.00
FROM stock_orders so, product_variants v WHERE so.po_number = 'PO-AGED-60D' AND v.sku = 'SHELF-LOFT-WIDE'
AND NOT EXISTS (SELECT 1 FROM stock_order_items WHERE stock_order_id = so.id AND variant_id = v.id);

INSERT INTO stock_order_items (stock_order_id, variant_id, quantity_ordered, quantity_received, unit_cost_foreign, unit_cost_aud, unit_cbm, unit_freight_aud, calculated_landed_cost_aud)
SELECT so.id, v.id, 14, 14, 230.00, 349.60, v.cbm, 60.40, 410.00
FROM stock_orders so, product_variants v WHERE so.po_number = 'PO-AGED-120D' AND v.sku = 'COFFEE-ORION-WHT'
AND NOT EXISTS (SELECT 1 FROM stock_order_items WHERE stock_order_id = so.id AND variant_id = v.id);

INSERT INTO stock_order_items (stock_order_id, variant_id, quantity_ordered, quantity_received, unit_cost_foreign, unit_cost_aud, unit_cbm, unit_freight_aud, calculated_landed_cost_aud)
SELECT so.id, v.id, 12, 12, 245.00, 372.40, v.cbm, 67.60, 440.00
FROM stock_orders so, product_variants v WHERE so.po_number = 'PO-AGED-35D' AND v.sku = 'COFFEE-ORION-BLK'
AND NOT EXISTS (SELECT 1 FROM stock_order_items WHERE stock_order_id = so.id AND variant_id = v.id);

-- 7. Insert Aged FIFO Lots (Received 35 to 150 days ago)
-- All lots have received_date and activated_at from >30 to 150 days ago
INSERT INTO fifo_lots (lot_number, variant_id, warehouse_id, stock_order_item_id, initial_quantity, remaining_quantity, unit_landed_cost_aud, current_stage, received_date, status, activated_at, created_at)
SELECT 'LOT-AGED-ARM-001', v.id, w.id, i.id, 15, 3, 420.00, 4, CURRENT_DATE - 150, 'ACTIVE', CURRENT_TIMESTAMP - INTERVAL '150 days', CURRENT_TIMESTAMP - INTERVAL '150 days'
FROM product_variants v, warehouses w, stock_order_items i, stock_orders so
WHERE v.sku = 'ARM-NORDIC-OAT' AND w.code = 'WH-164' AND so.po_number = 'PO-AGED-150D' AND i.stock_order_id = so.id AND i.variant_id = v.id
ON CONFLICT (lot_number) DO NOTHING;

INSERT INTO fifo_lots (lot_number, variant_id, warehouse_id, stock_order_item_id, initial_quantity, remaining_quantity, unit_landed_cost_aud, current_stage, received_date, status, activated_at, created_at)
SELECT 'LOT-AGED-ARM-002', v.id, w.id, i.id, 10, 4, 440.00, 3, CURRENT_DATE - 120, 'ACTIVE', CURRENT_TIMESTAMP - INTERVAL '120 days', CURRENT_TIMESTAMP - INTERVAL '120 days'
FROM product_variants v, warehouses w, stock_order_items i, stock_orders so
WHERE v.sku = 'ARM-NORDIC-EMR' AND w.code = 'WH-164' AND so.po_number = 'PO-AGED-120D' AND i.stock_order_id = so.id AND i.variant_id = v.id
ON CONFLICT (lot_number) DO NOTHING;

INSERT INTO fifo_lots (lot_number, variant_id, warehouse_id, stock_order_item_id, initial_quantity, remaining_quantity, unit_landed_cost_aud, current_stage, received_date, status, activated_at, created_at)
SELECT 'LOT-AGED-ARM-003', v.id, w.id, i.id, 12, 7, 520.00, 2, CURRENT_DATE - 90, 'ACTIVE', CURRENT_TIMESTAMP - INTERVAL '90 days', CURRENT_TIMESTAMP - INTERVAL '90 days'
FROM product_variants v, warehouses w, stock_order_items i, stock_orders so
WHERE v.sku = 'ARM-NORDIC-TAN' AND w.code = 'WH-164' AND so.po_number = 'PO-AGED-90D' AND i.stock_order_id = so.id AND i.variant_id = v.id
ON CONFLICT (lot_number) DO NOTHING;

INSERT INTO fifo_lots (lot_number, variant_id, warehouse_id, stock_order_item_id, initial_quantity, remaining_quantity, unit_landed_cost_aud, current_stage, received_date, status, activated_at, created_at)
SELECT 'LOT-AGED-RIVER-001', v.id, w.id, i.id, 6, 2, 1100.00, 4, CURRENT_DATE - 120, 'ACTIVE', CURRENT_TIMESTAMP - INTERVAL '120 days', CURRENT_TIMESTAMP - INTERVAL '120 days'
FROM product_variants v, warehouses w, stock_order_items i, stock_orders so
WHERE v.sku = 'TBL-RIVER-200' AND w.code = 'WH-164' AND so.po_number = 'PO-AGED-120D' AND i.stock_order_id = so.id AND i.variant_id = v.id
ON CONFLICT (lot_number) DO NOTHING;

INSERT INTO fifo_lots (lot_number, variant_id, warehouse_id, stock_order_item_id, initial_quantity, remaining_quantity, unit_landed_cost_aud, current_stage, received_date, status, activated_at, created_at)
SELECT 'LOT-AGED-RIVER-002', v.id, w.id, i.id, 8, 5, 1350.00, 2, CURRENT_DATE - 60, 'ACTIVE', CURRENT_TIMESTAMP - INTERVAL '60 days', CURRENT_TIMESTAMP - INTERVAL '60 days'
FROM product_variants v, warehouses w, stock_order_items i, stock_orders so
WHERE v.sku = 'TBL-RIVER-240' AND w.code = 'WH-164' AND so.po_number = 'PO-AGED-60D' AND i.stock_order_id = so.id AND i.variant_id = v.id
ON CONFLICT (lot_number) DO NOTHING;

INSERT INTO fifo_lots (lot_number, variant_id, warehouse_id, stock_order_item_id, initial_quantity, remaining_quantity, unit_landed_cost_aud, current_stage, received_date, status, activated_at, created_at)
SELECT 'LOT-AGED-DESK-001', v.id, w.id, i.id, 20, 6, 480.00, 3, CURRENT_DATE - 90, 'ACTIVE', CURRENT_TIMESTAMP - INTERVAL '90 days', CURRENT_TIMESTAMP - INTERVAL '90 days'
FROM product_variants v, warehouses w, stock_order_items i, stock_orders so
WHERE v.sku = 'DESK-APEX-WALNUT' AND w.code = 'WH-164' AND so.po_number = 'PO-AGED-90D' AND i.stock_order_id = so.id AND i.variant_id = v.id
ON CONFLICT (lot_number) DO NOTHING;

INSERT INTO fifo_lots (lot_number, variant_id, warehouse_id, stock_order_item_id, initial_quantity, remaining_quantity, unit_landed_cost_aud, current_stage, received_date, status, activated_at, created_at)
SELECT 'LOT-AGED-DESK-002', v.id, w.id, i.id, 15, 8, 460.00, 2, CURRENT_DATE - 60, 'ACTIVE', CURRENT_TIMESTAMP - INTERVAL '60 days', CURRENT_TIMESTAMP - INTERVAL '60 days'
FROM product_variants v, warehouses w, stock_order_items i, stock_orders so
WHERE v.sku = 'DESK-APEX-OAK' AND w.code = 'WH-164' AND so.po_number = 'PO-AGED-60D' AND i.stock_order_id = so.id AND i.variant_id = v.id
ON CONFLICT (lot_number) DO NOTHING;

INSERT INTO fifo_lots (lot_number, variant_id, warehouse_id, stock_order_item_id, initial_quantity, remaining_quantity, unit_landed_cost_aud, current_stage, received_date, status, activated_at, created_at)
SELECT 'LOT-AGED-DESK-003', v.id, w.id, i.id, 12, 9, 380.00, 1, CURRENT_DATE - 45, 'ACTIVE', CURRENT_TIMESTAMP - INTERVAL '45 days', CURRENT_TIMESTAMP - INTERVAL '45 days'
FROM product_variants v, warehouses w, stock_order_items i, stock_orders so
WHERE v.sku = 'DESK-APEX-WHT' AND w.code = 'WH-164' AND so.po_number = 'PO-AGED-45D' AND i.stock_order_id = so.id AND i.variant_id = v.id
ON CONFLICT (lot_number) DO NOTHING;

INSERT INTO fifo_lots (lot_number, variant_id, warehouse_id, stock_order_item_id, initial_quantity, remaining_quantity, unit_landed_cost_aud, current_stage, received_date, status, activated_at, created_at)
SELECT 'LOT-AGED-CHAIR-001', v.id, w.id, i.id, 30, 5, 220.00, 5, CURRENT_DATE - 150, 'ACTIVE', CURRENT_TIMESTAMP - INTERVAL '150 days', CURRENT_TIMESTAMP - INTERVAL '150 days'
FROM product_variants v, warehouses w, stock_order_items i, stock_orders so
WHERE v.sku = 'CHAIR-ERGOPRO-BLK' AND w.code = 'WH-164' AND so.po_number = 'PO-AGED-150D' AND i.stock_order_id = so.id AND i.variant_id = v.id
ON CONFLICT (lot_number) DO NOTHING;

INSERT INTO fifo_lots (lot_number, variant_id, warehouse_id, stock_order_item_id, initial_quantity, remaining_quantity, unit_landed_cost_aud, current_stage, received_date, status, activated_at, created_at)
SELECT 'LOT-AGED-CHAIR-002', v.id, w.id, i.id, 25, 11, 220.00, 3, CURRENT_DATE - 90, 'ACTIVE', CURRENT_TIMESTAMP - INTERVAL '90 days', CURRENT_TIMESTAMP - INTERVAL '90 days'
FROM product_variants v, warehouses w, stock_order_items i, stock_orders so
WHERE v.sku = 'CHAIR-ERGOPRO-GRY' AND w.code = 'WH-164' AND so.po_number = 'PO-AGED-90D' AND i.stock_order_id = so.id AND i.variant_id = v.id
ON CONFLICT (lot_number) DO NOTHING;

INSERT INTO fifo_lots (lot_number, variant_id, warehouse_id, stock_order_item_id, initial_quantity, remaining_quantity, unit_landed_cost_aud, current_stage, received_date, status, activated_at, created_at)
SELECT 'LOT-AGED-CHEST-001', v.id, w.id, i.id, 8, 3, 1050.00, 3, CURRENT_DATE - 90, 'ACTIVE', CURRENT_TIMESTAMP - INTERVAL '90 days', CURRENT_TIMESTAMP - INTERVAL '90 days'
FROM product_variants v, warehouses w, stock_order_items i, stock_orders so
WHERE v.sku = 'SOFA-CHESTER-NAVY' AND w.code = 'WH-164' AND so.po_number = 'PO-AGED-90D' AND i.stock_order_id = so.id AND i.variant_id = v.id
ON CONFLICT (lot_number) DO NOTHING;

INSERT INTO fifo_lots (lot_number, variant_id, warehouse_id, stock_order_item_id, initial_quantity, remaining_quantity, unit_landed_cost_aud, current_stage, received_date, status, activated_at, created_at)
SELECT 'LOT-AGED-CHEST-002', v.id, w.id, i.id, 10, 6, 1050.00, 2, CURRENT_DATE - 60, 'ACTIVE', CURRENT_TIMESTAMP - INTERVAL '60 days', CURRENT_TIMESTAMP - INTERVAL '60 days'
FROM product_variants v, warehouses w, stock_order_items i, stock_orders so
WHERE v.sku = 'SOFA-CHESTER-RUST' AND w.code = 'WH-164' AND so.po_number = 'PO-AGED-60D' AND i.stock_order_id = so.id AND i.variant_id = v.id
ON CONFLICT (lot_number) DO NOTHING;

INSERT INTO fifo_lots (lot_number, variant_id, warehouse_id, stock_order_item_id, initial_quantity, remaining_quantity, unit_landed_cost_aud, current_stage, received_date, status, activated_at, created_at)
SELECT 'LOT-AGED-CHEST-003', v.id, w.id, i.id, 8, 7, 1050.00, 1, CURRENT_DATE - 35, 'ACTIVE', CURRENT_TIMESTAMP - INTERVAL '35 days', CURRENT_TIMESTAMP - INTERVAL '35 days'
FROM product_variants v, warehouses w, stock_order_items i, stock_orders so
WHERE v.sku = 'SOFA-CHESTER-OLV' AND w.code = 'WH-164' AND so.po_number = 'PO-AGED-35D' AND i.stock_order_id = so.id AND i.variant_id = v.id
ON CONFLICT (lot_number) DO NOTHING;

INSERT INTO fifo_lots (lot_number, variant_id, warehouse_id, stock_order_item_id, initial_quantity, remaining_quantity, unit_landed_cost_aud, current_stage, received_date, status, activated_at, created_at)
SELECT 'LOT-AGED-BED-001', v.id, w.id, i.id, 14, 4, 580.00, 4, CURRENT_DATE - 120, 'ACTIVE', CURRENT_TIMESTAMP - INTERVAL '120 days', CURRENT_TIMESTAMP - INTERVAL '120 days'
FROM product_variants v, warehouses w, stock_order_items i, stock_orders so
WHERE v.sku = 'BED-HAVEN-Q-CHAR' AND w.code = 'WH-164' AND so.po_number = 'PO-AGED-120D' AND i.stock_order_id = so.id AND i.variant_id = v.id
ON CONFLICT (lot_number) DO NOTHING;

INSERT INTO fifo_lots (lot_number, variant_id, warehouse_id, stock_order_item_id, initial_quantity, remaining_quantity, unit_landed_cost_aud, current_stage, received_date, status, activated_at, created_at)
SELECT 'LOT-AGED-BED-002', v.id, w.id, i.id, 16, 8, 680.00, 3, CURRENT_DATE - 90, 'ACTIVE', CURRENT_TIMESTAMP - INTERVAL '90 days', CURRENT_TIMESTAMP - INTERVAL '90 days'
FROM product_variants v, warehouses w, stock_order_items i, stock_orders so
WHERE v.sku = 'BED-HAVEN-K-CHAR' AND w.code = 'WH-164' AND so.po_number = 'PO-AGED-90D' AND i.stock_order_id = so.id AND i.variant_id = v.id
ON CONFLICT (lot_number) DO NOTHING;

INSERT INTO fifo_lots (lot_number, variant_id, warehouse_id, stock_order_item_id, initial_quantity, remaining_quantity, unit_landed_cost_aud, current_stage, received_date, status, activated_at, created_at)
SELECT 'LOT-AGED-BED-003', v.id, w.id, i.id, 12, 9, 580.00, 2, CURRENT_DATE - 45, 'ACTIVE', CURRENT_TIMESTAMP - INTERVAL '45 days', CURRENT_TIMESTAMP - INTERVAL '45 days'
FROM product_variants v, warehouses w, stock_order_items i, stock_orders so
WHERE v.sku = 'BED-HAVEN-Q-BEIGE' AND w.code = 'WH-164' AND so.po_number = 'PO-AGED-45D' AND i.stock_order_id = so.id AND i.variant_id = v.id
ON CONFLICT (lot_number) DO NOTHING;

INSERT INTO fifo_lots (lot_number, variant_id, warehouse_id, stock_order_item_id, initial_quantity, remaining_quantity, unit_landed_cost_aud, current_stage, received_date, status, activated_at, created_at)
SELECT 'LOT-AGED-OUT-001', v.id, w.id, i.id, 8, 2, 1200.00, 5, CURRENT_DATE - 150, 'ACTIVE', CURRENT_TIMESTAMP - INTERVAL '150 days', CURRENT_TIMESTAMP - INTERVAL '150 days'
FROM product_variants v, warehouses w, stock_order_items i, stock_orders so
WHERE v.sku = 'OUT-BALI-8S' AND w.code = 'WH-164' AND so.po_number = 'PO-AGED-150D' AND i.stock_order_id = so.id AND i.variant_id = v.id
ON CONFLICT (lot_number) DO NOTHING;

INSERT INTO fifo_lots (lot_number, variant_id, warehouse_id, stock_order_item_id, initial_quantity, remaining_quantity, unit_landed_cost_aud, current_stage, received_date, status, activated_at, created_at)
SELECT 'LOT-AGED-OUT-002', v.id, w.id, i.id, 10, 4, 950.00, 3, CURRENT_DATE - 90, 'ACTIVE', CURRENT_TIMESTAMP - INTERVAL '90 days', CURRENT_TIMESTAMP - INTERVAL '90 days'
FROM product_variants v, warehouses w, stock_order_items i, stock_orders so
WHERE v.sku = 'OUT-BALI-6S' AND w.code = 'WH-164' AND so.po_number = 'PO-AGED-90D' AND i.stock_order_id = so.id AND i.variant_id = v.id
ON CONFLICT (lot_number) DO NOTHING;

INSERT INTO fifo_lots (lot_number, variant_id, warehouse_id, stock_order_item_id, initial_quantity, remaining_quantity, unit_landed_cost_aud, current_stage, received_date, status, activated_at, created_at)
SELECT 'LOT-AGED-CAB-001', v.id, w.id, i.id, 12, 5, 560.00, 3, CURRENT_DATE - 90, 'ACTIVE', CURRENT_TIMESTAMP - INTERVAL '90 days', CURRENT_TIMESTAMP - INTERVAL '90 days'
FROM product_variants v, warehouses w, stock_order_items i, stock_orders so
WHERE v.sku = 'CAB-MODERNO-WAL' AND w.code = 'WH-164' AND so.po_number = 'PO-AGED-90D' AND i.stock_order_id = so.id AND i.variant_id = v.id
ON CONFLICT (lot_number) DO NOTHING;

INSERT INTO fifo_lots (lot_number, variant_id, warehouse_id, stock_order_item_id, initial_quantity, remaining_quantity, unit_landed_cost_aud, current_stage, received_date, status, activated_at, created_at)
SELECT 'LOT-AGED-CAB-002', v.id, w.id, i.id, 10, 7, 520.00, 2, CURRENT_DATE - 45, 'ACTIVE', CURRENT_TIMESTAMP - INTERVAL '45 days', CURRENT_TIMESTAMP - INTERVAL '45 days'
FROM product_variants v, warehouses w, stock_order_items i, stock_orders so
WHERE v.sku = 'CAB-MODERNO-OAK' AND w.code = 'WH-164' AND so.po_number = 'PO-AGED-45D' AND i.stock_order_id = so.id AND i.variant_id = v.id
ON CONFLICT (lot_number) DO NOTHING;

INSERT INTO fifo_lots (lot_number, variant_id, warehouse_id, stock_order_item_id, initial_quantity, remaining_quantity, unit_landed_cost_aud, current_stage, received_date, status, activated_at, created_at)
SELECT 'LOT-AGED-SHELF-001', v.id, w.id, i.id, 18, 6, 310.00, 3, CURRENT_DATE - 90, 'ACTIVE', CURRENT_TIMESTAMP - INTERVAL '90 days', CURRENT_TIMESTAMP - INTERVAL '90 days'
FROM product_variants v, warehouses w, stock_order_items i, stock_orders so
WHERE v.sku = 'SHELF-LOFT-TALL' AND w.code = 'WH-164' AND so.po_number = 'PO-AGED-90D' AND i.stock_order_id = so.id AND i.variant_id = v.id
ON CONFLICT (lot_number) DO NOTHING;

INSERT INTO fifo_lots (lot_number, variant_id, warehouse_id, stock_order_item_id, initial_quantity, remaining_quantity, unit_landed_cost_aud, current_stage, received_date, status, activated_at, created_at)
SELECT 'LOT-AGED-SHELF-002', v.id, w.id, i.id, 15, 9, 340.00, 2, CURRENT_DATE - 60, 'ACTIVE', CURRENT_TIMESTAMP - INTERVAL '60 days', CURRENT_TIMESTAMP - INTERVAL '60 days'
FROM product_variants v, warehouses w, stock_order_items i, stock_orders so
WHERE v.sku = 'SHELF-LOFT-WIDE' AND w.code = 'WH-164' AND so.po_number = 'PO-AGED-60D' AND i.stock_order_id = so.id AND i.variant_id = v.id
ON CONFLICT (lot_number) DO NOTHING;

INSERT INTO fifo_lots (lot_number, variant_id, warehouse_id, stock_order_item_id, initial_quantity, remaining_quantity, unit_landed_cost_aud, current_stage, received_date, status, activated_at, created_at)
SELECT 'LOT-AGED-ORION-001', v.id, w.id, i.id, 14, 3, 410.00, 4, CURRENT_DATE - 120, 'ACTIVE', CURRENT_TIMESTAMP - INTERVAL '120 days', CURRENT_TIMESTAMP - INTERVAL '120 days'
FROM product_variants v, warehouses w, stock_order_items i, stock_orders so
WHERE v.sku = 'COFFEE-ORION-WHT' AND w.code = 'WH-164' AND so.po_number = 'PO-AGED-120D' AND i.stock_order_id = so.id AND i.variant_id = v.id
ON CONFLICT (lot_number) DO NOTHING;

INSERT INTO fifo_lots (lot_number, variant_id, warehouse_id, stock_order_item_id, initial_quantity, remaining_quantity, unit_landed_cost_aud, current_stage, received_date, status, activated_at, created_at)
SELECT 'LOT-AGED-ORION-002', v.id, w.id, i.id, 12, 10, 440.00, 1, CURRENT_DATE - 35, 'ACTIVE', CURRENT_TIMESTAMP - INTERVAL '35 days', CURRENT_TIMESTAMP - INTERVAL '35 days'
FROM product_variants v, warehouses w, stock_order_items i, stock_orders so
WHERE v.sku = 'COFFEE-ORION-BLK' AND w.code = 'WH-164' AND so.po_number = 'PO-AGED-35D' AND i.stock_order_id = so.id AND i.variant_id = v.id
ON CONFLICT (lot_number) DO NOTHING;

-- 8. Seed 5-Stage Prices for all aged FIFO lots
INSERT INTO fifo_lot_stage_prices (fifo_lot_id, stage, price_aud, vip_price_aud)
SELECT l.id, p.stage, p.price, p.vip FROM fifo_lots l
CROSS JOIN (VALUES (1, 799.00, 749.00), (2, 729.00, 679.00), (3, 649.00, 599.00), (4, 569.00, 519.00), (5, 499.00, 449.00)) p(stage, price, vip)
WHERE l.lot_number = 'LOT-AGED-ARM-001'
ON CONFLICT (fifo_lot_id, stage) DO NOTHING;

INSERT INTO fifo_lot_stage_prices (fifo_lot_id, stage, price_aud, vip_price_aud)
SELECT l.id, p.stage, p.price, p.vip FROM fifo_lots l
CROSS JOIN (VALUES (1, 849.00, 799.00), (2, 769.00, 719.00), (3, 699.00, 649.00), (4, 599.00, 549.00), (5, 529.00, 479.00)) p(stage, price, vip)
WHERE l.lot_number = 'LOT-AGED-ARM-002'
ON CONFLICT (fifo_lot_id, stage) DO NOTHING;

INSERT INTO fifo_lot_stage_prices (fifo_lot_id, stage, price_aud, vip_price_aud)
SELECT l.id, p.stage, p.price, p.vip FROM fifo_lots l
CROSS JOIN (VALUES (1, 999.00, 929.00), (2, 899.00, 829.00), (3, 799.00, 729.00), (4, 699.00, 629.00), (5, 599.00, 529.00)) p(stage, price, vip)
WHERE l.lot_number = 'LOT-AGED-ARM-003'
ON CONFLICT (fifo_lot_id, stage) DO NOTHING;

INSERT INTO fifo_lot_stage_prices (fifo_lot_id, stage, price_aud, vip_price_aud)
SELECT l.id, p.stage, p.price, p.vip FROM fifo_lots l
CROSS JOIN (VALUES (1, 2299.00, 2149.00), (2, 2099.00, 1949.00), (3, 1899.00, 1749.00), (4, 1599.00, 1449.00), (5, 1299.00, 1149.00)) p(stage, price, vip)
WHERE l.lot_number = 'LOT-AGED-RIVER-001'
ON CONFLICT (fifo_lot_id, stage) DO NOTHING;

INSERT INTO fifo_lot_stage_prices (fifo_lot_id, stage, price_aud, vip_price_aud)
SELECT l.id, p.stage, p.price, p.vip FROM fifo_lots l
CROSS JOIN (VALUES (1, 2699.00, 2499.00), (2, 2399.00, 2199.00), (3, 2099.00, 1899.00), (4, 1799.00, 1599.00), (5, 1499.00, 1299.00)) p(stage, price, vip)
WHERE l.lot_number = 'LOT-AGED-RIVER-002'
ON CONFLICT (fifo_lot_id, stage) DO NOTHING;

INSERT INTO fifo_lot_stage_prices (fifo_lot_id, stage, price_aud, vip_price_aud)
SELECT l.id, p.stage, p.price, p.vip FROM fifo_lots l
CROSS JOIN (VALUES (1, 949.00, 899.00), (2, 849.00, 799.00), (3, 749.00, 699.00), (4, 649.00, 599.00), (5, 549.00, 499.00)) p(stage, price, vip)
WHERE l.lot_number = 'LOT-AGED-DESK-001'
ON CONFLICT (fifo_lot_id, stage) DO NOTHING;

INSERT INTO fifo_lot_stage_prices (fifo_lot_id, stage, price_aud, vip_price_aud)
SELECT l.id, p.stage, p.price, p.vip FROM fifo_lots l
CROSS JOIN (VALUES (1, 899.00, 849.00), (2, 799.00, 749.00), (3, 699.00, 649.00), (4, 599.00, 549.00), (5, 499.00, 449.00)) p(stage, price, vip)
WHERE l.lot_number = 'LOT-AGED-DESK-002'
ON CONFLICT (fifo_lot_id, stage) DO NOTHING;

INSERT INTO fifo_lot_stage_prices (fifo_lot_id, stage, price_aud, vip_price_aud)
SELECT l.id, p.stage, p.price, p.vip FROM fifo_lots l
CROSS JOIN (VALUES (1, 749.00, 699.00), (2, 679.00, 629.00), (3, 599.00, 549.00), (4, 519.00, 469.00), (5, 449.00, 399.00)) p(stage, price, vip)
WHERE l.lot_number = 'LOT-AGED-DESK-003'
ON CONFLICT (fifo_lot_id, stage) DO NOTHING;

INSERT INTO fifo_lot_stage_prices (fifo_lot_id, stage, price_aud, vip_price_aud)
SELECT l.id, p.stage, p.price, p.vip FROM fifo_lots l
CROSS JOIN (VALUES (1, 499.00, 459.00), (2, 449.00, 409.00), (3, 399.00, 359.00), (4, 349.00, 309.00), (5, 299.00, 259.00)) p(stage, price, vip)
WHERE l.lot_number IN ('LOT-AGED-CHAIR-001', 'LOT-AGED-CHAIR-002')
ON CONFLICT (fifo_lot_id, stage) DO NOTHING;

INSERT INTO fifo_lot_stage_prices (fifo_lot_id, stage, price_aud, vip_price_aud)
SELECT l.id, p.stage, p.price, p.vip FROM fifo_lots l
CROSS JOIN (VALUES (1, 2199.00, 1999.00), (2, 1999.00, 1799.00), (3, 1799.00, 1599.00), (4, 1499.00, 1299.00), (5, 1199.00, 999.00)) p(stage, price, vip)
WHERE l.lot_number IN ('LOT-AGED-CHEST-001', 'LOT-AGED-CHEST-002', 'LOT-AGED-CHEST-003')
ON CONFLICT (fifo_lot_id, stage) DO NOTHING;

INSERT INTO fifo_lot_stage_prices (fifo_lot_id, stage, price_aud, vip_price_aud)
SELECT l.id, p.stage, p.price, p.vip FROM fifo_lots l
CROSS JOIN (VALUES (1, 1199.00, 1099.00), (2, 1099.00, 999.00), (3, 969.00, 869.00), (4, 849.00, 749.00), (5, 699.00, 599.00)) p(stage, price, vip)
WHERE l.lot_number IN ('LOT-AGED-BED-001', 'LOT-AGED-BED-003')
ON CONFLICT (fifo_lot_id, stage) DO NOTHING;

INSERT INTO fifo_lot_stage_prices (fifo_lot_id, stage, price_aud, vip_price_aud)
SELECT l.id, p.stage, p.price, p.vip FROM fifo_lots l
CROSS JOIN (VALUES (1, 1399.00, 1279.00), (2, 1249.00, 1129.00), (3, 1099.00, 979.00), (4, 949.00, 829.00), (5, 799.00, 679.00)) p(stage, price, vip)
WHERE l.lot_number = 'LOT-AGED-BED-002'
ON CONFLICT (fifo_lot_id, stage) DO NOTHING;

INSERT INTO fifo_lot_stage_prices (fifo_lot_id, stage, price_aud, vip_price_aud)
SELECT l.id, p.stage, p.price, p.vip FROM fifo_lots l
CROSS JOIN (VALUES (1, 2499.00, 2299.00), (2, 2199.00, 1999.00), (3, 1899.00, 1699.00), (4, 1599.00, 1399.00), (5, 1299.00, 1099.00)) p(stage, price, vip)
WHERE l.lot_number = 'LOT-AGED-OUT-001'
ON CONFLICT (fifo_lot_id, stage) DO NOTHING;

INSERT INTO fifo_lot_stage_prices (fifo_lot_id, stage, price_aud, vip_price_aud)
SELECT l.id, p.stage, p.price, p.vip FROM fifo_lots l
CROSS JOIN (VALUES (1, 1999.00, 1849.00), (2, 1749.00, 1599.00), (3, 1499.00, 1349.00), (4, 1249.00, 1099.00), (5, 999.00, 849.00)) p(stage, price, vip)
WHERE l.lot_number = 'LOT-AGED-OUT-002'
ON CONFLICT (fifo_lot_id, stage) DO NOTHING;

INSERT INTO fifo_lot_stage_prices (fifo_lot_id, stage, price_aud, vip_price_aud)
SELECT l.id, p.stage, p.price, p.vip FROM fifo_lots l
CROSS JOIN (VALUES (1, 1149.00, 1049.00), (2, 1029.00, 929.00), (3, 899.00, 799.00), (4, 769.00, 669.00), (5, 649.00, 549.00)) p(stage, price, vip)
WHERE l.lot_number = 'LOT-AGED-CAB-001'
ON CONFLICT (fifo_lot_id, stage) DO NOTHING;

INSERT INTO fifo_lot_stage_prices (fifo_lot_id, stage, price_aud, vip_price_aud)
SELECT l.id, p.stage, p.price, p.vip FROM fifo_lots l
CROSS JOIN (VALUES (1, 1049.00, 949.00), (2, 949.00, 849.00), (3, 829.00, 729.00), (4, 719.00, 619.00), (5, 599.00, 499.00)) p(stage, price, vip)
WHERE l.lot_number = 'LOT-AGED-CAB-002'
ON CONFLICT (fifo_lot_id, stage) DO NOTHING;

INSERT INTO fifo_lot_stage_prices (fifo_lot_id, stage, price_aud, vip_price_aud)
SELECT l.id, p.stage, p.price, p.vip FROM fifo_lots l
CROSS JOIN (VALUES (1, 649.00, 599.00), (2, 579.00, 529.00), (3, 499.00, 449.00), (4, 429.00, 379.00), (5, 359.00, 309.00)) p(stage, price, vip)
WHERE l.lot_number = 'LOT-AGED-SHELF-001'
ON CONFLICT (fifo_lot_id, stage) DO NOTHING;

INSERT INTO fifo_lot_stage_prices (fifo_lot_id, stage, price_aud, vip_price_aud)
SELECT l.id, p.stage, p.price, p.vip FROM fifo_lots l
CROSS JOIN (VALUES (1, 699.00, 649.00), (2, 629.00, 579.00), (3, 549.00, 499.00), (4, 469.00, 419.00), (5, 389.00, 339.00)) p(stage, price, vip)
WHERE l.lot_number = 'LOT-AGED-SHELF-002'
ON CONFLICT (fifo_lot_id, stage) DO NOTHING;

INSERT INTO fifo_lot_stage_prices (fifo_lot_id, stage, price_aud, vip_price_aud)
SELECT l.id, p.stage, p.price, p.vip FROM fifo_lots l
CROSS JOIN (VALUES (1, 849.00, 789.00), (2, 749.00, 689.00), (3, 649.00, 589.00), (4, 549.00, 489.00), (5, 459.00, 399.00)) p(stage, price, vip)
WHERE l.lot_number = 'LOT-AGED-ORION-001'
ON CONFLICT (fifo_lot_id, stage) DO NOTHING;

INSERT INTO fifo_lot_stage_prices (fifo_lot_id, stage, price_aud, vip_price_aud)
SELECT l.id, p.stage, p.price, p.vip FROM fifo_lots l
CROSS JOIN (VALUES (1, 899.00, 839.00), (2, 799.00, 739.00), (3, 699.00, 639.00), (4, 599.00, 539.00), (5, 499.00, 439.00)) p(stage, price, vip)
WHERE l.lot_number = 'LOT-AGED-ORION-002'
ON CONFLICT (fifo_lot_id, stage) DO NOTHING;

-- 9. Update Warehouse Inventory for each variant in WH-164
INSERT INTO warehouse_inventory (warehouse_id, variant_id, on_hand_quantity, reserved_quantity, low_stock_threshold)
SELECT w.id, v.id, l.remaining_quantity, 0, 3
FROM fifo_lots l
JOIN product_variants v ON l.variant_id = v.id
JOIN warehouses w ON l.warehouse_id = w.id
WHERE l.lot_number LIKE 'LOT-AGED-%'
ON CONFLICT (warehouse_id, variant_id) DO UPDATE 
SET on_hand_quantity = EXCLUDED.on_hand_quantity;

-- 10. Sync Store Variant Daily Prices for STR-PER-01 based on oldest active FIFO lot current stage price
INSERT INTO store_variant_prices (store_id, variant_id, current_daily_price)
SELECT s.id, v.id, sp.price_aud
FROM stores s, fifo_lots l
JOIN product_variants v ON l.variant_id = v.id
JOIN fifo_lot_stage_prices sp ON sp.fifo_lot_id = l.id AND sp.stage = l.current_stage
WHERE s.code = 'STR-PER-01' AND l.lot_number LIKE 'LOT-AGED-%'
ON CONFLICT (store_id, variant_id) DO UPDATE 
SET current_daily_price = EXCLUDED.current_daily_price, updated_at = CURRENT_TIMESTAMP;
