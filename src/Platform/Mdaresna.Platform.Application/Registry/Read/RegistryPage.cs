namespace Mdaresna.Platform.Application.Registry.Read;

public sealed record RegistryPage<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int PageNumber,
    int PageSize);
