using Mdaresna.Infrastructure.Repositories.Base;
using Mdaresna.Infrastructure.Repositories.CoinsManagement.Command;
using Mdaresna.Infrastructure.Repositories.CoinsManagement.Query;
using Mdaresna.Infrastructure.Repositories.Identity.Command;
using Mdaresna.Infrastructure.Repositories.Identity.Query;
using Mdaresna.Infrastructure.Repositories.SchoolManagement.ClassRoomManagement.Command;
using Mdaresna.Infrastructure.Repositories.SchoolManagement.ClassRoomManagement.Query;
using Mdaresna.Infrastructure.Repositories.SchoolManagement.SchoolManagement.Command;
using Mdaresna.Infrastructure.Repositories.SchoolManagement.SchoolManagement.Query;
using Mdaresna.Infrastructure.Repositories.SchoolManagement.StudentManagement.Command;
using Mdaresna.Infrastructure.Repositories.SchoolManagement.StudentManagement.Query;
using Mdaresna.Infrastructure.Repositories.UserManagement.Command;
using Mdaresna.Infrastructure.Repositories.UserManagement.Query;
using Mdaresna.Repository.IRepositories.Base;
using Mdaresna.Repository.IRepositories.CoinsManagement.Command;
using Mdaresna.Repository.IRepositories.CoinsManagement.Query;
using Mdaresna.Repository.IRepositories.IdentityManagement.Command;
using Mdaresna.Repository.IRepositories.IdentityManagement.Query;
using Mdaresna.Repository.IRepositories.SchoolManagement.ClassRoomManagement.Command;
using Mdaresna.Repository.IRepositories.SchoolManagement.ClassRoomManagement.Query;
using Mdaresna.Repository.IRepositories.SchoolManagement.SchoolManagement.Command;
using Mdaresna.Repository.IRepositories.SchoolManagement.SchoolManagement.Query;
using Mdaresna.Repository.IRepositories.SchoolManagement.StudentManagement.Command;
using Mdaresna.Repository.IRepositories.SchoolManagement.StudentManagement.Query;
using Mdaresna.Repository.IRepositories.UserManagement.Command;
using Mdaresna.Repository.IRepositories.UserManagement.Query;
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
            ConfigerCoinsManagementRepos(services);
            ConfigerIdentityManagementRepos(services);
            ConfigerSchoolManagementRepos(services);
            ConfigerClassRoomManagementRepos(services);
            ConfigerStudentManagementRepos(services);
            ConfigerUserManagementRepos(services);
        }

        private static void ConfigerBaseRepos(IServiceCollection services)
        {
            services.AddScoped(typeof(IBaseSharedRepository<>), typeof(BaseSharedRepository<>));
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

        #endregion
    }
}
