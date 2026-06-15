using System.ComponentModel.DataAnnotations;

namespace Mdaresna.Doamin.Models.Base
{
    public class BaseModel : AuditBase
    {
        public Guid Id { get; set; }

        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;
    }
}
