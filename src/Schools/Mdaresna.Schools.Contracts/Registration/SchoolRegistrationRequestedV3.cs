using Mdaresna.IntegrationContracts.Messaging;
namespace Mdaresna.Schools.Contracts.Registration;

public sealed record RegistrationPlanSelection(Guid PlanId, int Revision, int AdditionalQuantity = 0);
public sealed record RegistrationPlanOffer(Guid SystemId, string SystemNameAr, string SystemNameEn, string CapacityMetric,
    Guid PlanId, string PlanNameAr, string PlanNameEn, int SortOrder, int Revision, int Capacity,
    decimal BaseUnitPrice, decimal AddOnUnitPrice, decimal Amount, string Currency);
public sealed record PrepareRegistrationOffer(SchoolRegistrationRequestedV2 Request, RegistrationPlanSelection[] Plans);
public sealed record SchoolRegistrationRequestedV3(SchoolRegistrationRequestedV2 Request, Guid OfferId) : IIntegrationEvent
{
    public static string MessageType => "mdaresna.schools.registry.school-registration-subscriptions-requested";
    public static ushort SchemaVersion => 3;
}
