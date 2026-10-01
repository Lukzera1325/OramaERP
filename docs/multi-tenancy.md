# Multi-tenancy boundaries

Tenant identity is selected by the authenticated web session and supplied to application operations as `EmpresaId`. A missing/non-positive user or company is rejected by `BaseController`; the previous fallback to ID `1` is removed. Returning `404` for an inaccessible record is preferred where the caller should not learn whether another tenant's record exists; authorization decisions that do not disclose a record may return `403`.

## Fiscal data ownership

| Entity | Classification | Boundary |
| --- | --- | --- |
| `EmpresaFiscalConfig` | TENANT-SCOPED | Owned directly by `EmpresaId`; update and end-validity lookup use both configuration ID and tenant ID. |
| `ProdutoFiscalConfig` | TENANT-SCOPED (indirect) | References `ProdutoId`; the product must exist and have the requesting `EmpresaId`. |
| `OperacaoFiscalConfig` | GLOBAL | Shared rule keyed by operation type and validity window; no company owner exists in this model. |
| `NFeDocumento` | TENANT-SCOPED (indirect) | `VendaId` owns the document. Reads/mutations resolve the sale and constrain its `EmpresaId`. |
| `NFeItem` | TENANT-SCOPED (inherited) | Child of `NFeDocumento`; access only through a tenant-scoped parent document. |
| `NFeEvento` | TENANT-SCOPED (inherited) | Child of `NFeDocumento`; access only through a tenant-scoped parent document. |

## Verification

`TenantFiscalIsolationTests` proves cross-tenant updates/validity closure are rejected, product fiscal configuration is not returned for another tenant, and NFe consulta/cancelamento/emissão operations reject a foreign tenant. These tests use EF Core InMemory. PostgreSQL-specific query behavior and broader HTTP authorization paths remain unverified until the Testcontainers suite runs in CI.
