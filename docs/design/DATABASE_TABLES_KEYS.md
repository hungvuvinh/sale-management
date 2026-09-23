# Database Tables and Fields

> **Key notation:** `PK` = Primary Key, `FK` = Foreign Key. The `Key` column identifies keys directly on each field. FK values use `FK -> referenced_table.field`.

## addresses

| Field | Type | Key |
| --- | --- | --- |
| id | BIGSERIAL | PK |
| street_address | TEXT | - |
| address_detail | TEXT | - |
| suburb | VARCHAR(100) | - |
| city | VARCHAR(100) | - |
| state | VARCHAR(10) | - |
| postcode_id | BIGINT | FK -> postcodes.id |
| latitude | NUMERIC(10, 7) | - |
| longitude | NUMERIC(10, 7) | - |
| created_at | TIMESTAMPTZ | - |
| updated_at | TIMESTAMPTZ | - |

## stores

| Field | Type | Key |
| --- | --- | --- |
| id | BIGSERIAL | PK |
| code | VARCHAR(30) | - |
| name | VARCHAR(150) | - |
| address_id | BIGINT | FK -> addresses.id |
| phone | VARCHAR(30) | - |
| email | VARCHAR(100) | - |
| is_active | BOOLEAN | - |
| created_at | TIMESTAMPTZ | - |
| updated_at | TIMESTAMPTZ | - |

## users

| Field | Type | Key |
| --- | --- | --- |
| id | BIGSERIAL | PK |
| email | VARCHAR(255) | - |
| password_hash | VARCHAR(255) | - |
| first_name | VARCHAR(100) | - |
| last_name | VARCHAR(100) | - |
| phone | VARCHAR(30) | - |
| is_active | BOOLEAN | - |
| created_at | TIMESTAMPTZ | - |
| updated_at | TIMESTAMPTZ | - |

## roles

| Field | Type | Key |
| --- | --- | --- |
| id | BIGSERIAL | PK |
| code | VARCHAR(50) | - |
| name | VARCHAR(100) | - |
| description | TEXT | - |

## user_roles

| Field | Type | Key |
| --- | --- | --- |
| id | BIGSERIAL | PK |
| user_id | BIGINT | FK -> users.id |
| role_id | BIGINT | FK -> roles.id |
| store_id | BIGINT | FK -> stores.id, nullable |
| assigned_at | TIMESTAMPTZ | - |

## warehouses

| Field | Type | Key |
| --- | --- | --- |
| id | BIGSERIAL | PK |
| code | VARCHAR(30) | - |
| name | VARCHAR(150) | - |
| address_id | BIGINT | FK -> addresses.id |
| is_store | BOOLEAN | - |
| total_capacity_cbm | NUMERIC(12, 4) | - |
| is_active | BOOLEAN | - |
| created_at | TIMESTAMPTZ | - |
| updated_at | TIMESTAMPTZ | - |

## store_warehouses

| Field | Type | Key |
| --- | --- | --- |
| store_id | BIGINT | PK, FK -> stores.id |
| warehouse_id | BIGINT | PK, FK -> warehouses.id |
| priority | SMALLINT | - |

## postcodes

| Field | Type | Key |
| --- | --- | --- |
| id | BIGSERIAL | PK |
| postcode | VARCHAR(10) | UNIQUE |
| suburb_name | VARCHAR(100) | - |
| state | VARCHAR(10) | - |
| description | TEXT | - |

## postcode_stores

| Field | Type | Key |
| --- | --- | --- |
| postcode_id | BIGINT | PK, FK -> postcodes.id |
| store_id | BIGINT | PK, FK -> stores.id |

## store_shipping_rates

| Field | Type | Key |
| --- | --- | --- |
| id | BIGSERIAL | PK |
| store_id | BIGINT | FK -> stores.id |
| min_distance_km | NUMERIC(8, 2) | - |
| max_distance_km | NUMERIC(8, 2) | - |
| shipping_fee_aud | NUMERIC(15, 2) | - |
| created_at | TIMESTAMPTZ | - |

## product_categories

| Field | Type | Key |
| --- | --- | --- |
| id | BIGSERIAL | PK |
| parent_id | BIGINT | FK -> product_categories.id |
| name | VARCHAR(150) | - |
| slug | VARCHAR(150) | UNIQUE |
| description | TEXT | - |
| display_order | INTEGER | - |
| is_active | BOOLEAN | - |

