namespace Mdaresna.DTOs.Common
{
    public record class GetSchoolEmployeesDTO(
        Guid SchoolId,
        string EmployeeName,
        string EmployeePhoneNumber,
        string EmployeeEmail
        );
}
