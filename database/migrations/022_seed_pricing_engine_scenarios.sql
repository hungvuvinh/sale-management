-- MIGRATION 022: Comprehensive seed data for all Pricing Engine test scenarios
-- Covers Manager DB Configs (Stock %, Max Days, Min Days Hold), Default Fallback Rules (Stages 1-5 by Stock % & Time), Stable Lots, and Terminal Clearance Stage.

-- 1. Ensure Manager Pricing Stage Configurations for specific test variants
-- Variant SOFA-LUNA-GREY has manager configurations
INSERT INTO pricing_stage_configs (variant_id, from_stage, to_stage, target_stock_pct, min_days_at_stage, max_days_at_stage)
SELECT v.id, 1, 2, 70.0, 7, 45
FROM product_variants v WHERE v.sku = 'SOFA-LUNA-GREY'
ON CONFLICT (variant_id, from_stage) DO UPDATE 
SET target_stock_pct = EXCLUDED.target_stock_pct, min_days_at_stage = EXCLUDED.min_days_at_stage, max_days_at_stage = EXCLUDED.max_days_at_stage;

INSERT INTO pricing_stage_configs (variant_id, from_stage, to_stage, target_stock_pct, min_days_at_stage, max_days_at_stage)
SELECT v.id, 2, 3, 50.0, 7, 45
FROM product_variants v WHERE v.sku = 'SOFA-LUNA-GREY'
ON CONFLICT (variant_id, from_stage) DO UPDATE 
SET target_stock_pct = EXCLUDED.target_stock_pct, min_days_at_stage = EXCLUDED.min_days_at_stage, max_days_at_stage = EXCLUDED.max_days_at_stage;

-- Variant BED-PRADO-KING-BLK has manager configurations
INSERT INTO pricing_stage_configs (variant_id, from_stage, to_stage, target_stock_pct, min_days_at_stage, max_days_at_stage)
SELECT v.id, 2, 3, 50.0, 7, 45
FROM product_variants v WHERE v.sku = 'BED-PRADO-KING-BLK'
ON CONFLICT (variant_id, from_stage) DO UPDATE 
SET target_stock_pct = EXCLUDED.target_stock_pct, min_days_at_stage = EXCLUDED.min_days_at_stage, max_days_at_stage = EXCLUDED.max_days_at_stage;

-- 2. Create FIFO Lots representing distinct Pricing Engine test scenarios

-- [SCENARIO 1]: Manager Config - Triggered by Remaining Stock Pct (<= 70% & >= 7 days)
-- Initial: 20, Rem: 12 (60% <= 70%), Days: 10 (>= 7) -> Transition Stage 1 -> 2
INSERT INTO fifo_lots (lot_number, variant_id, warehouse_id, stock_order_item_id, initial_quantity, remaining_quantity, unit_landed_cost_aud, current_stage, received_date, status, activated_at, created_at)
SELECT 'LOT-PRICE-MGR-PCT', v.id, w.id, 1, 20, 12, 930.00, 1, CURRENT_DATE - 10, 'ACTIVE', CURRENT_TIMESTAMP - INTERVAL '10 days', CURRENT_TIMESTAMP - INTERVAL '10 days'
FROM product_variants v, warehouses w
WHERE v.sku = 'SOFA-LUNA-GREY' AND w.code = 'WH-164'
ON CONFLICT (lot_number) DO NOTHING;

-- [SCENARIO 2]: Manager Config - Blocked because Min Days NOT met (50% <= 70% BUT only 3 days < 7 min days)
-- Initial: 20, Rem: 10 (50%), Days: 3 (< 7 min days) -> NO transition (Hold)
INSERT INTO fifo_lots (lot_number, variant_id, warehouse_id, stock_order_item_id, initial_quantity, remaining_quantity, unit_landed_cost_aud, current_stage, received_date, status, activated_at, created_at)
SELECT 'LOT-PRICE-MGR-MIN-BLOCKED', v.id, w.id, 1, 20, 10, 930.00, 1, CURRENT_DATE - 3, 'ACTIVE', CURRENT_TIMESTAMP - INTERVAL '3 days', CURRENT_TIMESTAMP - INTERVAL '3 days'
FROM product_variants v, warehouses w
WHERE v.sku = 'SOFA-LUNA-GREY' AND w.code = 'WH-164'
ON CONFLICT (lot_number) DO NOTHING;

