from __future__ import annotations

import json
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
STATIC_DIR = ROOT / "static"


SPEC = {
    "openapi": "3.0.1",
    "info": {
        "title": "Sale Management API",
        "version": "v1",
        "description": "OpenAPI export for the multi-store sales management backend.",
    },
    "servers": [{"url": "http://localhost:3000"}],
    "paths": {
        "/api/healthz": {
            "get": {
                "summary": "Health check",
                "responses": {"200": {"description": "Service healthy"}},
            }
        },
        "/api/readyz": {
            "get": {
                "summary": "Readiness check",
                "responses": {"200": {"description": "Dependencies ready"}},
            }
        },
        "/api/auth/login": {
            "post": {
                "summary": "Authenticate user",
                "responses": {"200": {"description": "Login successful"}},
            }
        },
        "/api/orders": {
            "post": {
                "summary": "Create POS order with FIFO reservation",
                "responses": {"201": {"description": "Order created"}},
            }
        },
        "/api/products": {
            "get": {"summary": "List products and variants", "responses": {"200": {"description": "Product catalog"}}},
            "post": {"summary": "Create product", "responses": {"201": {"description": "Product created"}}},
        },
        "/api/products/{id}": {
            "get": {"summary": "Get product detail", "responses": {"200": {"description": "Product detail"}}},
            "put": {"summary": "Update product", "responses": {"200": {"description": "Product updated"}}},
            "delete": {"summary": "Deactivate product", "responses": {"200": {"description": "Product deactivated"}}},
        },
        "/api/products/{id}/variants": {
            "post": {"summary": "Create product variant", "responses": {"200": {"description": "Variant created"}}},
        },
        "/api/products/variants/{variantId}": {
            "put": {"summary": "Update product variant", "responses": {"200": {"description": "Variant updated"}}},
            "delete": {"summary": "Deactivate product variant", "responses": {"200": {"description": "Variant deactivated"}}},
        },
        "/api/stock-orders": {
            "get": {"summary": "List stock orders", "responses": {"200": {"description": "Stock orders"}}},
            "post": {"summary": "Create stock order", "responses": {"201": {"description": "Stock order created"}}},
        },
        "/api/stock-orders/{id}/receive": {
            "post": {"summary": "Receive stock order and create FIFO lots", "responses": {"200": {"description": "Stock received"}}},
        },
        "/api/inventory/transfers": {
            "get": {"summary": "List inventory transfers", "responses": {"200": {"description": "Transfers"}}},
            "post": {"summary": "Create inventory transfer", "responses": {"201": {"description": "Transfer created"}}},
        },
        "/api/inventory/transfers/{id}/status": {
            "put": {"summary": "Update inventory transfer status", "responses": {"200": {"description": "Transfer status updated"}}},
        },
        "/api/chat/sessions": {
            "post": {
                "summary": "Create chat session",
                "responses": {"201": {"description": "Session created"}},
            }
        },
    },
    "components": {
        "schemas": {
            "AuditLogEncryptedPayload": {
                "type": "object",
                "properties": {
                    "old_data_encrypted": {"type": "string", "nullable": True},
                    "new_data_encrypted": {"type": "string", "nullable": True},
                    "key_version": {"type": "string", "example": "v1"},
                },
            }
        }
    },
}


def main() -> None:
    STATIC_DIR.mkdir(parents=True, exist_ok=True)
    openapi_path = STATIC_DIR / "openapi.json"
    schema_path = STATIC_DIR / "db_schema.mmd"

    openapi_path.write_text(json.dumps(SPEC, indent=2), encoding="utf-8")
    schema_path.write_text(
        "```mermaid\n"
        "erDiagram\n"
        "  users ||--o{ user_roles : has\n"
        "  stores ||--o{ products : sells\n"
        "  variants ||--o{ fifo_lots : has\n"
        "  orders ||--o{ order_items : contains\n"
        "  orders ||--o{ order_audit_logs : audited\n"
        "```\n",
        encoding="utf-8",
    )


if __name__ == "__main__":
    main()
