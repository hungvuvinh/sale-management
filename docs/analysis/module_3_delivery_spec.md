# 📘 ĐẶC TẢ YÊU CẦU NGHIỆP VỤ - MODULE 3: QUẢN LÝ GIAO HÀNG & ĐIỀU PHỐI VẬN CHUYỂN (DELIVERY)
**Dự án:** Hệ Thống Quản Lý Bán Hàng & Thương Mại Điện Tử Đa Chi Nhánh (Sales Management System)  
**Tài liệu tham chiếu:** Phụ lục 01 - Tính năng Hệ thống Quản trị (Công ty TNHH Webkynang Việt Nam)  
**Phiên bản:** 1.0  
**Ngày hoàn thành phân tích:** 27/08/2026  

---

## 1. TỔNG QUAN PHÂN HỆ
Module 3 phụ trách toàn bộ hoạt động hậu cần giao nhận nội thất đến tận nhà khách hàng:
1. **Quản lý Lịch Giao Hàng (Delivery Calendar Scheduling).**
2. **Tối ưu hóa Tuyến đường Đa điểm Thông minh trên Google Maps (Multi-Stop Route Optimization).**
3. **Quản lý Đối tác Vận chuyển & Danh sách Shipper/Tài xế.**
4. **Theo dõi Trạng thái Vòng đời Chuyến giao hàng (Delivery Lifecycle Tracking).**
5. **Xuất Báo cáo & Biên bản Giao Hàng Hàng Ngày (Daily Delivery List & Invoicing).**

---

## 2. QUẢN LÝ LỊCH GIAO HÀNG (DELIVERY CALENDAR SCHEDULING)

### 2.1. Giao diện Lịch Đa Chế Độ (Multi-View Calendar)
- Cung cấp giao diện trực quan hỗ trợ 4 chế độ xem: **Theo Giờ (Hour View)**, **Theo Ngày (Day View)**, **Theo Tuần (Week View)** và **Theo Tháng (Month View)**.
- **Tóm tắt nhanh đầu ngày:** Trên từng ô ngày hiển thị tổng số lượng đơn delivery đã book, tổng số kiện hàng (boxes) và biểu tượng cảnh báo nếu có **Yêu cầu đặc biệt (Special Request)**.

### 2.2. Nhận diện Yêu cầu Đặc biệt (Special Request / Surcharge)
- Hệ thống tự động gắn nhãn và làm nổi bật các đơn hàng có tính chất phức tạp:
  - Lắp ráp sản phẩm tại nhà (Product Assembling: Giường, Tủ, Bàn ăn...).
  - Vác hàng lên tầng / cầu thang (Upstairs Delivery).
  - Khung giờ giao bắt buộc (Specific Delivery Window, ví dụ: Between 3-4pm).
- Nhấp vào ngày cụ thể để xem danh sách chi tiết các đơn, thông tin khách hàng, số điện thoại, địa chỉ và vị trí ghim (Pins) trên bản đồ Google Maps.

---

## 3. THUẬT TOÁN TỐI ƯU HÓA TUYẾN ĐƯỜNG ĐA ĐIỂM (ROUTE OPTIMIZATION)

### 3.1. Tự động Sắp xếp Thứ tự Điểm Dừng Ngắn Nhất
- Khi Điều phối viên chọn các đơn hàng cần giao trong ngày cho một chuyến xe tải:
  - Hệ thống tích hợp **Google Maps Waypoints / Distance Matrix API** để phân tích tọa độ địa lý của tất cả các điểm giao.
  - **Tự động tính toán và sắp xếp lại thứ tự các điểm dừng (Stops / Waypoints)** sao cho **tổng thời gian di chuyển và quãng đường chạy xe là ngắn nhất**.
  - Tính toán dự kiến thời gian di chuyển giữa các điểm dừng và tổng thời gian hoàn thành chuyến đi.

### 3.2. Hiển thị Trực quan trên Bản Đồ Google Maps
- Vẽ lộ trình di chuyển liên tục từ điểm xuất phát (Kho tổng/Cửa hàng) qua lần lượt từng điểm giao khách hàng và quay về.
- Hiển thị các điểm ghim (Pins) có đánh số thứ tự giao (1, 2, 3...) kèm thông tin tóm tắt khi bấm vào từng Pin.
- Cho phép Điều phối viên kéo-thả (drag & drop) để can thiệp điều chỉnh thứ tự thủ công nếu phát sinh yêu cầu ưu tiên đột xuất.

