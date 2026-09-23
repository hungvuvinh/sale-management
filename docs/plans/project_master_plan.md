# 📅 KẾ HOẠCH TỔNG THỂ DỰ ÁN (PROJECT MASTER PLAN)
**Hệ Thống Quản Lý Bán Hàng & Thương Mại Điện Tử (Sales Management & Multi-Store E-Commerce)**

- **Tổng thời gian dự án:** 3 tuần (21 ngày)
- **Thời điểm hiện tại:** Ngày thứ 3
- **Phân bổ thời gian:**
  - **Giai đoạn 1 & 2 (2 tuần - Ngày 3 đến Ngày 14):** Phân tích nghiệp vụ chuyên sâu, Thiết kế kiến trúc kỹ thuật, ERD Database, API Specs & UI/UX Wireframes.
  - **Giai đoạn 3 (1 tuần - Ngày 15 đến Ngày 21):** Triển khai xây dựng mã nguồn (Implementation Sprint), Tích hợp & Kiểm thử toàn trình (E2E Verification).
- **Phạm vi loại trừ theo yêu cầu:** *Mobile app cho Shipper* và *Web-app cho HRM*.

---

## 🗺️ LỘ TRÌNH TỔNG QUAN (GANTT TIMELINE)

`mermaid
gantt
    title KẾ HOẠCH DỰ ÁN 3 TUẦN (BẮT ĐẦU TỪ NGÀY 3)
    dateFormat  YYYY-MM-DD
    section TUẦN 1: PHÂN TÍCH NGHIỆP VỤ (HÔM NAY -> NGÀY 7)
    M1 - Sản Phẩm, Giá 5 Stage & POS        :active, 2026-08-26, 1d
    M2 - Kho, Stock Order & Landed Cost     :2026-08-27, 1d
    M3 - Giao hàng & Delivery Calendar      :2026-08-28, 1d
    M4 & M5 - Marketing, FIFO & Hoa hồng    :2026-08-29, 1d
    M6 - Website Storefront & Realtime Chat :2026-08-30, 1d
    section TUẦN 2: THIẾT KẾ KỸ THUẬT & UI/UX (NGÀY 8 -> NGÀY 14)
    Thiết kế Chi tiết Cơ sở Dữ liệu (ERD)   :2026-08-31, 2d
    Thiết kế API Specs (Swagger/OpenAPI)    :2026-09-02, 2d
    Thiết kế Sequence Diagram & Luồng Dữ Liệu:2026-09-04, 1d
    Thiết kế UI/UX Wireframe & Flow Screen  :2026-09-05, 1d
    Review & Hoàn thiện Tài liệu Thiết kế   :2026-09-06, 1d
    section TUẦN 3: TRIỂN KHAI PHẦN MỀM (NGÀY 15 -> NGÀY 21)
    Khởi tạo Core Backend & DB Migration   :2026-09-07, 1d
    Xây dựng Core Sản phẩm, Giá 5 Stage, Kho:2026-09-08, 1d
    Xây dựng POS Bán hàng & Delivery Booking:2026-09-09, 1d
    Xây dựng Engine FIFO & Tính Hoa hồng    :2026-09-10, 1d
    Xây dựng Storefront API & Checkout      :2026-09-11, 1d
    Xây dựng Live Chat & Worker Background  :2026-09-12, 1d
    Kiểm thử E2E, Verify Nghiệp vụ & Demo   :2026-09-13, 1d
`

---

## 📋 BẢNG PHÂN RÃ CÔNG VIỆC CHI TIẾT (WORK BREAKDOWN STRUCTURE)

### GIAI ĐOẠN 1: PHÂN TÍCH NGHIỆP VỤ CHUYÊN SÂU (TUẦN 1: NGÀY 3 – NGÀY 7)

