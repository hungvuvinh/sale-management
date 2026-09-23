# Task Progress Tracker

| Task ID | Task Description | Status | Evidence / Notes |
| :--- | :--- | :--- | :--- |
| **M1-ANA** | Phân tích Nghiệp vụ Module 1: Sản phẩm, Giá 5 Stage & POS | Completed | Đã hoàn thành & lưu tại docs/plans/module_1_product_pos_spec.md |
| **M2-ANA** | Phân tích Nghiệp vụ Module 2: Kho & Stock Order | Completed | Đã hoàn thành & lưu tại docs/plans/module_2_warehouse_stock_order_spec.md |
| **M3-ANA** | Phân tích Nghiệp vụ Module 3: Giao nhận & Delivery | Completed | Đã hoàn thành & lưu tại docs/plans/module_3_delivery_spec.md |
| **M4-ANA** | Phân tích Nghiệp vụ Module 4 & 5: Marketing, FIFO & Báo cáo | Completed | Đã hoàn thành & lưu tại docs/plans/module_4_5_marketing_reporting_spec.md |
| **M6-ANA** | Phân tích Nghiệp vụ Module 6: Storefront & Realtime Chat | Completed | Đã hoàn thành & lưu tại docs/plans/module_6_storefront_chat_spec.md |
| **DES-ERD** | Thiết kế Cơ sở Dữ liệu & ERD | Completed | Đã hoàn thành tại docs/DATABASE_DESIGN.md (ERD Mermaid, DDL 28+ bảng, Indexing SLA, Backend-driven logic) & docs/full_database_schema_design.md |
| **DES-API** | Thiết kế API Specs (Swagger/OpenAPI) | Completed | Đã hoàn thành tại docs/API_SPEC.md (RESTful Endpoints 6 phân hệ, OpenAPI standard, WebSocket Chat Gateway) |
| **DES-SEQ** | Thiết kế Sequence Diagram & Luồng Dữ Liệu | Completed | Đã hoàn thành tại docs/DATA_FLOW_DESIGN.md (Fine-grained Cache Keys, Rate-limiting, Audit Encryption) |
| **IMP-PLAN** | Kế hoạch Triển khai Demo 7 Ngày (7-Day Demo Implementation Plan) | Completed | Đã lưu tại docs/plans/system_implementation_plan.md (Tối ưu 7 ngày làm bản Demo MVP khả thi) |
| **TECH-STACK** | Xác định & lưu Tech Stack: C#, React, Tailwind CSS, PostgREST | Completed | Đã lưu tài liệu tại docs/TECH_STACK.md |
| **D1-INFRA** | Docker Compose: PostgreSQL 16 + PostgREST + Redis 7.2 | Completed | Đã hoàn thành tại docker-compose.yml (Postgres 16, Redis 7.2, PostgREST 12, C# Backend) |
| **D1-MIGRATION** | Database Migration: 28+ bảng SQL Migrations & Seed | Completed | Đã lưu tại database/migrations/ (001-009 SQL migrations + 010_seed.sql) |
| **D1-BACKEND** | Khởi tạo C# ASP.NET Core API (.NET 10/8) & Swagger | Completed | Đã build thành công (0 warning, 0 error) tại backend/src/SaleManagement.Api/ (JwtBearer, Dapper, Npgsql, Swagger) |
| **D1-AUTH** | Auth & Health API: /api/auth/login, /api/auth/me, /api/healthz, /api/readyz | Completed | Đã triển khai AuthController & HealthController với JWT Auth & BCrypt verification |
| **D1-FRONTEND** | Khởi tạo React + Tailwind CSS Frontend (Vite) | Completed | Đã build bundle thành công (0 error) tại frontend/ (Axios client, Tailwind CSS v4, Lucide icons, Auth UI) |
| **DES-UI** | Thiết kế UI/UX Wireframes | Pending | Tuần 2 (Ngày 13) |
| **DES-REV** | Đóng gói Tài liệu Kiến trúc SADD | Pending | Tuần 2 (Ngày 14) |
| **D2-STORES** | API Stores, Warehouses & Postcodes Lookup (`/api/stores`, `/api/warehouses`, `/api/postcodes/lookup`) | Completed | Đã triển khai StoresController, StoresService & DTOs (Build 0 errors) |
| **D2-CATALOG** | API Product Catalog & Variants CRUD (`/api/categories`, `/api/products`) | Completed | Đã triển khai ProductsController, ProductsService & DTOs |
| **D2-PRICE-ENGINE**| Worker/Engine Giá 5-Stage & Auto Stage Transition (`/api/products/price-stage-check`) | Completed | Đã triển khai PricingEngineService & Price Audit Logging |
| **D2-LANDED-COST** | API Landed Cost Calculator & Container Stock Orders (`/api/stock-orders/landed-cost`) | Completed | Đã triển khai StockOrdersController, SuppliersController & StockOrdersService |
| **IMP-DEV** | Triển khai Core Implementation Sprint (Ngày 2 -> Ngày 7) | In Progress | Đã hoàn thành Ngày 2 & Ngày 3 (Build succeeded 0 errors) |
| **D3-INVENTORY** | Entities & Repository: WarehouseInventory, InventoryTransaction | Completed | InventoryEntities.cs, InventoryRepository.cs, IInventoryRepository.cs |
| **D3-ORDERS** | POS Orders API (`/api/orders`, `/api/customers`) + FIFO Allocation Engine | Completed | OrderEntities.cs, OrderRepository.cs, OrdersService.cs, OrdersController.cs |
| **D3-INVENTORY-API** | Inventory Lookup API (`/api/inventory`) | Completed | InventoryController.cs — xem tồn kho theo kho/variant |
| **D3-DAILY-PRICE** | Store-Specific Daily Price (`store_variant_prices`) & Lot Stage Price Table Rename (`fifo_lot_stage_prices`) | Completed | 012_create_store_variant_prices.sql, OrdersService.cs, PricingEngineService.cs, ProductsController.cs, DATABASE_DESIGN.md, DATABASE_TABLES_KEYS.md, API_SPEC.md, DATA_FLOW_DESIGN.md (Build succeeded 0 errors) |
| **D4-DELIVERY** | Delivery System, Dynamic Service Rates & Delivery Calendar API | Completed | 013_create_delivery_service_rates.sql, DeliveryEntities.cs, DeliveryRepository.cs, DeliveryService.cs, DeliveryController.cs (Build succeeded 0 errors) |
| **D5-FIFO-PROFIT** | Actual FIFO Profit Engine: chốt `ActualProfitAud` khi Order → COMPLETED từ tổng `RealizedProfitAud` các lô FIFO | Completed | OrdersService + IOrderRepository + OrderRepository: FinalizeActualProfitAsync / ClearActualProfitAsync (Build 0 errors) |
| **D5-COMMISSION** | Seller Commission Engine: KPI doanh số/giờ + Superannuation 9.5%, vòng đời DRAFT→FINALIZED→PAID | Completed | CommissionsService.cs, CommissionsController.cs, MarketingModels.cs (Build 0 errors) |
| **D5-MARKETING** | Marketing CRUD: Promotions, Product Combos (cross-sell), Ad Spend Logs | Completed | MarketingService.cs, MarketingController.cs, AppDbContext (7 Marketing DbSets) (Build 0 errors) |
| **D6-CHAT** | SignalR Live Chat: guest chưa đăng nhập, chọn store, CS_AGENT store scope, text message và idempotency | Completed | ChatController, ChatHub, ChatService, ChatRepository, migrations 017-018 (8 tests passed) |
| **D6-ATTACHMENT** | File attachment trong chat | Out of Scope | Chưa hỗ trợ theo quyết định MVP |
| **D6-RATE-LIMIT** | Redis token-bucket rate limiting theo IP/user/role | Completed | RedisRateLimitingMiddleware, RedisRateLimitStore, 2 middleware tests; backend 7/7 tests passed |
| **D7-STOREFRONT-UI** | Storefront UI: Postcode Lookup, Category Filter, Product Detail Page, Variant Selector & Cart Drawer | Completed | StorefrontPage.tsx (Vite build 0 errors 940ms) |
| **D7-ADMIN-MODALS** | Admin CRUD Popups: Orders Modal, Stock Order PO Modal, Product Modal, Variant Modal (Image Placeholders) & FIFO Lots | Completed | AdminModals.tsx, AdminInventoryPage.tsx (6 tabs, Vite build 0 errors) |
| **D7-DATASEEDING** | Database Seeding cho Frontend Operations (Postcodes 6000/2000/3000, POS Orders, Stock Orders, FIFO Lots, Store Variant Prices) | Completed | 021_seed_frontend_operations.sql |
| **D7-REFINEMENTS** | Refinements: Ẩn giá Storefront mặc định, FIFO Lots API & filter kho, PO Details Modal & multi-variant, FIFO 5-stage pricing modal, xóa variant, bỏ stage ở detail | Completed | InventoryController.cs, AdminModals.tsx, AdminInventoryPage.tsx, StorefrontPage.tsx (10/10 tests passed) |
| **D7-PRICE-SEED** | Seed data 13 scenarios cho Pricing Engine (Manager DB Configs, Max Days, Min Days Hold, Default Fallback Stages 1-5, Stable & Terminal Lots) & Service Unit Tests | Completed | 022_seed_pricing_engine_scenarios.sql, PricingEngineServiceTests.cs (23/23 tests passed) |
| **D7-AGED-SEED** | Seed data mở rộng: 10 sản phẩm mới, 24 biến thể, 6 đơn nhập hàng lịch sử và 24 lô FIFO lưu kho lâu ngày (>30 đến 150 ngày) | Completed | 023_seed_aged_inventory_products.sql (Đã nạp DB PostgreSQL: 13 SP, 27 Biến thể, 29 Lô hàng >30d, 195 Bảng giá Stage) |
| **D7-PRICE-REASON** | Pricing Engine Log Refinement ("Do A và B", không dùng "hoặc") & Admin UI cài đặt % tồn kho / min-max days (`VariantStageConfigModal`) | Completed | PricingEngineService.cs, AdminInventoryPage.tsx, AdminModals.tsx, PricingEngineServiceTests.cs (24/24 tests passed, Vite build 0 errors) |


