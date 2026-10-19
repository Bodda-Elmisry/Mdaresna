using Mdaresna.Platform.Domain.Billing.Subscriptions;
using Mdaresna.Platform.Domain.Access;

namespace Mdaresna.Platform.Application.Billing.Subscriptions;

public sealed record CatalogActor(IdentityAccountId AccountId, Guid CorrelationId);
public sealed record SystemCatalogItem(Guid Id, string Code, string NameAr, string NameEn, CapacityMetric CapacityMetric,
    string[] Capabilities, bool IsActive, long Version);
public sealed record PlanOffer(int Revision, int Capacity, decimal BaseUnitPrice, decimal AddOnUnitPrice, string Currency, decimal BaseAmount);
public sealed record SystemPlanItem(Guid Id, Guid SystemId, string Code, string NameAr, string NameEn, int SortOrder,
    bool IsArchived, long Version, int? PublishedRevision, PlanOffer[] Offers);
public sealed record SystemWrite(string NameAr, string NameEn, string[] Capabilities, bool IsActive, long ExpectedVersion);
public sealed record PlanOfferWrite(int Capacity, decimal BaseUnitPrice, decimal AddOnUnitPrice, string Currency);
public sealed record PlanCreate(Guid Id, string Code, string NameAr, string NameEn, int SortOrder, long ExpectedSystemVersion, PlanOfferWrite Offer);
public sealed record PlanWrite(string NameAr, string NameEn, long ExpectedVersion, long ExpectedSystemVersion);

public interface ISystemCatalogService
{
    Task<SystemCatalogItem[]> ListSystemsAsync(CancellationToken ct);
    Task<SystemCatalogItem> CreateSystemAsync(Guid id, string code, CapacityMetric metric, SystemWrite input, CatalogActor actor, CancellationToken ct);
    Task<SystemCatalogItem> UpdateSystemAsync(Guid id, SystemWrite input, CatalogActor actor, CancellationToken ct);
    Task<SystemPlanItem[]> ListPlansAsync(Guid systemId, CancellationToken ct);
    Task<SystemPlanItem> CreatePlanAsync(Guid systemId, PlanCreate input, CatalogActor actor, CancellationToken ct);
    Task<SystemPlanItem> UpdatePlanAsync(Guid id, PlanWrite input, CatalogActor actor, CancellationToken ct);
    Task<SystemPlanItem> AddRevisionAsync(Guid id, long expectedVersion, long expectedSystemVersion, PlanOfferWrite input, CatalogActor actor, CancellationToken ct);
    Task<SystemPlanItem> PublishAsync(Guid id, int revision, long expectedVersion, long expectedSystemVersion, CatalogActor actor, CancellationToken ct);
    Task<SystemPlanItem> ArchiveAsync(Guid id, long expectedVersion, long expectedSystemVersion, CatalogActor actor, CancellationToken ct);
    Task ReorderAsync(Guid systemId, long expectedVersion, Guid[] orderedPlanIds, CatalogActor actor, CancellationToken ct);
    Task<SystemPlanItem[]> NextPlansAsync(Guid planId, CancellationToken ct);
}
