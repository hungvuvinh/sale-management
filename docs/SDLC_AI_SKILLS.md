# Quy trình sản xuất phần mềm với AI & Skills

## 1. Nguyên tắc bắt buộc

Hai dự án thực tập phải được thực hiện như **một dự án sản xuất phần mềm thực tế**, không phải bài tập “nhận yêu cầu rồi code”.

Mỗi chức năng phải đi qua tối thiểu:

```text
Business Request
  -> Requirement Analysis
  -> Functional Analysis
  -> Technical Analysis
  -> UX/UI & System Design
  -> Architecture / Data / API Design
  -> Task Breakdown
  -> AI-assisted Implementation
  -> Automated Test
  -> Code Review
  -> QA / UAT
  -> Deployment
  -> Monitoring
  -> Feedback / Improvement
```

**Không có Analysis/Design thì chưa được chuyển sang Implementation**, trừ bug rất nhỏ được reviewer xác nhận.

---

## 2. Deliverable theo từng giai đoạn

### Phase 1 — Requirement & Analysis

Trước khi code, nhóm phải tạo/cập nhật tài liệu phù hợp:

- `PROJECT_BRIEF.md` — mục tiêu, phạm vi, stakeholder.
- `REQUIREMENTS.md` — functional/non-functional requirements.
- `USE_CASES.md` hoặc User Stories — actor, flow, alternate/error flow.
- `ACCEPTANCE_CRITERIA.md` — điều kiện nghiệm thu có thể kiểm chứng.
- `BUSINESS_RULES.md` — quy tắc nghiệp vụ quan trọng.
- `ASSUMPTIONS.md` — giả định và câu hỏi chưa chốt.

AI được dùng để phân tích, tìm edge case, phản biện requirement và tạo câu hỏi; intern phải xác nhận kết quả với mentor/PO.

### Phase 2 — Design

Tùy task phải có một hoặc nhiều thiết kế:

- `ARCHITECTURE.md` — component/module boundaries, data flow.
- `DATABASE_DESIGN.md` + ERD — entity, relation, constraint, index, migration.
- `API_SPEC.md` — endpoint, request/response, error, auth.
- `SECURITY_DESIGN.md` — authentication, authorization, validation, audit.
- UI flow/wireframe/design reference đối với task giao diện.
- Sequence diagram/state diagram khi logic phức tạp.
- ADR (`docs/adr/`) cho quyết định kỹ thuật có trade-off đáng kể.

Intern phải giải thích được **vì sao chọn thiết kế A thay vì B**.

### Phase 3 — Planning

Từ thiết kế, chia thành Issue/Task nhỏ. Mỗi task phải có:

- Objective.
- Scope / Out of scope.
- Input / Output.
- Acceptance Criteria.
- Dependencies.
- Security/Data impact.
- Test plan.
- Definition of Done.

AI có thể đề xuất task breakdown và dependency graph, nhưng intern/leader phải review trước khi đưa vào sprint.

### Phase 4 — Implementation với AI

Quy trình chuẩn:

```text
Read context
 -> Select skills
 -> Ask AI to analyze
 -> Review AI plan
 -> Approve scope
 -> Implement incrementally
 -> Run test
 -> Inspect diff
 -> Commit checkpoint
```

Không yêu cầu agent “làm toàn bộ project” trong một prompt.

### Phase 5 — Verification

Tùy thay đổi phải có:

- Unit test.
- Integration/API test.
- E2E test khi cần.
- Security test/check.
- Migration test khi đổi DB.
- Responsive/visual/accessibility test đối với website.
- Regression test.

### Phase 6 — Review & QA

PR phải được review cả:

- Requirement.
- Design compliance.
- Business logic.
- Security.
- Data integrity.
- Tests.
- Maintainability.
- Performance.
- Documentation.

### Phase 7 — Release & Operation

Intern phải biết tối thiểu:

```text
Build -> CI -> DEV -> UAT -> Release -> Monitor -> Rollback
```

Mỗi release phải biết configuration/secrets ở đâu, migration nào chạy, health check nào kiểm tra và rollback thế nào.

---

## 3. Skills — bắt buộc biết sử dụng

