using Mdaresna.Doamin.MainDB.Enums;
using Mdaresna.Doamin.Models.Base;

namespace Mdaresna.Doamin.MainDB.Models;

public class MdaresnaService : BaseModel
{
    public DBTypeEnum DBType { get; set; }
    public string DBSource { get; set; } = string.Empty;
    public string? DBPort { get; set; }
    public string DBUser { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
