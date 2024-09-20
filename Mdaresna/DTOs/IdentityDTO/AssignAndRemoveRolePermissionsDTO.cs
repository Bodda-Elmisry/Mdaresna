namespace Mdaresna.DTOs.IdentityDTO
{
    public class AssignAndRemoveRolePermissionsDTO
    {
        public Guid RoleId { get; set; }
        public IEnumerable<Guid> Permissions { get; set; }
    }
}
