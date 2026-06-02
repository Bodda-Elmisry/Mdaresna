using Mdaresna.Doamin.MainDB.Enums;
using Mdaresna.Doamin.Models.Base;

namespace Mdaresna.Doamin.MainDB.Models;

public class MdaresnaSchool : BaseModel
{
    public DBTypeEnum DBType { get; set; }
    public string DBSource { get; set; } = string.Empty;
    public string? DBPort { get; set; }
    public string DBUser { get; set; } = string.Empty;
    public string DBPassword { get; set; } = string.Empty;
    public string DBCatlog { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
