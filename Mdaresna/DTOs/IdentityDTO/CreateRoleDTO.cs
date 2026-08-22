namespace Mdaresna.DTOs.IdentityDTO
{
    public class CreateRoleDTO
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public bool Active { get; set; }
        public bool IsSchoolRole { get; set; }
        public Guid? SchoolId { get; set; }

        public IEnumerable<Guid> Permissions { get; set; }
    }
}
