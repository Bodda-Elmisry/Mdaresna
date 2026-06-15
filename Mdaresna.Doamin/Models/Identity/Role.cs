using Mdaresna.Doamin.Models.Base;

namespace Mdaresna.Doamin.Models.Identity
{
    public class Role : BaseModel
    {

        public string? Description { get; set; }
        
        public bool Active { get; set; }
        
        public bool SchoolRole { get; set; }

        public bool AdminRole { get; set; }
    }
}
