using Mdaresna.Doamin.MainDB.Enums;

namespace Mdaresna.Doamin.MainDB.Models;

public class MdaresnaSchoolService
{
    public Guid SchoolId { get; set; }
    public MdaresnaSchool School { get; set; } = new();
    public Guid ServiceId { get; set; }
    public MdaresnaService Service { get; set; } = new();
    public DBTypeEnum DBType { get; set; }
    public string DBSource { get; set; } = string.Empty;
    public string? DBPort { get; set; }
    public string DBUser { get; set; } = string.Empty;
    public string DBPassword { get; set; } = string.Empty;
    public string DBCatlog { get; set; } = string.Empty;
}
