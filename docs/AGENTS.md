# AGENTS.md — VissSoft Internship AI Coding Rules

## 1. Mục đích

File này là quy tắc bắt buộc cho mọi AI coding agent và intern làm việc trong repository.

Áp dụng cho:

- Antigravity + Stick
- Codex
- Claude Code
- VS Code AI
- GitHub Copilot / Copilot Code
- Các coding agent khác được mentor cho phép

---

## 2. Nguyên tắc chung

1. Không sửa code trước khi hiểu task.
2. Đọc architecture và source liên quan trước khi implement.
3. Không thay đổi architecture ngoài scope task nếu chưa được approval.
4. Không xóa code hoặc test chỉ để làm build pass.
5. Không hardcode secret, password, token, API key.
6. Không thao tác Production.
7. Không xóa database hoặc dữ liệu thật.
8. Mọi database change phải có migration.
9. Mọi feature phải có test phù hợp.
10. Mọi endpoint nhạy cảm phải kiểm tra authorization.
11. Không sửa unrelated code.
12. Không tự đổi framework/library lớn nếu chưa được mentor duyệt.
13. Không thêm dependency nếu chưa giải thích nhu cầu và tradeoff.
14. Sau khi sửa phải chạy test liên quan.
15. Nếu không chắc chắn, phải nêu rõ assumption.

---

## 3. Quy trình bắt buộc trước khi implement

Agent phải trả về plan gồm:

```text
1. Requirement understanding
2. Relevant code
3. Proposed approach
4. Files affected
5. Database impact
6. Security impact
7. Edge cases
8. Test plan
9. Regression risks
10. Open questions
```

Chỉ implement sau khi intern review plan.

---

## 4. Quy tắc chỉnh sửa code

Ưu tiên:

- Thay đổi nhỏ nhất có thể.
- Giữ compatibility.
- Giữ naming và project conventions.
- Tái sử dụng component/service hiện có.
- Tránh duplicate logic.
- Tách business logic khỏi controller/UI.

Không tự ý:

- Refactor toàn bộ module vì “code chưa đẹp”.
- Rewrite architecture.
- Đổi database.
- Đổi authentication strategy.
- Đổi deployment model.

---

## 5. Quy tắc database

- Không sửa/xóa dữ liệu production.
- Không dùng destructive migration nếu chưa có kế hoạch rollback.
- Dữ liệu Raw Attendance không được sửa trực tiếp.
- Thay đổi schema phải có migration.
- Cần transaction khi nhiều bước phải thành công hoặc rollback cùng nhau.
- Phải xem xét index với query lớn.

---

## 6. Quy tắc security

Không được:

```text
secret in source
password in log
token in log
SQL concatenation
missing authorization
insecure direct object access
```

Phải kiểm tra:

```text
Authentication
Authorization
Input Validation
User Isolation
Rate Limit khi cần
Audit Log khi cần
```

---

## 7. Quy tắc test

Mỗi thay đổi phải trả lời:

- Test nào chứng minh feature chạy?
- Test nào kiểm tra edge case?
- Test nào ngăn regression?

Không được sửa/xóa test chỉ để pipeline xanh.

---

## 8. Quy tắc UI/Website

Khi làm VissSoft website:

- Đọc `DESIGN_SYSTEM.md` nếu có.
- Không tự đổi brand color.
- Không dùng template ngoài thay thiết kế.
- Không dùng icon để thay imagery chính nếu design yêu cầu ảnh/visual.
- Respect responsive design.
- Respect `prefers-reduced-motion`.
- Không hy sinh performance chỉ để thêm animation.
- Không hardcode content tràn lan.

---

## 9. Quy tắc báo cáo sau task

Agent/intern phải tóm tắt:

```text
Changed
Tests Run
Known Risks
Migration
Security Impact
Follow-up
```

Nếu test chưa chạy được, phải nói rõ lý do.

---

## 10. Quy trình Analysis → Design → Implementation

Agent không được mặc định chuyển thẳng từ Issue sang code.

Với task Medium/Large, phải kiểm tra các artifact liên quan đã tồn tại/chấp thuận chưa:

```text
Requirement / Acceptance Criteria
Architecture / Design
Database/API/UI impact
Security impact
Test plan
```

Nếu thiếu, agent phải dừng ở bước phân tích và chỉ ra phần cần quyết định trước khi implement.

---

## 11. Quy tắc sử dụng Skills

Trước task, agent/intern phải xem **Skills Catalog** và chọn skill liên quan nếu có.

Ví dụ:

```text
Requirement -> requirement-analysis
Architecture -> architecture-review
Database -> database-design
API -> api-design
UI -> frontend-ui / visssoft-design-system
Test -> testing
Security -> security-review
Debug -> debugging
PR -> code-review
Release -> deployment
Attendance -> attendance-domain
```

Không nạp skill không liên quan chỉ để tăng context.

Nếu một quy trình được lặp lại từ 3 lần trở lên, intern nên đánh giá việc đóng gói thành skill hoặc cải tiến skill hiện có.

Mọi thay đổi skill dùng chung phải qua Pull Request và review như source code.

Skill không được phép ghi đè các nguyên tắc bảo mật, kiến trúc và phạm vi task trong `AGENTS.md`.

---

## 12. Quy tắc Tự động hóa & Đồng bộ Tài liệu (Doc-Sync Pipeline - #10)

1. Mọi Pull Request có thay đổi liên quan đến API Route Annotation hoặc CSDL Schema **bắt buộc phải thực thi script đồng bộ tài liệu tự động**:
   ```bash
   python -m backend.docs.generate_openapi
   ```
2. Kết quả tự động xuất file OpenAPI schema tại `static/openapi.json` và sơ đồ CSDL tại `static/db_schema.mmd`.
3. Kiểm tra **Doc-Sync Gate** là điều kiện tiên quyết trong pipeline CI/CD và quy trình Code Review trước khi merge code vào nhánh chính (`main` / `develop`).