| Ngày | Module / Hạng mục | Nhiệm vụ chi tiết (Action Items) | Kết quả đầu ra (Deliverables) |
| :--- | :--- | :--- | :--- |
| **Ngày 3 (Hôm nay)** | **Module 1: Sản phẩm, Giá 5 Stage & POS Bán hàng** | - Đặc tả logic 5 stage giá tự động theo % tồn kho & thời gian tồn kho.<br/>- Đặc tả luồng POS đa tab, kiểm tra tồn kho âm/dương theo ngày hẹn giao.<br/>- Quy tắc đơn Order Now vs Pre-Order, chính sách VIP (/1pt). | Tài liệu đặc tả phân tích Module 1 |
| **Ngày 4** | **Module 2: Quản lý Kho & Mua hàng (Stock Order)** | - Đặc tả quy cách hàng hóa (CBM, dimensions, kg/box).<br/>- Công thức tính Landed Cost (giá mua USD/AUD + cước cont CBM) & công cụ tính giá bán theo % lãi mục tiêu.<br/>- Quy tắc xuất/nhập/chuyển kho định kỳ và cảnh báo tồn theo lead-time. | Tài liệu đặc tả phân tích Module 2 |
| **Ngày 5** | **Module 3: Điều phối Giao hàng (Delivery)** | - Đặc tả cơ chế trừ tồn kho khả dụng vào thời điểm giao hàng tương lai.<br/>- Đặc tả Delivery Calendar (Hour/Day/Week/Month) & thông tin đơn đặc biệt (vác lầu, lắp ráp).<br/>- Thuật toán định vị đa điểm trên Google Maps, tích hợp TNT/Startrack. | Tài liệu đặc tả phân tích Module 3 |
| **Ngày 6** | **Module 4 & 5: Marketing, Báo cáo FIFO & Hoa hồng** | - Cơ chế tự động điều chỉnh Lợi nhuận Thực tế theo FIFO khi giao hàng thành công (xử lý đổi/trả/hủy đơn).<br/>- Công thức tính hoa hồng Seller theo định mức KPI sales/giờ.<br/>- Phân định chi phí chịu thuế (Superannuation 9.5%) & chi phí tiền mặt; Quản lý chiến dịch Sale & chi phí quảng cáo. | Tài liệu đặc tả phân tích Module 4 & 5 |
| **Ngày 7** | **Module 6: Website Storefront & Real-time Chat** | - Cơ chế tự gán cửa hàng theo Postcode và hiển thị bảng giá riêng.<br/>- Luồng giỏ hàng, bảng phí ship theo km / API hãng xe, thanh toán Visa/Paypal.<br/>- Thiết kế luồng chat WebSocket độc lập cho khách và bàn CSKH Admin.<br/>- **Tổng kết & Đóng gói tài liệu Phân tích Nghiệp vụ Tuần 1.** | Tài liệu đặc tả phân tích Module 6 & Bản tổng hợp nghiệp vụ Tuần 1 |

---

### GIAI ĐOẠN 2: THIẾT KẾ KỸ THUẬT & KIẾN TRÚC CHI TIẾT (TUẦN 2: NGÀY 8 – NGÀY 14)

| Ngày | Hạng mục Thiết kế | Nhiệm vụ chi tiết (Action Items) | Kết quả đầu ra (Deliverables) |
| :--- | :--- | :--- | :--- |
| **Ngày 8 – 9** | **Thiết kế Cơ sở Dữ liệu (Database Design & ERD)** | - Thiết kế toàn bộ Schema các bảng (products, ariants, pricing_stages, warehouses, orders, order_items, deliveries, stock_orders, ifo_lots, commissions, chats).<br/>- Thiết lập Indexing tối ưu tìm kiếm (.2s - 2s$), Foreign Keys, Connection Pools & Read-Write splitting. | Bản vẽ ERD & Data Dictionary chi tiết |
| **Ngày 10 – 11** | **Thiết kế API Specs (Swagger / OpenAPI)** | - Đặc tả chuẩn hóa toàn bộ RESTful Endpoints (Auth, POS, Inventory, Storefront, Delivery, Redis Cache, Chat WebSocket).<br/>- Quy định Schema Request/Response, Validation Rules và Error Handling. | Tài liệu Swagger / OpenAPI 3.0 Specs |
| **Ngày 12** | **Thiết kế Sequence Diagrams & DFD** | - Xây dựng sơ đồ tuần tự các ca phức tạp: (1) Quét tự động nhảy giá 5-stage; (2) Tạo đơn Pre-order và giữ chỗ cont hàng; (3) Hoàn tất giao hàng và ghi đè lợi nhuận FIFO. | Bộ sơ đồ Sequence Diagrams & DFD |
| **Ngày 13** | **Thiết kế Giao diện UI/UX Wireframes** | - Thiết kế Wireframe màn hình POS Bán hàng tại quầy (tối ưu thao tác nhanh).<br/>- Wireframe Delivery Calendar, Stock Order Calculator, và Website Storefront. | Bộ bản vẽ Wireframes các màn hình chính |
| **Ngày 14** | **Review & Đóng gói Thiết kế Kiến trúc** | - Rà soát toàn bộ tài liệu Software Architecture & System Design Document (SADD).<br/>- Thiết lập môi trường phát triển (Docker Compose, Database Migrations, Repository setup). | Tài liệu SADD hoàn chỉnh & Môi trường sẵn sàng Code |

