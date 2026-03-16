using Mdaresna.Repository.IRepositories.AdminManagement.Command;
using Mdaresna.Repository.IRepositories.CoinsManagement.Command;
using Mdaresna.Repository.IRepositories.IdentityManagement.Command;
using Mdaresna.Repository.IRepositories.SchoolManagement.ClassRoomManagement.Command;
using Mdaresna.Repository.IRepositories.SchoolManagement.SchoolManagement.Command;
using Mdaresna.Repository.IRepositories.SchoolManagement.StudentManagement.Command;
using Mdaresna.Repository.IRepositories.SettingsManagement.Command;
using Mdaresna.Repository.IRepositories.UserManagement.Command;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Mdaresna.Repository.IUnitOfWork
{
    public interface ICommandUnitOfWork : IAsyncDisposable
    {
        ILanguageCommandRepository LanguageCommandRepository { get; }

        ICoinTypeCommandRepository CoinTypeCommandRepository { get; }
        IPaymentTransactionCommandRepository PaymentTransactionCommandRepository { get; }
        IPaymentTypeCommandRepository PaymentTypeCommandRepository { get; }
        ISchoolPaymentRequestCommandRepository SchoolPaymentRequestCommandRepository { get; }

        IPermissionCommandRepository PermissionCommandRepository { get; }
        IRoleCommandRepository RoleCommandRepository { get; }
        IRolePermissionCommandRepository RolePermissionCommandRepository { get; }
        IUserPermissionCommandRepository UserPermissionCommandRepository { get; }
        IUserPermissionSchoolClassRoomCommandRepository UserPermissionSchoolClassRoomCommandRepository { get; }
        IUserRoleCommandRepository UserRoleCommandRepository { get; }

        IClassRoomLanguageCommandRepository ClassRoomLanguageCommandRepository { get; }
        ISchoolCommandRepository SchoolCommandRepository { get; }
        ISchoolContactCommandRepository SchoolContactCommandRepository { get; }
        ISchoolContactTypeCommandRepository SchoolContactTypeCommandRepository { get; }
        ISchoolCourseCommandRepository SchoolCourseCommandRepository { get; }
        ISchoolExamRateHeaderCommandRepository SchoolExamRateHeaderCommandRepository { get; }
        ISchoolGradeCommandRepository SchoolGradeCommandRepository { get; }
        ISchoolPostCommandRepository SchoolPostCommandRepository { get; }
        ISchoolPostImageCommandRepository SchoolPostImageCommandRepository { get; }
        ISchoolPostReportCommandRepository SchoolPostReportCommandRepository { get; }
        ISchoolEmployeeCommandRepository SchoolEmployeeCommandRepository { get; }
        ISchoolTeacherCommandRepository SchoolTeacherCommandRepository { get; }
        ISchoolTeacherCourseCommandRepository SchoolTeacherCourseCommandRepository { get; }
        ISchoolTypeCommandRepository SchoolTypeCommandRepository { get; }
        ISchoolYearCommandRepository SchoolYearCommandRepository { get; }
        ISchoolYearMonthCommandRepository SchoolYearMonthCommandRepository { get; }

        IClassRoomActivityCommandRepository ClassRoomActivityCommandRepository { get; }
        IClassRoomAssignmentCommandRepository ClassRoomAssignmentCommandRepository { get; }
        IClassRoomCommandRepository ClassRoomCommandRepository { get; }
        IClassRoomExamCommandRepository ClassRoomExamCommandRepository { get; }
        IClassroomEmployeeCommandRepository ClassroomEmployeeCommandRepository { get; }
        IClassRoomTeacherCourseCommandRepository ClassRoomTeacherCourseCommandRepository { get; }

        IClassRoomStudentActivityCommandRepository ClassRoomStudentActivityCommandRepository { get; }
        IClassRoomStudentAssignmentCommandRepository ClassRoomStudentAssignmentCommandRepository { get; }
        IClassRoomStudentExamCommandRepository ClassRoomStudentExamCommandRepository { get; }
        IStudentAttendanceCommandRepository StudentAttendanceCommandRepository { get; }
        IStudentCommandRepository StudentCommandRepository { get; }
        IStudentExamRateCommandRepository StudentExamRateCommandRepository { get; }
        IStudentNoteCommandRepository StudentNoteCommandRepository { get; }
        IStudentParentCommandRepository StudentParentCommandRepository { get; }

        IRelationTypeCommandRepository RelationTypeCommandRepository { get; }
        ISchoolUserCommandRepository SchoolUserCommandRepository { get; }
        IUserCommandRepository UserCommandRepository { get; }
        IUserDeviceCommandRepository UserDeviceCommandRepository { get; }

        ISMSProviderCommandRepository SMSProviderCommandRepository { get; }
        IEmailProviderCommandRepository EmailProviderCommandRepository { get; }
        ISMSLogCommandRepository SMSLogCommandRepository { get; }

        Task BeginTransactionAsync(CancellationToken cancellationToken = default);
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    }
}
