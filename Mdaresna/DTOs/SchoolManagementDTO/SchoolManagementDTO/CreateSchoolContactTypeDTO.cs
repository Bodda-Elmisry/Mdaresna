namespace Mdaresna.DTOs.SchoolManagementDTO.SchoolManagementDTO
{
    public class CreateSchoolContactTypeDTO
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public global::Mdaresna.Doamin.Enums.ContactActionType? ActionType { get; set; }
    }
}
