# Mdaresna Technical Foundation

Status: Initial foundation accepted for incremental migration.

## System boundaries

- Platform owns the product control plane, global identity, subscriptions, entitlements and platform operations.
- School owns operational and official school data, including branches, memberships and academic records.
- Family owns the family experience, family-specific writes and projections received from Platform and School.
- The existing API remains the compatibility host until each route is migrated and verified.

## Dependency direction

```text
SharedKernel                 (no Mdaresna project dependencies)
Tenancy.Abstractions         (no Mdaresna project dependencies)
IntegrationContracts        -> Tenancy.Abstractions
Messaging.Abstractions      -> IntegrationContracts
```

Foundation projects must not reference the legacy Domain, Repository or Infrastructure projects. They must also remain independent of ASP.NET Core, Entity Framework Core and RabbitMQ.

## Identifier semantics

- `TenantId` is the customer, isolation and deployment boundary.
- `SchoolId` is an academic institution inside a tenant.
- `BranchId` is a physical or operational branch inside a school.
- During migration only, a legacy school's `TenantId` may have the same GUID value as its `SchoolId`. This is a mapping rule, not a permanent identity rule.
- Existing legacy entities and DTOs keep their `Guid` properties until they are migrated behind compatibility adapters.
- Strong IDs convert to and from `Guid` explicitly; compatibility adapters must use `.Value` deliberately.

## Integration rules

- Integration events and directed commands are immutable and explicitly versioned.
- Every payload declares its own stable message type and schema version; the envelope validates both.
- The canonical integration JSON serializer and golden fixtures define casing, null handling and stable wire names.
- Messages carry correlation and causation identifiers and optional tenant, school and branch scope.
- Aggregate metadata is optional because global control messages and synchronization handshakes may not represent one aggregate.
- Broker implementations belong outside the contracts and abstractions projects.
- A future Outbox implementation must persist the message in the same database transaction as its business change.
- There is one write owner per aggregate; projections are not additional write owners.

## Migration safety

- Keep existing routes and response contracts until a deliberate API version is introduced.
- Use expand-migrate-contract database changes.
- Switch routes per tenant behind feature flags.
- Shadow reads are allowed; dual writes are not.
- Do not add RabbitMQ, new databases or new API hosts until the foundation contracts and ownership boundaries are covered by tests.

## Security gates before the first consumer

- A message scope is routing and audit metadata, not proof of authorization.
- Every consumer must resolve and verify that the school belongs to the tenant and the branch belongs to the school before a write.
- Tenant context must come from a validated account, membership or connector identity; client headers and payload fields only select a candidate context.
- Runtime secrets must move out of tracked configuration before the new hosts are deployed.
- Sensitive SQL logging and credential, token or PII logging must be removed before production traffic is routed to new components.

## Toolchain

- `global.json` pins the repository to .NET SDK `9.0.317`, which is supported by the installed Visual Studio 2022, while current production projects continue to target `net8.0`.
- Developer workstations and CI must install that SDK before building the repository.
