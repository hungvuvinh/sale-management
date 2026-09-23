# 📘 ĐẶC TẢ YÊU CẦU NGHIỆP VỤ - MODULE 1: SẢN PHẨM, ĐỊNH GIÁ 5 STAGE & POS BÁN HÀNG
**Dự án:** Hệ Thống Quản Lý Bán Hàng & Thương Mại Điện Tử Đa Chi Nhánh (Sales Management System)  
**Tài liệu tham chiếu:** Phụ lục 01 - Tính năng Hệ thống Quản trị (Công ty TNHH Webkynang Việt Nam)  
**Phiên bản:** 1.0  
**Ngày hoàn thành phân tích:** 26/08/2026  

---

## 1. TỔNG QUAN PHÂN HỆ
Module 1 bao gồm các nghiệp vụ cốt lõi tại cửa hàng và trên hệ thống quản trị:
1. **Quản lý danh mục Sản phẩm & Biến thể (Variants).**
2. **Thuật toán Tự động Định giá 5 Giai đoạn (5-Stage Dynamic Pricing Engine).**
3. **Màn hình Bán hàng tại Quầy (Point of Sale - POS).**
4. **Quản lý Khách hàng & Chính sách VIP (CRM & Loyalty).**
5. **Quản lý Đơn hàng (Order Management Lifecycle).**

---

## 2. QUẢN LÝ SẢN PHẨM & BIẾN THỂ (PRODUCT & VARIANTS)

### 2.1. Cấu trúc Dữ liệu Sản phẩm
Mỗi sản phẩm là loại sản phẩm biến thể (Variable Product), gồm một sản phẩm cha (Parent Product) và nhiều biến thể con (Variants/SKU).
- **Thông tin cơ bản:** Mã SKU, Tên sản phẩm, Hình ảnh đại diện, Thư viện ảnh, Mô tả chi tiết, Danh mục (Category/Sub-category), Shop/Kho liên kết.
- **Thuộc tính biến thể (Attributes):** Type, Size (Single, King Single, Double, Queen, King), Side, Material (Leather look, Fabric), Color (White, Black, Grey), Dimensions (Kích thước), Weight (Cân nặng).
- **Trạng thái tồn kho khả dụng (Available Stock):**
  - Số lượng thực tế trong kho.
  - Số lượng đang được giữ chỗ (Hold for Pre-order / Future Delivery).
  - Số lượng hàng sắp về trong các cont tương lai (Incoming Stock).
  - Hiển thị trực quan cảnh báo tồn: Bình thường, Tồn thấp, Âm tồn (được phân biệt bằng màu sắc).

### 2.2. Bộ lọc & Gợi ý Combo / Set
- **Bộ lọc thông minh đa tiêu chí:** Lọc theo toàn bộ tiêu chí biến thể (kích thước, màu sắc, chất liệu), khoảng giá (lower than, greater than, in between), tình trạng có hàng sẵn, thời gian hàng sắp về (1 ngày đến 365 ngày), chi nhánh/shop.
- **Gợi ý Set sản phẩm (Living / Bedroom / Dining Room Sets):**
  - Tự động gợi ý gói sản phẩm phối hợp (Ví dụ: Giường + Đệm + Sofa + Bàn ăn 6 ghế).
  - Ràng buộc: Toàn bộ sản phẩm trong Set phải có sẵn hoặc cùng có hàng về trong vòng 1 tháng, tổng ngân sách của cả Set không vượt quá định mức (ví dụ $\le \$).

### 2.3. Nhập / Xuất Dữ liệu
- Hỗ trợ tạo mới và cập nhật sản phẩm hàng loạt qua file **Excel**.
- Xuất dữ liệu sản phẩm, biến thể, giá và tồn kho ra file **Excel**.

---

## 3. THUẬT TOÁN TỰ ĐỘNG ĐỊNH GIÁ 5 GIAI ĐOẠN (5-STAGE DYNAMIC PRICING ENGINE)

