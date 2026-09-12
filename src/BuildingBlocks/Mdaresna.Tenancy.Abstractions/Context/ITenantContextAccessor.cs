namespace Mdaresna.Tenancy.Abstractions.Context;

public interface ITenantContextAccessor
{
    TenantContext? Current { get; }
}
