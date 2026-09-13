using Mdaresna.Platform.Application.Errors;
using Mdaresna.Platform.Domain.Access;

namespace Mdaresna.Platform.Application.Billing.Units;

internal static class UnitCommerceGuard
{
    public static void ValidateActorAndCorrelation(
        IdentityAccountId actorId,
        Guid correlationId)
    {
        if (actorId.IsEmpty || correlationId == Guid.Empty)
        {
            throw new ArgumentException("Actor and correlation identifiers are required.");
        }
    }

    public static void ValidateTypeAndVersion(Guid unitTypeId, long expectedVersion)
    {
        if (unitTypeId == Guid.Empty || expectedVersion < 0)
        {
            throw new ArgumentException("Unit type ID and expected version are invalid.");
        }
    }

    public static void EnsureVersion(long actualVersion, long expectedVersion)
    {
        if (actualVersion != expectedVersion)
        {
            throw new PlatformConflictException(
                "unit_type.version_conflict",
                "Unit type offer changed since it was last read.");
        }
    }
}
