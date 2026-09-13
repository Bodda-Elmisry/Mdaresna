# Platform PostgreSQL path

The Platform API, Worker, and bootstrap tool can use PostgreSQL 17 through
`PlatformDatabase:Provider=PostgreSql`. Platform and Identity remain **separate
databases**. The API and Worker `Development` configurations now select
PostgreSQL, using `mdaresna_platform_dev` and `mdaresna_identity_dev`. The
provider fallback outside that configuration is still SQL Server. No SQL
Server data is copied or deleted by this change.

## Configure an environment

On this development machine, complete connection strings are stored in each
entry project's .NET User Secrets store (`Mdaresna.Platform.Api` and
`Mdaresna.Platform.Worker`). The tracked `appsettings.Development.json` files
contain only non-secret host, database, and username values. On another machine,
configure equivalent User Secrets or environment variables. For CLI migration
and bootstrap commands, supply these environment variables or equivalent
deployment secrets:

```powershell
$env:PlatformDatabase__Provider = 'PostgreSql'
$env:ConnectionStrings__PlatformConnection = 'Host=localhost;Port=5432;Database=MdaresnaPlatformDev;Username=postgres;Password=<secret>'
$env:ConnectionStrings__IdentityConnection = 'Host=localhost;Port=5432;Database=MdaresnaIdentityDev;Username=postgres;Password=<secret>'
```

Use different database names. Do not put passwords into tracked appsettings,
command history, or migration files. The API still needs its existing
`PlatformAuth`, `PlatformActivation`, and `PlatformSms` secrets.

Create the empty databases explicitly, then apply both provider-specific
migration chains:

```powershell
$env:MDARESNA_PLATFORM_POSTGRES_DESIGNTIME_CONNECTION = $env:ConnectionStrings__PlatformConnection
$env:MDARESNA_IDENTITY_POSTGRES_DESIGNTIME_CONNECTION = $env:ConnectionStrings__IdentityConnection

dotnet tool run dotnet-ef database update --context PostgreSqlPlatformDbContext --project src/Platform/Mdaresna.Platform.Infrastructure/Mdaresna.Platform.Infrastructure.csproj --startup-project src/Platform/Mdaresna.Platform.Infrastructure/Mdaresna.Platform.Infrastructure.csproj
dotnet tool run dotnet-ef database update --context PostgreSqlIdentityDbContext --project src/Platform/Mdaresna.Platform.Infrastructure/Mdaresna.Platform.Infrastructure.csproj --startup-project src/Platform/Mdaresna.Platform.Infrastructure/Mdaresna.Platform.Infrastructure.csproj
```

Run those commands from the repository root. The PostgreSQL migrations are
stored under `Persistence/PostgreSql/Migrations`, separately from the SQL Server
migrations, and each database has its own migration history. Apply future model
changes to both provider chains while both providers are supported.

## Behaviour and verification

- Email identifiers, school codes, role keys, and unit codes are normalized
  before equality lookups. Free-text tenant, school, and unit-type searches use
  PostgreSQL `ILIKE` with escaped literal wildcard characters; the SQL Server
  path keeps its existing `Contains` queries.
- PostgreSQL timestamps are `timestamptz`; application writes must use UTC.
  The database cannot retain an original non-UTC offset after conversion to
  `timestamptz`, so the old SQL Server offset check becomes a provider-level
  UTC write requirement.
- PostgreSQL triggers generate 16-byte concurrency tokens for mapped
  `RowVersion` fields. Billing triggers enforce append-only records and the
  approved/pending payment invariants.
- Verify `/health/ready` after migrations and before enabling traffic. Perform
  bootstrap only on the intended new databases with a stable operation ID.
  Do not send an SMS merely to test the database cutover.
- The opt-in `PostgreSqlIntegrationTests` writes inside a rolled-back
  transaction. Set `MDARESNA_POSTGRES_TEST_PLATFORM` to a local, migrated
  database whose name begins `mdaresna_pg_verify_`; the test refuses other
  targets.

## Existing SQL Server data

This is a provider conversion, **not a data migration**. Before a real cutover,
choose whether the PostgreSQL environment starts empty or needs a separately
planned and reconciled copy of current SQL Server data. For a data copy, preserve
IDs, normalize timestamps and codes, verify counts and references across both
databases, and rehearse rollback on a non-production snapshot before switching
the provider setting. Do not run the two providers against the same logical
tenant simultaneously during a cutover.
