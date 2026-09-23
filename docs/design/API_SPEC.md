# 🌐 TÀI LIỆU ĐẶC TẢ ĐẦY ĐỦ API (FULL DEMO & DEVELOPMENT SPEC)
**Dự án:** Hệ Thống Quản Lý Bán Hàng & Thương Mại Điện Tử Đa Chi Nhánh  
**Môi trường:** Local Development, Testing & Demo  
**Base URL:** `http://localhost:3000/api`  
**SignalR Hub URL:** `http://localhost:5000/hubs/chat` (SignalR negotiates WebSocket when available)
**Tài liệu CSDL liên kết:** [DATABASE_DESIGN.md](file:///d:/Intern/sale-management/docs/DATABASE_DESIGN.md)  
**Phiên bản:** Demo 2.0 (Đầy đủ 11 Phân hệ / Toàn bộ 6 Modules)  

---

## 1. QUY ƯỚC CHUNG

- **Định dạng dữ liệu:** JSON (`Content-Type: application/json`)
- **Tiền tệ:** Đô la Úc (**AUD**) dạng số thực 2 chữ số thập phân (ví dụ: `1099.00`).
- **Xác thực:** Header `Authorization: Bearer <token>` (đối với các API nội bộ quản trị/POS).
- **Phân trang chuẩn (Pagination - #2):**
  - Mọi API trả về dạng danh sách (list endpoints) hỗ trợ Query Parameters:
    - `page` (integer, mặc định `1`, giá trị $\ge 1$)
    - `limit` (integer, mặc định `20`, tối đa `100`)
  - Cấu trúc phản hồi danh sách kèm Metadata:
    ```json
    {
      "success": true,
      "data": [ ... ],
      "meta": {
        "total": 125,
        "page": 1,
        "limit": 20,
        "totalPages": 7
      }
    }
    ```
- **Rate-Limiting & Throttling (#3):**
  - Hệ thống áp dụng kiểm soát lưu lượng theo thuật toán Token Bucket / Sliding Window qua Redis.
  - Headers phản hồi đi kèm mỗi request:
    - `X-RateLimit-Limit`: Tổng số request cho phép trong cửa sổ thời gian.
    - `X-RateLimit-Remaining`: Số request còn lại trong cửa sổ hiện tại.
    - `X-RateLimit-Reset`: Thời gian còn lại (tính bằng giây) cho tới khi bucket được nạp đầy.
  - Cấu trúc phản hồi lỗi khi vượt ngưỡng `429 Too Many Requests`:
    ```json
    {
      "success": false,
      "message": "Quá số lượng truy cập cho phép. Vui lòng thử lại sau.",
      "retryAfter": 30
    }
    ```
  - Ngưỡng giới hạn truy cập theo vai trò:
    - Storefront Public (Khách vãng lai): **60 request / phút**
    - POS Terminal & Staff: **300 request / phút**
    - Admin & System Background Services: **1000 request / phút**
  - `/api/healthz` và `/api/readyz` không tính vào quota để health probe không bị giới hạn.
  - Nếu Redis rate limiter không khả dụng, API trả `503`; các event SignalR `send_message` và `typing` cũng có quota riêng.
- **Cấu trúc phản hồi chi tiết/đơn lẻ:**
  - Thành công: `{ "success": true, "data": { ... }, "message": "Thông báo nếu có" }`
  - Thất bại: `{ "success": false, "message": "Mô tả lỗi chi tiết" }`

---

## 2. BẢNG MỤC LỤC TOÀN BỘ ENDPOINTS THEO PHÂN HỆ

### 2.1. Phân hệ 1: Xác Thực & Người Dùng (Auth & Users)
| Method | Endpoint | Mô tả |
| :---: | :--- | :--- |
| `POST` | `/api/auth/login` | Đăng nhập hệ thống (Admin / Quản lý / Thu ngân / Kho / Tài xế) |
| `POST` | `/api/auth/logout` | Đăng xuất hệ thống |
| `GET` | `/api/auth/me` | Lấy thông tin tài khoản hiện tại kèm danh sách vai trò |
| `GET` | `/api/users` | Danh sách nhân viên trong hệ thống theo chi nhánh |
| `POST` | `/api/users` | Tạo mới tài khoản nhân viên & phân quyền RBAC |

### 2.2. Phân hệ 2: Cửa Hàng, Kho Bãi & Định Tuyến Postcode (Stores, Warehouses & Postcodes)
| Method | Endpoint | Mô tả |
| :---: | :--- | :--- |
| `GET` | `/api/stores` | Danh sách Cửa hàng / Showroom trưng bày |
| `GET` | `/api/warehouses` | Danh sách Kho hàng (Kho tổng 164, 171, 53 & kho tại showroom) |
| `GET` | `/api/postcodes/lookup` | Tra cứu Cửa hàng & Kho chịu trách nhiệm theo Postcode |
| `POST` | `/api/postcodes/assign-store` | Cấu hình gán Postcode cho Cửa hàng phục vụ |

### 2.3. Phân hệ 3: Danh Mục, Sản Phẩm & Giá 5 Giai Đoạn (Catalog & 5-Stage Pricing - Module 1 & 6)
| Method | Endpoint | Mô tả |
| :---: | :--- | :--- |
| `GET` | `/api/categories` | Lấy danh mục 4 nhóm lớn (Living, Dining, Bedroom, Outdoor) |
| `POST` | `/api/categories` | Tạo mới danh mục sản phẩm |
| `GET` | `/api/products` | Danh sách & bộ lọc sản phẩm (từ khóa, danh mục, stage giá, còn hàng) |
| `GET` | `/api/products/:id` | Chi tiết sản phẩm cha kèm toàn bộ danh sách biến thể con (variants) |
| `POST` | `/api/products` | Tạo mới sản phẩm cha (Parent Product) |
| `PUT` | `/api/products/:id` | Cập nhật thông tin sản phẩm, hình ảnh gallery, video |
| `DELETE` | `/api/products/:id` | Xóa hoặc ẩn sản phẩm |
| `POST` | `/api/products/:id/variants` | Thêm biến thể con (Size, Color, Material, CBM, Box count) |
| `PUT` | `/api/products/variants/:variantId/lots/:lotId/stage-prices` | Cập nhật bảng giá 5 giai đoạn & giá VIP của một lô hàng (`fifo_lot_stage_prices`) |
| `POST` | `/api/products/price-stage-check` | Kích hoạt worker kiểm tra, nhảy stage giá và đồng bộ giá ngày theo Cửa hàng |
| `GET` | `/api/products/variants/:variantId/lots/:lotId/price-history` | Xem lịch sử nhảy giá của một lô hàng |
| `GET` | `/api/stores/:storeId/variant-prices` | Tra cứu giá cố định trong ngày của biến thể tại một Cửa hàng |
| `PUT` | `/api/stores/:storeId/variants/:variantId/daily-price` | Quản lý / Admin force update giá bán trong ngày của biến thể tại Cửa hàng |

### 2.4. Phân hệ 4: Nhà Cung Cấp, Đặt Hàng Cont & Landed Cost (Suppliers & Stock Orders - Module 2)
| Method | Endpoint | Mô tả |
| :---: | :--- | :--- |
| `GET` | `/api/suppliers` | Danh sách Nhà cung cấp kèm Lead-time |
| `POST` | `/api/suppliers` | Thêm mới Nhà cung cấp |
| `GET` | `/api/stock-orders` | Danh sách Container / Đơn đặt hàng NCC đang về hoặc đã nhập |
| `GET` | `/api/stock-orders/:id` | Chi tiết Container: ngày ETA, thể tích CBM, cước cont, danh sách SKU |
| `POST` | `/api/stock-orders` | Tạo mới đơn đặt hàng Container |
| `POST` | `/api/stock-orders/landed-cost` | Công cụ tính Landed Cost hai chiều & tự động sinh giá 5 giai đoạn |
| `PUT` | `/api/stock-orders/:id/status` | Cập nhật trạng thái Cont (SAILING, CUSTOMS_CLEARING, ARRIVED) |
| `POST` | `/api/stock-orders/:id/receive` | Nhập kho cont hàng thực tế $\rightarrow$ tự động sinh Lô hàng FIFO (`fifo_lots`) |

### 2.5. Phân hệ 5: Quản Lý Tồn Kho & Lô Hàng FIFO (Warehouse Inventory & FIFO Lots - Module 2 & 5)
| Method | Endpoint | Mô tả |
| :---: | :--- | :--- |
| `GET` | `/api/inventory/stock` | Tra cứu tồn kho (Tồn vật lý on-hand, Giữ chỗ reserved, Khả dụng available) |
| `GET` | `/api/inventory/transactions` | Sổ cái lịch sử xuất/nhập/chuyển/điều chỉnh tồn kho |
| `GET` | `/api/inventory/fifo-lots` | Danh sách Lô hàng FIFO (`ACTIVE`, `QUEUED`, `EXHAUSTED`) |
| `POST` | `/api/inventory/transfers` | Tạo phiếu yêu cầu điều chuyển hàng giữa các kho |
| `PUT` | `/api/inventory/transfers/:id/status` | Duyệt / Xác nhận nhận hàng chuyển kho |

### 2.6. Phân hệ 6: Quản Lý Khách Hàng & VIP Loyalty (CRM & VIP Points - Module 1 & 6)
| Method | Endpoint | Mô tả |
| :---: | :--- | :--- |
| `GET` | `/api/customers` | Danh sách khách hàng, tìm theo tên, số điện thoại, email |
| `GET` | `/api/customers/:id` | Hồ sơ khách hàng chi tiết, lịch sử mua hàng, điểm VIP |
| `POST` | `/api/customers` | Thêm mới hồ sơ khách hàng |
| `PUT` | `/api/customers/:id` | Cập nhật thông tin khách hàng, địa chỉ giao hàng |
| `GET` | `/api/customers/:id/loyalty-history` | Lịch sử cộng / trừ điểm thưởng VIP Loyalty ($1\text{ AUD} = 1\text{ pt}$) |

### 2.7. Phân hệ 7: Bán Hàng Tại Quầy POS & Đơn Hàng (POS & Orders - Module 1, 3, 5)
| Method | Endpoint | Mô tả |
| :---: | :--- | :--- |
| `GET` | `/api/orders` | Danh sách đơn hàng (lọc theo kênh POS/Web, trạng thái, chi nhánh, ngày) |
| `GET` | `/api/orders/:id` | Chi tiết hóa đơn bán hàng kèm danh mục mặt hàng & thông tin delivery |
| `GET` | `/api/orders/:id/audit-logs` | Lịch sử kiểm toán chỉnh sửa đơn hàng (người sửa, nội dung, thời gian) |
| `POST` | `/api/orders` | Tạo đơn hàng POS (Order Now, Pre-Order giữ cont, Hẹn giao tương lai) |
| `PUT` | `/api/orders/:id/status` | Cập nhật trạng thái đơn hàng (PROCESSING, SHIPPING, COMPLETED...) |
| `POST` | `/api/orders/:id/payments` | Thu tiền đợt mới / Đặt cọc / Thanh toán dứt điểm (Tiền mặt, EFTPOS, Thẻ) |
| `POST` | `/api/orders/:id/cancel` | Hủy đơn hàng $\rightarrow$ Thu hồi giữ chỗ & chỉnh Lợi nhuận Thực tế về 0 |
| `POST` | `/api/orders/:id/exchange` | Đổi trả sản phẩm $\rightarrow$ Tự động điều chỉnh chênh lệch Lợi nhuận FIFO |

### 2.8. Phân hệ 8: Lịch Giao Hàng & Điều Phối Tuyến Đường (Delivery & Dispatch - Module 3)
| Method | Endpoint | Mô tả |
| :---: | :--- | :--- |
| `GET` | `/api/deliveries/calendar` | Lấy lịch delivery đa chế độ (Hour, Day, Week, Month) |
| `POST` | `/api/deliveries/bookings` | Đặt lịch delivery, bóc tách phụ phí đặc biệt (Lắp ráp, Vác lầu cao) |
| `GET` | `/api/deliveries/routes` | Danh sách các chuyến xe tải giao hàng trong ngày |
| `POST` | `/api/deliveries/optimize-route` | Tối ưu hóa thứ tự các điểm dừng ngắn nhất (Google Maps Waypoints API) |
| `PUT` | `/api/deliveries/stops/:id/status` | Cập nhật trạng thái điểm dừng (`DONE` $\rightarrow$ Ghi đè Lợi nhuận FIFO, `COMEBACK`) |
| `GET` | `/api/deliveries/carriers` | Danh sách đối tác vận tải 3PL và tài xế xe tải |

### 2.9. Phân hệ 9: Marketing, Bán Combo & Hoa Hồng Seller (Marketing, Combos & Commission - Module 4 & 5)
| Method | Endpoint | Mô tả |
| :---: | :--- | :--- |
| `GET` | `/api/marketing/promotions` | Danh sách các chương trình khuyến mại, giảm giá theo % hoặc tiền mặt |
| `POST` | `/api/marketing/promotions` | Tạo mới đợt Sale, gán nhãn Badge (Hot Deal, Clearance, Noel) |
| `GET` | `/api/marketing/combos` | Danh sách các gói Combo nội thất (Giường + Đệm + Sofa...) |
| `POST` | `/api/marketing/combos` | Tạo mới gói Combo với giá ưu đãi trọn gói |
| `POST` | `/api/marketing/ad-spend` | Nhập chi phí quảng cáo Facebook / Google theo tuần để đẩy sang P&L |
| `POST` | `/api/commissions/shifts` | Chấm công số giờ làm việc thực tế của nhân viên bán hàng trong ngày |
| `POST` | `/api/commissions/calculate` | Chốt hoa hồng Seller theo KPI Doanh số / Giờ & quỹ Superannuation 9.5% |
| `GET` | `/api/reports/financial-pnl` | Báo cáo tài chính P&L: Doanh thu, Giá vốn FIFO, Lợi nhuận thực tế, Chi phí Ads |

### 2.10. Phân hệ 10: Website Bán Hàng Công Khai (Public Storefront - Module 6)
| Method | Endpoint | Mô tả |
| :---: | :--- | :--- |
| `POST` | `/api/storefront/resolve-postcode` | Mở khóa xem giá theo Postcode & xác định chi nhánh gần nhất |
| `GET` | `/api/storefront/menu` | Lấy danh mục Mega Menu 4 nhóm lớn và các sản phẩm nổi bật |
| `GET` | `/api/storefront/products` | Danh sách sản phẩm trên Website (giá hiển thị theo Postcode đã lưu) |
| `GET` | `/api/storefront/products/:slug` | Trang chi tiết sản phẩm, bộ chọn biến thể, video, sản phẩm mua kèm |
| `POST` | `/api/storefront/shipping-estimate` | Tính cước vận chuyển tự động theo km ($\le 200\text{km}$) hoặc API hãng xe |
| `POST` | `/api/storefront/checkout` | Khách hàng đặt mua và thanh toán online (Visa, Mastercard, Paypal) |

### 2.11. Phân hệ 11: Live Chat Thời Gian Thực (WebSocket Live Chat - Module 6)
| Method / Event | Endpoint / Tên Event | Mô tả |
| :---: | :--- | :--- |
| `POST` | `/api/chat/session` | Public endpoint; guest chưa đăng nhập tạo session với `participantType=GUEST` và nhận `sessionToken` |
| `GET` | `/api/chat/sessions` | Bàn CSKH: Danh sách các cuộc hội thoại đang chờ hoặc đang phục vụ |
| `GET` | `/api/chat/sessions/:id/messages` | Lịch sử tin nhắn; guest gửi token qua header `X-Chat-Session-Token` |
| `POST` | `/api/postcodes/select-store` | Chọn store khi một postcode được mapping tới nhiều store |
| `WS Sub` | `join_session` | Khách vào phòng chat theo `sessionToken` |
| `WS Sub` | `agent_join` | Nhân viên CSKH nhận phiên chat của khách |
| `WS Pub` | `send_message` | Guest hoặc CS_AGENT gửi tin nhắn văn bản; bắt buộc `sessionToken` và `clientMessageId` |
| `WS Sub` | `receive_message` | Nhận tin nhắn mới thời gian thực |
| `WS Pub` | `typing` | Phát tín hiệu đang soạn tin nhắn |

Tin nhắn realtime bắt buộc có `clientMessageId` duy nhất trong phạm vi session để hỗ trợ retry an toàn. MVP chưa hỗ trợ attachment.

Guest không cần JWT. Guest phải giữ bí mật `sessionToken`; token này được dùng khi gọi
`join_session`, đọc lịch sử và gửi message. Chỉ `CS_AGENT` có JWT và đúng `store_id`
mới được nhận session và xem danh sách session của store.

---

## 3. CHI TIẾT REQUEST & RESPONSE MẪU CÁC FLOW TRỌNG TÂM

### 3.1. Xác thực (`POST /api/auth/login`)
- **Request:**
```json
{
  "email": "manager.syd@demo.local",
  "password": "123"
}
```
- **Response (200):**
```json
{
  "success": true,
  "data": {
    "token": "demo-jwt-token-syd",
    "user": {
      "id": 1,
      "fullName": "David Nguyen",
      "roles": ["STORE_MANAGER"],
      "storeId": 1,
      "storeName": "Sydney Flagship Showroom"
    }
  }
}
```

---

### 3.2. Tra cứu sản phẩm & Giá 5 Stage (`GET /api/products`)
- **Query:** `?keyword=prado&categoryId=3&storeId=1`
- **Response (200):**
```json
{
  "success": true,
  "data": [
    {
      "id": 101,
      "sku": "BED-PRADO-01",
      "name": "Prado Luxury Bed Frame",
      "category": "Bedroom",
      "currentStage": 2,
      "variants": [
        {
          "variantId": 501,
          "sku": "BED-PRADO-KING-BLK",
          "size": "King",
          "color": "Black",
          "material": "Leather look",
          "cbm": 0.4500,
          "boxCount": 3,
          "currentPrice": 1099.00,
          "vipPrice": 999.00,
          "stagePrices": {
            "stage1": 1299.00,
            "stage2": 1099.00,
            "stage3": 899.00,
            "stage4": 749.00,
            "stage5": 599.00
          },
          "stock": {
            "onHand": 12,
            "reserved": 4,
            "available": 8
          },
          "incomingContainer": {
            "containerCode": "CONT-MSKU-SEP01",
            "eta": "2026-09-28",
            "incomingQuantity": 20
          }
        }
      ]
    }
  ]
}
```

---

### 3.3. Worker quét chuyển Stage giá tự động (`POST /api/products/price-stage-check`)
- **Request:**
```json
{
  "variantId": 501
}
```
- **Response (200):**
```json
{
  "success": true,
  "data": {
    "variantId": 501,
    "sku": "BED-PRADO-KING-BLK",
    "stageChanged": true,
    "oldStage": 1,
    "newStage": 2,
    "oldPriceAud": 1299.00,
    "newPriceAud": 1099.00,
    "reason": "Tồn kho còn 35% (< 40% ngưỡng) và đã giữ giá 16 ngày (> 14 ngày tối thiểu)"
  }
}
```

---

### 3.4. Công cụ tính Landed Cost Container (`POST /api/stock-orders/landed-cost`)
- **Request:**
```json
{
  "containerFreightAud": 6000.00,
  "customsTaxAud": 800.00,
  "exchangeRateUsdAud": 1.50,
  "items": [
    {
      "variantId": 501,
      "quantity": 100,
      "unitCostUsd": 200.00,
      "cbmPerUnit": 0.40,
      "targetMarginPct": 60.0
    }
  ]
}
```
- **Response (200):**
```json
{
  "success": true,
  "data": {
    "totalCbm": 40.0,
    "freightPerCbmAud": 150.00,
    "calculation": {
      "variantId": 501,
      "unitCostAud": 300.00,
      "freightAud": 60.00,
      "taxAud": 8.00,
      "landedCostAud": 368.00,
      "suggestedStage1Price": 920.00,
      "generated5Stages": {
        "stage1": 920.00,
        "stage2": 828.00,
        "stage3": 690.00,
        "stage4": 598.00,
        "stage5": 460.00
      }
    }
  }
}
```

---

### 3.5. Nhập kho cont hàng sinh Lô FIFO (`POST /api/stock-orders/:id/receive`)
- **Request:**
```json
{
  "warehouseId": 1,
  "actualArrivalDate": "2026-09-05",
  "receivedItems": [
    { "variantId": 501, "quantityReceived": 100 }
  ]
}
```
- **Response (201):**
```json
{
  "success": true,
  "data": {
    "stockOrderId": 12,
    "createdLots": [
      {
        "lotNumber": "LOT-202609-WH164-VAR501",
        "quantity": 100,
        "unitLandedCostAud": 368.00,
        "status": "ACTIVE",
        "message": "Lô cũ đã hết, kích hoạt lô mới và reset giá về Stage 1"
      }
    ]
  }
}
```

---

### 3.6. Tạo đơn hàng POS đa tab (`POST /api/orders`)
- **Request:**
```json
{
  "storeId": 1,
  "customerId": 10,
  "orderType": "PRE_ORDER",
  "items": [
    {
      "variantId": 501,
      "quantity": 2,
      "unitPriceAud": 1099.00,
      "stageApplied": 2
    }
  ],
  "deliveryBooking": {
    "scheduledDate": "2026-09-28",
    "street": "120 George St",
    "suburb": "Sydney",
    "postcode": "2000",
    "isAssembling": true,
    "isUpstairs": true,
    "stairsFloorCount": 2,
    "surchargeAud": 80.00
  },
  "payment": {
    "method": "VISA",
    "amountPaid": 500.00,
    "transactionRef": "POS-CARD-98412"
  }
}
```
- **Response (201):**
```json
{
  "success": true,
  "data": {
    "orderId": 701,
    "orderCode": "ORD-2026-SYD-0701",
    "orderType": "PRE_ORDER",
    "status": "PROCESSING",
    "subtotalAud": 2198.00,
    "shippingFeeAud": 45.00,
    "specialSurchargeAud": 80.00,
    "totalAud": 2323.00,
    "paidAmountAud": 500.00,
    "remainingBalanceAud": 1823.00,
    "reservation": {
      "containerCode": "CONT-MSKU-SEP01",
      "reservedQty": 2,
      "etaDate": "2026-09-28"
    }
  }
}
```

#### Xem lịch sử kiểm toán đơn hàng (`GET /api/orders/:id/audit-logs`):
- **Response (200):**
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "orderId": 701,
      "action": "CREATED",
      "user": { "id": 2, "fullName": "Sarah Jenkins" },
      "changeSummary": "Tạo đơn hàng POS mới ORD-2026-SYD-0701",
      "createdAt": "2026-09-05T14:20:00Z"
    },
    {
      "id": 2,
      "orderId": 701,
      "action": "DELIVERY_UPDATED",
      "user": { "id": 1, "fullName": "David Nguyen" },
      "changeSummary": "Đổi ngày hẹn giao từ 2026-09-28 sang 2026-10-02 theo yêu cầu khách",
      "oldData": { "scheduledDate": "2026-09-28" },
      "newData": { "scheduledDate": "2026-10-02" },
      "createdAt": "2026-09-05T15:10:00Z"
    }
  ]
}
```

---

### 3.7. Tối ưu lộ trình Google Maps & Cập nhật Giao hàng (`POST /api/deliveries/optimize-route`)
- **Request:**
```json
{
  "deliveryDate": "2026-09-28",
  "carrierId": 1,
  "bookingIds": [101, 102, 103]
}
```
- **Response (200):**
```json
{
  "success": true,
  "data": {
    "routeCode": "ROUTE-20260928-01",
    "totalDistanceKm": 48.5,
    "estimatedMins": 120,
    "orderedStops": [
      { "sequence": 1, "bookingId": 102, "suburb": "Surry Hills", "eta": "09:30" },
      { "sequence": 2, "bookingId": 101, "suburb": "Chatswood", "eta": "10:45" },
      { "sequence": 3, "bookingId": 103, "suburb": "Parramatta", "eta": "12:15" }
    ]
  }
}
```

#### Cập nhật Giao thành công $\rightarrow$ Ghi đè Lợi nhuận FIFO (`PUT /api/deliveries/stops/:id/status`):
- **Request:**
```json
{
  "status": "DONE",
  "proofOfDeliveryUrl": "http://localhost:3000/uploads/pod-sign.png"
}
```
- **Response (200):**
```json
{
  "success": true,
  "data": {
    "stopId": 101,
    "deliveryStatus": "DONE",
    "fifoProfitOverride": {
      "lotNumber": "LOT-202609-WH164-VAR501",
      "quantityDeducted": 2,
      "unitLandedCostAud": 368.00,
      "sellingPriceAud": 1099.00,
      "realizedProfitAud": 1462.00,
      "message": "Đã ghi đè Lợi nhuận Dự kiến thành Lợi nhuận Thực tế theo FIFO"
    }
  }
}
```

---

### 3.8. Chốt hoa hồng Seller theo KPI & Hưu bổng Úc (`POST /api/commissions/calculate`)
- **Request:**
```json
{
  "salespersonId": 2,
  "periodStart": "2026-08-01",
  "periodEnd": "2026-08-31"
}
```
- **Response (200):**
```json
{
  "success": true,
  "data": {
    "salesperson": "Sarah Jenkins",
    "totalSalesAud": 52000.00,
    "hoursWorked": 125.0,
    "salesPerHourAud": 416.00,
    "kpiThresholdAud": 350.00,
    "kpiAchieved": true,
    "targetSalesAud": 43750.00,
    "excessSalesAud": 8250.00,
    "commissionRatePct": 2.50,
    "grossCommissionAud": 206.25,
    "superannuationPct": 9.50,
    "superannuationDeductionAud": 19.59,
    "netCommissionPayableAud": 186.66,
    "formulaExplanation": "Hoa hồng = (52,000 - (125 * 350)) * 2.5% = 8,250 * 2.5% = 206.25 AUD; Trích Superannuation 9.5% = 19.59 AUD; Thực nhận = 186.66 AUD",
    "status": "APPROVED"
  }
}
```

---

### 3.9. Mở khóa xem giá Storefront theo Postcode (`POST /api/storefront/resolve-postcode`)
- **Request:**
```json
{
  "postcode": "6000"
}
```
- **Response (200):**
```json
{
  "success": true,
  "data": {
    "postcode": "6000",
    "suburb": "Perth",
    "assignedStore": {
      "id": 3,
      "name": "Perth Showroom",
      "code": "STR-PER-01"
    },
    "priceUnlocked": true,
    "shippingRate": {
      "distanceKm": 8.5,
      "feeAud": 45.00
    }
  }
}
```

---

### 3.10. SignalR Live Chat (`/hubs/chat`)
- **Khách gửi tin (`send_message`):**
```json
{
  "sessionToken": "GUEST-PER-9821",
  "clientMessageId": "msg-001",
  "messageText": "Cho mình hỏi giường Prado có sẵn hàng tại showroom Perth không?"
}
```
- **Nhân viên trả lời (`receive_message`):**
```json
{
  "sessionId": 12,
  "senderType": "AGENT",
  "clientMessageId": "msg-agent-001",
  "messageText": "Dạ giường Prado hiện đang có sẵn màu đen cỡ King tại kho Perth ạ!",
  "sentAt": "2026-09-05T15:45:00Z"
}
```

---

### 3.11. Phân hệ Health Checks & System Monitoring (#8)

- **Liveness Probe (`GET /api/healthz`):**
  - Kiểm tra ứng dụng process đang hoạt động bình thường.
  - **Response (200 OK):**
    ```json
    {
      "status": "UP",
      "timestamp": "2026-09-14T14:20:00Z"
    }
    ```

- **Readiness Probe (`GET /api/readyz`):**
  - Kiểm tra kết nối tới các dịch vụ hạ tầng phụ thuộc (PostgreSQL, Redis, External Payment/Maps Gateways).
  - **Response (200 OK - All Healthy):**
    ```json
    {
      "status": "READY",
      "checks": {
        "database": { "status": "UP", "latencyMs": 4.2 },
        "redis": { "status": "UP", "latencyMs": 1.1 },
        "paymentGateway": { "status": "UP", "latencyMs": 45.0 }
      },
      "timestamp": "2026-09-14T14:20:00Z"
    }
    ```
  - **Response (503 Service Unavailable - Component Degraded):**
    ```json
    {
      "status": "UNREADY",
      "checks": {
        "database": { "status": "UP", "latencyMs": 3.8 },
        "redis": { "status": "DOWN", "error": "Connection refused" }
      },
      "timestamp": "2026-09-14T14:20:00Z"
    }
    ```

---
*Tài liệu này bao quát 100% tất cả 11 phân hệ và 6 modules nghiệp vụ, được viết theo đúng chuẩn URL demo đơn giản `localhost:3000/api` để phục vụ phát triển phần mềm nhanh và hiệu quả.*
