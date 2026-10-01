# Engineering Audit

Audit date: 2026-10-01  
Branch: `main`  
Scope: repository state after restoring the Git link; findings below were verified against this checkout.

## Baseline verification

| Check | Result | Evidence |
| --- | --- | --- |
| `dotnet restore Orama.sln` | PASS | Completed after allowing NuGet access. |
| Release `dotnet build Orama.sln --no-restore --configuration Release` | PASS with 4 warnings | Two nullable assignments in `ClientesApiController`; two nullable dereferences in Razor views. |
| New fiscal tenant tests | PASS: 3 | EF Core InMemory; latest integration run: 3 passed, 1 skipped. |
| PostgreSQL/Testcontainers HTTP smoke test | NOT VERIFIED locally | Compiled, skipped unless `ORAMA_RUN_POSTGRES_TESTS=1`; Docker daemon is unavailable here. CI enables it. |
| Docker image/Compose runtime | NOT VERIFIED | Docker daemon unavailable. |
| `docker compose config --quiet` | PASS | Compose interpolation/schema accepted with temporary local-check environment variables; Docker CLI emitted a warning that its user config was inaccessible. |
| GitHub Actions run | NOT VERIFIED | Workflow committed locally only; remote execution not observed. |
| MAUI build | NOT VERIFIED | Required iOS/MacCatalyst workloads are missing; Windows target could not write build artifacts in this environment. |

The current test suite is newly checked in during this reconstruction. No historical test totals or coverage percentages are claimed for this checkout.

## Findings and status

| ID | Severity | Finding | Status | Notes |
| --- | --- | --- | --- | --- |
| SEC-01 | CRITICAL | JWT key and issuer/audience had hardcoded fallbacks. | RESOLVED | Startup now requires an external 32+ character key and explicit issuer/audience; API token lifetime set to 8 hours. |
| SEC-02 | CRITICAL | A known demo administrator was seeded by EF and credentials were printed/documented. | PARTIALLY RESOLVED | Seed removed from current model/migration snapshots and UI/docs/scripts; optional one-time local seed requires externally configured email/password. Existing databases may still contain the old account and require manual password rotation/removal. |
| SEC-03 | HIGH | CORS allowed any origin, method and header. | RESOLVED | Origins are configuration-driven; no origin is allowed when configuration is empty. |
| SEC-04 | HIGH | `BaseController` defaulted missing user/company IDs to `1`. | RESOLVED | Missing or non-positive IDs now throw instead of impersonating a default identity. |
| SEC-05 | HIGH | Development startup could delete the database when a connection failed. | RESOLVED | Destructive `EnsureDeleted` flow removed; startup uses non-destructive `EnsureCreated` only in Development. |
| SEC-06 | HIGH | Fiscal configuration/NF-e operations could query by record ID without tenant scope. | PARTIALLY RESOLVED | Company configuration and NF-e consult/cancel/sign/generate/validate now scope by `EmpresaId`; product fiscal configuration is scoped via its owning product. Broader fiscal verification still required. |
| SEC-07 | HIGH | Mobile app accepted local demo credentials and locally fabricated JWT/refresh tokens. | RESOLVED | Login now calls `api/AuthApi/login`; refresh is not faked and clears the local session because the backend has no refresh endpoint. |
| OPS-01 | HIGH | Local Compose used fixed database credentials and omitted the web service. | PARTIALLY RESOLVED | Compose now requires local `.env` values and includes app, PostgreSQL, health checks and persistent storage. Runtime is NOT VERIFIED. |
| OPS-02 | HIGH | Existing EF migrations are SQLite-specific while Compose uses PostgreSQL. | OPEN | Compose demo uses `EnsureCreated` in Development. A production-ready PostgreSQL migration baseline and deploy migration process remain necessary. |
| OPS-03 | MEDIUM | Build/test/Docker CI was absent. | PARTIALLY RESOLVED | A GitHub Actions workflow now restores, builds, runs tests and builds the image; no remote run verified. |
| TEST-01 | HIGH | No test source projects were tracked in the restored checkout. | PARTIALLY RESOLVED | Added three InMemory tenant/fiscal tests and an opt-in Testcontainers + WebApplicationFactory PostgreSQL HTTP smoke test. No full sales/rollback/concurrency flow yet. |
| DOC-01 | MEDIUM | README overclaimed completeness/readiness and linked missing screenshots. | PARTIALLY RESOLVED | Claims and broken image links removed/qualified; root Markdown inventory and archival work remain. |
| DEV-01 | LOW | Web EF Tools version was 10.x with EF runtime 8.x. | RESOLVED | Aligned to 8.0.0. |

## Remaining release blockers

- Verify PostgreSQL/Testcontainers and HTTP test on a Docker-enabled machine or CI run.
- Generate and validate PostgreSQL migrations; do not use the current SQLite migrations against PostgreSQL.
- Validate provisioning/login and complete API mobile connectivity on supported devices.
- Audit all business modules for tenant scope; this round focused on fiscal/NF-e and the existing base controller fallback.
- Test end-to-end invoice/faturamento transaction rollback and concurrent stock consumption against PostgreSQL.
- Rotate credentials for any existing database using the old seeded administrator; removing the seed from source does not change existing databases or Git history.
- Run the full documented workflow and inspect CI results before production or public release.