### 3.1. Mục tiêu Nghiệp vụ
- **Tối đa hóa lợi nhuận:** Giữ mức giá cao nhất có thể (**Stage 1 - New Arrival**) khi sản phẩm có hàng mới hoặc đang bán tốt.
- **Tự động giảm giá:** Hạ dần sang **Stage 2 $\rightarrow$ Stage 3 (Overstock / Clearance) $\rightarrow$ Stage 4 $\rightarrow$ Stage 5** khi tốc độ bán không đạt kỳ vọng hoặc hàng lưu kho quá lâu.
- **Chốt chặn an toàn theo thời gian:** Ngăn việc giảm giá quá sớm nếu hàng bán quá nhanh, và ép hạ giá nếu hàng bán quá chậm (ế).

### 3.2. Cơ chế Hàng Đợi Định Giá Theo Lô (FIFO Batch-Based Pricing Queue)
1. **Lô hàng đang kích hoạt (Active Batch):**
   - Sản phẩm luôn áp dụng mức giá của Lô hàng đang Active.
   - Khi có **Lô hàng mới (Cont mới)** cập cảng về kho: Lô mới được đưa vào trạng thái chờ (QUEUED). Hàng tồn của Lô cũ tiếp tục được bán với mức giá của Stage hiện tại (ví dụ: Stage 3 Clearance) cho đến khi bán hết toàn bộ số lượng của Lô cũ (Old Batch Remaining Quantity = 0).
2. **Kích hoạt Lô mới & Reset Stage:**
   - Khi Lô cũ đã bán hết sạch: Hệ thống đóng Lô cũ (EXHAUSTED), kích hoạt Lô mới thành ACTIVE.
   - **Tự động reset mức giá về Stage 1 (New Arrival)**, đồng thời reset mốc thời gian bắt đầu chu kỳ lưu kho mới từ thời điểm kích hoạt.

### 3.3. Công thức & Quy tắc Chuyển Stage
- **Công thức tính % Hàng còn lại có thể bán:**
  \% \text{ Hàng còn lại} = \frac{\text{Hàng còn lại có thể bán hiện tại}}{\text{Tổng lượng hàng đợt nhập gần nhất}} \times 100\%
  *Trong đó:*
  \text{Tổng lượng hàng đợt nhập} = \text{Số lượng nhập của Lô} + \text{Tồn cũ gối đầu} - \text{Pre-orders đã giữ chỗ}

- **2 Điều kiện kích hoạt chuyển Stage:**
  1. **Điều kiện Nhu cầu (% Tồn kho giảm):** $\%$ hàng còn lại giảm xuống dưới ngưỡng cài đặt {\text{target}}$ **VÀ** thời gian giữ giá hiện tại đã đạt tối thiểu {\text{min}}$ (ngăn giảm giá quá sớm).
  2. **Điều kiện Thời gian (Hàng bán ế / Tồn kho quá hạn):** Dù $\%$ hàng tồn chưa giảm đủ, nhưng thời gian lưu kho đã vượt quá {\text{max}}$ $\Rightarrow$ Hệ thống tự động nhảy sang Stage tiếp theo để kích cầu xả hàng.

### 3.4. Cơ chế Vận hành của Daily Worker 
- **Chu kỳ chạy:** 1 lần/ngày vào lúc **00:00 đêm** (tránh làm thay đổi giá giữa giờ bán hàng ban ngày).
- **Quy trình xử lý:**
  1. Quét tất cả SKU có Automatic Pricing = ON.
  2. Kiểm tra trạng thái Lô cũ: Nếu đã hết $\Rightarrow$ Kích hoạt Lô mới $\Rightarrow$ Reset về Stage 1.
  3. Nếu chưa hết: Đánh giá 2 điều kiện $\%$ tồn và Thời gian lưu kho $\Rightarrow$ Chuyển Stage hoặc Giữ nguyên Stage.
  4. Cập nhật bảng giá niêm yết mới, kích hoạt xóa cache Redis (Redis Invalidation) để Website Storefront nhận giá tức thời và gọi Amazon SP-API đồng bộ gian hàng Amazon.
