# 📘 ĐẶC TẢ YÊU CẦU NGHIỆP VỤ - MODULE 6: WEBSITE BÁN HÀNG & REAL-TIME CHAT
**Dự án:** Hệ Thống Quản Lý Bán Hàng & Thương Mại Điện Tử Đa Chi Nhánh (Sales Management System)  
**Tài liệu tham chiếu:** Phụ lục 01 - Tính năng Hệ thống Quản trị (Công ty TNHH Webkynang Việt Nam)  
**Phiên bản:** 1.0  
**Ngày hoàn thành phân tích:** 28/08/2026  

---

## 1. TỔNG QUAN PHÂN HỆ
Module 6 bao gồm toàn bộ trải nghiệm mua sắm trực tuyến của khách hàng tại thị trường Úc và hệ thống tương tác thời gian thực:
1. **Trải nghiệm Định vị Đa Giá theo Postcode (Postcode-Gated Pricing Flow).**
2. **Cấu trúc Menu Catalogue 4 Nhóm Lớn & Trang Sản phẩm Chi tiết.**
3. **Thuật toán Tính Cước Vận Chuyển Tự Động theo Khoảng cách & Tích hợp API Hãng Vận Tải (TNT / Startrack).**
4. **Giỏ hàng, Thanh toán Đa Cổng (Payment Gateways) & Chương trình VIP Loyalty.**
5. **Hệ thống Live Chat Thương Hiệu Riêng (Real-Time WebSocket Chat Plugin).**

---

## 2. QUY TRÌNH ĐỊNH VỊ ĐA GIÁ THEO POSTCODE (POSTCODE PRICING UX FLOW)

### 2.1. Trải nghiệm Khách vãng lai khi chưa có Postcode
- **Duyệt xem tự do:** Khách hàng mới truy cập Website được phép xem toàn bộ danh mục sản phẩm, hình ảnh, thông số kỹ thuật và video sản phẩm nhưng **mặc định KHÔNG hiển thị giá bán**.
- **Nút  View Price (Xem Giá):** Tại mỗi sản phẩm trên trang danh mục (Shop page) và trang chi tiết (Product page) có nút *View Price*.
- **Cơ chế kích hoạt:**
  1. Khách bấm *View Price* => Hệ thống hiển thị Modal/Popup yêu cầu nhập **Mã bưu điện (Postcode)**.
  2. Khách nhập Postcode hợp lệ (ví dụ: 6000 - Perth WA) => Hệ thống tự động xác định Cửa hàng/Kho gần nhất => Lưu Postcode vào Session/Cookie => **Mở khóa và hiển thị toàn bộ giá bán của Cửa hàng đó trên toàn Website**.
  3. Nếu khách từ chối/bỏ qua popup => Hệ thống tiếp tục ẩn giá và không cho phép thêm sản phẩm vào giỏ hàng.

### 2.2. Ràng buộc tại Bước Thanh toán (Checkout Gate)
- **Bắt buộc nhập Postcode:** Khách hàng bắt buộc phải có Postcode để tiến hành Checkout nhằm phục vụ 2 mục đích:
  1. Xác định chính xác đơn giá sản phẩm theo chi nhánh phục vụ.
  2. Làm căn cứ tính toán tự động khoảng cách giao hàng và cước phí vận chuyển.

---

## 3. CẤU TRÚC MENU CATALOGUE & TRANG SẢN PHẨM

### 3.1. Phân loại Menu 4 Tabs Lớn (Home Mega Menu)
1. **Living Room:** Packages & Sales, Sofabed, Sofas, Recliners, L Shaped, U Shaped, Chairs, Ottomans, TV Units, Coffee Tables.
2. **Dining Room:** Packages & Sales, Dining Tables, Extension Dining Tables, Dining Chairs, Buffet/Sideboard, Display Cabinets, Bar Stools.
3. **Bedroom:** Packages & Sales, Beds, Sofa Beds, Mattresses, Bedsides, Dressing Tables, Tallboy/Chest.
4. **Outdoor & Others:** Packages & Sales, Outdoor, Others.
- **Sub-catalogue dropdown:** Hiển thị danh mục con và các sản phẩm mới nhất vừa đăng.
- **Thanh menu bám dính (Sticky Menu):** Hiển thị xuyên suốt khi cuộn trang (Bản Desktop: hiện menu bỏ logo; Bản Mobile: hiện icon menu rút gọn).

### 3.2. Tính năng Trang Chi tiết Sản phẩm (Single Product Page)
- **Đa phương tiện:** Hiển thị thư viện ảnh độ phân giải cao và Video ngắn xem nhanh (Quick Video).
- **Bộ chọn biến thể (Variant Matrix):** Cho phép khách hàng chọn Size (Single -> King), Màu sắc (Black, White, Grey), Chất liệu (Leather look, Fabric) của cùng 1 mẫu sản phẩm mẹ.
- **Sản phẩm liên quan (Associated Products):** Tự động hiển thị các sản phẩm bổ trợ được cấu hình từ Admin (Ví dụ: xem Giường Prado => gợi ý thêm Tủ đầu giường Prado Bedside và Tủ ngăn kéo Tallboy).
- **Đánh giá khách hàng (Reviews):** Cho phép khách upload hình ảnh/video đánh giá sản phẩm (được kiểm duyệt bởi Admin trước khi xuất bản).

