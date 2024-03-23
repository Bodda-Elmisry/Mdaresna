using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mdaresna.Doamin.Models.Identity;
using Mdaresna.Doamin.Models.UserManagement;
using Mdaresna.Doamin.Models.CoinsManagement;
using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Mdaresna.Doamin.ModelsConfigrations.SchoolManagement;
using Mdaresna.Doamin.ModelsConfigrations.CoinsManagement;
using Mdaresna.Doamin.ModelsConfigrations.Identity;
using System.Reflection.Emit;
using Mdaresna.Doamin.ModelsConfigrations.UserManagement;
using Microsoft.IdentityModel.Abstractions;
using Mdaresna.Doamin.ModelsConfigrations.SchoolManagement.SchoolManagement;
using Mdaresna.Doamin.ModelsConfigrations.SchoolManagement.ClassRoomManagement;
using Mdaresna.Doamin.Models.SchoolManagement.ClassRoomManagement;
using Mdaresna.Doamin.Models.SchoolManagement.StudentManagement;
using Mdaresna.Doamin.ModelsConfigrations.SchoolManagement.StudentManagement;

namespace Mdaresna.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            #region Config files

            ApplyIdentityConfigrations(modelBuilder);
            ApplyUserManagementConfigrations(modelBuilder);
            ApplyCoinManagementConfigrations(modelBuilder);
            ApplySchoolManagementConfigrations(modelBuilder);
            ApplyClassRoomManagementConfigrations(modelBuilder);
            ApplyStudentManagementConfigrations(modelBuilder);

            #endregion
        }

        private void ApplyIdentityConfigrations(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new RolePermissionConfig());
            modelBuilder.ApplyConfiguration(new UserPermissionConfig());
            modelBuilder.ApplyConfiguration(new UserPermissionSchoolClassRoomConfig());
            modelBuilder.ApplyConfiguration(new UserRoleConfig());

        }

        private void ApplyUserManagementConfigrations(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new SchoolUserConfig());
        }

        private void ApplyCoinManagementConfigrations(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new CoinTypeConfig());
            modelBuilder.ApplyConfiguration(new PaymentTransactionConfig());
            modelBuilder.ApplyConfiguration(new SchoolPaymentRequestConfig());
        }

        private void ApplySchoolManagementConfigrations(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new SchoolTeacherCourseConfig());
            modelBuilder.ApplyConfiguration(new SchoolExamRateHeaderConfig());
            modelBuilder.ApplyConfiguration(new SchoolPostConfig());
            modelBuilder.ApplyConfiguration(new SchoolContactConfig());
            modelBuilder.ApplyConfiguration(new SchoolYearConfig());
            modelBuilder.ApplyConfiguration(new SchoolYearMonthConfig());
            modelBuilder.ApplyConfiguration(new SchoolCourseConfig());
            modelBuilder.ApplyConfiguration(new SchoolGradeConfig());
        }

        private void ApplyClassRoomManagementConfigrations(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new ClassRoomConfig());
            modelBuilder.ApplyConfiguration(new ClassRoomAssignmentConfig());
            modelBuilder.ApplyConfiguration(new ClassRoomTeacherCourseConfig());
            modelBuilder.ApplyConfiguration(new ClassRoomExamConfig());
            modelBuilder.ApplyConfiguration(new ClassRoomActivityConfig());
        }

        private void ApplyStudentManagementConfigrations(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new StudentConfig());
        }


        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<UserPermission> userPermissions { get; set; }
        public DbSet<UserPermissionSchoolClassRoom> userPermissionSchoolClassRooms { get; set; }
        public DbSet<RelationType> relationTypes { get; set; }
        public DbSet<SchoolUser> SchoolUsers { get; set; }
        public DbSet<CoinType> CoinsTypes { get; set; }
        public DbSet<PaymentType> PaymentTypes { get; set; }
        public DbSet<PaymentTransaction> PaymentTransactions { get; set; }
        public DbSet<SchoolPaymentRequest> SchoolPaymentRequests { get; set; }
        public DbSet<SchoolPost> SchoolPosts { get; set; }
        public DbSet<SchoolPostImage> SchoolPostImages { get; set; }
        public DbSet<SchoolType> SchoolTypes { get; set; }
        public DbSet<School> Schools { get; set; }
        public DbSet<SchoolContactType> SchoolContactTypes { get; set; }
        public DbSet<SchoolContact> SchoolContacts { get; set; }
        public DbSet<SchoolYear> SchoolYears { get; set; }
        public DbSet<SchoolYearMonth> SchoolYearMonths { get; set; }
        public DbSet<SchoolExamRateHeader> SchoolExamRateHeaders { get; set; }
        public DbSet<SchoolGrade> SchoolGrades { get; set; }
        public DbSet<ClassRoomLanguage> ClassRoomLanguages { get; set; }
        public DbSet<SchoolCourse> SchoolCourses { get; set; }
        public DbSet<SchoolTeacherCourse> schoolTeacherCourses { get; set; }
        public DbSet<ClassRoom> ClassRooms { get; set; }
        public DbSet<ClassRoomAssignment> ClassRoomAssignments { get; set; }
        public DbSet<ClassRoomTeacherCourse> ClassRoomTeacherCourses { get; set; }
        public DbSet<ClassRoomExam> ClassRoomExams { get; set; }
        public DbSet<ClassRoomActivity> ClassRoomActivities { get; set; }
        public DbSet<Student> Students { get; set; }



    }
}