## products

| Field | Type | Key |
| --- | --- | --- |
| id | BIGSERIAL | PK |
| sku | VARCHAR(50) | UNIQUE |
| name | VARCHAR(255) | - |
| slug | VARCHAR(255) | UNIQUE |
| category_id | BIGINT | FK -> product_categories.id |
| description | TEXT | - |
| materials_summary | TEXT | - |
| main_image_url | TEXT | - |
| gallery_images | JSONB | - |
| video_url | TEXT | - |
| is_active | BOOLEAN | - |
| created_at | TIMESTAMPTZ | - |
| updated_at | TIMESTAMPTZ | - |

## product_variants

| Field | Type | Key |
| --- | --- | --- |
| id | BIGSERIAL | PK |
| product_id | BIGINT | FK -> products.id |
| sku | VARCHAR(60) | UNIQUE |
| name | VARCHAR(255) | - |
| barcode | VARCHAR(50) | UNIQUE |
| attributes_json | JSONB | - |
| weight_kg | NUMERIC(10, 3) | - |
| cbm | NUMERIC(10, 4) | - |
| box_count | INTEGER | - |
| dimensions_cm | JSONB | - |
| is_active | BOOLEAN | - |
| created_at | TIMESTAMPTZ | - |
| updated_at | TIMESTAMPTZ | - |

## fifo_lot_stage_prices

| Field | Type | Key |
| --- | --- | --- |
| id | BIGSERIAL | PK |
| fifo_lot_id | BIGINT | FK -> fifo_lots.id |
| stage | SMALLINT | - |
| price_aud | NUMERIC(15, 2) | - |
| vip_price_aud | NUMERIC(15, 2) | - |
| updated_at | TIMESTAMPTZ | - |

## store_variant_prices

| Field | Type | Key |
| --- | --- | --- |
| id | BIGSERIAL | PK |
| store_id | BIGINT | FK -> stores.id |
| variant_id | BIGINT | FK -> product_variants.id |
| current_daily_price | NUMERIC(15, 2) | - |
| updated_at | TIMESTAMPTZ | - |

## pricing_stage_configs

| Field | Type | Key |
| --- | --- | --- |
| id | BIGSERIAL | PK |
| variant_id | BIGINT | FK -> product_variants.id |
| from_stage | SMALLINT | - |
| to_stage | SMALLINT | - |
| target_stock_pct | NUMERIC(5, 2) | - |
| min_days_at_stage | INTEGER | - |
| max_days_at_stage | INTEGER | - |

## stage_price_audit_logs

| Field | Type | Key |
| --- | --- | --- |
| id | BIGSERIAL | PK |
| fifo_lot_id | BIGINT | FK -> fifo_lots.id |
| old_stage | SMALLINT | - |
| new_stage | SMALLINT | - |
| old_price_aud | NUMERIC(15, 2) | - |
| new_price_aud | NUMERIC(15, 2) | - |
| triggered_by | VARCHAR(30) | - |
| triggered_by_user | BIGINT | FK -> users.id |
| remaining_stock_qty | INTEGER | - |
| remaining_stock_pct | NUMERIC(5, 2) | - |
| days_at_old_stage | INTEGER | - |
| reason | TEXT | - |
| changed_at | TIMESTAMPTZ | - |

## suppliers

| Field | Type | Key |
| --- | --- | --- |
| id | BIGSERIAL | PK |
| name | VARCHAR(200) | - |
| country | VARCHAR(100) | - |
| contact_name | VARCHAR(100) | - |
| contact_phone | VARCHAR(30) | - |
| contact_email | VARCHAR(100) | - |
| lead_time_days | INTEGER | - |
| payment_terms | TEXT | - |
| is_active | BOOLEAN | - |
| created_at | TIMESTAMPTZ | - |

## stock_orders

