namespace Mdaresna.DTOs.IdentityDTO
{
    public class UpdateRoleDTO
    {
        public Guid RoleId { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
    }
}
