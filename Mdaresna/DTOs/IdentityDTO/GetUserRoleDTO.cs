using Mdaresna.DTOs.Common;

namespace Mdaresna.DTOs.IdentityDTO
{
    public class GetUserRoleDTO : UserIdRoleIdDTO
    {
        public Guid? SchoolId { get; set; }
    }
}
