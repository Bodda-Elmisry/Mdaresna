using System.Text.Json.Serialization;
using Mdaresna.Tenancy.Abstractions.Identifiers;
using Mdaresna.Tenancy.Abstractions.Serialization;

namespace Mdaresna.Platform.Domain.Access;

[JsonConverter(typeof(GuidIdJsonConverter<PlatformRoleAssignmentId>))]
public readonly record struct PlatformRoleAssignmentId : IGuidId<PlatformRoleAssignmentId>
{
    private PlatformRoleAssignmentId(Guid value) => Value = value;

    public Guid Value { get; }

    public bool IsEmpty => Value == Guid.Empty;

    public static PlatformRoleAssignmentId New() => new(Guid.NewGuid());

    public static PlatformRoleAssignmentId From(Guid value) => value == Guid.Empty
        ? throw new ArgumentException("PlatformRoleAssignmentId cannot be empty.", nameof(value))
        : new PlatformRoleAssignmentId(value);

    public static bool TryFrom(Guid value, out PlatformRoleAssignmentId assignmentId)
    {
        assignmentId = default;

        if (value == Guid.Empty)
        {
            return false;
        }

        assignmentId = new PlatformRoleAssignmentId(value);
        return true;
    }

    public static explicit operator Guid(PlatformRoleAssignmentId id) => id.Value;

    public static explicit operator PlatformRoleAssignmentId(Guid value) => From(value);

    public override string ToString() => Value.ToString("D");
}
