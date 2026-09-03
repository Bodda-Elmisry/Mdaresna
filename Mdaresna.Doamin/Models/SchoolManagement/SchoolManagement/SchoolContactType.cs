using Mdaresna.Doamin.Models.Base;

using Mdaresna.Doamin.Enums;

namespace Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement
{
    public class SchoolContactType : BaseModel
    {
        public string? Description { get; set; }

        public string? IconUrl { get; set; }

        public ContactActionType ActionType { get; set; } = ContactActionType.Text;
    }
}