-- [SCENARIO 3]: Manager Config - Triggered by Max Days Exceeded (>= 45 days, even with 90% stock remaining)
-- Initial: 10, Rem: 9 (90%), Days: 50 (>= 45 max days) -> Transition Stage 2 -> 3
INSERT INTO fifo_lots (lot_number, variant_id, warehouse_id, stock_order_item_id, initial_quantity, remaining_quantity, unit_landed_cost_aud, current_stage, received_date, status, activated_at, created_at)
SELECT 'LOT-PRICE-MGR-MAXDAYS', v.id, w.id, 2, 10, 9, 745.00, 2, CURRENT_DATE - 50, 'ACTIVE', CURRENT_TIMESTAMP - INTERVAL '50 days', CURRENT_TIMESTAMP - INTERVAL '50 days'
FROM product_variants v, warehouses w
WHERE v.sku = 'BED-PRADO-KING-BLK' AND w.code = 'WH-164'
ON CONFLICT (lot_number) DO NOTHING;

-- [SCENARIO 4]: Default Rule - Stage 1 -> 2 Triggered by Low Stock (<= 80%)
-- Initial: 10, Rem: 7 (70% <= 80%), Days: 3 (< 14) -> Transition Stage 1 -> 2
INSERT INTO fifo_lots (lot_number, variant_id, warehouse_id, stock_order_item_id, initial_quantity, remaining_quantity, unit_landed_cost_aud, current_stage, received_date, status, activated_at, created_at)
SELECT 'LOT-PRICE-DEF-STG1-STOCK', v.id, w.id, 3, 10, 7, 535.00, 1, CURRENT_DATE - 3, 'ACTIVE', CURRENT_TIMESTAMP - INTERVAL '3 days', CURRENT_TIMESTAMP - INTERVAL '3 days'
FROM product_variants v, warehouses w
WHERE v.sku = 'DIN-MARLOW-OAK' AND w.code = 'WH-164'
ON CONFLICT (lot_number) DO NOTHING;

-- [SCENARIO 5]: Default Rule - Stage 1 -> 2 Triggered by Time (>= 14 days, even with 90% stock)
-- Initial: 10, Rem: 9 (90% > 80%), Days: 18 (>= 14) -> Transition Stage 1 -> 2
INSERT INTO fifo_lots (lot_number, variant_id, warehouse_id, stock_order_item_id, initial_quantity, remaining_quantity, unit_landed_cost_aud, current_stage, received_date, status, activated_at, created_at)
SELECT 'LOT-PRICE-DEF-STG1-TIME', v.id, w.id, 3, 10, 9, 535.00, 1, CURRENT_DATE - 18, 'ACTIVE', CURRENT_TIMESTAMP - INTERVAL '18 days', CURRENT_TIMESTAMP - INTERVAL '18 days'
FROM product_variants v, warehouses w
WHERE v.sku = 'DIN-MARLOW-OAK' AND w.code = 'WH-164'
ON CONFLICT (lot_number) DO NOTHING;

-- [SCENARIO 6]: Default Rule - Stage 2 -> 3 Triggered by Low Stock (<= 60%)
-- Initial: 10, Rem: 5 (50% <= 60%), Days: 8 (< 30) -> Transition Stage 2 -> 3
INSERT INTO fifo_lots (lot_number, variant_id, warehouse_id, stock_order_item_id, initial_quantity, remaining_quantity, unit_landed_cost_aud, current_stage, received_date, status, activated_at, created_at)
SELECT 'LOT-PRICE-DEF-STG2-STOCK', v.id, w.id, 3, 10, 5, 535.00, 2, CURRENT_DATE - 8, 'ACTIVE', CURRENT_TIMESTAMP - INTERVAL '8 days', CURRENT_TIMESTAMP - INTERVAL '8 days'
FROM product_variants v, warehouses w
WHERE v.sku = 'DIN-MARLOW-OAK' AND w.code = 'WH-164'
ON CONFLICT (lot_number) DO NOTHING;

-- [SCENARIO 7]: Default Rule - Stage 2 -> 3 Triggered by Time (>= 30 days)
-- Initial: 10, Rem: 8 (80% > 60%), Days: 35 (>= 30) -> Transition Stage 2 -> 3
INSERT INTO fifo_lots (lot_number, variant_id, warehouse_id, stock_order_item_id, initial_quantity, remaining_quantity, unit_landed_cost_aud, current_stage, received_date, status, activated_at, created_at)
SELECT 'LOT-PRICE-DEF-STG2-TIME', v.id, w.id, 3, 10, 8, 535.00, 2, CURRENT_DATE - 35, 'ACTIVE', CURRENT_TIMESTAMP - INTERVAL '35 days', CURRENT_TIMESTAMP - INTERVAL '35 days'
FROM product_variants v, warehouses w
WHERE v.sku = 'DIN-MARLOW-OAK' AND w.code = 'WH-164'
ON CONFLICT (lot_number) DO NOTHING;

