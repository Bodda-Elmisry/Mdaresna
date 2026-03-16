using Mdaresna.Repository.IRepositories.AdminManagement.Query;
using Mdaresna.Repository.IRepositories.CoinsManagement.Query;
using Mdaresna.Repository.IRepositories.IdentityManagement.Query;
using Mdaresna.Repository.IRepositories.SchoolManagement.ClassRoomManagement.Command;
using Mdaresna.Repository.IRepositories.SchoolManagement.ClassRoomManagement.Query;
using Mdaresna.Repository.IRepositories.SchoolManagement.SchoolManagement.Query;
using Mdaresna.Repository.IRepositories.SchoolManagement.StudentManagement.Query;
using Mdaresna.Repository.IRepositories.SettingsManagement.Query;
using Mdaresna.Repository.IRepositories.UserManagement.Query;

namespace Mdaresna.Repository.IUnitOfWork
{
    public interface IQueryUnitOfWork
    {
        ILanguageQueryRepository LanguageQueryRepository { get; }

        ICoinTypeQueryRepository CoinTypeQueryRepository { get; }
        IPaymentTransactionQueryRepository PaymentTransactionQueryRepository { get; }
        IPaymentTypeQueryRepository PaymentTypeQueryRepository { get; }
        ISchoolPaymentRequestQueryRepository SchoolPaymentRequestQueryRepository { get; }

        IPermissionQueryRepository PermissionQueryRepository { get; }
        IRolePermissionQueryRepository RolePermissionQueryRepository { get; }
        IRoleQueryRepository RoleQueryRepository { get; }
        IUserPermissionQueryRepository UserPermissionQueryRepository { get; }
        IUserPermissionSchoolClassRoomQueryRepository UserPermissionSchoolClassRoomQueryRepository { get; }
        IUserRoleQueryRepository UserRoleQueryRepository { get; }

        ISMSProviderQueryRepository SMSProviderQueryRepository { get; }
        IEmailProviderQueryRepository EmailProviderQueryRepository { get; }

        IClassRoomStudentActivityQueryRepository ClassRoomStudentActivityQueryRepository { get; }
        IClassRoomStudentAssignmentQueryRepository ClassRoomStudentAssignmentQueryRepository { get; }
        IClassRoomStudentExamQueryRepository ClassRoomStudentExamQueryRepository { get; }
        IStudentAttendanceQueryRepository StudentAttendanceQueryRepository { get; }
        IStudentExamRateQueryRepository StudentExamRateQueryRepository { get; }
        IStudentNoteQueryRepository StudentNoteQueryRepository { get; }
        IStudentParentQueryRepository StudentParentQueryRepository { get; }
        IStudentQueryRepository StudentQueryRepository { get; }

        IClassRoomLanguageQueryRepository ClassRoomLanguageQueryRepository { get; }
        ISchoolContactQueryRepository SchoolContactQueryRepository { get; }
        ISchoolContactTypeQueryRepository SchoolContactTypeQueryRepository { get; }
        ISchoolCourseQueryRepository SchoolCourseQueryRepository { get; }
        ISchoolExamRateHeaderQueryRepository SchoolExamRateHeaderQueryRepository { get; }
        ISchoolGradeQueryRepository SchoolGradeQueryRepository { get; }
        ISchoolPostImageQueryRepository SchoolPostImageQueryRepository { get; }
        ISchoolPostQueryRepository SchoolPostQueryRepository { get; }
        ISchoolPostReportQueryRepository SchoolPostReportQueryRepository { get; }
        ISchoolQueryRepository SchoolQueryRepository { get; }
        ISchoolEmployeeQueryRepository SchoolEmployeeQueryRepository { get; }
        ISchoolTeacherQueryRepository SchoolTeacherQueryRepository { get; }
        ISchoolTeacherCourseQueryRepository SchoolTeacherCourseQueryRepository { get; }
        ISchoolTypeQueryRepository SchoolTypeQueryRepository { get; }
        ISchoolYearMonthQueryRepository SchoolYearMonthQueryRepository { get; }
        ISchoolYearQueryRepository SchoolYearQueryRepository { get; }

        IClassRoomActivityQueryRepository ClassRoomActivityQueryRepository { get; }
        IClassRoomAssignmentQueryRepository ClassRoomAssignmentQueryRepository { get; }
        IClassRoomExamQueryRepository ClassRoomExamQueryRepository { get; }
        IClassRoomQueryRepository ClassRoomQueryRepository { get; }
        IClassroomEmployeeQueryRepository ClassroomEmployeeQueryRepository { get; }
        IClassRoomTeacherCourseQueryRepository ClassRoomTeacherCourseQueryRepository { get; }
        IClassRoomHelpDataQueryRepository ClassRoomHelpDataQueryRepository { get; }

        IRelationTypeQueryRepository RelationTypeQueryRepository { get; }
        ISchoolUserQueryRepository SchoolUserQueryRepository { get; }
        IUserQueryRepository UserQueryRepository { get; }
        IUserDeviceQueryRepository UserDeviceQueryRepository { get; }
    }
}
