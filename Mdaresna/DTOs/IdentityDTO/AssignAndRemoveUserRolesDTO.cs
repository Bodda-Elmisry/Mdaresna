using Mdaresna.Doamin.DTOs.Identity;

namespace Mdaresna.DTOs.IdentityDTO
{
    public class AssignAndRemoveUserRolesDTO
    {
        public IEnumerable<UserRoleDTO> UserRoles { get; set; }
    }
}