-- [SCENARIO 8]: Default Rule - Stage 3 -> 4 Triggered by Low Stock (<= 40%)
-- Initial: 10, Rem: 3 (30% <= 40%), Days: 10 (< 45) -> Transition Stage 3 -> 4
INSERT INTO fifo_lots (lot_number, variant_id, warehouse_id, stock_order_item_id, initial_quantity, remaining_quantity, unit_landed_cost_aud, current_stage, received_date, status, activated_at, created_at)
SELECT 'LOT-PRICE-DEF-STG3-STOCK', v.id, w.id, 2, 10, 3, 745.00, 3, CURRENT_DATE - 10, 'ACTIVE', CURRENT_TIMESTAMP - INTERVAL '10 days', CURRENT_TIMESTAMP - INTERVAL '10 days'
FROM product_variants v, warehouses w
WHERE v.sku = 'BED-PRADO-KING-BLK' AND w.code = 'WH-164'
ON CONFLICT (lot_number) DO NOTHING;

-- [SCENARIO 9]: Default Rule - Stage 3 -> 4 Triggered by Time (>= 45 days)
-- Initial: 10, Rem: 7 (70% > 40%), Days: 48 (>= 45) -> Transition Stage 3 -> 4
INSERT INTO fifo_lots (lot_number, variant_id, warehouse_id, stock_order_item_id, initial_quantity, remaining_quantity, unit_landed_cost_aud, current_stage, received_date, status, activated_at, created_at)
SELECT 'LOT-PRICE-DEF-STG3-TIME', v.id, w.id, 3, 10, 7, 535.00, 3, CURRENT_DATE - 48, 'ACTIVE', CURRENT_TIMESTAMP - INTERVAL '48 days', CURRENT_TIMESTAMP - INTERVAL '48 days'
FROM product_variants v, warehouses w
WHERE v.sku = 'DIN-MARLOW-OAK' AND w.code = 'WH-164'
ON CONFLICT (lot_number) DO NOTHING;

-- [SCENARIO 10]: Default Rule - Stage 4 -> 5 Clearance Triggered by Low Stock (<= 20%)
-- Initial: 10, Rem: 1 (10% <= 20%), Days: 15 (< 60) -> Transition Stage 4 -> 5 (Clearance)
INSERT INTO fifo_lots (lot_number, variant_id, warehouse_id, stock_order_item_id, initial_quantity, remaining_quantity, unit_landed_cost_aud, current_stage, received_date, status, activated_at, created_at)
SELECT 'LOT-PRICE-DEF-STG4-STOCK', v.id, w.id, 3, 10, 1, 535.00, 4, CURRENT_DATE - 15, 'ACTIVE', CURRENT_TIMESTAMP - INTERVAL '15 days', CURRENT_TIMESTAMP - INTERVAL '15 days'
FROM product_variants v, warehouses w
WHERE v.sku = 'DIN-MARLOW-OAK' AND w.code = 'WH-164'
ON CONFLICT (lot_number) DO NOTHING;

-- [SCENARIO 11]: Default Rule - Stage 4 -> 5 Clearance Triggered by Time (>= 60 days)
-- Initial: 10, Rem: 5 (50% > 20%), Days: 65 (>= 60) -> Transition Stage 4 -> 5 (Clearance)
INSERT INTO fifo_lots (lot_number, variant_id, warehouse_id, stock_order_item_id, initial_quantity, remaining_quantity, unit_landed_cost_aud, current_stage, received_date, status, activated_at, created_at)
SELECT 'LOT-PRICE-DEF-STG4-TIME', v.id, w.id, 3, 10, 5, 535.00, 4, CURRENT_DATE - 65, 'ACTIVE', CURRENT_TIMESTAMP - INTERVAL '65 days', CURRENT_TIMESTAMP - INTERVAL '65 days'
FROM product_variants v, warehouses w
WHERE v.sku = 'DIN-MARLOW-OAK' AND w.code = 'WH-164'
ON CONFLICT (lot_number) DO NOTHING;