| Field | Type | Key |
| --- | --- | --- |
| id | BIGSERIAL | PK |
| po_number | VARCHAR(50) | UNIQUE |
| supplier_id | BIGINT | FK -> suppliers.id |
| destination_warehouse_id | BIGINT | FK -> warehouses.id |
| container_code | VARCHAR(50) | - |
| eta_date | DATE | - |
| actual_arrival_date | DATE | - |
| status | VARCHAR(30) | - |
| total_cbm | NUMERIC(12, 4) | - |
| container_freight_aud | NUMERIC(15, 2) | - |
| customs_tax_aud | NUMERIC(15, 2) | - |
| currency_code | VARCHAR(3) | - |
| exchange_rate | NUMERIC(12, 6) | - |
| notes | TEXT | - |
| created_at | TIMESTAMPTZ | - |
| updated_at | TIMESTAMPTZ | - |

## stock_order_items

| Field | Type | Key |
| --- | --- | --- |
| id | BIGSERIAL | PK |
| stock_order_id | BIGINT | FK -> stock_orders.id |
| variant_id | BIGINT | FK -> product_variants.id |
| quantity_ordered | INTEGER | - |
| quantity_received | INTEGER | - |
| unit_cost_foreign | NUMERIC(15, 2) | - |
| unit_cost_aud | NUMERIC(15, 2) | - |
| unit_cbm | NUMERIC(10, 4) | - |
| unit_freight_aud | NUMERIC(15, 2) | - |
| calculated_landed_cost_aud | NUMERIC(15, 2) | - |
| created_at | TIMESTAMPTZ | - |

## fifo_lots

| Field | Type | Key |
| --- | --- | --- |
| id | BIGSERIAL | PK |
| lot_number | VARCHAR(60) | UNIQUE |
| variant_id | BIGINT | FK -> product_variants.id |
| warehouse_id | BIGINT | FK -> warehouses.id |
| stock_order_item_id | BIGINT | FK -> stock_order_items.id |
| initial_quantity | INTEGER | - |
| remaining_quantity | INTEGER | - |
| unit_landed_cost_aud | NUMERIC(15, 2) | - |
| current_stage | SMALLINT | - |
| received_date | DATE | - |
| status | VARCHAR(20) | - |
| activated_at | TIMESTAMPTZ | - |
| exhausted_at | TIMESTAMPTZ | - |
| created_at | TIMESTAMPTZ | - |

## warehouse_inventory

| Field | Type | Key |
| --- | --- | --- |
| warehouse_id | BIGINT | PK, FK -> warehouses.id |
| variant_id | BIGINT | PK, FK -> product_variants.id |
| on_hand_quantity | INTEGER | - |
| reserved_quantity | INTEGER | - |
| available_quantity | INTEGER | - |
| low_stock_threshold | INTEGER | - |

## inventory_transactions

| Field | Type | Key |
| --- | --- | --- |
| id | BIGSERIAL | PK |
| transaction_code | VARCHAR(50) | - |
| warehouse_id | BIGINT | FK -> warehouses.id |
| fifo_lot_id | BIGINT | FK -> fifo_lots.id |
| transaction_type | VARCHAR(30) | - |
| change_quantity | INTEGER | - |
| reference_id | BIGINT | - |
| reference_type | VARCHAR(50) | - |
| performed_by_user | BIGINT | FK -> users.id |
| notes | TEXT | - |
| created_at | TIMESTAMPTZ | - |

## inventory_transfers

| Field | Type | Key |
| --- | --- | --- |
| id | BIGSERIAL | PK |
| transfer_code | VARCHAR(50) | - |
| from_warehouse_id | BIGINT | FK -> warehouses.id |
| to_warehouse_id | BIGINT | FK -> warehouses.id |  
| status | VARCHAR(30) | - |
| requested_by | BIGINT | FK -> users.id |
| approved_by | BIGINT | FK -> users.id |
| created_at | TIMESTAMPTZ | - |
| received_at | TIMESTAMPTZ | - |

## inventory_transfer_items

| Field | Type | Key |
| --- | --- | --- |
| id | BIGSERIAL | PK |
| transfer_id | BIGINT | FK -> inventory_transfers.id |
| variant_id | BIGINT | FK -> product_variants.id |
| quantity_requested | INTEGER | - |
| quantity_received | INTEGER | - |

## customers

| Field | Type | Key |
| --- | --- | --- |
| id | BIGSERIAL | PK |
| customer_code | VARCHAR(50) | - |
| first_name | VARCHAR(100) | - |
| last_name | VARCHAR(100) | - |
| email | VARCHAR(255) | - |
| phone | VARCHAR(30) | - |
| address_id | BIGINT | FK -> addresses.id |
| is_vip | BOOLEAN | - |
| loyalty_points | INTEGER | - |
| vip_granted_at | TIMESTAMPTZ | - |
| notes | TEXT | - |
| created_at | TIMESTAMPTZ | - |
| updated_at | TIMESTAMPTZ | - |

