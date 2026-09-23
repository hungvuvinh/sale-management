# Danh sách Actor và Use Case trong Hệ thống Quản lý Bán hàng

## 1. Ban giám đốc
*Là người có quyền cao nhất trong hệ thống, theo dõi hoạt động kinh doanh và ra quyết định chiến lược.*
- **Báo cáo tổng quan:** Xem báo cáo tổng hợp doanh thu, lợi nhuận, chi phí và hiệu suất kinh doanh của hệ thống.
- **Quản lý nhân viên:** Theo dõi tình trạng nhân sự và các hoạt động liên quan đến đội ngũ.

## 2. Admin
*Quản trị viên hệ thống, chịu trách nhiệm quản lý người dùng, quyền truy cập và cấu hình chung.*
- **Phân quyền:** Cấp, chỉnh sửa và thu hồi quyền truy cập cho các nhân viên trong hệ thống.
- **Quản lý cấu hình hệ thống:** Thiết lập các thông số chung và cấu hình hoạt động của hệ thống.
- **Quản lý người dùng hệ thống:** Tạo, cập nhật và quản lý thông tin của người dùng trong hệ thống.

## 3. Kế toán
*Phụ trách công việc tài chính, tính lương và báo cáo kế toán.*
- **Quản lý chi phí:** Theo dõi các khoản chi, định mức chi phí và báo cáo tài chính.
- **Tính lương:** Tính lương cơ bản, phụ cấp, hoa hồng và các khoản liên quan đến nhân viên.
- **Xuất báo cáo:** Lập báo cáo tài chính, báo cáo chấm công, báo cáo doanh thu và bảng lương.
- **Quản lý công nợ:** Quản lý các khoản phải thu, phải trả và đối soát giao dịch.

## 4. Quản lý 
*Tham gia điều hành hoạt động của cửa hàng hoặc kho.*
- **Quản lý nhân viên:** Theo dõi, phân công và giám sát nhân viên trong chi nhánh.
- **Quản lý sản phẩm:** Theo dõi danh mục sản phẩm, giá bán và tình trạng hàng hóa.
- **Báo cáo chi nhánh:** Xem thống kê doanh thu, lợi nhuận, tồn kho và hiệu suất hoạt động của chi nhánh.

## 5. Nhân viên kho
*Chịu trách nhiệm quản lý hàng hóa và vận hành kho.*
- **Quản lý sản phẩm:** Theo dõi số lượng hàng tồn, hàng sắp hết và mức tồn tối thiểu. Kiểm tra và cập nhật hàng nhập, hàng xuất và điều chuyển kho.
- **Quản lý đặt hàng phía nhà cung cấp:** Tạo và xử lý đơn đặt hàng từ nhà cung cấp.
- **Quản lý lô hàng/FIFO:** Theo dõi lô hàng, hạn sử dụng và thực hiện tính toán theo nguyên tắc FIFO.

## 6. Nhân viên bán hàng
*Thực hiện nghiệp vụ bán hàng trực tiếp và hỗ trợ khách hàng tại cửa hàng.*
- **Bán hàng:** Tạo đơn hàng, áp dụng khuyến mãi và xử lý thanh toán.
- **Quản lý khách hàng:** Thêm, sửa, tìm kiếm và quản lý thông tin khách hàng.
- **Tra cứu tồn kho:** Kiểm tra số lượng hàng còn lại trong kho để tiến hành bán.
- **Lịch sử bán hàng:** Kiểm tra các đơn hàng đã bán và tra cứu hoa hồng nhận được trong đơn hàng mình đã bán.

## 7. Nhân viên marketing
*Phụ trách xây dựng chiến dịch bán hàng, khuyến mãi và hiển thị sản phẩm.*
- **Quản lý khuyến mãi:** Thiết lập các chương trình giảm giá, ưu đãi và banner truyền thông.

## 8. Nhân viên CSKH
*Hỗ trợ khách hàng qua các kênh giao tiếp trực tuyến.*
- **Chat trực tuyến:** Trả lời câu hỏi và hỗ trợ khách hàng ngay khi có nhu cầu.
- **Hỗ trợ đơn hàng:** Tư vấn về sản phẩm, trạng thái đơn hàng và giải quyết khiếu nại, hỗ trợ đổi trả.

## 9. Khách hàng
*Người dùng cuối của hệ thống, thực hiện mua sắm và theo dõi đơn hàng.*
- **Tìm kiếm sản phẩm:** Duyệt và tìm kiếm sản phẩm theo danh mục, giá, tính năng hoặc nhu cầu.
- **Mua hàng:** Chọn sản phẩm, thêm vào giỏ hàng và thực hiện thanh toán.
- **Theo dõi đơn hàng:** Kiểm tra trạng thái đơn hàng đang giao hoặc đã hoàn tất.
- **Theo dõi thông tin tài khoản:** Theo dõi đơn hàng cũ, sản phẩm đã mua, các chương trình ưu đãi đã tham gia và điểm đã tích được.

## Tổng hợp
Các actor trong hệ thống tập trung vào ba nhóm chính:
1. **Quản trị và điều hành:** Ban giám đốc, Admin, Quản lý, Kế toán.
2. **Vận hành bán hàng và kho:** Nhân viên bán hàng, Nhân viên kho, Marketing, CSKH.
3. **Người dùng cuối:** Khách hàng.

