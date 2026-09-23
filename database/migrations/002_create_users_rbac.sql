-- ================================================================
-- MIGRATION 002: Users & RBAC Tables
-- Tables: users, roles, user_roles
-- ================================================================

-- ----------------------------------------------------------------
-- 1. users — Người dùng nội bộ hệ thống
-- ----------------------------------------------------------------
CREATE TABLE IF NOT EXISTS users (
    id                  BIGSERIAL PRIMARY KEY,
    email               VARCHAR(255) NOT NULL UNIQUE,
    password_hash       VARCHAR(255) NOT NULL,
    first_name          VARCHAR(100) NOT NULL,
    last_name           VARCHAR(100) NOT NULL,
    phone               VARCHAR(30),
    is_active           BOOLEAN DEFAULT TRUE,
    created_at          TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    updated_at          TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP
);
CREATE INDEX IF NOT EXISTS idx_users_email ON users(email);

-- ----------------------------------------------------------------
-- 2. roles — Vai trò hệ thống (RBAC)
-- ----------------------------------------------------------------
CREATE TABLE IF NOT EXISTS roles (
    id                  BIGSERIAL PRIMARY KEY,
    code                VARCHAR(50) NOT NULL UNIQUE,
    -- 'SUPER_ADMIN', 'STORE_MANAGER', 'SALESPERSON', 'WAREHOUSE_STAFF',
    -- 'DISPATCHER', 'MARKETER', 'ACCOUNTANT', 'CS_AGENT'
    name                VARCHAR(100) NOT NULL,
    description         TEXT
);

-- ----------------------------------------------------------------
-- 3. user_roles — Bảng nối User <-> Role (đa vai trò)
-- ----------------------------------------------------------------
CREATE TABLE IF NOT EXISTS user_roles (
    id                  BIGSERIAL PRIMARY KEY,
    user_id             BIGINT NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    role_id             BIGINT NOT NULL REFERENCES roles(id) ON DELETE CASCADE,
    store_id            BIGINT REFERENCES stores(id) ON DELETE SET NULL,
    -- store_id = NULL => áp dụng toàn hệ thống (SUPER_ADMIN)
    assigned_at         TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT uq_user_role_store UNIQUE NULLS NOT DISTINCT (user_id, role_id, store_id)
);
CREATE INDEX IF NOT EXISTS idx_user_roles_lookup ON user_roles(user_id, role_id);