## customer_loyalty_transactions

| Field | Type | Key |
| --- | --- | --- |
| id | BIGSERIAL | PK |
| customer_id | BIGINT | FK -> customers.id |
| order_id | BIGINT | FK -> orders.id |
| points_delta | INTEGER | - |
| reason | VARCHAR(150) | - |
| created_at | TIMESTAMPTZ | - |

## orders

| Field | Type | Key |
| --- | --- | --- |
| id | BIGSERIAL | PK |
| order_code | VARCHAR(60) | - |
| customer_id | BIGINT | FK -> customers.id |
| store_id | BIGINT | FK -> stores.id |
| salesperson_user_id | BIGINT | FK -> users.id |
| order_channel | VARCHAR(30) | - |
| order_type | VARCHAR(30) | - |
| status | VARCHAR(30) | - |
| payment_status | VARCHAR(30) | - |
| subtotal_aud | NUMERIC(15, 2) | - |
| discount_aud | NUMERIC(15, 2) | - |
| shipping_fee_aud | NUMERIC(15, 2) | - |
| total_aud | NUMERIC(15, 2) | - |
| paid_amount_aud | NUMERIC(15, 2) | - |
| expected_profit_aud | NUMERIC(15, 2) | - |
| actual_profit_aud | NUMERIC(15, 2) | - |
| target_delivery_date | DATE | - |
| notes | TEXT | - |
| created_at | TIMESTAMPTZ | - |
| updated_at | TIMESTAMPTZ | - |

## order_items

| Field | Type | Key |
| --- | --- | --- |
| id | BIGSERIAL | PK |
| order_id | BIGINT | FK -> orders.id |
| variant_id | BIGINT | FK -> product_variants.id |
| quantity | INTEGER | - |
| unit_price_aud | NUMERIC(15, 2) | - |
| stage_applied | SMALLINT | - |
| is_vip_price | BOOLEAN | - |
| discount_aud | NUMERIC(15, 2) | - |
| line_total_aud | NUMERIC(15, 2) | - |
| created_at | TIMESTAMPTZ | - |

## order_audit_logs

| Field | Type | Key |
| --- | --- | --- |
| id | BIGSERIAL | PK |
| order_id | BIGINT | FK -> orders.id |
| user_id | BIGINT | FK -> users.id |
| action | VARCHAR(50) | - |
| change_summary | TEXT | - |
| old_data_encrypted | TEXT | - |
| new_data_encrypted | TEXT | - |
| key_version | VARCHAR(10) | - |
| ip_address | VARCHAR(45) | - |
| created_at | TIMESTAMPTZ | - |

## order_item_fifo_allocations

| Field | Type | Key |
| --- | --- | --- |
| id | BIGSERIAL | PK |
| order_item_id | BIGINT | FK -> order_items.id |
| fifo_lot_id | BIGINT | FK -> fifo_lots.id |
| quantity_allocated | INTEGER | - |
| unit_landed_cost_aud | NUMERIC(15, 2) | - |
| unit_selling_price_aud | NUMERIC(15, 2) | - |
| realized_profit_aud | NUMERIC(15, 2) | - |
| status | VARCHAR(20) | - |
| created_at | TIMESTAMPTZ | - |

## order_payments

| Field | Type | Key |
| --- | --- | --- |
| id | BIGSERIAL | PK |
| order_id | BIGINT | FK -> orders.id |
| payment_method | VARCHAR(30) | - |
| amount_aud | NUMERIC(15, 2) | - |
| cash_tendered_aud | NUMERIC(15, 2) | - |
| change_given_aud | NUMERIC(15, 2) | - |
| transaction_ref | VARCHAR(100) | - |
| idempotency_key | VARCHAR(100) | - |
| cashier_user_id | BIGINT | FK -> users.id |
| paid_at | TIMESTAMPTZ | - |

## carriers

