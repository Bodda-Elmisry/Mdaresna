using Mdaresna.Schools.Domain.Documents;

namespace Mdaresna.Schools.Domain.School;

public sealed class SchoolPresentation
{
    public Guid SchoolInformationId { get; set; }
    public string TaglineAr { get; set; } = "";
    public string TaglineEn { get; set; } = "";
    public string AboutAr { get; set; } = "";
    public string AboutEn { get; set; } = "";
    public string VisionAr { get; set; } = "";
    public string VisionEn { get; set; } = "";
    public string MissionAr { get; set; } = "";
    public string MissionEn { get; set; } = "";
    public string GoalsJson { get; set; } = "[]";
    public string? Email { get; set; }
    public string? Website { get; set; }
    public string? ContactAddress { get; set; }
    public string? ContactPhone { get; set; }
    public Guid? CoverImageId { get; set; }
    public decimal CoverPosition { get; set; } = 50;
    public int Revision { get; set; } = 1;
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public SchoolInformation SchoolInformation { get; set; } = null!;
}

public sealed class SchoolProfileImage
{
    public Guid Id { get; set; }
    public Guid SchoolInformationId { get; set; }
    public Guid DocumentId { get; set; }
    public bool IsLogo { get; set; }
    public string CaptionAr { get; set; } = "";
    public string CaptionEn { get; set; } = "";
    public int SortOrder { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public SchoolInformation SchoolInformation { get; set; } = null!;
    public SchoolDocument Document { get; set; } = null!;
}