---

## 4. QUẢN LÝ ĐỐI TÁC VẬN CHUYỂN & SHIPPER

### 4.1. Danh mục Đơn vị Vận chuyển Bên Ngoài (3PL Partners)
- Quản lý thông tin đối tác: Tên công ty vận chuyển (ví dụ: TNT, Startrack, đối tác xe tải ngoài), Mã đối tác, Người liên hệ, SĐT, Email, Bảng giá cước thỏa thuận.
- Quản lý danh sách tài xế (Shippers) trực thuộc hoặc đối tác.
- Quản lý danh sách các cửa hàng / kho hàng được phân công cho đối tác.

### 4.2. Quản lý Công Nợ Đối Tác Vận Chuyển
- Tự động thống kê số lượng chuyến giao, số lượng đơn hoàn thành theo từng đối tác.
- Theo dõi công nợ chi phí vận chuyển phát sinh theo tuần/tháng để phục vụ đối soát và thanh toán.
- Hỗ trợ nhập (Import) và xuất (Export) dữ liệu đối tác vận chuyển qua file **Excel**.

---

## 5. THEO DÕI VÒNG ĐỜI ĐƠN GIAO HÀNG (LIFECYCLE TRACKING)

### 5.1. Các Trạng Thái Vận Chuyển
Đơn giao hàng được theo dõi qua 4 trạng thái chuẩn:
1. Prepare (Chuẩn bị hàng): Hàng đang được bốc từ các kho (Kho 164, 171, 53) lên xe tải.
2. Shipping (Đang giao hàng): Tài xế bắt đầu di chuyển trên tuyến đường.
3. Done (Giao thành công): Khách hàng đã nhận hàng và ký biên bản bàn giao.
4. Comeback (Giao không thành công / Quay đầu): Khách vắng nhà, không liên lạc được hoặc từ chối nhận hàng $\Rightarrow$ hàng được chở quay về kho.

### 5.2. Công cụ Tìm kiếm & Lọc
- Lọc đơn giao hàng theo trạng thái (Prepare, Shipping, Done, Comeback).
- Lọc theo ngày giao, khoảng thời gian (từ ngày... đến ngày...), theo đơn vị vận chuyển hoặc theo mã hóa đơn POS / mã đơn Online.

---

## 6. XUẤT BÁO CÁO DAILY DELIVERY LIST (THEO MẪU TÀI LIỆU TRANG 10)

### 6.1. Cấu trúc Bảng Báo Cáo Daily Delivery List
Hàng ngày, hệ thống hỗ trợ xuất file Excel bàn giao cho tài xế xe tải với định dạng chuẩn:

| Cột | Tiêu đề | Ý nghĩa & Dữ liệu |
| :---: | :--- | :--- |
| 1 | **No** | Số thứ tự điểm dừng (theo thứ tự lộ trình đã tối ưu). |
| 2 | **Customer's details** | Tên khách hàng, Số điện thoại liên hệ, Địa chỉ giao hàng chi tiết. |
| 3 | **From 164** | Tên sản phẩm (Item) & Số lượng kiện (No of box) bốc từ Kho 164. |
| 4 | **From 171** | Tên sản phẩm (Item) & Số lượng kiện (No of box) bốc từ Kho 171. |
| 5 | **From 53** | Tên sản phẩm (Item) & Số lượng kiện (No of box) bốc từ Kho 53. |
| 6 | **Customer's signature** | Ô dành cho khách hàng ký nhận khi nhận đủ số box. |
| 7 | **Notes** | Ghi chú khung giờ giao (ví dụ: *Between 3-4pm*) hoặc yêu cầu lắp ráp. |

---

## 7. TIÊU CHÍ NGHIỆM THU PHI CHỨC NĂNG (NFR)
1. **Thời gian Tối ưu Lộ trình (Optimization Latency):** Phân tích và trả về tuyến đường tối ưu cho 10-20 điểm dừng trong thời gian $\le 3.0\text{s}$.
2. **Độ chính xác địa chỉ (Geocoding Accuracy):** 100% địa chỉ giao hàng được chuẩn hóa tọa độ qua Google Maps API.
3. **Tính toàn vẹn của Báo cáo Delivery:** File Daily Delivery List tự động bóc tách chính xác số lượng Box cần bốc từ từng kho tương ứng.
