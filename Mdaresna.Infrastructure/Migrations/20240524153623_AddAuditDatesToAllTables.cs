using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditDatesToAllTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifyDate",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                table: "UserRoles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifyDate",
                table: "UserRoles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                table: "userPermissionSchoolClassRooms",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifyDate",
                table: "userPermissionSchoolClassRooms",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                table: "userPermissions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifyDate",
                table: "userPermissions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                table: "Students",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifyDate",
                table: "Students",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                table: "StudentParents",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifyDate",
                table: "StudentParents",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                table: "studentNotes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifyDate",
                table: "studentNotes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                table: "studentExamRates",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifyDate",
                table: "studentExamRates",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                table: "StudentAttendances",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifyDate",
                table: "StudentAttendances",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                table: "SMSProviders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifyDate",
                table: "SMSProviders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                table: "SchoolYears",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifyDate",
                table: "SchoolYears",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                table: "SchoolYearMonths",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifyDate",
                table: "SchoolYearMonths",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                table: "SchoolUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifyDate",
                table: "SchoolUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                table: "SchoolTypes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifyDate",
                table: "SchoolTypes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                table: "schoolTeacherCourses",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifyDate",
                table: "schoolTeacherCourses",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                table: "Schools",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifyDate",
                table: "Schools",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                table: "SchoolPosts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifyDate",
                table: "SchoolPosts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                table: "SchoolPostImages",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifyDate",
                table: "SchoolPostImages",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                table: "SchoolPaymentRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifyDate",
                table: "SchoolPaymentRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                table: "SchoolGrades",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifyDate",
                table: "SchoolGrades",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                table: "SchoolExamRateHeaders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifyDate",
                table: "SchoolExamRateHeaders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                table: "SchoolCourses",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifyDate",
                table: "SchoolCourses",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                table: "SchoolContactTypes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifyDate",
                table: "SchoolContactTypes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                table: "SchoolContacts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifyDate",
                table: "SchoolContacts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                table: "Roles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifyDate",
                table: "Roles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                table: "RolePermissions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifyDate",
                table: "RolePermissions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                table: "relationTypes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifyDate",
                table: "relationTypes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                table: "Permissions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifyDate",
                table: "Permissions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                table: "PaymentTypes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifyDate",
                table: "PaymentTypes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                table: "PaymentTransactions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifyDate",
                table: "PaymentTransactions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                table: "EmailProviders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifyDate",
                table: "EmailProviders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                table: "CoinsTypes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifyDate",
                table: "CoinsTypes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                table: "ClassRoomTeacherCourses",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifyDate",
                table: "ClassRoomTeacherCourses",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                table: "ClassRoomStudentExams",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifyDate",
                table: "ClassRoomStudentExams",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                table: "ClassRoomStudentAssignments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifyDate",
                table: "ClassRoomStudentAssignments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                table: "ClassRoomStudentActivities",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifyDate",
                table: "ClassRoomStudentActivities",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                table: "ClassRooms",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifyDate",
                table: "ClassRooms",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                table: "ClassRoomLanguages",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifyDate",
                table: "ClassRoomLanguages",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                table: "ClassRoomExams",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifyDate",
                table: "ClassRoomExams",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                table: "ClassRoomAssignments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifyDate",
                table: "ClassRoomAssignments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                table: "ClassRoomActivities",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifyDate",
                table: "ClassRoomActivities",
                type: "datetime2",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "CoinsTypes",
                keyColumn: "Id",
                keyValue: new Guid("aa619624-c134-4e20-867b-e635a5a3b609"),
                columns: new[] { "CreateDate", "LastModifyDate" },
                values: new object[] { DateTime.Now, DateTime.Now });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("0225dbd5-9675-438c-87f2-63fb6921841c"),
                columns: new[] { "CreateDate", "LastModifyDate" },
                values: new object[] { DateTime.Now, DateTime.Now });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("219007ea-620e-4d96-8292-2d015ef68db1"),
                columns: new[] { "CreateDate", "LastModifyDate" },
                values: new object[] { DateTime.Now, DateTime.Now });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("9301fc37-ae75-4ef6-b6fa-a656452e5a2e"),
                columns: new[] { "CreateDate", "LastModifyDate" },
                values: new object[] { DateTime.Now, DateTime.Now });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("9301fc37-ae75-4ef6-b6fa-a656452e5a2e"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73") },
                columns: new[] { "CreateDate", "LastModifyDate" },
                values: new object[] { DateTime.Now, DateTime.Now });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("219007ea-620e-4d96-8292-2d015ef68db1"), new Guid("92d00b28-9d25-4bd2-a587-6c22a3a07a92") },
                columns: new[] { "CreateDate", "LastModifyDate" },
                values: new object[] { DateTime.Now, DateTime.Now });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("228ae7f5-c704-4660-aeb0-0e1f43112ae1"),
                columns: new[] { "CreateDate", "LastModifyDate" },
                values: new object[] { DateTime.Now, DateTime.Now });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73"),
                columns: new[] { "CreateDate", "LastModifyDate" },
                values: new object[] { DateTime.Now, DateTime.Now });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("6e473a7a-083c-405b-8f0d-04dd85b94edb"),
                columns: new[] { "CreateDate", "LastModifyDate" },
                values: new object[] { DateTime.Now, DateTime.Now });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("92d00b28-9d25-4bd2-a587-6c22a3a07a92"),
                columns: new[] { "CreateDate", "LastModifyDate" },
                values: new object[] { DateTime.Now, DateTime.Now });

            migrationBuilder.UpdateData(
                table: "SchoolTypes",
                keyColumn: "Id",
                keyValue: new Guid("04d490ad-0994-404e-9d8b-8ecf504811f3"),
                columns: new[] { "CreateDate", "LastModifyDate" },
                values: new object[] { DateTime.Now, DateTime.Now });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastModifyDate",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "UserRoles");

            migrationBuilder.DropColumn(
                name: "LastModifyDate",
                table: "UserRoles");

            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "userPermissionSchoolClassRooms");

            migrationBuilder.DropColumn(
                name: "LastModifyDate",
                table: "userPermissionSchoolClassRooms");

            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "userPermissions");

            migrationBuilder.DropColumn(
                name: "LastModifyDate",
                table: "userPermissions");

            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "LastModifyDate",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "StudentParents");

            migrationBuilder.DropColumn(
                name: "LastModifyDate",
                table: "StudentParents");

            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "studentNotes");

            migrationBuilder.DropColumn(
                name: "LastModifyDate",
                table: "studentNotes");

            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "studentExamRates");

            migrationBuilder.DropColumn(
                name: "LastModifyDate",
                table: "studentExamRates");

            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "StudentAttendances");

            migrationBuilder.DropColumn(
                name: "LastModifyDate",
                table: "StudentAttendances");

            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "SMSProviders");

            migrationBuilder.DropColumn(
                name: "LastModifyDate",
                table: "SMSProviders");

            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "SchoolYears");

            migrationBuilder.DropColumn(
                name: "LastModifyDate",
                table: "SchoolYears");

            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "SchoolYearMonths");

            migrationBuilder.DropColumn(
                name: "LastModifyDate",
                table: "SchoolYearMonths");

            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "SchoolUsers");

            migrationBuilder.DropColumn(
                name: "LastModifyDate",
                table: "SchoolUsers");

            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "SchoolTypes");

            migrationBuilder.DropColumn(
                name: "LastModifyDate",
                table: "SchoolTypes");

            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "schoolTeacherCourses");

            migrationBuilder.DropColumn(
                name: "LastModifyDate",
                table: "schoolTeacherCourses");

            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "Schools");

            migrationBuilder.DropColumn(
                name: "LastModifyDate",
                table: "Schools");

            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "SchoolPosts");

            migrationBuilder.DropColumn(
                name: "LastModifyDate",
                table: "SchoolPosts");

            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "SchoolPostImages");

            migrationBuilder.DropColumn(
                name: "LastModifyDate",
                table: "SchoolPostImages");

            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "SchoolPaymentRequests");

            migrationBuilder.DropColumn(
                name: "LastModifyDate",
                table: "SchoolPaymentRequests");

            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "SchoolGrades");

            migrationBuilder.DropColumn(
                name: "LastModifyDate",
                table: "SchoolGrades");

            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "SchoolExamRateHeaders");

            migrationBuilder.DropColumn(
                name: "LastModifyDate",
                table: "SchoolExamRateHeaders");

            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "SchoolCourses");

            migrationBuilder.DropColumn(
                name: "LastModifyDate",
                table: "SchoolCourses");

            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "SchoolContactTypes");

            migrationBuilder.DropColumn(
                name: "LastModifyDate",
                table: "SchoolContactTypes");

            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "SchoolContacts");

            migrationBuilder.DropColumn(
                name: "LastModifyDate",
                table: "SchoolContacts");

            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "LastModifyDate",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "RolePermissions");

            migrationBuilder.DropColumn(
                name: "LastModifyDate",
                table: "RolePermissions");

            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "relationTypes");

            migrationBuilder.DropColumn(
                name: "LastModifyDate",
                table: "relationTypes");

            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "LastModifyDate",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "PaymentTypes");

            migrationBuilder.DropColumn(
                name: "LastModifyDate",
                table: "PaymentTypes");

            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "PaymentTransactions");

            migrationBuilder.DropColumn(
                name: "LastModifyDate",
                table: "PaymentTransactions");

            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "EmailProviders");

            migrationBuilder.DropColumn(
                name: "LastModifyDate",
                table: "EmailProviders");

            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "CoinsTypes");

            migrationBuilder.DropColumn(
                name: "LastModifyDate",
                table: "CoinsTypes");

            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "ClassRoomTeacherCourses");

            migrationBuilder.DropColumn(
                name: "LastModifyDate",
                table: "ClassRoomTeacherCourses");

            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "ClassRoomStudentExams");

            migrationBuilder.DropColumn(
                name: "LastModifyDate",
                table: "ClassRoomStudentExams");

            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "ClassRoomStudentAssignments");

            migrationBuilder.DropColumn(
                name: "LastModifyDate",
                table: "ClassRoomStudentAssignments");

            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "ClassRoomStudentActivities");

            migrationBuilder.DropColumn(
                name: "LastModifyDate",
                table: "ClassRoomStudentActivities");

            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "ClassRooms");

            migrationBuilder.DropColumn(
                name: "LastModifyDate",
                table: "ClassRooms");

            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "ClassRoomLanguages");

            migrationBuilder.DropColumn(
                name: "LastModifyDate",
                table: "ClassRoomLanguages");

            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "ClassRoomExams");

            migrationBuilder.DropColumn(
                name: "LastModifyDate",
                table: "ClassRoomExams");

            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "ClassRoomAssignments");

            migrationBuilder.DropColumn(
                name: "LastModifyDate",
                table: "ClassRoomAssignments");

            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "ClassRoomActivities");

            migrationBuilder.DropColumn(
                name: "LastModifyDate",
                table: "ClassRoomActivities");
        }
    }
}
