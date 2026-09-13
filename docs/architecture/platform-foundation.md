# Platform Foundation

Status: foundation and first authenticated business slice implemented. No legacy API route is switched to it. The Platform Worker has an opt-in SMS-audit RabbitMQ consumer, but no publisher or Schools/Family producer is live yet. See [Platform business first slice](platform-business-first-slice.md) for the API scope and remaining gates.

## Responsibility

The Platform application is the product control plane. This slice establishes:

- the global tenant and school registry;
- the school's deployment mode and provisioning lifecycle;
- centralized identity persistence;
- Platform-only roles and permissions;
- audit, feature-flag, inbox and transactional-outbox storage;
- independent API and background-worker hosts.
- a shared `Mdaresna.Api.Contracts` response model for this and future Schools/Family HTTP APIs.

Branches, staff/student memberships, academic data and school operations remain owned by the future School application. The Platform registry stores only the school information needed to identify, activate and provision a school.
One school is one tenant: `SchoolId` and `TenantId` have the same GUID value, with branches beneath the school in the future School app.

## Project boundaries

```text
Mdaresna.Platform.Api -----------+
                                  +--> Mdaresna.Platform.Infrastructure
Mdaresna.Platform.Worker --------+                 |
                                                   v
Mdaresna.Platform.Application --> Mdaresna.Platform.Domain
              |                            |
              v                            v
Mdaresna.Platform.Contracts          shared building blocks
```

- `Domain` owns tenant, school-registration and Platform-access rules.
- `Contracts` owns versioned integration messages.
- `Application` owns use cases and persistence abstractions.
- `Infrastructure` owns EF Core, SQL Server and outbox persistence.
- `Api` is the authenticated control-plane HTTP host for the first business slice.
- `Worker` can consume cross-system SMS-audit events when explicitly enabled; outbox publishing and other message consumers remain future work.

## Databases

The hosts use two independent EF Core contexts and two independent databases:

| Context | Development database | Schemas | Ownership |
| --- | --- | --- | --- |
| `PlatformDbContext` | `MdaresnaPlatformLocal` | `registry`, `access`, `operations`, `messaging`, `billing` | tenants, schools, Platform RBAC, school-to-platform payment requests/ledger, audit and integration reliability |
| `IdentityDbContext` | `MdaresnaIdentityLocal` | `identity`, `messaging` | accounts, login identifiers, password hashes, sessions, MFA metadata and security events |

There are intentionally no database-to-database foreign keys. Platform role assignments retain the global account identifier, while account existence and authentication remain owned by Identity.

Email and phone identifiers are global. School usernames and student codes are scoped by `SchoolId`; database constraints prevent mixing these two rules. A person keeps one central account and can later receive multiple School/Family memberships, so being both a teacher and a parent does not require two accounts.

## Configuration

Runtime connection-string keys are:

- `ConnectionStrings:PlatformConnection`
- `ConnectionStrings:IdentityConnection`
- `PlatformAuth:Issuer`, `PlatformAuth:Audience`, `PlatformAuth:AccessTokenMinutes`
- `PlatformAuth:SigningKey` supplied only by environment/user-secrets (at least 32 UTF-8 bytes)

Development settings point to `(localdb)\MSSQLLocalDB`. Non-development settings are intentionally blank so a deployed host fails fast until secrets are supplied by the deployment environment or user-secrets provider.

Design-time migrations use the same LocalDB databases by default. They can be redirected explicitly with:

- `MDARESNA_PLATFORM_DESIGNTIME_CONNECTION`
- `MDARESNA_IDENTITY_DESIGNTIME_CONNECTION`

The CORS allowlist may be empty for native Flutter deployments; that means no browser origin is trusted. Flutter Web deployments must supply their exact HTTPS origin (the development file uses fixed ports).

## Local commands

Build and test only the new slice:

```powershell
dotnet tool restore
dotnet build Mdaresna.Platform.slnf -c Release
dotnet test Mdaresna.Platform.slnf -c Release --no-build
```

Apply the reviewed migrations:

```powershell
dotnet ef database update --context PlatformDbContext --project src/Platform/Mdaresna.Platform.Infrastructure
dotnet ef database update --context IdentityDbContext --project src/Platform/Mdaresna.Platform.Infrastructure
```

Run the hosts:

```powershell
dotnet run --project src/Platform/Mdaresna.Platform.Api
dotnet run --project src/Platform/Mdaresna.Platform.Worker
```

The API exposes `/health/live`, `/health/ready`, development-only Swagger and the first permission-protected business endpoints. The worker exposes loopback-only liveness and readiness endpoints. Its [SMS-audit consumer](platform-sms-event-logging.md) is disabled by default; RabbitMQ publishing and provisioning-result consumption are not active yet.

New business routes use the shared [`ApiResponse<T>` / `PagedApiResponse<T>` contract](api-response-contract.md). Platform maps HTTP failures to the same envelope and real HTTP status. The legacy API and Flutter parsing remain unchanged until their endpoints are migrated together.

## Release gates before production business traffic

- Provision the first operator with the reviewed one-time [local bootstrap tool](platform-first-owner-bootstrap.md); no public registration or default administrator exists.
- Complete verified employee invitations, MFA challenge and refresh-token lifecycle before general operator use.
- Keep actor IDs derived from trusted claims; never accept them from a Flutter request body.
- Enforce Platform permission policies around every use case; only protected business endpoints are currently exposed.
- Implement the outbox publisher and authenticated provisioning-result consumer before treating a school as Active.
- For every newly exposed mutation, write actor-attributed audit and any applicable outbox records in the same transaction; clear domain events only after a successful commit. In particular, `Tenant.Rename`, `SchoolRegistration.Rename` and `ChangeDeploymentMode` must not be exposed until their actor/event mapping is added.
- Translate concurrent unique-key races into stable idempotency/conflict responses.
- Configure trusted proxy addresses and the worker management bind for the real deployment environment.
- Give the database principals only the permissions each host needs; audit and security-event tables should be insert/read only.
