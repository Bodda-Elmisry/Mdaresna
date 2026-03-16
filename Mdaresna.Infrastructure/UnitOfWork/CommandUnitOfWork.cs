using Mdaresna.Infrastructure.Data;
using Mdaresna.Repository.IRepositories.AdminManagement.Command;
using Mdaresna.Repository.IRepositories.CoinsManagement.Command;
using Mdaresna.Repository.IRepositories.IdentityManagement.Command;
using Mdaresna.Repository.IRepositories.SchoolManagement.ClassRoomManagement.Command;
using Mdaresna.Repository.IRepositories.SchoolManagement.SchoolManagement.Command;
using Mdaresna.Repository.IRepositories.SchoolManagement.StudentManagement.Command;
using Mdaresna.Repository.IRepositories.SettingsManagement.Command;
using Mdaresna.Repository.IRepositories.UserManagement.Command;
using Mdaresna.Repository.IUnitOfWork;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.UnitOfWork
{
    public class CommandUnitOfWork : ICommandUnitOfWork
    {
        private readonly AppDbContext _context;
        private readonly IServiceProvider _serviceProvider;
        private IDbContextTransaction? _currentTransaction;
        private ILanguageCommandRepository? _languageCommandRepository;
        private ICoinTypeCommandRepository? _coinTypeCommandRepository;
        private IPaymentTransactionCommandRepository? _paymentTransactionCommandRepository;
        private IPaymentTypeCommandRepository? _paymentTypeCommandRepository;
        private ISchoolPaymentRequestCommandRepository? _schoolPaymentRequestCommandRepository;
        private IPermissionCommandRepository? _permissionCommandRepository;
        private IRoleCommandRepository? _roleCommandRepository;
        private IRolePermissionCommandRepository? _rolePermissionCommandRepository;
        private IUserPermissionCommandRepository? _userPermissionCommandRepository;
        private IUserPermissionSchoolClassRoomCommandRepository? _userPermissionSchoolClassRoomCommandRepository;
        private IUserRoleCommandRepository? _userRoleCommandRepository;
        private IClassRoomLanguageCommandRepository? _classRoomLanguageCommandRepository;
        private ISchoolCommandRepository? _schoolCommandRepository;
        private ISchoolContactCommandRepository? _schoolContactCommandRepository;
        private ISchoolContactTypeCommandRepository? _schoolContactTypeCommandRepository;
        private ISchoolCourseCommandRepository? _schoolCourseCommandRepository;
        private ISchoolExamRateHeaderCommandRepository? _schoolExamRateHeaderCommandRepository;
        private ISchoolGradeCommandRepository? _schoolGradeCommandRepository;
        private ISchoolPostCommandRepository? _schoolPostCommandRepository;
        private ISchoolPostImageCommandRepository? _schoolPostImageCommandRepository;
        private ISchoolPostReportCommandRepository? _schoolPostReportCommandRepository;
        private ISchoolEmployeeCommandRepository? _schoolEmployeeCommandRepository;
        private ISchoolTeacherCommandRepository? _schoolTeacherCommandRepository;
        private ISchoolTeacherCourseCommandRepository? _schoolTeacherCourseCommandRepository;
        private ISchoolTypeCommandRepository? _schoolTypeCommandRepository;
        private ISchoolYearCommandRepository? _schoolYearCommandRepository;
        private ISchoolYearMonthCommandRepository? _schoolYearMonthCommandRepository;
        private IClassRoomActivityCommandRepository? _classRoomActivityCommandRepository;
        private IClassRoomAssignmentCommandRepository? _classRoomAssignmentCommandRepository;
        private IClassRoomCommandRepository? _classRoomCommandRepository;
        private IClassRoomExamCommandRepository? _classRoomExamCommandRepository;
        private IClassroomEmployeeCommandRepository? _classroomEmployeeCommandRepository;
        private IClassRoomTeacherCourseCommandRepository? _classRoomTeacherCourseCommandRepository;
        private IClassRoomStudentActivityCommandRepository? _classRoomStudentActivityCommandRepository;
        private IClassRoomStudentAssignmentCommandRepository? _classRoomStudentAssignmentCommandRepository;
        private IClassRoomStudentExamCommandRepository? _classRoomStudentExamCommandRepository;
        private IStudentAttendanceCommandRepository? _studentAttendanceCommandRepository;
        private IStudentCommandRepository? _studentCommandRepository;
        private IStudentExamRateCommandRepository? _studentExamRateCommandRepository;
        private IStudentNoteCommandRepository? _studentNoteCommandRepository;
        private IStudentParentCommandRepository? _studentParentCommandRepository;
        private IRelationTypeCommandRepository? _relationTypeCommandRepository;
        private ISchoolUserCommandRepository? _schoolUserCommandRepository;
        private IUserCommandRepository? _userCommandRepository;
        private IUserDeviceCommandRepository? _userDeviceCommandRepository;
        private ISMSProviderCommandRepository? _smsProviderCommandRepository;
        private IEmailProviderCommandRepository? _emailProviderCommandRepository;
        private ISMSLogCommandRepository? _smsLogCommandRepository;

        public CommandUnitOfWork(AppDbContext context, IServiceProvider serviceProvider)
        {
            _context = context;
            _serviceProvider = serviceProvider;
        }

        public ILanguageCommandRepository LanguageCommandRepository => _languageCommandRepository ??=
            _serviceProvider.GetRequiredService<ILanguageCommandRepository>();

        public ICoinTypeCommandRepository CoinTypeCommandRepository => _coinTypeCommandRepository ??=
            _serviceProvider.GetRequiredService<ICoinTypeCommandRepository>();

        public IPaymentTransactionCommandRepository PaymentTransactionCommandRepository => _paymentTransactionCommandRepository ??=
            _serviceProvider.GetRequiredService<IPaymentTransactionCommandRepository>();

        public IPaymentTypeCommandRepository PaymentTypeCommandRepository => _paymentTypeCommandRepository ??=
            _serviceProvider.GetRequiredService<IPaymentTypeCommandRepository>();

        public ISchoolPaymentRequestCommandRepository SchoolPaymentRequestCommandRepository => _schoolPaymentRequestCommandRepository ??=
            _serviceProvider.GetRequiredService<ISchoolPaymentRequestCommandRepository>();

        public IPermissionCommandRepository PermissionCommandRepository => _permissionCommandRepository ??=
            _serviceProvider.GetRequiredService<IPermissionCommandRepository>();

        public IRoleCommandRepository RoleCommandRepository => _roleCommandRepository ??=
            _serviceProvider.GetRequiredService<IRoleCommandRepository>();

        public IRolePermissionCommandRepository RolePermissionCommandRepository => _rolePermissionCommandRepository ??=
            _serviceProvider.GetRequiredService<IRolePermissionCommandRepository>();

        public IUserPermissionCommandRepository UserPermissionCommandRepository => _userPermissionCommandRepository ??=
            _serviceProvider.GetRequiredService<IUserPermissionCommandRepository>();

        public IUserPermissionSchoolClassRoomCommandRepository UserPermissionSchoolClassRoomCommandRepository => _userPermissionSchoolClassRoomCommandRepository ??=
            _serviceProvider.GetRequiredService<IUserPermissionSchoolClassRoomCommandRepository>();

        public IUserRoleCommandRepository UserRoleCommandRepository => _userRoleCommandRepository ??=
            _serviceProvider.GetRequiredService<IUserRoleCommandRepository>();

        public IClassRoomLanguageCommandRepository ClassRoomLanguageCommandRepository => _classRoomLanguageCommandRepository ??=
            _serviceProvider.GetRequiredService<IClassRoomLanguageCommandRepository>();

        public ISchoolCommandRepository SchoolCommandRepository => _schoolCommandRepository ??=
            _serviceProvider.GetRequiredService<ISchoolCommandRepository>();

        public ISchoolContactCommandRepository SchoolContactCommandRepository => _schoolContactCommandRepository ??=
            _serviceProvider.GetRequiredService<ISchoolContactCommandRepository>();

        public ISchoolContactTypeCommandRepository SchoolContactTypeCommandRepository => _schoolContactTypeCommandRepository ??=
            _serviceProvider.GetRequiredService<ISchoolContactTypeCommandRepository>();

        public ISchoolCourseCommandRepository SchoolCourseCommandRepository => _schoolCourseCommandRepository ??=
            _serviceProvider.GetRequiredService<ISchoolCourseCommandRepository>();

        public ISchoolExamRateHeaderCommandRepository SchoolExamRateHeaderCommandRepository => _schoolExamRateHeaderCommandRepository ??=
            _serviceProvider.GetRequiredService<ISchoolExamRateHeaderCommandRepository>();

        public ISchoolGradeCommandRepository SchoolGradeCommandRepository => _schoolGradeCommandRepository ??=
            _serviceProvider.GetRequiredService<ISchoolGradeCommandRepository>();

        public ISchoolPostCommandRepository SchoolPostCommandRepository => _schoolPostCommandRepository ??=
            _serviceProvider.GetRequiredService<ISchoolPostCommandRepository>();

        public ISchoolPostImageCommandRepository SchoolPostImageCommandRepository => _schoolPostImageCommandRepository ??=
            _serviceProvider.GetRequiredService<ISchoolPostImageCommandRepository>();

        public ISchoolPostReportCommandRepository SchoolPostReportCommandRepository => _schoolPostReportCommandRepository ??=
            _serviceProvider.GetRequiredService<ISchoolPostReportCommandRepository>();

        public ISchoolEmployeeCommandRepository SchoolEmployeeCommandRepository => _schoolEmployeeCommandRepository ??=
            _serviceProvider.GetRequiredService<ISchoolEmployeeCommandRepository>();

        public ISchoolTeacherCommandRepository SchoolTeacherCommandRepository => _schoolTeacherCommandRepository ??=
            _serviceProvider.GetRequiredService<ISchoolTeacherCommandRepository>();

        public ISchoolTeacherCourseCommandRepository SchoolTeacherCourseCommandRepository => _schoolTeacherCourseCommandRepository ??=
            _serviceProvider.GetRequiredService<ISchoolTeacherCourseCommandRepository>();

        public ISchoolTypeCommandRepository SchoolTypeCommandRepository => _schoolTypeCommandRepository ??=
            _serviceProvider.GetRequiredService<ISchoolTypeCommandRepository>();

        public ISchoolYearCommandRepository SchoolYearCommandRepository => _schoolYearCommandRepository ??=
            _serviceProvider.GetRequiredService<ISchoolYearCommandRepository>();

        public ISchoolYearMonthCommandRepository SchoolYearMonthCommandRepository => _schoolYearMonthCommandRepository ??=
            _serviceProvider.GetRequiredService<ISchoolYearMonthCommandRepository>();

        public IClassRoomActivityCommandRepository ClassRoomActivityCommandRepository => _classRoomActivityCommandRepository ??=
            _serviceProvider.GetRequiredService<IClassRoomActivityCommandRepository>();

        public IClassRoomAssignmentCommandRepository ClassRoomAssignmentCommandRepository => _classRoomAssignmentCommandRepository ??=
            _serviceProvider.GetRequiredService<IClassRoomAssignmentCommandRepository>();

        public IClassRoomCommandRepository ClassRoomCommandRepository => _classRoomCommandRepository ??=
            _serviceProvider.GetRequiredService<IClassRoomCommandRepository>();

        public IClassRoomExamCommandRepository ClassRoomExamCommandRepository => _classRoomExamCommandRepository ??=
            _serviceProvider.GetRequiredService<IClassRoomExamCommandRepository>();

        public IClassroomEmployeeCommandRepository ClassroomEmployeeCommandRepository => _classroomEmployeeCommandRepository ??=
            _serviceProvider.GetRequiredService<IClassroomEmployeeCommandRepository>();

        public IClassRoomTeacherCourseCommandRepository ClassRoomTeacherCourseCommandRepository => _classRoomTeacherCourseCommandRepository ??=
            _serviceProvider.GetRequiredService<IClassRoomTeacherCourseCommandRepository>();

        public IClassRoomStudentActivityCommandRepository ClassRoomStudentActivityCommandRepository => _classRoomStudentActivityCommandRepository ??=
            _serviceProvider.GetRequiredService<IClassRoomStudentActivityCommandRepository>();

        public IClassRoomStudentAssignmentCommandRepository ClassRoomStudentAssignmentCommandRepository => _classRoomStudentAssignmentCommandRepository ??=
            _serviceProvider.GetRequiredService<IClassRoomStudentAssignmentCommandRepository>();

        public IClassRoomStudentExamCommandRepository ClassRoomStudentExamCommandRepository => _classRoomStudentExamCommandRepository ??=
            _serviceProvider.GetRequiredService<IClassRoomStudentExamCommandRepository>();

        public IStudentAttendanceCommandRepository StudentAttendanceCommandRepository => _studentAttendanceCommandRepository ??=
            _serviceProvider.GetRequiredService<IStudentAttendanceCommandRepository>();

        public IStudentCommandRepository StudentCommandRepository => _studentCommandRepository ??=
            _serviceProvider.GetRequiredService<IStudentCommandRepository>();

        public IStudentExamRateCommandRepository StudentExamRateCommandRepository => _studentExamRateCommandRepository ??=
            _serviceProvider.GetRequiredService<IStudentExamRateCommandRepository>();

        public IStudentNoteCommandRepository StudentNoteCommandRepository => _studentNoteCommandRepository ??=
            _serviceProvider.GetRequiredService<IStudentNoteCommandRepository>();

        public IStudentParentCommandRepository StudentParentCommandRepository => _studentParentCommandRepository ??=
            _serviceProvider.GetRequiredService<IStudentParentCommandRepository>();

        public IRelationTypeCommandRepository RelationTypeCommandRepository => _relationTypeCommandRepository ??=
            _serviceProvider.GetRequiredService<IRelationTypeCommandRepository>();

        public ISchoolUserCommandRepository SchoolUserCommandRepository => _schoolUserCommandRepository ??=
            _serviceProvider.GetRequiredService<ISchoolUserCommandRepository>();

        public IUserCommandRepository UserCommandRepository => _userCommandRepository ??=
            _serviceProvider.GetRequiredService<IUserCommandRepository>();

        public IUserDeviceCommandRepository UserDeviceCommandRepository => _userDeviceCommandRepository ??=
            _serviceProvider.GetRequiredService<IUserDeviceCommandRepository>();

        public ISMSProviderCommandRepository SMSProviderCommandRepository => _smsProviderCommandRepository ??=
            _serviceProvider.GetRequiredService<ISMSProviderCommandRepository>();

        public IEmailProviderCommandRepository EmailProviderCommandRepository => _emailProviderCommandRepository ??=
            _serviceProvider.GetRequiredService<IEmailProviderCommandRepository>();

        public ISMSLogCommandRepository SMSLogCommandRepository => _smsLogCommandRepository ??=
            _serviceProvider.GetRequiredService<ISMSLogCommandRepository>();

        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction != null)
            {
                return;
            }

            _currentTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction == null)
            {
                return;
            }

            await _currentTransaction.CommitAsync(cancellationToken);
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction == null)
            {
                return;
            }

            await _currentTransaction.RollbackAsync(cancellationToken);
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }

        public async ValueTask DisposeAsync()
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }
    }
}
