using Mdaresna.Platform.Application.Abstractions.Persistence;
using Mdaresna.Platform.Application.Errors;
using Mdaresna.Platform.Domain.Access;
using Mdaresna.Platform.Domain.Registry;
using Mdaresna.Platform.Infrastructure.Persistence.Platform.Entities;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Platform.Infrastructure.Persistence.Platform.Repositories;

internal sealed class PlatformUnitOfWork(PlatformDbContext dbContext) : IPlatformUnitOfWork
{
    public async Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);
        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await dbContext.Database
                .BeginTransactionAsync(cancellationToken);
            await operation(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        });
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await SynchronizeRolePermissionsAsync(cancellationToken);
        try
        {
            return await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (
            exception.Entries.Any(entry =>
                entry.Entity is Tenant or SchoolRegistration or SchoolDatabaseEndpoint) &&
            DatabaseErrorClassifier.IsUniqueViolation(exception.InnerException))
        {
            throw new PlatformConflictException(
                "registry.concurrent_conflict",
                "Tenant, school registration, or database endpoint changed concurrently; reload and retry.");
        }
    }

    private async Task SynchronizeRolePermissionsAsync(CancellationToken cancellationToken)
    {
        var changedRoles = dbContext.ChangeTracker
            .Entries<PlatformRole>()
            .Where(entry => entry.State is EntityState.Added or EntityState.Modified)
            .Select(entry => entry.Entity)
            .ToArray();

        foreach (var role in changedRoles)
        {
            await dbContext.RolePermissions
                .Where(x => x.RoleId == role.Id)
                .LoadAsync(cancellationToken);

            var current = dbContext.RolePermissions.Local
                .Where(x => x.RoleId == role.Id)
                .ToArray();
            var desired = role.Permissions.ToHashSet();

            foreach (var removed in current.Where(x => !desired.Contains(x.PermissionCode)))
            {
                dbContext.RolePermissions.Remove(removed);
            }

            var currentCodes = current.Select(x => x.PermissionCode).ToHashSet();
            foreach (var added in desired.Where(code => !currentCodes.Contains(code)))
            {
                dbContext.RolePermissions.Add(new PlatformRolePermissionRecord
                {
                    RoleId = role.Id,
                    PermissionCode = added
                });
            }
        }
    }
}