- **Can thiệp thủ công (Admin Override):** Cho phép Admin bấm nút *Cập nhật giá ngay* trên giao diện quản trị khi có yêu cầu khẩn cấp.

---

## 4. MÀN HÌNH BÁN HÀNG TẠI QUẦY (POINT OF SALE - POS)

### 4.1. Thao tác Bán hàng & Giao diện
- **Đa hóa đơn (Multi-tab Billing):** Cho phép thu ngân mở và thao tác nhiều hóa đơn cùng lúc mà không bị mất dữ liệu tạm thời.
- **Tìm kiếm siêu tốc (.2s - 2s$):** Tối ưu hóa truy vấn trên danh mục .000 - 50.000$ sản phẩm.
- **Tra cứu tồn kho thông minh:** Hiển thị số lượng hàng có thể bán (âm hoặc dương), số lượng sắp về và ngày dự kiến về (ETA).

### 4.2. Phân loại Hình thức Đặt đơn
1. **Order Now (Lấy hàng ngay):**
   - Đơn hàng xuất kho ngay tại chỗ hoặc giao ngay.
   - Trừ thẳng vào tồn kho vật lý khả dụng hiện tại (Stock Available).
2. **Pre-Order (Đặt trước hàng tương lai):**
   - Áp dụng khi kho hiện tại hết hàng nhưng có Container sắp về.
   - **Giao diện POS cho phép nhân viên chọn đích danh Container/Lô hàng sắp về trong danh sách** (có hiển thị ngày cập bến dự kiến ETA) để giữ chỗ (Reserve Allocation).
   - Khi Cont về kho, hệ thống tự động gán thẳng cho đơn Pre-order này.
3. **Future Delivery Booking (Mua hôm nay, hẹn giao sau nhiều tháng):**
   - Dành cho khách mua nhà chưa hoàn thiện, mua giữ giá sale.
   - Hệ thống căn cứ vào **thời điểm khách hẹn giao** để khấu trừ tồn vào đợt hàng tương ứng tại thời điểm đó (không làm giảm tồn hàng có thể bán ngay hôm nay).

### 4.3. Liên kết Bảng Lịch Giao Hàng (Delivery Booking)
- Khi tạo đơn trên POS, nhân viên bấm vào tab **Delivery Booking**:
  - Hệ thống mở lịch (Calendar) hiển thị nhanh số lượng đơn đã book trong từng ngày.
  - Cảnh báo các yêu cầu đặc biệt (**Special Order:** lắp ráp giường tủ, vác lên lầu...).
  - Tích hợp **Google Maps API Autocomplete** để tìm kiếm địa chỉ giao hàng chuẩn xác và hiển thị vị trí ghim (Pins).

### 4.4. Thanh toán & Chiết khấu
- **Hình thức thanh toán:**
  - Tiền mặt (Cash): Tự động gợi ý tiền thừa cho thu ngân.
  - Thẻ ngân hàng nội địa / Quốc tế (EFTPOS, Visa, Mastercard, American Express, Paypal).
- **Chiết khấu (Discount & Voucher):**
  - Cho phép nhập giá bán đặc biệt cho khách (Once-off special price) $\Rightarrow$ Hệ thống tự tính ra $\%$ discount tương ứng.
  - Hoặc nhập $\%$ discount $\Rightarrow$ Hệ thống tự tính ra giá mới.
  - Áp dụng mã Voucher quà tặng hoặc chương trình khuyến mại khả dụng.

### 4.5. Lịch sử & Quyền Hoàn tác
- Nhân viên bán hàng chỉ được xem lịch sử hóa đơn do chính mình tạo trong ngày.
- Quản lý / Sale Leader được quyền xem toàn bộ lịch sử chi nhánh.
- Cho phép hoàn tác hoặc hủy đơn trong một khoảng thời gian giới hạn sau khi hoàn thành đơn (cấu hình bởi doanh nghiệp).