---

### GIAI ĐOẠN 3: TRIỂN KHAI CORE & VERIFICATION (TUẦN 3: NGÀY 15 – NGÀY 21)

| Ngày | Hạng mục Triển khai (Coding Sprint) | Nhiệm vụ chi tiết (Action Items) | Kết quả kiểm thử (Verification) |
| :--- | :--- | :--- | :--- |
| **Ngày 15** | **Khởi tạo Core Backend & Database Infrastructure** | - Setup project NestJS/NodeJS, Docker Compose (PostgreSQL Master-Replica, Redis, PgBouncer).<br/>- Chạy toàn bộ Database Migrations và Seeding dữ liệu mẫu (.000$ sản phẩm). | DB kết nối ổn định, API Auth & RBAC hoạt động |
| **Ngày 16** | **Xây dựng Module Sản phẩm, Giá 5 Stage & Kho hàng** | - CRUD Sản phẩm & Biến thể, bộ lọc tìm kiếm siêu tốc.<br/>- Thuật toán Cron Job quét và chuyển giá 5 stage tự động.<br/>- Quản lý Stock Order & công cụ tính Landed Cost/Giá bán. | Test case 5-stage pricing & tính giá Landed Cost đạt |
| **Ngày 17** | **Xây dựng Module POS Bán hàng & Delivery Booking** | - API POS tạo đơn Order Now / Pre-order, tích hợp Google Maps.<br/>- Tính năng Booking lịch Delivery và kiểm tra tồn kho theo ngày giao. | Test case tạo đơn POS & đặt lịch delivery thành công |
| **Ngày 18** | **Xây dựng Engine FIFO & Tính Hoa hồng Seller** | - Logic xuất kho FIFO tính lợi nhuận thực tế.<br/>- Logic điều chỉnh lợi nhuận khi hủy/đổi đơn và tính hoa hồng theo KPI doanh số/giờ. | Test case ghi đè lợi nhuận FIFO & tính hoa hồng chính xác |
| **Ngày 19** | **Xây dựng Storefront API & Website Bán hàng cơ bản** | - API Storefront định vị theo Postcode, hiển thị giá theo chi nhánh.<br/>- Luồng Giỏ hàng, Tính cước vận chuyển tự động và Checkout thanh toán. | Test case mua hàng & tính phí ship theo km/hãng xe đạt |
| **Ngày 20** | **Xây dựng Real-time Chat & Background Workers** | - Socket.io Live Chat giữa khách hàng trên Web và Admin Desk.<br/>- Worker xử lý xuất file Excel báo cáo và mock sync Amazon Seller API. | Chat realtime thông suốt, xuất Excel không nghẽn hệ thống |
| **Ngày 21** | **Kiểm thử Tích hợp (E2E Test) & Bàn giao Dự án** | - Chạy kịch bản kiểm thử toàn trình từ Đặt hàng $\rightarrow$ Giao hàng $\rightarrow$ Tính lợi nhuận FIFO $\rightarrow$ Báo cáo.<br/>- Tổng kết nghiệm thu và bàn giao bộ tài liệu thiết kế + mã nguồn. | Hệ thống chạy trơn tru, bàn giao toàn bộ sản phẩm |
