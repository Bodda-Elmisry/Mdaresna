# Platform business — first slice

The Platform app is the Mdaresna product control plane, not a school-management tenant. Its data stays in `PlatformDbContext` and the central accounts stay in `IdentityDbContext`; no database-to-database joins or writes are used. This slice does not replace any legacy `ApplicationManager` endpoint yet.

## Implemented

| Area | Platform API | Permission | Notes |
| --- | --- | --- | --- |
| Operator login | `POST /api/platform/v1/auth/login` | anonymous, IP rate-limited | Verified central email or phone identifies the person; the password is checked in Platform-local credentials. Active central person, local user and Platform role are required. JWT expires within 15 minutes. Enabled MFA fails closed until a challenge is built. |
| Tenant registry | `GET/POST /api/platform/v1/tenants`, `GET /api/platform/v1/tenants/{id}` | `platform.schools.read` / `platform.schools.manage` | One tenant represents one school; branches are owned by the future School app. |
| School registry | `GET /api/platform/v1/schools`, tenant-scoped list/detail, tenant-scoped registration | `platform.schools.read` / `platform.schools.manage` | School code, type, deployment mode and lifecycle are Platform metadata. `SchoolId` and `TenantId` have the same GUID value, while retaining distinct types/contracts. |
| School lifecycle | tenant-scoped `submit-for-verification`, `approve`, `suspend`, `reinstate`, `close`, `provisioning` | `platform.schools.manage`, `platform.schools.activate`, `platform.deployments.manage` | Lifecycle transitions require expected version; audit and outbox are saved with the school state. There is no direct Activate API. Provisioning returns `503` unless explicitly enabled after a functioning publisher and result consumer are deployed. |
| Platform staff | `GET /api/platform/v1/staff`, `GET /staff/roles`, `POST /staff/{accountId}/roles`, `DELETE /staff/roles/{assignmentId}` | `platform.access.manage` | Directory and role catalog; assignments require an existing active central account with verified global email or phone. Role changes and audit share one Platform DB commit. Self-assignment/revocation and privileged-role revocation are blocked. |
| School-to-platform payments | `GET/POST /api/platform/v1/payments`, `GET /payments/{requestId}`, `POST /payments/{requestId}/review` | `platform.billing.read`, `platform.billing.manage`, `platform.payments.approve` | Platform back-office submission, read and review only. Unique transfer reference per school, immutable review, no self-review, audit, approved ledger row and outbox event. Approval is an operator-reviewed transfer record, not a payment-provider settlement. |

The authenticated account ID is always taken from the validated token. All new API responses use the shared `ApiResponse<T>` / `PagedApiResponse<T>` envelope and correlation ID. `401`/`403` are never turned into a success envelope. Business endpoints require explicit permissions, and a fallback policy requires authentication for any future endpoint without an explicit attribute. The public root, health endpoints and development-only Swagger are diagnostic, not business access.

## Financial boundary

These payments are between a **school and Mdaresna Platform**. They are unrelated to student fees. Generic payment review does not add units or touch the School database. Unit-purchase requests now have a separate price snapshot and produce a unique `UnitGrant` and `SchoolUnitsGrantedV1` outbox event on approval; see [Platform unit commerce](platform-unit-commerce.md). A school's own view and submission endpoint must be built in the School app with school-scoped identity; the current Platform submission endpoints are limited to Platform operators. Outbox events are not yet delivered to RabbitMQ.
The request currently accepts a three-letter currency code with two decimal places. Define supported currencies and minor-unit rules before enabling multi-currency billing.

## Deliberately still pending

- General employee invitations/verification, role-definition administration, owner recovery and safe privileged-role revocation; no public registration or seeded default admin exists. The first owner can be created with the one-time [local bootstrap tool](platform-first-owner-bootstrap.md), which is not a general user-management API.
- MFA challenge, refresh token/session management, DB-backed HTTP integration tests and production proxy/rate-limit policy.
- Outbox publisher, school provisioning worker/result consumer, and deployment-specific activation (including government on-premises). `PlatformFeatures:SchoolProvisioningEnabled` defaults to `false`; do not turn it on until that flow exists. A school cannot become `Active` through the current API.
- School-scoped payment submission and synchronized school-side finance read model. No direct database sharing is intended.
- Legacy `ApplicationManager` scope not yet ported: reports/moderation, legal policies, SMS logs, support complaints/suggestions, payment-method catalog, and legacy coin balance/activation behavior. The new Platform unit-type catalog and grant event do not directly replace the School-owned balance or student consumption logic. Each remaining area needs its own ownership and authorization design; do not proxy the legacy routes blindly.
- Actor-attributed, transactionally audited updates for tenant/school names and deployment mode before exposing those operations.

## Local setup and verification

Set `PlatformAuth__SigningKey` outside source control to a secret of at least 32 UTF-8 bytes. Issuer and audience are in `appsettings.json`; connection strings in `appsettings.Development.json` target LocalDB. Apply only reviewed migrations to the intended database:

```powershell
dotnet tool restore
dotnet ef migrations list --context PlatformDbContext --project src/Platform/Mdaresna.Platform.Infrastructure
dotnet ef database update --context PlatformDbContext --project src/Platform/Mdaresna.Platform.Infrastructure
dotnet ef database update --context IdentityDbContext --project src/Platform/Mdaresna.Platform.Infrastructure
dotnet test Mdaresna.Platform.slnf -c Release
```

`20260913092435_SchoolPlatformBilling` adds the `billing` schema and two tables. `20260913094009_EnforceSchoolTenantAndPaymentLedgerIntegrity` enforces one school/tenant and approved, matching, append-only financial postings. Both were applied to the developer LocalDB only in this slice; SQL metadata checks confirmed the school constraint/index and three enabled billing triggers. Before applying the second migration to an environment with existing schools or postings, inspect its preflight checks; it fails safely rather than rewriting mismatched IDs or financial rows. No production database was changed.