| Field | Type | Key |
| --- | --- | --- |
| id | BIGSERIAL | PK |
| name | VARCHAR(150) | - |
| code | VARCHAR(50) | - |
| phone | VARCHAR(30) | - |
| email | VARCHAR(100) | - |
| is_internal | BOOLEAN | - |
| rate_card_json | JSONB | - |
| is_active | BOOLEAN | - |
| created_at | TIMESTAMPTZ | - |

## drivers

| Field | Type | Key |
| --- | --- | --- |
| id | BIGSERIAL | PK |
| carrier_id | BIGINT | FK -> carriers.id |
| user_id | BIGINT | FK -> users.id |
| full_name | VARCHAR(100) | - |
| phone | VARCHAR(30) | - |
| license_number | VARCHAR(50) | - |
| is_active | BOOLEAN | - |
| created_at | TIMESTAMPTZ | - |

## delivery_bookings

| Field | Type | Key |
| --- | --- | --- |
| id | BIGSERIAL | PK |
| order_id | BIGINT | FK -> orders.id |
| delivery_address_id | BIGINT | FK -> addresses.id |
| scheduled_date | DATE | - |
| time_slot | VARCHAR(30) | - |
| is_assembling | BOOLEAN | - |
| is_upstairs | BOOLEAN | - |
| stairs_floor_count | INTEGER | - |
| special_notes | TEXT | - |
| surcharge_aud | NUMERIC(15, 2) | - |
| status | VARCHAR(30) | - |
| created_at | TIMESTAMPTZ | - |
| updated_at | TIMESTAMPTZ | - |

## delivery_service_rates

| Field | Type | Key |
| --- | --- | --- |
| id | BIGSERIAL | PK |
| code | VARCHAR(50) | UNIQUE |
| name | VARCHAR(100) | - |
| fee_type | VARCHAR(30) | - |
| fee_aud | NUMERIC(15, 2) | - |
| is_active | BOOLEAN | - |
| created_at | TIMESTAMPTZ | - |
| updated_at | TIMESTAMPTZ | - |

## delivery_booking_services

| Field | Type | Key |
| --- | --- | --- |
| id | BIGSERIAL | PK |
| booking_id | BIGINT | FK -> delivery_bookings.id |
| service_rate_id | BIGINT | FK -> delivery_service_rates.id |
| quantity | INTEGER | - |
| unit_fee_aud | NUMERIC(15, 2) | - |
| line_surcharge_aud | NUMERIC(15, 2) | - |
| created_at | TIMESTAMPTZ | - |

## delivery_routes

| Field | Type | Key |
| --- | --- | --- |
| id | BIGSERIAL | PK |
| route_code | VARCHAR(50) | - |
| carrier_id | BIGINT | FK -> carriers.id |
| driver_id | BIGINT | FK -> drivers.id |
| origin_warehouse_id | BIGINT | FK -> warehouses.id |
| delivery_date | DATE | - |
| total_distance_km | NUMERIC(8, 2) | - |
| total_duration_mins | INTEGER | - |
| status | VARCHAR(30) | - |
| created_at | TIMESTAMPTZ | - |

## delivery_stops

| Field | Type | Key |
| --- | --- | --- |
| id | BIGSERIAL | PK |
| route_id | BIGINT | FK -> delivery_routes.id |
| booking_id | BIGINT | FK -> delivery_bookings.id |
| sequence_order | INTEGER | - |
| estimated_arrival | TIMESTAMPTZ | - |
| actual_arrival | TIMESTAMPTZ | - |
| status | VARCHAR(30) | - |
| proof_of_delivery_url | TEXT | - |
| customer_feedback | TEXT | - |
| failure_reason | TEXT | - |

## promotions

| Field | Type | Key |
| --- | --- | --- |
| id | BIGSERIAL | PK |
| name | VARCHAR(150) | - |
| badge_label | VARCHAR(50) | - |
| discount_type | VARCHAR(20) | - |
| discount_value | NUMERIC(15, 2) | - |
| start_date | TIMESTAMPTZ | - |
| end_date | TIMESTAMPTZ | - |
| is_active | BOOLEAN | - |
| created_at | TIMESTAMPTZ | - |

## promotion_products

| Field | Type | Key |
| --- | --- | --- |
| promotion_id | BIGINT | PK, FK -> promotions.id |
| product_id | BIGINT | PK, FK -> products.id |

## product_combos