---

## 4. THUẬT TOÁN TÍNH CƯỚC VẬN CHUYỂN TỰ ĐỘNG (SHIPPING RATE CALCULATOR)

### 4.1. Bảng Cước Phí theo Bậc Thang Khoảng Cách (Bán kính <= 200km)
Hệ thống tính khoảng cách từ Cửa hàng/Kho gần nhất đến địa chỉ giao hàng của khách:

| Khoảng cách (Km) | Cước phí vận chuyển (AUD) |
| :--- | :---: |
| Dưới hoặc bằng 3 km | **** |
| Trên 3 km đến dưới 10 km | **** |
| 10 km - 20 km | **** |
| 21 km - 30 km | **** |
| 31 km - 40 km | **** |
| 41 km - 50 km | **** |
| 51 km - 60 km | **** |
| 61 km - 70 km | **** |
| 71 km - 80 km | **** |
| 81 km - 149 km | **** |
| 150 km - 200 km | **** |

### 4.2. Tích hợp API Đơn vị Vận chuyển Bên Ngoài (Khoảng cách > 200km)
- Khi khoảng cách giao hàng **lớn hơn 200 km**, hệ thống tự động gọi API của **TNT** hoặc **Startrack**.
- Căn cứ vào: Tổng số kiện hàng (No of boxes), Thể tích kích thước từng box (L x W x H) và Tổng trọng lượng (kg) lấy từ Module 2 để trả về mức phí vận chuyển chính xác thời gian thực.

---

## 5. GIỎ HÀNG, THANH TOÁN & QUYỀN LỢI VIP

### 5.1. Quy trình Đặt hàng & Thanh toán
- **Guest Checkout:** Khách hàng không bắt buộc phải tạo tài khoản để mua hàng.
- **Gợi ý Đăng ký VIP:** Tại trang Checkout, hệ thống hiển thị thông báo mời đăng ký thành viên để nhận tích điểm ( = 1 point) và hưởng mức giá ưu đãi VIP cho các lần mua tiếp theo.
- **Cổng thanh toán hỗ trợ:** Tích hợp thanh toán an toàn qua Thẻ tín dụng/ghi nợ quốc tế (Visa, Mastercard, American Express) và ví điện tử Paypal.

---

## 6. HỆ THỐNG LIVE CHAT THỜI GIAN THỰC (REAL-TIME CHAT)

### 6.0. Quyền truy cập khách vãng lai
- Khách chưa đăng nhập vẫn được mở widget và chat với CSKH.
- Backend tạo session `GUEST`, không yêu cầu JWT và lưu `customer_id = NULL`.
- Backend trả `sessionToken`; trình duyệt dùng token này để join phòng, đọc lịch sử và gửi tin nhắn.
- `sessionToken` phải được giữ bí mật như credential của phiên; không dùng email guest để xác thực.
- Chỉ CS_AGENT có JWT hợp lệ và cùng `store_id` mới được nhận session.

### 6.1. Kiến trúc Độc lập
- Xây dựng plugin Live Chat thương hiệu riêng (không phụ thuộc vào nền tảng Facebook Messenger/Meta).
- Giao tiếp thời gian thực 2 chiều thông qua **ASP.NET Core SignalR** tại `/hubs/chat`, với Redis dùng cho rate limit và fan-out khi scale.

### 6.2. Luồng Vận hành
1. **Phía Khách hàng (Website Widget):** Khách hàng nhấn vào biểu tượng Chat góc dưới màn hình, gửi tin nhắn trao đổi về sản phẩm, tình trạng còn hàng hoặc yêu cầu tư vấn.
2. **Phía Nhân viên (Admin CSKH Desk):** Tin nhắn mới được đẩy tức thì về màn hình Chăm sóc Khách hàng trên Web-App Admin; nhân viên bán hàng/CSKH có thể trả lời trực tiếp cho khách hàng trong tích tắc.

---

## 7. TIÊU CHÍ NGHIỆM THU PHI CHỨC NĂNG (NFR)
1. **Tốc độ Tải trang (Page Speed):** Tối ưu hóa thời gian tải trang đầu tiên (FCP) $\le 1.5\text{s}$ cho người dùng tại Úc (Western Australia).
2. **Độ trễ Tin nhắn Chat (Chat Latency):** Tin nhắn gửi và nhận giữa Website và Admin có độ trễ $\le 200\text{ms}$.
3. **Tính bảo mật Thanh toán:** 100% giao dịch thanh toán trực tuyến tuân thủ chuẩn an toàn dữ liệu PCI-DSS (không lưu thông tin thẻ thô trên hệ thống).
