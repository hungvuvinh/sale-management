-- ================================================================
-- MIGRATION 009: Live Chat & System Audit Logs
-- Tables: chat_sessions, chat_messages, system_audit_logs
-- ================================================================

-- ----------------------------------------------------------------
-- 1. chat_sessions — Phiên Live Chat (Khách <-> CS Agent)
-- ----------------------------------------------------------------
CREATE TABLE IF NOT EXISTS chat_sessions (
    id                      BIGSERIAL PRIMARY KEY,
    session_token           VARCHAR(100) NOT NULL UNIQUE,
    store_id                BIGINT NOT NULL REFERENCES stores(id) ON DELETE RESTRICT,
    customer_id             BIGINT REFERENCES customers(id) ON DELETE SET NULL,
    guest_name              VARCHAR(100),
    guest_email             VARCHAR(150),
    guest_postcode          VARCHAR(10),
    assigned_agent_user_id  BIGINT REFERENCES users(id) ON DELETE SET NULL,
    participant_type        VARCHAR(20) NOT NULL DEFAULT 'GUEST',
    status                  VARCHAR(30) NOT NULL DEFAULT 'OPEN',
    -- OPEN, ASSIGNED, RESOLVED, CLOSED
    created_at              TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    updated_at              TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT chk_chat_sessions_participant_type CHECK (
        (participant_type = 'GUEST' AND customer_id IS NULL)
        OR (participant_type = 'CUSTOMER' AND customer_id IS NOT NULL)
    )
);
CREATE INDEX IF NOT EXISTS idx_chat_sessions_lookup ON chat_sessions(session_token, status);
CREATE INDEX IF NOT EXISTS idx_chat_sessions_store ON chat_sessions(store_id, status);
CREATE INDEX IF NOT EXISTS idx_chat_sessions_participant_type ON chat_sessions(participant_type, status);

-- ----------------------------------------------------------------
-- 2. chat_messages — Tin nhắn trong phiên Live Chat
-- ----------------------------------------------------------------
CREATE TABLE IF NOT EXISTS chat_messages (
    id                  BIGSERIAL PRIMARY KEY,
    session_id          BIGINT NOT NULL REFERENCES chat_sessions(id) ON DELETE CASCADE,
    sender_type         VARCHAR(20) NOT NULL,
    -- GUEST, CUSTOMER, AGENT
    sender_user_id      BIGINT REFERENCES users(id) ON DELETE SET NULL,
    message_text        TEXT NOT NULL,
    client_message_id   VARCHAR(100),
    attachment_url      TEXT,
    is_read             BOOLEAN DEFAULT FALSE,
    sent_at             TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP
);
CREATE INDEX IF NOT EXISTS idx_chat_messages_session ON chat_messages(session_id, sent_at ASC);
CREATE UNIQUE INDEX IF NOT EXISTS uq_chat_messages_client_message
    ON chat_messages(session_id, client_message_id)
    WHERE client_message_id IS NOT NULL;

-- ----------------------------------------------------------------
-- 3. system_audit_logs — Nhật ký kiểm toán toàn hệ thống (AES-256-GCM)
-- ----------------------------------------------------------------
CREATE TABLE IF NOT EXISTS system_audit_logs (
    id                  BIGSERIAL PRIMARY KEY,
    entity_type         VARCHAR(50) NOT NULL,
    entity_id           BIGINT NOT NULL,
    actor_id            BIGINT REFERENCES users(id) ON DELETE SET NULL,
    action              VARCHAR(50) NOT NULL,
    old_data_encrypted  TEXT,           -- AES-256-GCM Base64
    new_data_encrypted  TEXT,           -- AES-256-GCM Base64
    key_version         VARCHAR(10) NOT NULL DEFAULT 'v1',
    ip_address          VARCHAR(45),
    created_at          TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP
);
CREATE INDEX IF NOT EXISTS idx_system_audit_lookup ON system_audit_logs(entity_type, entity_id, created_at DESC);
