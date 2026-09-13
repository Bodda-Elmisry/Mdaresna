using Mdaresna.Platform.Infrastructure.Persistence.Identity;
using Mdaresna.Platform.Infrastructure.Persistence.Platform;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Platform.Infrastructure.Persistence;

/// <summary>
/// Distinct context types keep PostgreSQL migrations separate from the existing
/// SQL Server migration history and model snapshots.
/// </summary>
public sealed class PostgreSqlPlatformDbContext(
    DbContextOptions<PostgreSqlPlatformDbContext> options) : PlatformDbContext(options);

public sealed class PostgreSqlIdentityDbContext(
    DbContextOptions<PostgreSqlIdentityDbContext> options) : IdentityDbContext(options);
