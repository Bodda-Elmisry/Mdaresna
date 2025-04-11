namespace Mdaresna.DTOs.SchoolManagementDTO.ClassRoomManagementDTO
{
    public record class GetClassroomsEmployeesDTO(
        Guid? EmployeeId,
        Guid? ClassRoomId,
        Guid SchoolId);
}
