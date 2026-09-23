-- MIGRATION 019: Ensure the development admin account exists.
-- This is intentionally separate from the broad demo seed so it can be
-- applied safely to an existing development volume.

INSERT INTO roles (code, name, description)
VALUES ('SUPER_ADMIN', 'Super Administrator', 'Quyền toàn hệ thống, không giới hạn store')
ON CONFLICT (code) DO NOTHING;

INSERT INTO users (email, password_hash, first_name, last_name, phone, is_active)
VALUES (
    'admin@visssoft.com.au',
    '$2b$10$r8QiSijUa3/Krt8DMJ6xlO/cIgxXvACC05XrRXO89V0vig/y92vCK',
    'Super',
    'Admin',
    '+61 8 9000 0000',
    TRUE
)
ON CONFLICT (email) DO UPDATE
SET password_hash = EXCLUDED.password_hash,
    is_active = TRUE,
    updated_at = CURRENT_TIMESTAMP;

INSERT INTO user_roles (user_id, role_id, store_id)
SELECT u.id, r.id, NULL
FROM users u
JOIN roles r ON r.code = 'SUPER_ADMIN'
WHERE u.email = 'admin@visssoft.com.au'
ON CONFLICT (user_id, role_id, store_id) DO NOTHING;