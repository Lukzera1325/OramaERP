# Final adversarial review

Review date: 2026-10-01. Findings below are based on source inspection. This document does not claim the unrun Docker/PostgreSQL tests passed.

## BLOCKER

- PostgreSQL migrations are not available as a verified deployment path. Current migrations are SQLite-generated; the Compose sample creates a schema with `EnsureCreated` only in Development. Do not use this as a production deployment mechanism.
- Existing databases may still contain the formerly seeded administrator. Removing its model/migration seed does not update a database that has already been created; rotate/remove that account.

## IMPORTANT

- Fiscal tenant isolation is covered by InMemory service tests, but the PostgreSQL/Testcontainers health/connectivity test was skipped locally because Docker is unavailable. It also does not yet exercise cross-tenant HTTP CRUD.
- Sales/faturamento atomicity, rollback under injected failure, and concurrent stock consumption lack PostgreSQL integration tests.
- Broad tenant review of user, finance, purchase, reports, and production query paths remains incomplete.
- Mobile login now uses the server endpoint, but endpoint routing, API base address, TLS, token-expiry behavior, and sign-out/revocation need device-level verification.
- Existing four nullable warnings remain in API mapping and Razor views.

## NICE TO HAVE

- Add a true PostgreSQL migration baseline and test it from an empty container.
- Add HTTP tenant-boundary tests for product, sale, production and fiscal routes.
- Add a reproducible local account bootstrap/setup flow that does not grant cross-tenant super-admin rights.
- Finish moving historical root Markdown to `docs/archive/` after validating inbound links.
- Capture sanitized screenshots from a running application; no screenshots were fabricated.
