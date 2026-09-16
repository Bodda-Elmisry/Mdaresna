# Mdaresna Schools backend

This bounded context owns the operational backend used by school tenants.

## Dependency direction

- `Mdaresna.Schools.Domain`: school business rules and value objects.
- `Mdaresna.Schools.Contracts`: versioned messages and public contracts.
- `Mdaresna.Schools.Application`: use cases and ports; depends on Domain and Contracts.
- `Mdaresna.Schools.Infrastructure`: persistence and external adapters; depends on Application and Domain.
- `Mdaresna.Schools.Api`: HTTP host and composition root.

The Schools backend must not read the Platform database directly. Platform owns the school database
registry and provisioning workflow. A later adapter will resolve the authenticated school to its
database endpoint without introducing a static, shared school connection string.
