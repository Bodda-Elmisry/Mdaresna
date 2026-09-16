using Mdaresna.Schools.Domain;

namespace Mdaresna.Schools.Application.Abstractions;

/// <summary>
/// Exposes the validated school scope for the current operation. Authentication will populate this
/// before any school-owned repository is resolved.
/// </summary>
public interface ISchoolScopeAccessor
{
    SchoolScope? Current { get; }
}