---

## 5. QUẢN LÝ KHÁCH HÀNG & CHÍNH SÁCH VIP (CRM & LOYALTY)

### 5.1. Hồ sơ Khách hàng
- Lưu trữ thông tin định danh: Tên, SĐT, Email, Địa chỉ chi tiết (Số nhà, Đường, Phường/Quận, Thành phố, Postcode).
- Xem lịch sử mua hàng, tổng chi tiêu, số lần mua, lịch sử giao dịch thanh toán và công nợ.

### 5.2. Chính sách Khách hàng VIP
- **Điều kiện xét VIP:**
  - Tích điểm tự động: Chi tiêu $\ = 1\text{ điểm}$ (Point). Đạt mốc điểm quy định sẽ tự động lên VIP.
  - Hoặc được gán quyền VIP thủ công bởi Admin.
- **Quy trình khi lên VIP:**
  - Tự động gửi Email chúc mừng kèm mã thẻ VIP cho khách.
  - Gửi thông báo cho Admin/Marketing để có kế hoạch chăm sóc riêng.
  - Cấp tài khoản VIP để khách đăng nhập nhận ưu đãi trên Website.
- **Quyền lợi giá của VIP:**
  1. Nếu sản phẩm có khai báo **Giá VIP riêng (VIP Customer Price)**: Luôn áp dụng giá này.
  2. Nếu không khai báo giá VIP: **Luôn được hưởng giá ưu đãi sớm hơn 1 stage** (Ví dụ: Sản phẩm đang ở Stage 1 $\Rightarrow$ VIP được mua với giá Stage 2; đang ở Stage 2 $\Rightarrow$ VIP được mua với giá Stage 3).
  3. Được hưởng các đợt Sale khác nếu mức giảm sâu hơn (áp dụng mức ưu đãi cao nhất, không cộng dồn chồng chéo).

---

## 6. QUẢN LÝ ĐƠN HÀNG (ORDER MANAGEMENT LIFECYCLE)

### 6.1. Trạng thái Đơn hàng (Order Statuses)
- Draft (Đơn nháp)
- Pending (Chờ xử lý / Chờ thanh toán cọc)
- Processing (Đang chuẩn bị hàng / Đang giữ chỗ Cont)
- Shipping (Đang vận chuyển giao hàng)
- Completed (Đã giao hàng & thanh toán hoàn tất)
- Cancelled (Đã hủy đơn)
- Returned / Refunded (Đã đổi / trả hàng)

### 6.2. Phân quyền Can thiệp Đơn hàng
- Chỉ Cửa hàng trưởng (Store Manager) hoặc Super Admin mới có quyền chỉnh sửa thông tin đơn hàng sau khi tạo, duyệt hủy đơn hoặc đổi sản phẩm.
- Mọi thao tác chỉnh sửa đơn hàng đều được ghi log kiểm toán (Audit Trail) chi tiết (người sửa, nội dung sửa, thời gian).
- Xuất file Excel danh sách đơn hàng phục vụ công tác kế toán và đối soát.

---

## 7. TIÊU CHÍ NGHIỆM THU & PHI CHỨC NĂNG (NFR)
1. **Tốc độ tìm kiếm (Performance SLA):** Thời gian tìm kiếm sản phẩm và đơn hàng đạt từ .2\text{s} - 2.0\text{s}$ trên quy mô .000 - 50.000$ SKU.
2. **Toàn vẹn dữ liệu (Data Consistency):** Số lượng giữ chỗ (Reserved) và số lượng khả dụng (Available) luôn khớp chính xác khi có giao dịch POS đồng thời.
3. **Audit Log:** 100% các hành động nhảy giá của Worker và thay đổi đơn hàng của người dùng phải được lưu lịch sử đầy đủ.
