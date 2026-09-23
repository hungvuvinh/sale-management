# Project Tech Stack Specification

Document ID: `TECH_STACK.md`  
Last Updated: 2026-09-14  

---

## 1. Overview & Architecture Selection

The Sale Management System (Hệ thống Quản lý Bán hàng & Chuỗi Cửa hàng) uses a 3-tier architecture. **The Frontend React application interacts EXCLUSIVELY with the C# ASP.NET Core Backend API.** Direct frontend queries to the Database or PostgREST are strictly prohibited for security, audit, and domain logic integrity.

```
┌─────────────────────────────────────────────────────────┐
│              React SPA + Tailwind CSS                   │
│       (Vite, TanStack Query, Zustand, Radix UI)         │
└────────────────────────────┬────────────────────────────┘
                             │
                             │ (All HTTP API Requests to /api/v1/*)
                             ▼
               ┌───────────────────────────┐
               │    C# ASP.NET Core        │
               │  Backend API (.NET 8/9)   │
               │ (Auth, RBAC, Domain, CQRS)│
               └─────────────┬─────────────┘
                             │
             ┌───────────────┴───────────────┐
             │ (EF Core / Dapper / Internal) │
             ▼                               ▼
   ┌──────────────────┐            ┌──────────────────┐
   │    PostgREST     │            │  PostgreSQL 16   │
   │ (Internal Data)  │            │     Database     │
   └─────────┬────────┘            └──────────────────┘
             │                               ▲
             └───────────────────────────────┘
```

---

## 2. Core Tech Stack Breakdown

| Component | Technology | Target Version | Primary Responsibilities |
| :--- | :--- | :--- | :--- |
| **Frontend UI** | **React** | 18+ / 19 | Single Page Application (SPA) built with Vite, TypeScript, dynamic UI components. |
| **Styling** | **Tailwind CSS** | v3.4+ / v4.0 | Utility-first CSS framework, custom dark mode, responsive design system. |
| **Backend API Gateway & Core** | **C# / ASP.NET Core** | .NET 8 / .NET 9 | **Single entry point for ALL API requests.** Handles Auth, RBAC, 5-stage pricing, FIFO inventory allocations, delivery routing, SignalR real-time events. |
| **Internal Data API Engine** | **PostgREST** | v12+ | Internal auto-generated REST service consumed by C# Backend services for rapid internal data access. |
| **Database** | **PostgreSQL** | 16+ | Core relational data store, RLS policies, triggers, stored procedures, audit logging. |

---

## 3. Recommended Libraries & Dependencies

### 3.1 Backend: C# (.NET ASP.NET Core)

#### Data Access & Database
- **`Npgsql.EntityFrameworkCore.PostgreSQL`**: Official Entity Framework Core provider for PostgreSQL.
- **`Dapper`**: High-performance micro-ORM for complex SQL queries, analytical reports, and high-speed batch operations.
- **`postgrest-csharp`**: Native C# client for consuming internal PostgREST data endpoints inside .NET services.

#### Authentication & Authorization
- **`Microsoft.AspNetCore.Authentication.JwtBearer`**: JWT Authentication middleware (verifies tokens for all API requests).
- **`BCrypt.Net-Next`**: Secure password hashing algorithms for user management.

#### Validation & CQRS / Business Logic
- **`MediatR`**: In-process messaging for CQRS pattern, decoupling controller handlers from business domain services.
- **`FluentValidation.DependencyInjectionExtensions`**: Fluent validation rules for request DTOs.

#### API Documentation & Real-time Communication
- **`Swashbuckle.AspNetCore`** / **`Scalar.AspNetCore`**: OpenAPI / Swagger UI interactive documentation generator.
- **`Microsoft.AspNetCore.SignalR`**: Real-time WebSockets engine for live POS transactions, delivery tracking, and CSKH chat.

#### Background Tasks & Caching
- **`StackExchange.Redis`**: Distributed caching and session store integration.
- **`Quartz.NET`** or **`Hangfire`**: Distributed background job scheduler for recurring marketing campaigns, inventory syncs, and commission calculations.

---

### 3.2 Frontend: React + Tailwind CSS Stack

#### Core & Build System
- **`vite`** & **`@vitejs/plugin-react`**: Ultra-fast build tool, local dev server with HMR, TypeScript support.
- **`react-router-dom`**: Standard declarative routing for React SPA navigation.

#### Styling & Component Ecosystem
- **`tailwindcss`**, **`postcss`**, **`autoprefixer`**: Utility-first CSS engine.
- **`clsx`** & **`tailwind-merge`**: Utility functions for conditionally merging Tailwind classes without conflicts.
- **`lucide-react`**: Comprehensive, lightweight SVG icon library.
- **`@radix-ui/react-*`** (or **`shadcn/ui`**): Headless, accessible UI primitives (Dialog, DropdownMenu, Tabs, Popover, Select).
- **`framer-motion`**: Declarative animations and transition effects for smooth UI interactions.

#### State Management & Data Fetching
- **`@tanstack/react-query`**: Server state management, smart caching, optimistic UI updates, and automated refetching for C# API endpoints.
- **`axios`**: Standard HTTP client with interceptors (JWT header injection, token refresh, error handling) to call C# ASP.NET Core Backend API endpoints.
- **`zustand`**: Fast, lightweight client-side state store (for shopping cart, current POS register session, theme preferences).

#### Forms & Validation
- **`react-hook-form`**: High-performance HTML5 form state management without unnecessary re-renders.
- **`zod`**: TypeScript-first schema validation with seamless `react-hook-form` resolver integration.

#### Visualizations & Feedback
- **`recharts`**: Responsive, composable charting library for executive dashboards and analytics reports.
- **`sonner`**: Beautiful, customizable toast notification component.

---

## 4. Architectural Rules & Security Directives

1. **Strict 3-Tier Security Boundaries**:
   - **Frontend React app NEVER communicates directly with PostgreSQL or PostgREST.**
   - All HTTP requests pass through C# ASP.NET Core Controllers (`/api/v1/*`).
2. **Centralized Authentication & Authorization**:
   - C# ASP.NET Core validates JWT tokens, inspects permissions, and enforces Role-Based Access Control (RBAC) before executing queries or commands.
3. **Database Migrations**:
   - Migration scripts in `database/migrations/*.sql` remain the single source of truth for PostgreSQL database schema definitions.
