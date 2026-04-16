using Mdaresna.Repository.IRepositories.AdminManagement.Query;
using Mdaresna.Repository.IRepositories.CoinsManagement.Query;
using Mdaresna.Repository.IRepositories.IdentityManagement.Query;
using Mdaresna.Repository.IRepositories.SchoolManagement.ClassRoomManagement.Command;
using Mdaresna.Repository.IRepositories.SchoolManagement.ClassRoomManagement.Query;
using Mdaresna.Repository.IRepositories.SchoolManagement.SchoolManagement.Query;
using Mdaresna.Repository.IRepositories.SchoolManagement.StudentManagement.Query;
using Mdaresna.Repository.IRepositories.SettingsManagement.Query;
using Mdaresna.Repository.IRepositories.UserManagement.Query;
using Mdaresna.Repository.IUnitOfWork;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Mdaresna.Infrastructure.UnitOfWork
{
    public class QueryUnitOfWork : IQueryUnitOfWork
    {
        private readonly IServiceProvider _serviceProvider;
        private ILanguageQueryRepository? _languageQueryRepository;
        private ICoinTypeQueryRepository? _coinTypeQueryRepository;
        private IPaymentTransactionQueryRepository? _paymentTransactionQueryRepository;
        private IPaymentTypeQueryRepository? _paymentTypeQueryRepository;
        private ISchoolPaymentRequestQueryRepository? _schoolPaymentRequestQueryRepository;
        private IPermissionQueryRepository? _permissionQueryRepository;
        private IRolePermissionQueryRepository? _rolePermissionQueryRepository;
        private IRoleQueryRepository? _roleQueryRepository;
        private IUserPermissionQueryRepository? _userPermissionQueryRepository;
        private IUserPermissionSchoolClassRoomQueryRepository? _userPermissionSchoolClassRoomQueryRepository;
        private IUserRoleQueryRepository? _userRoleQueryRepository;
        private ISMSProviderQueryRepository? _smsProviderQueryRepository;
        private IEmailProviderQueryRepository? _emailProviderQueryRepository;
        private ISMSLogQueryRepository? _smsLogQueryRepository;
        private IClassRoomStudentActivityQueryRepository? _classRoomStudentActivityQueryRepository;
        private IClassRoomStudentAssignmentQueryRepository? _classRoomStudentAssignmentQueryRepository;
        private IClassRoomStudentExamQueryRepository? _classRoomStudentExamQueryRepository;
        private IStudentAttendanceQueryRepository? _studentAttendanceQueryRepository;
        private IStudentExamRateQueryRepository? _studentExamRateQueryRepository;
        private IStudentNoteQueryRepository? _studentNoteQueryRepository;
        private IStudentParentQueryRepository? _studentParentQueryRepository;
        private IStudentQueryRepository? _studentQueryRepository;
        private IClassRoomLanguageQueryRepository? _classRoomLanguageQueryRepository;
        private ISchoolContactQueryRepository? _schoolContactQueryRepository;
        private ISchoolContactTypeQueryRepository? _schoolContactTypeQueryRepository;
        private ISchoolCourseQueryRepository? _schoolCourseQueryRepository;
        private ISchoolExamRateHeaderQueryRepository? _schoolExamRateHeaderQueryRepository;
        private ISchoolGradeQueryRepository? _schoolGradeQueryRepository;
        private ISchoolPostImageQueryRepository? _schoolPostImageQueryRepository;
        private ISchoolPostQueryRepository? _schoolPostQueryRepository;
        private ISchoolPostReportQueryRepository? _schoolPostReportQueryRepository;
        private ISchoolQueryRepository? _schoolQueryRepository;
        private ISchoolEmployeeQueryRepository? _schoolEmployeeQueryRepository;
        private ISchoolTeacherQueryRepository? _schoolTeacherQueryRepository;
        private ISchoolTeacherCourseQueryRepository? _schoolTeacherCourseQueryRepository;
        private ISchoolTypeQueryRepository? _schoolTypeQueryRepository;
        private ISchoolYearMonthQueryRepository? _schoolYearMonthQueryRepository;
        private ISchoolYearQueryRepository? _schoolYearQueryRepository;
        private IClassRoomActivityQueryRepository? _classRoomActivityQueryRepository;
        private IClassRoomAssignmentQueryRepository? _classRoomAssignmentQueryRepository;
        private IClassRoomExamQueryRepository? _classRoomExamQueryRepository;
        private IClassRoomQueryRepository? _classRoomQueryRepository;
        private IClassroomEmployeeQueryRepository? _classroomEmployeeQueryRepository;
        private IClassRoomTeacherCourseQueryRepository? _classRoomTeacherCourseQueryRepository;
        private IClassRoomHelpDataQueryRepository? _classRoomHelpDataQueryRepository;
        private IRelationTypeQueryRepository? _relationTypeQueryRepository;
        private ISchoolUserQueryRepository? _schoolUserQueryRepository;
        private IUserQueryRepository? _userQueryRepository;
        private IUserDeviceQueryRepository? _userDeviceQueryRepository;
        private IUserReportQueryRepository? _userReportQueryRepository;

        public QueryUnitOfWork(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public ILanguageQueryRepository LanguageQueryRepository => _languageQueryRepository ??=
            _serviceProvider.GetRequiredService<ILanguageQueryRepository>();

        public ICoinTypeQueryRepository CoinTypeQueryRepository => _coinTypeQueryRepository ??=
            _serviceProvider.GetRequiredService<ICoinTypeQueryRepository>();

        public IPaymentTransactionQueryRepository PaymentTransactionQueryRepository => _paymentTransactionQueryRepository ??=
            _serviceProvider.GetRequiredService<IPaymentTransactionQueryRepository>();

        public IPaymentTypeQueryRepository PaymentTypeQueryRepository => _paymentTypeQueryRepository ??=
            _serviceProvider.GetRequiredService<IPaymentTypeQueryRepository>();

        public ISchoolPaymentRequestQueryRepository SchoolPaymentRequestQueryRepository => _schoolPaymentRequestQueryRepository ??=
            _serviceProvider.GetRequiredService<ISchoolPaymentRequestQueryRepository>();

        public IPermissionQueryRepository PermissionQueryRepository => _permissionQueryRepository ??=
            _serviceProvider.GetRequiredService<IPermissionQueryRepository>();

        public IRolePermissionQueryRepository RolePermissionQueryRepository => _rolePermissionQueryRepository ??=
            _serviceProvider.GetRequiredService<IRolePermissionQueryRepository>();

        public IRoleQueryRepository RoleQueryRepository => _roleQueryRepository ??=
            _serviceProvider.GetRequiredService<IRoleQueryRepository>();

        public IUserPermissionQueryRepository UserPermissionQueryRepository => _userPermissionQueryRepository ??=
            _serviceProvider.GetRequiredService<IUserPermissionQueryRepository>();

        public IUserPermissionSchoolClassRoomQueryRepository UserPermissionSchoolClassRoomQueryRepository => _userPermissionSchoolClassRoomQueryRepository ??=
            _serviceProvider.GetRequiredService<IUserPermissionSchoolClassRoomQueryRepository>();

        public IUserRoleQueryRepository UserRoleQueryRepository => _userRoleQueryRepository ??=
            _serviceProvider.GetRequiredService<IUserRoleQueryRepository>();

        public ISMSProviderQueryRepository SMSProviderQueryRepository => _smsProviderQueryRepository ??=
            _serviceProvider.GetRequiredService<ISMSProviderQueryRepository>();

        public IEmailProviderQueryRepository EmailProviderQueryRepository => _emailProviderQueryRepository ??=
            _serviceProvider.GetRequiredService<IEmailProviderQueryRepository>();

        public ISMSLogQueryRepository SMSLogQueryRepository => _smsLogQueryRepository ??=
            _serviceProvider.GetRequiredService<ISMSLogQueryRepository>();

        public IClassRoomStudentActivityQueryRepository ClassRoomStudentActivityQueryRepository => _classRoomStudentActivityQueryRepository ??=
            _serviceProvider.GetRequiredService<IClassRoomStudentActivityQueryRepository>();

        public IClassRoomStudentAssignmentQueryRepository ClassRoomStudentAssignmentQueryRepository => _classRoomStudentAssignmentQueryRepository ??=
            _serviceProvider.GetRequiredService<IClassRoomStudentAssignmentQueryRepository>();

        public IClassRoomStudentExamQueryRepository ClassRoomStudentExamQueryRepository => _classRoomStudentExamQueryRepository ??=
            _serviceProvider.GetRequiredService<IClassRoomStudentExamQueryRepository>();

        public IStudentAttendanceQueryRepository StudentAttendanceQueryRepository => _studentAttendanceQueryRepository ??=
            _serviceProvider.GetRequiredService<IStudentAttendanceQueryRepository>();

        public IStudentExamRateQueryRepository StudentExamRateQueryRepository => _studentExamRateQueryRepository ??=
            _serviceProvider.GetRequiredService<IStudentExamRateQueryRepository>();

        public IStudentNoteQueryRepository StudentNoteQueryRepository => _studentNoteQueryRepository ??=
            _serviceProvider.GetRequiredService<IStudentNoteQueryRepository>();

        public IStudentParentQueryRepository StudentParentQueryRepository => _studentParentQueryRepository ??=
            _serviceProvider.GetRequiredService<IStudentParentQueryRepository>();

        public IStudentQueryRepository StudentQueryRepository => _studentQueryRepository ??=
            _serviceProvider.GetRequiredService<IStudentQueryRepository>();

        public IClassRoomLanguageQueryRepository ClassRoomLanguageQueryRepository => _classRoomLanguageQueryRepository ??=
            _serviceProvider.GetRequiredService<IClassRoomLanguageQueryRepository>();

        public ISchoolContactQueryRepository SchoolContactQueryRepository => _schoolContactQueryRepository ??=
            _serviceProvider.GetRequiredService<ISchoolContactQueryRepository>();

        public ISchoolContactTypeQueryRepository SchoolContactTypeQueryRepository => _schoolContactTypeQueryRepository ??=
            _serviceProvider.GetRequiredService<ISchoolContactTypeQueryRepository>();

        public ISchoolCourseQueryRepository SchoolCourseQueryRepository => _schoolCourseQueryRepository ??=
            _serviceProvider.GetRequiredService<ISchoolCourseQueryRepository>();

        public ISchoolExamRateHeaderQueryRepository SchoolExamRateHeaderQueryRepository => _schoolExamRateHeaderQueryRepository ??=
            _serviceProvider.GetRequiredService<ISchoolExamRateHeaderQueryRepository>();

        public ISchoolGradeQueryRepository SchoolGradeQueryRepository => _schoolGradeQueryRepository ??=
            _serviceProvider.GetRequiredService<ISchoolGradeQueryRepository>();

        public ISchoolPostImageQueryRepository SchoolPostImageQueryRepository => _schoolPostImageQueryRepository ??=
            _serviceProvider.GetRequiredService<ISchoolPostImageQueryRepository>();

        public ISchoolPostQueryRepository SchoolPostQueryRepository => _schoolPostQueryRepository ??=
            _serviceProvider.GetRequiredService<ISchoolPostQueryRepository>();

        public ISchoolPostReportQueryRepository SchoolPostReportQueryRepository => _schoolPostReportQueryRepository ??=
            _serviceProvider.GetRequiredService<ISchoolPostReportQueryRepository>();

        public ISchoolQueryRepository SchoolQueryRepository => _schoolQueryRepository ??=
            _serviceProvider.GetRequiredService<ISchoolQueryRepository>();

        public ISchoolEmployeeQueryRepository SchoolEmployeeQueryRepository => _schoolEmployeeQueryRepository ??=
            _serviceProvider.GetRequiredService<ISchoolEmployeeQueryRepository>();

        public ISchoolTeacherQueryRepository SchoolTeacherQueryRepository => _schoolTeacherQueryRepository ??=
            _serviceProvider.GetRequiredService<ISchoolTeacherQueryRepository>();

        public ISchoolTeacherCourseQueryRepository SchoolTeacherCourseQueryRepository => _schoolTeacherCourseQueryRepository ??=
            _serviceProvider.GetRequiredService<ISchoolTeacherCourseQueryRepository>();

        public ISchoolTypeQueryRepository SchoolTypeQueryRepository => _schoolTypeQueryRepository ??=
            _serviceProvider.GetRequiredService<ISchoolTypeQueryRepository>();

        public ISchoolYearMonthQueryRepository SchoolYearMonthQueryRepository => _schoolYearMonthQueryRepository ??=
            _serviceProvider.GetRequiredService<ISchoolYearMonthQueryRepository>();

        public ISchoolYearQueryRepository SchoolYearQueryRepository => _schoolYearQueryRepository ??=
            _serviceProvider.GetRequiredService<ISchoolYearQueryRepository>();

        public IClassRoomActivityQueryRepository ClassRoomActivityQueryRepository => _classRoomActivityQueryRepository ??=
            _serviceProvider.GetRequiredService<IClassRoomActivityQueryRepository>();

        public IClassRoomAssignmentQueryRepository ClassRoomAssignmentQueryRepository => _classRoomAssignmentQueryRepository ??=
            _serviceProvider.GetRequiredService<IClassRoomAssignmentQueryRepository>();

        public IClassRoomExamQueryRepository ClassRoomExamQueryRepository => _classRoomExamQueryRepository ??=
            _serviceProvider.GetRequiredService<IClassRoomExamQueryRepository>();

        public IClassRoomQueryRepository ClassRoomQueryRepository => _classRoomQueryRepository ??=
            _serviceProvider.GetRequiredService<IClassRoomQueryRepository>();

        public IClassroomEmployeeQueryRepository ClassroomEmployeeQueryRepository => _classroomEmployeeQueryRepository ??=
            _serviceProvider.GetRequiredService<IClassroomEmployeeQueryRepository>();

        public IClassRoomTeacherCourseQueryRepository ClassRoomTeacherCourseQueryRepository => _classRoomTeacherCourseQueryRepository ??=
            _serviceProvider.GetRequiredService<IClassRoomTeacherCourseQueryRepository>();

        public IClassRoomHelpDataQueryRepository ClassRoomHelpDataQueryRepository => _classRoomHelpDataQueryRepository ??=
            _serviceProvider.GetRequiredService<IClassRoomHelpDataQueryRepository>();

        public IRelationTypeQueryRepository RelationTypeQueryRepository => _relationTypeQueryRepository ??=
            _serviceProvider.GetRequiredService<IRelationTypeQueryRepository>();

        public ISchoolUserQueryRepository SchoolUserQueryRepository => _schoolUserQueryRepository ??=
            _serviceProvider.GetRequiredService<ISchoolUserQueryRepository>();

        public IUserQueryRepository UserQueryRepository => _userQueryRepository ??=
            _serviceProvider.GetRequiredService<IUserQueryRepository>();

        public IUserDeviceQueryRepository UserDeviceQueryRepository => _userDeviceQueryRepository ??=
            _serviceProvider.GetRequiredService<IUserDeviceQueryRepository>();

        public IUserReportQueryRepository UserReportQueryRepository => _userReportQueryRepository ??=
            _serviceProvider.GetRequiredService<IUserReportQueryRepository>();
    }
}
