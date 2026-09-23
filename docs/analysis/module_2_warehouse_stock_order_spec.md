# 📘 ĐẶC TẢ YÊU CẦU NGHIỆP VỤ - MODULE 2: QUẢN LÝ KHO & ĐẶT HÀNG NHÀ CUNG CẤP (STOCK ORDER)
**Dự án:** Hệ Thống Quản Lý Bán Hàng & Thương Mại Điện Tử Đa Chi Nhánh (Sales Management System)  
**Tài liệu tham chiếu:** Phụ lục 01 - Tính năng Hệ thống Quản trị (Công ty TNHH Webkynang Việt Nam)  
**Phiên bản:** 1.0  
**Ngày hoàn thành phân tích:** 27/08/2026  

---

## 1. TỔNG QUAN PHÂN HỆ
Module 2 đóng vai trò trung tâm trong chuỗi cung ứng của doanh nghiệp nội thất:
1. **Quản lý Hệ thống Kho hàng & Cửa hàng (Multi-Warehouse & Multi-Shop).**
2. **Quản lý Nhà cung cấp & Quy cách Hàng hóa Cont (Suppliers & Inbound Specifications).**
3. **Công cụ Tính Giá Thành Cập Bến (Landed Cost) & Định Giá 5 Giai Đoạn Hai Chiều.**
4. **Thuật toán Thông minh Gợi ý Đặt hàng lại (Smart Reorder Point by Supplier Lead-Time).**
5. **Quản lý Luân chuyển Hàng hóa (Phiếu Nhập, Phiếu Xuất, Phiếu Chuyển Kho Định kỳ).**

---

## 2. QUẢN LÝ HỆ THỐNG KHO HÀNG (WAREHOUSE MANAGEMENT)

### 2.1. Cấu trúc Mô hình Kho & Cửa hàng
- **Đa kho & Đa chi nhánh:** Hệ thống quản lý nhiều kho tổng (Main Warehouses/Distribution Centers) và nhiều cửa hàng (Showrooms/Shops).
- **Cửa hàng là một kho chi nhánh:** Mỗi Showroom được định nghĩa và theo dõi như một kho hàng con để quản lý lượng hàng trưng bày và hàng có sẵn tại chỗ.
- **Bật / Tắt hoạt động của Kho:** Cung cấp tính năng kích hoạt hoặc tạm dừng hoạt động của kho trên thực tế $\Rightarrow$ Dữ liệu này được tự động liên kết sang Module Chi phí để tính toán chi phí thuê kho theo kỳ.

### 2.2. Báo cáo & Thống kê Kho Hàng
- Thống kê thời gian thực theo mốc thời gian tùy chọn (mặc định là Hôm nay):
  - Số lượng hàng hóa nhập đến kho từ NCC.
  - Số lượng hàng hóa điều chuyển từ kho đến các cửa hàng.
  - Số lượng hàng hóa xuất kho giao cho khách hàng (Delivery Outbound).
  - Số lượng hàng hóa khách trả về kho (Return Inbound).
- Báo cáo tổng dung lượng và tỷ lệ lấp đầy của từng kho.

---

## 3. QUẢN LÝ NHÀ CUNG CẤP & ĐẶT HÀNG CONT (STOCK ORDER)

### 3.1. Thông tin Nhà cung cấp (Suppliers)
- Thông tin định danh: Tên NCC, Quốc gia/Khu vực, Người liên hệ, SĐT, Email, Điều khoản thanh toán.
- **Cấu hình Lead-Time theo từng NCC:** Cài đặt thời gian vận chuyển/sản xuất (số ngày) đặc thù cho từng nhà cung cấp dựa trên khoảng cách địa lý (ví dụ: NCC Trung Quốc 90 ngày, Việt Nam 45 ngày, NCC nội địa Úc 14 ngày).

### 3.2. Quản lý Đơn Đặt Hàng Nhà Cung Cấp (Stock Order / Inbound Container)
- Mỗi Stock Order gắn với một Container hoặc Lô hàng nhập khẩu, quản lý chi tiết:
  - **Danh sách biến thể (SKU List):** Số lượng đặt của từng variation (màu sắc, kích thước, chất liệu).
  - **Ngày dự kiến về kho (ETA - Estimated Time of Arrival):** Mốc thời gian cốt lõi để hiển thị cho nhân viên POS bán đơn Pre-Order.
  - **Quy cách đóng gói & Thể tích:** 
    - Thể tích khối của sản phẩm ($).
    - Số lượng kiện/thùng của 1 sản phẩm (Number of Boxes per Item).
    - Kích thước chiều dài $\times$ rộng $\times$ cao của từng box ( \times W \times H$).
    - Trọng lượng của từng box ($).

---

## 4. CÔNG CỤ TÍNH GIÁ THÀNH (LANDED COST) & ĐỊNH GIÁ HAI CHIỀU

### 4.1. Công thức Tính Giá Vốn Cập Bến (Landed Cost Calculator)
\text{Cước vận chuyển / } CBM = \frac{\text{Tổng cước vận chuyển cả Container}}{\text{Tổng } CBM \text{ của Container}}
\text{Chi phí vận chuyển từng sản phẩm} = CBM_{\text{sản phẩm}} \times \text{Cước vận chuyển / } CBM
\text{Giá vốn cập bến (Landed Cost)} = \text{Giá mua (USD/AUD quy đổi)} + \text{Chi phí vận chuyển từng sản phẩm} + \text{Thuế/Phí hải quan (nếu có)}

