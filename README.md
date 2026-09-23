# Sale Management System

Hệ thống quản lý bán hàng và chuỗi cửa hàng đa chi nhánh. Project hiện đang ở
giai đoạn MVP, gồm frontend React, backend ASP.NET Core, PostgreSQL, Redis và
PostgREST.

## Yêu cầu môi trường

- Docker Desktop và Docker Compose v2, nếu chạy bằng Docker.
- Node.js 20+ và npm, nếu chạy frontend local.
- .NET 10 SDK, hiện project đang target `net10.0`.
- Git.

Kiểm tra nhanh:

```powershell
docker --version
docker compose version
node --version
npm --version
dotnet --version
```

## Cách chạy nhanh bằng Docker

Từ thư mục gốc project:

```powershell
docker compose up --build -d
```

Sau khi các service khởi động:

- Frontend local: cần chạy riêng theo hướng dẫn ở mục [Chạy frontend local](#chạy-frontend-local).
- Backend API: <http://localhost:5000>
- Swagger UI: <http://localhost:5000/swagger>
- Health check: <http://localhost:5000/api/healthz>
- Readiness check: <http://localhost:5000/api/readyz>
- PostgreSQL: `localhost:5432`
- Redis: `localhost:6379`

Docker Compose tự chạy các migration trong `database/migrations/` khi tạo
volume PostgreSQL lần đầu. Nếu cần tạo lại database từ đầu:

```powershell
docker compose down -v
docker compose up --build
```

Lệnh `down -v` sẽ xóa dữ liệu PostgreSQL và Redis local.

## Chạy backend local

Có thể chạy các dependency bằng Docker rồi chạy API trực tiếp trên máy để
debug C#:

```powershell
docker compose up -d postgres redis postgrest
dotnet restore .\backend\src\SaleManagement.Api\SaleManagement.Api.csproj
dotnet run --project .\backend\src\SaleManagement.Api\SaleManagement.Api.csproj --no-launch-profile --urls http://localhost:5000
```

Backend local chạy tại <http://localhost:5000>. Swagger ở
<http://localhost:5000/swagger>. Port `5000` khớp với Vite proxy khi chạy
frontend local.

Nếu chỉ muốn chạy backend theo profile mặc định của Visual Studio/.NET CLI,
dùng `--launch-profile http`; khi đó API sẽ chạy ở port `5203`, Swagger ở
<http://localhost:5203/swagger>. Frontend sẽ cần đổi target proxy từ `5000`
sang `5203` trong `frontend/vite.config.ts`.

Khi chạy local, connection string mặc định trỏ tới PostgreSQL tại
`localhost:5432` với thông tin phát triển:

```text
Database: sale_management
Username: visssoft
Password: visssoft_dev_2026
```

## Chạy frontend local

Mở terminal thứ hai:

```powershell
Set-Location .\frontend
npm install
npm run dev
```

Mở <http://localhost:5173>. Vite proxy các request `/api/*` tới
`http://localhost:5000`, nên hãy chạy backend local bằng lệnh ở mục trên hoặc
chạy backend bằng Docker.


## Tài khoản phát triển

Migration seed hiện cung cấp tài khoản quản trị mặc định:

```text
Email:    admin@visssoft.com.au
Password: Admin123!
```

Không sử dụng thông tin này ở môi trường production. Các secret trong
`docker-compose.yml` chỉ dành cho development và phải được thay bằng biến môi
trường an toàn khi deploy.

## Dừng và dọn môi trường

```powershell
docker compose down
```

Xóa cả container và dữ liệu local:

```powershell
docker compose down -v
```
