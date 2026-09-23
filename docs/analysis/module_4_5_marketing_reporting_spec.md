# 📘 ĐẶC TẢ YÊU CẦU NGHIỆP VỤ - MODULE 4 & 5: MARKETING, BÁO CÁO TÀI CHÍNH FIFO, HOA HỒNG & HỆ THỐNG
**Dự án:** Hệ Thống Quản Lý Bán Hàng & Thương Mại Điện Tử Đa Chi Nhánh (Sales Management System)  
**Tài liệu tham chiếu:** Phụ lục 01 - Tính năng Hệ thống Quản trị (Công ty TNHH Webkynang Việt Nam)  
**Phiên bản:** 1.1 (Cập nhật cơ chế đồng bộ đa kênh: Dùng Trực tiếp Backend API & Redis Cache thay cho Webhook)  
**Ngày cập nhật:** 28/08/2026  

---

## 1. TỔNG QUAN PHÂN HỆ
Module 4 & 5 phụ trách toàn bộ hoạt động tiếp thị đa kênh, quản trị tài chính - kế toán và vận hành hệ thống:
1. **Quản lý Chiến dịch Khuyến mại & Package Sale (Marketing Promotions).**
2. **Đồng bộ Đa Kênh: Truy vấn Backend API Trực Tiếp (Storefront) & Amazon Seller Partner API.**
3. **Cơ chế Caching Tối ưu Hiệu Năng (Redis On-Demand Cache Invalidation).**
4. **Theo dõi Chi phí Quảng cáo & Tự động Liên kết P&L (Ad Cost Tracking).**
5. **Thuật toán Tính Lợi Nhuận Thực Tế theo FIFO (First-In, First-Out Profit Engine).**
6. **Thuật toán Tính Hoa Hồng Nhân Viên Bán Hàng theo KPI Doanh Số/Giờ (Sales Commission Engine).**
7. **Cấu hình Chi phí, Hóa đơn Điện tử & Phân định Thuế/Hưu bổng Úc (Superannuation 9.5%).**
8. **Quản trị Phân quyền (RBAC) & Cảnh báo Đầu ngày (Priority Notifications).**

---

## 2. MODULE 4: MARKETING & ĐỒNG BỘ ĐA KÊNH

### 2.1. Quản lý & Setup Chiến dịch Sale (Promotion Engine)
- **Thiết lập giá khuyến mại linh hoạt:**
  - Nhập số tiền giảm trực tiếp (AUD) hoặc nhập tỷ lệ giảm (%). Hệ thống tự động tính toán giá sale tương ứng.
  - **Liên kết động với giá gốc:** Nếu giá sale được thiết lập theo % trên giá gốc, khi giá gốc tự động thay đổi (do Worker chuyển Stage giá 5 giai đoạn hoặc do Admin cập nhật), giá sale sẽ **tự động biến thiên theo % tương ứng**.
- **Package Sale (Combo nhiều sản phẩm):** Cho phép gom nhiều sản phẩm (ví dụ: Giường + Đệm + 2 Tủ đầu giường) thành 1 gói Combo với giá ưu đãi và gán hiển thị lên các Catalogues tương ứng trên Website.
- **Quản lý thời gian & Nhãn Badge:** Cài đặt thời gian bắt đầu - kết thúc, lý do sale, chọn Icon/Badge (ví dụ: *Hot Deal*, *Clearance*, *Christmas Sale*) để tự động hiển thị lên Website bán hàng.

### 2.2. Cơ chế Đồng bộ Đa Kênh & Tối ưu Hiệu Năng (Multi-Channel Sync & Performance)
- **Đối với các Website Bán hàng (Storefront 1, Storefront 2):**
  - **Không sử dụng Webhook:** Toàn bộ các Website bán hàng kết nối trực tiếp vào **Backend API tập trung (Single Source of Truth)**.
  - **Cơ chế On-Demand API Fetch:** Khi khách hàng truy cập hoặc tải lại trang (F5), Frontend gọi API lấy thông tin mới nhất trực tiếp từ Backend.
- **Đối với sàn thương mại điện tử bên thứ 3 (Amazon Seller):**
  - Sử dụng **Amazon Selling Partner API (SP-API)** chính thức.
  - Khi có cập nhật mô tả, trạng thái giá hoặc thay đổi tồn kho khả dụng => Worker gọi API của Amazon để đồng bộ 2 chiều dữ liệu giữa hệ thống quản trị và gian hàng Amazon.

### 2.3. Quản lý Chi phí Quảng cáo & Phân khúc Khách hàng
- **Nhập chi phí Ads hàng tuần:** Marketer nhập chi phí đã chi theo tuần (Facebook Ads, Google BM...) => Số liệu này tự động đẩy sang phân hệ Báo cáo Tài chính P&L.
- **Báo cáo biến động giá tuần:** Bảng so sánh toàn bộ sản phẩm được áp dụng giá mới từ đầu tuần đến thời điểm kiểm tra, đối chiếu doanh số, lợi nhuận, lượng tồn kho còn lại và % tồn để đánh giá hiệu quả Marketing.
- **Trích xuất dữ liệu phân khúc (Target Audience Export):** Xuất file Excel danh sách khách hàng theo tiêu chí (mua nhiều nhất, khách hàng trong bán kính 30km quanh Showroom, khách đến từ kênh Google/Facebook) để phục vụ chiến dịch Remarketing.