| Field | Type | Key |
| --- | --- | --- |
| id | BIGSERIAL | PK |
| name | VARCHAR(150) | - |
| code | VARCHAR(50) | UNIQUE |
| combo_price_aud | NUMERIC(15, 2) | - |
| description | TEXT | - |
| is_active | BOOLEAN | - |
| created_at | TIMESTAMPTZ | - |

## combo_items

| Field | Type | Key |
| --- | --- | --- |
| id | BIGSERIAL | PK |
| combo_id | BIGINT | FK -> product_combos.id |
| variant_id | BIGINT | FK -> product_variants.id |
| quantity | INTEGER | - |

## product_associations

| Field | Type | Key |
| --- | --- | --- |
| product_id | BIGINT | PK, FK -> products.id |
| associated_product_id | BIGINT | PK, FK -> products.id |
| association_type | VARCHAR(30) | - |

## ad_spend_logs

| Field | Type | Key |
| --- | --- | --- |
| id | BIGSERIAL | PK |
| campaign_name | VARCHAR(200) | - |
| platform | VARCHAR(50) | - |
| spend_aud | NUMERIC(15, 2) | - |
| start_date | DATE | - |
| end_date | DATE | - |
| created_at | TIMESTAMPTZ | - |

## sales_shifts

| Field | Type | Key |
| --- | --- | --- |
| id | BIGSERIAL | PK |
| user_id | BIGINT | FK -> users.id |
| store_id | BIGINT | FK -> stores.id |
| work_date | DATE | - |
| hours_worked | NUMERIC(6, 2) | - |
| created_at | TIMESTAMPTZ | - |

## sales_commissions

| Field | Type | Key |
| --- | --- | --- |
| id | BIGSERIAL | PK |
| user_id | BIGINT | FK -> users.id |
| period_start | DATE | - |
| period_end | DATE | - |
| total_sales_aud | NUMERIC(15, 2) | - |
| total_hours_worked | NUMERIC(8, 2) | - |
| sales_per_hour_aud | NUMERIC(15, 2) | - |
| kpi_threshold_aud | NUMERIC(15, 2) | - |
| target_sales_aud | NUMERIC(15, 2) | - |
| excess_sales_aud | NUMERIC(15, 2) | - |
| commission_rate_pct | NUMERIC(5, 2) | - |
| gross_commission_aud | NUMERIC(15, 2) | - |
| superannuation_pct | NUMERIC(5, 2) | - |
| superannuation_aud | NUMERIC(15, 2) | - |
| net_commission_aud | NUMERIC(15, 2) | - |
| status | VARCHAR(30) | - |
| created_at | TIMESTAMPTZ | - |

## chat_sessions

| Field | Type | Key |
| --- | --- | --- |
| id | BIGSERIAL | PK |
| session_token | VARCHAR(100) | - |
| store_id | BIGINT | FK -> stores.id |
| customer_id | BIGINT | FK -> customers.id |
| participant_type | VARCHAR(20) | - (`GUEST` hoặc `CUSTOMER`) |
| guest_name | VARCHAR(100) | - |
| guest_email | VARCHAR(150) | - |
| guest_postcode | VARCHAR(10) | - |
| assigned_agent_user_id | BIGINT | FK -> users.id |
| status | VARCHAR(30) | - |
| created_at | TIMESTAMPTZ | - |
| updated_at | TIMESTAMPTZ | - |

## chat_messages

| Field | Type | Key |
| --- | --- | --- |
| id | BIGSERIAL | PK |
| session_id | BIGINT | FK -> chat_sessions.id |
| sender_user_id | BIGINT | FK -> users.id, nullable |
| sender_type | VARCHAR(20) | - |
| client_message_id | VARCHAR(100) | nullable, unique per session |
| message_text | TEXT | - |
| attachment_url | TEXT | - |
| is_read | BOOLEAN | - |
| sent_at | TIMESTAMPTZ | - |

## system_audit_logs

| Field | Type | Key |
| --- | --- | --- |
| id | BIGSERIAL | PK |
| entity_type | VARCHAR(50) | - |
| entity_id | BIGINT | - |
| actor_id | BIGINT | FK -> users.id |
| action | VARCHAR(50) | - |
| old_data_encrypted | TEXT | - |
| new_data_encrypted | TEXT | - |
| key_version | VARCHAR(10) | - |
| ip_address | VARCHAR(45) | - |
| created_at | TIMESTAMPTZ | - |

