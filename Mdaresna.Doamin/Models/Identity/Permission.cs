using Mdaresna.Doamin.Models.Base;
using System.ComponentModel.DataAnnotations;

namespace Mdaresna.Doamin.Models.Identity
{
    public class Permission : BaseModel
    {
        [MaxLength(200)]
        public string Name_AR { get; set; }
        [MaxLength(200)]
        public string? Key { get; set; }
        public string? Description { get; set; }
        public string? Description_AR { get; set; }
        public bool SchoolPermission { get; set; }
        public bool AppPermission { get; set; }
        public bool AllowedToMapToClassroom { get; set; }
        public bool AvailableForSchoolCustomRoles { get; set; } = true;
    }
}