---

## 3. MODULE 5: BÁO CÁO TÀI CHÍNH & THUẬT TOÁN FIFO

### 3.1. Thuật toán Tính Lợi Nhuận Thực Tế theo FIFO (First-In, First-Out)
- **Phân biệt 2 khái niệm lợi nhuận:**
  1. **Lợi nhuận dự kiến (Expected Profit):** Được tạm tính tại thời điểm tạo đơn hàng/thu tiền cọc dựa trên giá vốn ước tính.
  2. **Lợi nhuận thực tế (Actual Realized Profit):** Chỉ được xác định chính xác **tại thời điểm hàng thực tế được xuất kho giao cho khách**, dựa trên giá vốn cập bến (Landed Cost) của lô hàng xuất kho theo nguyên tắc FIFO.
- **Quy tắc điều chỉnh lợi nhuận (Profit Adjustment):**
  - Khi tài xế xuất kho giao hàng => Hệ thống ghi đè Lợi nhuận dự kiến thành Lợi nhuận thực tế.
  - Khi khách hàng **Hủy đơn (Cancel)** => Lợi nhuận thực tế điều chỉnh về 0 AUD, hệ thống phát thông báo cho người quản lý.
  - Khi khách hàng **Đổi sản phẩm (Exchange)** => Lợi nhuận thực tế được điều chỉnh tăng/giảm theo mức chênh lệch của sản phẩm mới.

---

## 4. THUẬT TOÁN TÍNH HOA HỒNG NHÂN VIÊN BÁN HÀNG (COMMISSION ENGINE)

### 4.1. Công thức Tính Hoa Hồng theo KPI Doanh Số / Giờ
- **Yêu cầu doanh số cơ bản** = Tổng số giờ làm việc * Định mức bán cơ bản mỗi giờ (AUD/giờ)
- **Hoa hồng trước thuế** = (Tổng doanh số thực tế sau điều chỉnh - Yêu cầu doanh số cơ bản) * % Hoa hồng được hưởng

*Ví dụ:* Định mức cơ bản 500 AUD/giờ, % hoa hồng vượt định mức là 1%. Nhân viên làm 40 giờ trong tuần (Định mức = 20.000 AUD). Doanh số thực tế đạt 35.000 AUD => Hoa hồng = (35.000 - 20.000) * 1% = 150 AUD.

### 4.2. Nguyên tắc Chốt Kỳ Hoa Hồng (Commission Settlement Rule)
- **Chốt theo Đơn hàng Hoàn tất (Delivered Orders Only):** Hoa hồng của nhân viên bán hàng chỉ được **chốt và thanh toán trong kỳ khi đơn hàng đã hoàn tất giao hàng thành công (Completed/Delivered)**.
- Đảm bảo doanh số tính hoa hồng là doanh số thực tế 100%, loại bỏ hoàn toàn rủi ro sai lệch do khách hàng hủy hoặc đổi đơn sau đó.

---

## 5. CẤU HÌNH CHI PHÍ, THUẾ & HỆ THỐNG (SYSTEM SETTINGS)

### 5.1. Cấu hình Chi phí & Phân loại Thuế (Taxable vs Cash)
- **Chi phí cố định:** Khai báo tiền thuê nhà xưởng, showroom, điện nước, bảo hiểm (khai báo 1 lần, có thể chỉnh sửa theo tuần/tháng).
- **Chi phí lưu động:** Tự động tổng hợp từ chi phí nhân công, tiền lương, chi phí vận chuyển và chi phí Marketing.
- **Phân định Thuế & Hưu bổng Úc (Superannuation 9.5%):**
  - *Khoản chi có hóa đơn / Chịu thuế (Taxable):* Tự động tính kèm quỹ hưu bổng 9.5% (ví dụ: lương chuyển khoản ngân hàng).
  - *Khoản chi tiền mặt / Phi thuế (Non-taxable / Cash):* Không tính hưu bổng 9.5% vào chi phí doanh nghiệp.

### 5.2. Hóa đơn Điện tử (Tax Invoice Úc) & Phân quyền RBAC
- **In & Gửi Tax Invoice:** Tự động tạo mẫu Hóa đơn thuế điện tử theo chuẩn Úc (đầy đủ thông tin ABN, Bank details, Delivery surcharge, GST 10%) và gửi qua email cho khách hàng.
- **Quản lý Thông báo Ưu tiên Đầu ngày:** Cấu hình thông báo quan trọng xuất hiện dưới dạng Pop-up lớn giữa màn hình khi người dùng đăng nhập vào đầu ngày (gồm 2 tùy chọn: *Bỏ qua* hoặc *Đã xác nhận/xử lý*).
- **Phân quyền người dùng (RBAC):** Thiết lập chi tiết vai trò (Super Admin, Store Manager, Salesperson, Inventory Officer, Delivery Coordinator, Marketer) với ma trận quyền hạn chặt chẽ.
