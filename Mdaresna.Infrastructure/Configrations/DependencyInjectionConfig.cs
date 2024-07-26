using Mdaresna.Infrastructure.BServices.Common;
using Mdaresna.Infrastructure.BServices.IdentityManagement;
using Mdaresna.Infrastructure.Repositories.AdminManagement.Command;
using Mdaresna.Infrastructure.Repositories.AdminManagement.Query;
using Mdaresna.Infrastructure.Repositories.Base;
using Mdaresna.Infrastructure.Repositories.CoinsManagement.Command;
using Mdaresna.Infrastructure.Repositories.CoinsManagement.Query;
using Mdaresna.Infrastructure.Repositories.Common;
using Mdaresna.Infrastructure.Repositories.IdentityManagement.Command;
using Mdaresna.Infrastructure.Repositories.IdentityManagement.Query;
using Mdaresna.Infrastructure.Repositories.SchoolManagement.ClassRoomManagement.Command;
using Mdaresna.Infrastructure.Repositories.SchoolManagement.ClassRoomManagement.Query;
using Mdaresna.Infrastructure.Repositories.SchoolManagement.SchoolManagement.Command;
using Mdaresna.Infrastructure.Repositories.SchoolManagement.SchoolManagement.Query;
using Mdaresna.Infrastructure.Repositories.SchoolManagement.StudentManagement.Command;
using Mdaresna.Infrastructure.Repositories.SchoolManagement.StudentManagement.Query;
using Mdaresna.Infrastructure.Repositories.SettingsManagement.Command;
using Mdaresna.Infrastructure.Repositories.SettingsManagement.Query;
using Mdaresna.Infrastructure.Repositories.UserManagement.Command;
using Mdaresna.Infrastructure.Repositories.UserManagement.Query;
using Mdaresna.Infrastructure.Services.AdminManagement.Command;
using Mdaresna.Infrastructure.Services.AdminManagement.Query;
using Mdaresna.Infrastructure.Services.Base;
using Mdaresna.Infrastructure.Services.CoinsManagement.Command;
using Mdaresna.Infrastructure.Services.CoinsManagement.Query;
using Mdaresna.Infrastructure.Services.IdentityManagement.Command;
using Mdaresna.Infrastructure.Services.IdentityManagement.Query;
using Mdaresna.Infrastructure.Services.SchoolManagement.ClassRoomManagement.Command;
using Mdaresna.Infrastructure.Services.SchoolManagement.ClassRoomManagement.Query;
using Mdaresna.Infrastructure.Services.SchoolManagement.SchoolManagement.Command;
using Mdaresna.Infrastructure.Services.SchoolManagement.SchoolManagement.Query;
using Mdaresna.Infrastructure.Services.SchoolManagement.StudentManagement.Command;
using Mdaresna.Infrastructure.Services.SchoolManagement.StudentManagement.Query;
using Mdaresna.Infrastructure.Services.SettingsManagement.Command;
using Mdaresna.Infrastructure.Services.SettingsManagement.Query;
using Mdaresna.Infrastructure.Services.UserManagement.Command;
using Mdaresna.Infrastructure.Services.UserManagement.Query;
using Mdaresna.Repository.IBServices.Common;
using Mdaresna.Repository.IBServices.IdentityManagement;
using Mdaresna.Repository.IRepositories.AdminManagement.Command;
using Mdaresna.Repository.IRepositories.AdminManagement.Query;
using Mdaresna.Repository.IRepositories.Base;
using Mdaresna.Repository.IRepositories.CoinsManagement.Command;
using Mdaresna.Repository.IRepositories.CoinsManagement.Query;
using Mdaresna.Repository.IRepositories.Common;
using Mdaresna.Repository.IRepositories.IdentityManagement.Command;
using Mdaresna.Repository.IRepositories.IdentityManagement.Query;
using Mdaresna.Repository.IRepositories.SchoolManagement.ClassRoomManagement.Command;
using Mdaresna.Repository.IRepositories.SchoolManagement.ClassRoomManagement.Query;
using Mdaresna.Repository.IRepositories.SchoolManagement.SchoolManagement.Command;
using Mdaresna.Repository.IRepositories.SchoolManagement.SchoolManagement.Query;
using Mdaresna.Repository.IRepositories.SchoolManagement.StudentManagement.Command;
using Mdaresna.Repository.IRepositories.SchoolManagement.StudentManagement.Query;
using Mdaresna.Repository.IRepositories.SettingsManagement.Command;
using Mdaresna.Repository.IRepositories.SettingsManagement.Query;
using Mdaresna.Repository.IRepositories.UserManagement.Command;
using Mdaresna.Repository.IRepositories.UserManagement.Query;
using Mdaresna.Repository.IServices.AdminManagement.Command;
using Mdaresna.Repository.IServices.AdminManagement.Query;
using Mdaresna.Repository.IServices.Base;
using Mdaresna.Repository.IServices.CoinsManagement.Command;
using Mdaresna.Repository.IServices.CoinsManagement.Query;
using Mdaresna.Repository.IServices.IdentityManagement.Command;
using Mdaresna.Repository.IServices.IdentityManagement.Query;
using Mdaresna.Repository.IServices.SchoolManagement.ClassRoomManagement.Command;
using Mdaresna.Repository.IServices.SchoolManagement.ClassRoomManagement.Query;
using Mdaresna.Repository.IServices.SchoolManagement.SchoolManagement.Command;
using Mdaresna.Repository.IServices.SchoolManagement.SchoolManagement.Query;
using Mdaresna.Repository.IServices.SchoolManagement.StudentManagement.Command;
using Mdaresna.Repository.IServices.SchoolManagement.StudentManagement.Query;
using Mdaresna.Repository.IServices.SettingsManagement.Command;
using Mdaresna.Repository.IServices.SettingsManagement.Query;
using Mdaresna.Repository.IServices.UserManagement.Command;
using Mdaresna.Repository.IServices.UserManagement.Query;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Configrations
{
    public static class DependencyInjectionConfig
    {
        #region Repositories
        public static void ConfigerRepositories(IServiceCollection services)
        {
            ConfigerBaseRepos(services);
            ConfigerAdminRepos(services);
            ConfigerCoinsManagementRepos(services);
            ConfigerIdentityManagementRepos(services);
            ConfigerSchoolManagementRepos(services);
            ConfigerClassRoomManagementRepos(services);
            ConfigerStudentManagementRepos(services);
            ConfigerUserManagementRepos(services);
            ConfigerSettingsManagementRepos(services);
            ConfigerCommonRepos(services);
        }

        private static void ConfigerBaseRepos(IServiceCollection services)
        {
            services.AddScoped(typeof(IBaseCommandRepository<>), typeof(BaseCommandRepository<>));
            services.AddScoped(typeof(IBaseQueryRepository<>), typeof(BaseQueryRepository<>));
            services.AddScoped(typeof(IBaseSharedRepository<>), typeof(BaseSharedRepository<>));
        }

        private static void ConfigerAdminRepos(IServiceCollection services)
        {
            services.AddScoped(typeof(ILanguageCommandRepository), typeof(LanguageCommandRepository));
            services.AddScoped(typeof(ILanguageQueryRepository), typeof(LanguageQueryRepository));
        }

        private static void ConfigerCoinsManagementRepos(IServiceCollection services)
        {
            #region Command
            services.AddScoped(typeof(ICoinTypeCommandRepository), typeof(CoinTypeCommandRepository));
            services.AddScoped(typeof(IPaymentTransactionCommandRepository), typeof(PaymentTransactionCommandRepository));
            services.AddScoped(typeof(IPaymentTypeCommandRepository), typeof(PaymentTypeCommandRepository));
            services.AddScoped(typeof(ISchoolPaymentRequestCommandRepository), typeof(SchoolPaymentRequestCommandRepository));
            #endregion

            #region Query
            services.AddScoped(typeof(ICoinTypeQueryRepository), typeof(CoinTypeQueryRepository));
            services.AddScoped(typeof(IPaymentTransactionQueryRepository), typeof(PaymentTransactionQueryRepository));
            services.AddScoped(typeof(IPaymentTypeQueryRepository), typeof(PaymentTypeQueryRepository));
            services.AddScoped(typeof(ISchoolPaymentRequestQueryRepository), typeof(SchoolPaymentRequestQueryRepository));

            #endregion
        }

        private static void ConfigerIdentityManagementRepos(IServiceCollection services)
        {
            #region Command
            services.AddScoped(typeof(IPermissionCommandRepository), typeof(PermissionCommandRepository));
            services.AddScoped(typeof(IRoleCommandRepository), typeof(RoleCommandRepository));
            services.AddScoped(typeof(IRolePermissionCommandRepository), typeof(RolePermissionCommandRepository));
            services.AddScoped(typeof(IUserPermissionCommandRepository), typeof(UserPermissionCommandRepository));
            services.AddScoped(typeof(IUserPermissionSchoolClassRoomCommandRepository), typeof(UserPermissionSchoolClassRoomCommandRepository));
            services.AddScoped(typeof(IUserRoleCommandRepository), typeof(UserRoleCommandRepository));

            #endregion

            #region Query
            services.AddScoped(typeof(IPermissionQueryRepository), typeof(PermissionQueryRepository));
            services.AddScoped(typeof(IRolePermissionQueryRepository), typeof(RolePermissionQueryRepository));
            services.AddScoped(typeof(IRoleQueryRepository), typeof(RoleQueryRepository));
            services.AddScoped(typeof(IUserPermissionQueryRepository), typeof(UserPermissionQueryRepository));
            services.AddScoped(typeof(IUserPermissionSchoolClassRoomQueryRepository), typeof(UserPermissionSchoolClassRoomQueryRepository));
            services.AddScoped(typeof(IUserRoleQueryRepository), typeof(UserRoleQueryRepository));

            #endregion
        }

        private static void ConfigerSchoolManagementRepos(IServiceCollection services)
        {
            #region Command
            services.AddScoped(typeof(IClassRoomLanguageCommandRepository), typeof(ClassRoomLanguageCommandRepository));
            services.AddScoped(typeof(ISchoolCommandRepository), typeof(SchoolCommandRepository));
            services.AddScoped(typeof(ISchoolContactCommandRepository), typeof(SchoolContactCommandRepository));
            services.AddScoped(typeof(ISchoolContactTypeCommandRepository), typeof(SchoolContactTypeCommandRepository));
            services.AddScoped(typeof(ISchoolCourseCommandRepository), typeof(SchoolCourseCommandRepository));
            services.AddScoped(typeof(ISchoolExamRateHeaderCommandRepository), typeof(SchoolExamRateHeaderCommandRepository));
            services.AddScoped(typeof(ISchoolGradeCommandRepository), typeof(SchoolGradeCommandRepository));
            services.AddScoped(typeof(ISchoolPostCommandRepository), typeof(SchoolPostCommandRepository));
            services.AddScoped(typeof(ISchoolPostImageCommandRepository), typeof(SchoolPostImageCommandRepository));
            services.AddScoped(typeof(ISchoolTeacherCommandRepository), typeof(SchoolTeacherCommandRepository));
            services.AddScoped(typeof(ISchoolTeacherCourseCommandRepository), typeof(SchoolTeacherCourseCommandRepository));
            services.AddScoped(typeof(ISchoolTypeCommandRepository), typeof(SchoolTypeCommandRepository));
            services.AddScoped(typeof(ISchoolYearCommandRepository), typeof(SchoolYearCommandRepository));
            services.AddScoped(typeof(ISchoolYearMonthCommandRepository), typeof(SchoolYearMonthCommandRepository));

            #endregion

            #region Query
            services.AddScoped(typeof(IClassRoomLanguageQueryRepository), typeof(ClassRoomLanguageQueryRepository));
            services.AddScoped(typeof(ISchoolContactQueryRepository), typeof(SchoolContactQueryRepository));
            services.AddScoped(typeof(ISchoolContactTypeQueryRepository), typeof(SchoolContactTypeQueryRepository));
            services.AddScoped(typeof(ISchoolCourseQueryRepository), typeof(SchoolCourseQueryRepository));
            services.AddScoped(typeof(ISchoolExamRateHeaderQueryRepository), typeof(SchoolExamRateHeaderQueryRepository));
            services.AddScoped(typeof(ISchoolGradeQueryRepository), typeof(SchoolGradeQueryRepository));
            services.AddScoped(typeof(ISchoolPostImageQueryRepository), typeof(SchoolPostImageQueryRepository));
            services.AddScoped(typeof(ISchoolPostQueryRepository), typeof(SchoolPostQueryRepository));
            services.AddScoped(typeof(ISchoolQueryRepository), typeof(SchoolQueryRepository));
            services.AddScoped(typeof(ISchoolTeacherQueryRepository), typeof(SchoolTeacherQueryRepository));
            services.AddScoped(typeof(ISchoolTeacherCourseQueryRepository), typeof(SchoolTeacherCourseQueryRepository));
            services.AddScoped(typeof(ISchoolTypeQueryRepository), typeof(SchoolTypeQueryRepository));
            services.AddScoped(typeof(ISchoolYearMonthQueryRepository), typeof(SchoolYearMonthQueryRepository));
            services.AddScoped(typeof(ISchoolYearQueryRepository), typeof(SchoolYearQueryRepository));

            #endregion
        }

        private static void ConfigerClassRoomManagementRepos(IServiceCollection services)
        {
            #region Command
            services.AddScoped(typeof(IClassRoomActivityCommandRepository), typeof(ClassRoomActivityCommandRepository));
            services.AddScoped(typeof(IClassRoomAssignmentCommandRepository), typeof(ClassRoomAssignmentCommandRepository));
            services.AddScoped(typeof(IClassRoomCommandRepository), typeof(ClassRoomCommandRepository));
            services.AddScoped(typeof(IClassRoomExamCommandRepository), typeof(ClassRoomExamCommandRepository));
            services.AddScoped(typeof(IClassRoomTeacherCourseCommandRepository), typeof(ClassRoomTeacherCourseCommandRepository));

            #endregion

            #region Query
            services.AddScoped(typeof(IClassRoomActivityQueryRepository), typeof(ClassRoomActivityQueryRepository));
            services.AddScoped(typeof(IClassRoomAssignmentQueryRepository), typeof(ClassRoomAssignmentQueryRepository));
            services.AddScoped(typeof(IClassRoomExamQueryRepository), typeof(ClassRoomExamQueryRepository));
            services.AddScoped(typeof(IClassRoomQueryRepository), typeof(ClassRoomQueryRepository));
            services.AddScoped(typeof(IClassRoomHelpDataQueryRepository), typeof(ClassRoomHelpDataQueryRepository));
            services.AddScoped(typeof(IClassRoomTeacherCourseQueryRepository), typeof(ClassRoomTeacherCourseQueryRepository));

            #endregion
        }

        private static void ConfigerStudentManagementRepos(IServiceCollection services)
        {
            #region Command
            services.AddScoped(typeof(IClassRoomStudentActivityCommandRepository), typeof(ClassRoomStudentActivityCommandRepository));
            services.AddScoped(typeof(IClassRoomStudentAssignmentCommandRepository), typeof(ClassRoomStudentAssignmentCommandRepository));
            services.AddScoped(typeof(IClassRoomStudentExamCommandRepository), typeof(ClassRoomStudentExamCommandRepository));
            services.AddScoped(typeof(IStudentAttendanceCommandRepository), typeof(StudentAttendanceCommandRepository));
            services.AddScoped(typeof(IStudentCommandRepository), typeof(StudentCommandRepository));
            services.AddScoped(typeof(IStudentExamRateCommandRepository), typeof(StudentExamRateCommandRepository));
            services.AddScoped(typeof(IStudentNoteCommandRepository), typeof(StudentNoteCommandRepository));
            services.AddScoped(typeof(IStudentParentCommandRepository), typeof(StudentParentCommandRepository));

            #endregion

            #region Query
            services.AddScoped(typeof(IClassRoomStudentActivityQueryRepository), typeof(ClassRoomStudentActivityQueryRepository));
            services.AddScoped(typeof(IClassRoomStudentAssignmentQueryRepository), typeof(ClassRoomStudentAssignmentQueryRepository));
            services.AddScoped(typeof(IClassRoomStudentExamQueryRepository), typeof(ClassRoomStudentExamQueryRepository));
            services.AddScoped(typeof(IStudentAttendanceQueryRepository), typeof(StudentAttendanceQueryRepository));
            services.AddScoped(typeof(IStudentExamRateQueryRepository), typeof(StudentExamRateQueryRepository));
            services.AddScoped(typeof(IStudentNoteQueryRepository), typeof(StudentNoteQueryRepository));
            services.AddScoped(typeof(IStudentParentQueryRepository), typeof(StudentParentQueryRepository));
            services.AddScoped(typeof(IStudentQueryRepository), typeof(StudentQueryRepository));

            #endregion
        }

        private static void ConfigerUserManagementRepos(IServiceCollection services)
        {
            #region Command
            services.AddScoped(typeof(IRelationTypeCommandRepository), typeof(RelationTypeCommandRepository));
            services.AddScoped(typeof(ISchoolUserCommandRepository), typeof(SchoolUserCommandRepository));
            services.AddScoped(typeof(IUserCommandRepository), typeof(UserCommandRepository));

            #endregion

            #region Query
            services.AddScoped(typeof(IRelationTypeQueryRepository), typeof(RelationTypeQueryRepository));
            services.AddScoped(typeof(ISchoolUserQueryRepository), typeof(SchoolUserQueryRepository));
            services.AddScoped(typeof(IUserQueryRepository), typeof(UserQueryRepository));

            #endregion
        }

        private static void ConfigerSettingsManagementRepos(IServiceCollection services)
        {
            #region Command
            services.AddScoped(typeof(ISMSProviderCommandRepository), typeof(SMSProviderCommandRepository));
            services.AddScoped(typeof(IEmailProviderCommandRepository), typeof(EmailProviderCommandRepository));

            #endregion

            #region Query
            services.AddScoped(typeof(ISMSProviderQueryRepository), typeof(SMSProviderQueryRepository));
            services.AddScoped(typeof(IEmailProviderQueryRepository), typeof(EmailProviderQueryRepository));

            #endregion
        }

        private static void ConfigerCommonRepos(IServiceCollection services)
        {
            services.AddScoped<IImageUploderRepository, ImageUploderRepository>();
        }

        #endregion


        #region Services


        public static void ConfigerServices(IServiceCollection services)
        {
            //ConfigeBaseServ(services);
            ConfigeAdminServ(services);
            ConfigerCoinsManagementServ(services);
            ConfigerIdentityManagementServ(services);
            ConfigerSchoolManagementServ(services);
            ConfigerClassRoomManagementServ(services);
            ConfigerStudentManagementServ(services);
            ConfigerUserManagementServ(services);
            ConfigerSettingsManagementServ(services);
            ConfigerBIdentityManagementServ(services);
            ConfigerCommonSer(services);

        }

        private static void ConfigerBIdentityManagementServ(IServiceCollection services)
        {
            services.AddScoped<IIdentityService, IdentityService>();

        }

        private static void ConfigeBaseServ(IServiceCollection services)
        {
            #region Command
            
            #endregion

            #region Query
            services.AddScoped(typeof(IBaseQueryService<>), typeof(BaseQueryService<>));
            
            #endregion
        }

        private static void ConfigeAdminServ(IServiceCollection services)
        {
            #region Command
            services.AddScoped(typeof(ILanguageCommandService), typeof(LanguageCommandService));

            #endregion

            #region Query
            services.AddScoped(typeof(ILanguageQueryService), typeof(LanguageQueryService));

            #endregion
        }

        private static void ConfigerCoinsManagementServ(IServiceCollection services)
        {
            #region Command
            services.AddScoped(typeof(ICoinTypeCommandService), typeof(CoinTypeCommandService));
            services.AddScoped(typeof(IPaymentTransactionCommandService), typeof(PaymentTransactionCommandService));
            services.AddScoped(typeof(IPaymentTypeCommandService), typeof(PaymentTypeCommandService));
            services.AddScoped(typeof(ISchoolPaymentRequestCommandService), typeof(SchoolPaymentRequestCommandService));
            #endregion

            #region Query
            services.AddScoped(typeof(ICoinTypeQueryService), typeof(CoinTypeQueryService));
            services.AddScoped(typeof(IPaymentTransactionQueryService), typeof(PaymentTransactionQueryService));
            services.AddScoped(typeof(IPaymentTypeQueryService), typeof(PaymentTypeQueryService));
            services.AddScoped(typeof(ISchoolPaymentRequestQueryService), typeof(SchoolPaymentRequestQueryService));

            #endregion
        }

        private static void ConfigerIdentityManagementServ(IServiceCollection services)
        {
            #region Command
            services.AddScoped(typeof(IPermissionCommandService), typeof(PermissionCommandService));
            services.AddScoped(typeof(IRoleCommandService), typeof(RoleCommandService));
            services.AddScoped(typeof(IRolePermissionCommandService), typeof(RolePermissionCommandService));
            services.AddScoped(typeof(IUserPermissionCommandService), typeof(UserPermissionCommandService));
            services.AddScoped(typeof(IUserPermissionSchoolClassRoomCommandService), typeof(UserPermissionSchoolClassRoomCommandService));
            services.AddScoped(typeof(IUserRoleCommandService), typeof(UserRoleCommandService));

            #endregion

            #region Query
            services.AddScoped(typeof(IPermissionQueryService), typeof(PermissionQueryService));
            services.AddScoped(typeof(IRolePermissionQueryService), typeof(RolePermissionQueryService));
            services.AddScoped(typeof(IRoleQueryService), typeof(RoleQueryService));
            services.AddScoped(typeof(IUserPermissionQueryService), typeof(UserPermissionQueryService));
            services.AddScoped(typeof(IUserPermissionSchoolClassRoomQueryService), typeof(UserPermissionSchoolClassRoomQueryService));
            services.AddScoped(typeof(IUserRoleQueryService), typeof(UserRoleQueryService));

            #endregion
        }

        private static void ConfigerSchoolManagementServ(IServiceCollection services)
        {
            #region Command
            services.AddScoped(typeof(IClassRoomLanguageCommandService), typeof(ClassRoomLanguageCommandService));
            services.AddScoped(typeof(ISchoolCommandService), typeof(SchoolCommandService));
            services.AddScoped(typeof(ISchoolContactCommandService), typeof(SchoolContactCommandService));
            services.AddScoped(typeof(ISchoolContactTypeCommandService), typeof(SchoolContactTypeCommandService));
            services.AddScoped(typeof(ISchoolCourseCommandService), typeof(SchoolCourseCommandService));
            services.AddScoped(typeof(ISchoolExamRateHeaderCommandService), typeof(SchoolExamRateHeaderCommandService));
            services.AddScoped(typeof(ISchoolGradeCommandService), typeof(SchoolGradeCommandService));
            services.AddScoped(typeof(ISchoolPostCommandService), typeof(SchoolPostCommandService));
            services.AddScoped(typeof(ISchoolPostImageCommandService), typeof(SchoolPostImageCommandService));
            services.AddScoped(typeof(ISchoolTeacherCommandService), typeof(SchoolTeacherCommandService));
            services.AddScoped(typeof(ISchoolTeacherCourseCommandService), typeof(SchoolTeacherCourseCommandService));
            services.AddScoped(typeof(ISchoolTypeCommandService), typeof(SchoolTypeCommandService));
            services.AddScoped(typeof(ISchoolYearCommandService), typeof(SchoolYearCommandService));
            services.AddScoped(typeof(ISchoolYearMonthCommandService), typeof(SchoolYearMonthCommandService));

            #endregion

            #region Query
            services.AddScoped(typeof(IClassRoomLanguageQueryService), typeof(ClassRoomLanguageQueryService));
            services.AddScoped(typeof(ISchoolContactQueryService), typeof(SchoolContactQueryService));
            services.AddScoped(typeof(ISchoolContactTypeQueryService), typeof(SchoolContactTypeQueryService));
            services.AddScoped(typeof(ISchoolCourseQueryService), typeof(SchoolCourseQueryService));
            services.AddScoped(typeof(ISchoolExamRateHeaderQueryService), typeof(SchoolExamRateHeaderQueryService));
            services.AddScoped(typeof(ISchoolGradeQueryService), typeof(SchoolGradeQueryService));
            services.AddScoped(typeof(ISchoolPostImageQueryService), typeof(SchoolPostImageQueryService));
            services.AddScoped(typeof(ISchoolPostQueryService), typeof(SchoolPostQueryService));
            services.AddScoped(typeof(ISchoolQueryService), typeof(SchoolQueryService));
            services.AddScoped(typeof(ISchoolTeacherQueryService), typeof(SchoolTeacherQueryService));
            services.AddScoped(typeof(ISchoolTeacherCourseQueryService), typeof(SchoolTeacherCourseQueryService));
            services.AddScoped(typeof(ISchoolTypeQueryService), typeof(SchoolTypeQueryService));
            services.AddScoped(typeof(ISchoolYearMonthQueryService), typeof(SchoolYearMonthQueryService));
            services.AddScoped(typeof(ISchoolYearQueryService), typeof(SchoolYearQueryService));

            #endregion
        }

        private static void ConfigerClassRoomManagementServ(IServiceCollection services)
        {
            #region Command
            services.AddScoped(typeof(IClassRoomActivityCommandService), typeof(ClassRoomActivityCommandService));
            services.AddScoped(typeof(IClassRoomAssignmentCommandService), typeof(ClassRoomAssignmentCommandService));
            services.AddScoped(typeof(IClassRoomCommandService), typeof(ClassRoomCommandService));
            services.AddScoped(typeof(IClassRoomExamCommandService), typeof(ClassRoomExamCommandService));
            services.AddScoped(typeof(IClassRoomTeacherCourseCommandService), typeof(ClassRoomTeacherCourseCommandService));

            #endregion

            #region Query
            services.AddScoped(typeof(IClassRoomActivityQueryService), typeof(ClassRoomActivityQueryService));
            services.AddScoped(typeof(IClassRoomAssignmentQueryService), typeof(ClassRoomAssignmentQueryService));
            services.AddScoped(typeof(IClassRoomExamQueryService), typeof(ClassRoomExamQueryService));
            services.AddScoped(typeof(IClassRoomQueryService), typeof(ClassRoomQueryService));
            services.AddScoped(typeof(IClassRoomTeacherCourseQueryService), typeof(ClassRoomTeacherCourseQueryService));

            #endregion
        }

        private static void ConfigerStudentManagementServ(IServiceCollection services)
        {
            #region Command
            services.AddScoped(typeof(IClassRoomStudentActivityCommandService), typeof(ClassRoomStudentActivityCommandService));
            services.AddScoped(typeof(IClassRoomStudentAssignmentCommandService), typeof(ClassRoomStudentAssignmentCommandService));
            services.AddScoped(typeof(IClassRoomStudentExamCommandService), typeof(ClassRoomStudentExamCommandService));
            services.AddScoped(typeof(IStudentAttendanceCommandService), typeof(StudentAttendanceCommandService));
            services.AddScoped(typeof(IStudentCommandService), typeof(StudentCommandService));
            services.AddScoped(typeof(IStudentExamRateCommandService), typeof(StudentExamRateCommandService));
            services.AddScoped(typeof(IStudentNoteCommandService), typeof(StudentNoteCommandService));
            services.AddScoped(typeof(IStudentParentCommandService), typeof(StudentParentCommandService));

            #endregion

            #region Query
            services.AddScoped(typeof(IClassRoomStudentActivityQueryService), typeof(ClassRoomStudentActivityQueryService));
            services.AddScoped(typeof(IClassRoomStudentAssignmentQueryService), typeof(ClassRoomStudentAssignmentQueryService));
            services.AddScoped(typeof(IClassRoomStudentExamQueryService), typeof(ClassRoomStudentExamQueryService));
            services.AddScoped(typeof(IStudentAttendanceQueryService), typeof(StudentAttendanceQueryService));
            services.AddScoped(typeof(IStudentExamRateQueryService), typeof(StudentExamRateQueryService));
            services.AddScoped(typeof(IStudentNoteQueryService), typeof(StudentNoteQueryService));
            services.AddScoped(typeof(IStudentParentQueryService), typeof(StudentParentQueryService));
            services.AddScoped(typeof(IStudentQueryService), typeof(StudentQueryService));

            #endregion
        }

        private static void ConfigerUserManagementServ(IServiceCollection services)
        {
            #region Command
            services.AddScoped(typeof(IRelationTypeCommandService), typeof(RelationTypeCommandService));
            services.AddScoped(typeof(ISchoolUserCommandService), typeof(SchoolUserCommandService));
            services.AddScoped(typeof(IUserCommandService), typeof(UserCommandService));

            #endregion

            #region Query
            services.AddScoped(typeof(IRelationTypeQueryService), typeof(RelationTypeQueryService));
            services.AddScoped(typeof(ISchoolUserQueryService), typeof(SchoolUserQueryService));
            services.AddScoped(typeof(IUserQueryService), typeof(UserQueryService));

            #endregion
        }

        private static void ConfigerSettingsManagementServ(IServiceCollection services)
        {
            #region Command
            services.AddScoped(typeof(ISMSProviderCommandService), typeof(SMSProviderCommandService));
            services.AddScoped(typeof(IEmailProviderCommandService), typeof(EmailProviderCommandService));

            #endregion

            #region Query
            services.AddScoped(typeof(ISMSProviderQueryService), typeof(SMSProviderQueryService));
            services.AddScoped(typeof(IEmailProviderQueryService), typeof(EmailProviderQueryService));

            #endregion
        }

        private static void ConfigerCommonSer(IServiceCollection services)
        {
            services.AddScoped<IImageUploderService, ImageUploderService>();
        }

        #endregion
    }
}