Trong chương trình này, **skill** là một gói hướng dẫn/chuyên môn tái sử dụng cho AI agent. Skill giúp agent thực hiện một loại công việc theo cùng tiêu chuẩn thay vì mỗi lần lại prompt từ đầu.

Ví dụ skill nên có:

```text
skills/
  requirement-analysis/
  architecture-review/
  database-design/
  api-design/
  frontend-ui/
  testing/
  security-review/
  debugging/
  code-review/
  deployment/
  documentation/
  attendance-domain/
  visssoft-design-system/
```

Tên/cấu trúc kỹ thuật cụ thể có thể khác tùy công cụ, nhưng repository phải có một **Skills Catalog** mô tả skill nào dùng cho việc gì.

### Intern phải biết 4 việc với Skill

1. **Discover** — tìm skill phù hợp trước khi làm task.
2. **Select** — chọn đúng skill, không nạp mọi skill vào mọi task.
3. **Apply** — yêu cầu agent tuân thủ skill cùng `AGENTS.md`, architecture và task.
4. **Improve** — nếu một workflow lặp lại nhiều lần, đề xuất cập nhật/tạo skill mới và đưa qua review.

### Không được

- Dùng skill không đọc nội dung rồi tin hoàn toàn.
- Tự sửa skill dùng chung mà không review.
- Đưa secret/credential vào skill.
- Dùng skill để vượt qua architecture/security rule.

`AGENTS.md` và yêu cầu dự án có ưu tiên cao hơn hướng dẫn trong skill nếu có xung đột.

---

## 4. Skill Selection Matrix

| Công việc | Skill nên dùng |
|---|---|
| Làm rõ yêu cầu | requirement-analysis |
| Thiết kế module | architecture-review |
| Schema/migration/query | database-design |
| REST/API contract | api-design |
| Next.js/component/motion | frontend-ui + visssoft-design-system |
| Viết test | testing |
| Auth/AuthZ/input/audit | security-review |
| Lỗi khó | debugging |
| Trước khi merge | code-review |
| Docker/CI/UAT/release | deployment |
| Tài liệu | documentation |
| Logic chấm công | attendance-domain |

---

## 5. Cách dùng AI theo vai trò

Có thể dùng Antigravity + Stick, Codex, Claude Code, VS Code AI và GitHub Copilot. Không bắt buộc một task phải dùng tất cả.

Khuyến nghị phân vai:

```text
Human / Intern
  -> owns requirement, decision, approval, accountability

Antigravity + Stick
  -> orchestration, goal/task loop, multi-step workflow

Codex / Claude Code
  -> codebase analysis, plan, implementation, test, refactor, review

VS Code + AI / Copilot
  -> local editing, completion, focused changes, explain/debug

Skills
  -> reusable domain/process expertise for all compatible agents
```

Với task quan trọng, có thể dùng **AI thứ hai làm reviewer** nhưng kết luận cuối cùng vẫn thuộc intern/reviewer người.

---

## 6. AI Work Log

Với task mức Medium/Large, PR phải ghi ngắn gọn:

```text
AI tools used:
Skills used:
AI proposal accepted:
AI proposal rejected:
Human decisions:
Tests executed:
Known risks:
```

Mục đích không phải theo dõi số token, mà để đánh giá intern có biết **điều khiển, phản biện và kiểm chứng AI** hay không.

---

## 7. Quality Gates

Task không được chuyển bước nếu chưa qua gate:

```text
Analysis Gate
  -> requirement + acceptance criteria approved

Design Gate
  -> architecture/data/API/UI impact approved

Implementation Gate
  -> AI plan reviewed

Merge Gate
  -> tests + review + documentation pass + doc-sync pipeline check (#10)

Release Gate
  -> UAT + migration/config + rollback/health checks ready
```

---

## 8. Chuẩn đầu ra

Intern phải chứng minh được mình có thể:

```text
Không rõ yêu cầu
  -> đặt câu hỏi
  -> phân tích
  -> thiết kế
  -> chia task
  -> chọn AI + skills
  -> kiểm soát agent
  -> code/test
  -> review
  -> deploy
  -> theo dõi
  -> xử lý lỗi
  -> cải tiến skill/process
```

Đó là tiêu chuẩn **AI-assisted Software Engineer**, không phải “người dùng AI để sinh code”.