### 4.2. Quy trình Tính Giá Hai Chiều & Sinh Giá 5 Giai Đoạn
1. **Bước 1: Tính Landed Cost:** Hệ thống tự động tính ra Landed Cost theo công thức trên.
2. **Bước 2: Tính toán tương hỗ hai chiều giữa Giá Stage 1 & Lợi nhuận:**
   - **Cách A:** Nhân viên nhập **Giá bán Stage 1** $\Rightarrow$ Hệ thống tự động tính:
     \text{Tiền lãi dự kiến} = \text{Giá Stage 1} - \text{Landed Cost}
     \% \text{ Lãi mục tiêu (Margin)} = \frac{\text{Tiền lãi}}{\text{Giá Stage 1}} \times 100\%
   - **Cách B:** Nhân viên nhập **% Lãi mục tiêu** $\Rightarrow$ Hệ thống tự động tính:
     \text{Giá Stage 1} = \frac{\text{Landed Cost}}{1 - \% \text{ Lãi mục tiêu}}
3. **Bước 3: Tự động sinh dải giá Stage 2 $\rightarrow$ Stage 5:**
   - Căn cứ vào Giá Stage 1 vừa xác định, hệ thống tự động sinh ra các mức giá từ Stage 2 đến Stage 5 theo tỷ lệ $\%$ giảm mặc định của danh mục sản phẩm (ví dụ: Stage 2 giảm 10%, Stage 3 Clearance giảm 25%, Stage 4 giảm 35%, Stage 5 giảm 50%).
4. **Bước 4: Kiểm tra & Chỉnh sửa thủ công:** Cho phép nhân viên/quản lý tinh chỉnh lại con số cụ thể của từng Stage trước khi lưu.
5. **Bước 5: Nhấn nút APPLY:** Gán toàn bộ dải giá này vào cấu hình định giá của Lô hàng/Sản phẩm để sẵn sàng kích hoạt bán.

---

## 5. THUẬT TOÁN GỢI Ý ĐẶT HÀNG THÔNG MINH (SMART REORDER POINT)

### 5.1. Phân loại Mức Tồn Kho Khả Dụng
Hệ thống gắn nhãn cảnh báo trực quan theo 5 cấp độ tồn kho:
1. Good Volume (Tồn dồi dào - Đủ bán trên 60 ngày).
2. Medium Volume (Tồn ổn định - Đủ bán 30 đến 60 ngày).
3. Low Volume (Tồn thấp - Cần chuẩn bị đặt hàng).
4. Almost Empty (Gần hết hàng - Đã chạm ngưỡng đặt hàng lại).
5. Empty (Hết hàng - Cần mở bán Pre-order).

### 5.2. Công thức Điểm Đặt Hàng Lại (Reorder Point - ROP)
\text{Tốc độ bán trung bình/ngày (Daily Velocity } D) = \frac{\text{Số lượng bán 30 ngày gần nhất}}{30}
\text{Nhu cầu trong thời gian chờ hàng (Lead-Time Demand)} = D \times \text{Lead-Time}_{\text{NCC (ngày)}}
\text{Điểm Đặt Hàng Lại (ROP)} = \text{Lead-Time Demand} + \text{Tồn kho an toàn (Safety Stock)}

- **Cơ chế Cảnh báo:** Khi:
  \text{Tồn thực tế} + \text{Hàng đang trên đường về} \le \text{ROP}
  $\Rightarrow$ Hệ thống tự động đẩy cảnh báo màu đỏ lên Dashboard của Quản lý kho, kèm theo gợi ý số lượng cần đặt thêm để tối ưu hóa thể tích đóng đầy Container (FCL - Full Container Load).

---

## 6. QUẢN LÝ PHIẾU NHẬP, PHIẾU XUẤT & ĐIỀU CHUYỂN KHO

### 6.1. Phiếu Nhập Kho (Inbound Goods Receipt)
- Tạo phiếu nhập từ Nhà cung cấp khi Container cập bến.
- Kiểm đếm số lượng thực nhận so với Stock Order (ghi nhận dư/thiếu/hư hỏng).
- In Bill/Phiếu nhập kho theo mẫu chuẩn để thủ kho và tài xế ký nhận.
- Cập nhật số lượng vật lý vào Kho và chuyển trạng thái Lô hàng (Batch Status).

### 6.2. Phiếu Xuất Kho & Chuyển Kho Cửa Hàng (Store Transfer)
- **Chuyển hàng định kỳ:** Lập lịch điều chuyển hàng định kỳ từ Kho tổng đến các Showroom.
- **Quy trình:** Tạo phiếu chuyển $\rightarrow$ Xuất kho tổng $\rightarrow$ Vận chuyển $\rightarrow$ Cửa hàng trưởng kiểm đếm và bấm Ký nhận bàn giao trên Web-App $\Rightarrow$ Tự động tăng tồn Showroom và giảm tồn Kho tổng.
- In Phiếu bàn giao vận chuyển kèm mã vạch / SKU để đối soát.

---

## 7. TIÊU CHÍ NGHIỆM THU PHI CHỨC NĂNG (NFR)
1. **Chính xác toán học (Calculation Precision):** Công thức tính Landed Cost và Giá bán 5 Stage không được phép sai lệch quá $\pm 0.01\text{ AUD/USD}$.
2. **Khả năng Tìm kiếm & Lọc:** Lọc thông minh theo mã phiếu, SKU, NCC, trạng thái kho trong thời gian $\le 1.0\text{s}$.
3. **Audit Trail:** Lưu trữ toàn bộ lịch sử xuất/nhập/điều chỉnh tồn kho kèm người thực hiện và lý do điều chỉnh.