-- [SCENARIO 12]: Stable Lot - Stage 1, High Stock & Low Days (No transition)
-- Initial: 10, Rem: 10 (100%), Days: 2 (< 14) -> NO transition
INSERT INTO fifo_lots (lot_number, variant_id, warehouse_id, stock_order_item_id, initial_quantity, remaining_quantity, unit_landed_cost_aud, current_stage, received_date, status, activated_at, created_at)
SELECT 'LOT-PRICE-STABLE-STG1', v.id, w.id, 1, 10, 10, 930.00, 1, CURRENT_DATE - 2, 'ACTIVE', CURRENT_TIMESTAMP - INTERVAL '2 days', CURRENT_TIMESTAMP - INTERVAL '2 days'
FROM product_variants v, warehouses w
WHERE v.sku = 'SOFA-LUNA-GREY' AND w.code = 'WH-164'
ON CONFLICT (lot_number) DO NOTHING;

-- [SCENARIO 13]: Terminal Clearance Stage 5 - Max Stage Reached (No further transition)
-- Initial: 10, Rem: 2 (20%), Days: 90 (> 60), CurrentStage: 5 -> NO transition (Terminal stage)
INSERT INTO fifo_lots (lot_number, variant_id, warehouse_id, stock_order_item_id, initial_quantity, remaining_quantity, unit_landed_cost_aud, current_stage, received_date, status, activated_at, created_at)
SELECT 'LOT-PRICE-TERMINAL-STG5', v.id, w.id, 3, 10, 2, 535.00, 5, CURRENT_DATE - 90, 'ACTIVE', CURRENT_TIMESTAMP - INTERVAL '90 days', CURRENT_TIMESTAMP - INTERVAL '90 days'
FROM product_variants v, warehouses w
WHERE v.sku = 'DIN-MARLOW-OAK' AND w.code = 'WH-164'
ON CONFLICT (lot_number) DO NOTHING;

-- 3. Seed 5-Stage Prices for each of these test lots
INSERT INTO fifo_lot_stage_prices (fifo_lot_id, stage, price_aud, vip_price_aud)
SELECT l.id, p.stage, p.price, p.vip FROM fifo_lots l
CROSS JOIN (VALUES (1, 1699.00, 1599.00), (2, 1499.00, 1399.00), (3, 1299.00, 1199.00), (4, 1099.00, 999.00), (5, 899.00, 799.00)) p(stage, price, vip)
WHERE l.lot_number IN ('LOT-PRICE-MGR-PCT', 'LOT-PRICE-MGR-MIN-BLOCKED', 'LOT-PRICE-STABLE-STG1')
ON CONFLICT (fifo_lot_id, stage) DO NOTHING;

INSERT INTO fifo_lot_stage_prices (fifo_lot_id, stage, price_aud, vip_price_aud)
SELECT l.id, p.stage, p.price, p.vip FROM fifo_lots l
CROSS JOIN (VALUES (1, 1399.00, 1299.00), (2, 1249.00, 1149.00), (3, 1099.00, 999.00), (4, 899.00, 799.00), (5, 749.00, 649.00)) p(stage, price, vip)
WHERE l.lot_number IN ('LOT-PRICE-MGR-MAXDAYS', 'LOT-PRICE-DEF-STG3-STOCK')
ON CONFLICT (fifo_lot_id, stage) DO NOTHING;

INSERT INTO fifo_lot_stage_prices (fifo_lot_id, stage, price_aud, vip_price_aud)
SELECT l.id, p.stage, p.price, p.vip FROM fifo_lots l
CROSS JOIN (VALUES (1, 999.00, 949.00), (2, 899.00, 849.00), (3, 799.00, 749.00), (4, 699.00, 649.00), (5, 599.00, 549.00)) p(stage, price, vip)
WHERE l.lot_number IN (
    'LOT-PRICE-DEF-STG1-STOCK', 'LOT-PRICE-DEF-STG1-TIME',
    'LOT-PRICE-DEF-STG2-STOCK', 'LOT-PRICE-DEF-STG2-TIME',
    'LOT-PRICE-DEF-STG3-TIME',
    'LOT-PRICE-DEF-STG4-STOCK', 'LOT-PRICE-DEF-STG4-TIME',
    'LOT-PRICE-TERMINAL-STG5'
)
ON CONFLICT (fifo_lot_id, stage) DO NOTHING;
