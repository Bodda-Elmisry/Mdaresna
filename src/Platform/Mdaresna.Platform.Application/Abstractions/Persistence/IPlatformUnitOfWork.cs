namespace Mdaresna.Platform.Application.Abstractions.Persistence;

public interface IPlatformUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
