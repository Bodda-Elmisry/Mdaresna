using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDeleteToDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "UserRoles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "userPermissionSchoolClassRooms",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "userPermissions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "Students",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "StudentParents",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "studentNotes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "studentExamRates",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "StudentAttendances",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "SMSProviders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "SchoolYears",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "SchoolYearMonths",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "SchoolUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "SchoolTypes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "schoolTeachers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "schoolTeacherCourses",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "Schools",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "SchoolPosts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "SchoolPostImages",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "SchoolPaymentRequests",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "SchoolGrades",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "SchoolExamRateHeaders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "SchoolEmployees",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "SchoolCourses",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "SchoolContactTypes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "SchoolContacts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "Roles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "RolePermissions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "relationTypes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "Permissions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "PaymentTypes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "PaymentTransactions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "Languages",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "EmailProviders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "CoinsTypes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "ClassRoomTeacherCourses",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "ClassRoomStudentExams",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "ClassRoomStudentAssignments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "ClassRoomStudentActivities",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "ClassRooms",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "ClassRoomLanguages",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "ClassRoomExams",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "ClassroomEmployees",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "ClassRoomAssignments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "ClassRoomActivities",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "UserRoles");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "userPermissionSchoolClassRooms");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "userPermissions");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "StudentParents");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "studentNotes");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "studentExamRates");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "StudentAttendances");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "SMSProviders");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "SchoolYears");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "SchoolYearMonths");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "SchoolUsers");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "SchoolTypes");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "schoolTeachers");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "schoolTeacherCourses");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "Schools");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "SchoolPosts");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "SchoolPostImages");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "SchoolPaymentRequests");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "SchoolGrades");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "SchoolExamRateHeaders");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "SchoolEmployees");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "SchoolCourses");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "SchoolContactTypes");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "SchoolContacts");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "RolePermissions");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "relationTypes");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "PaymentTypes");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "PaymentTransactions");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "Languages");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "EmailProviders");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "CoinsTypes");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "ClassRoomTeacherCourses");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "ClassRoomStudentExams");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "ClassRoomStudentAssignments");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "ClassRoomStudentActivities");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "ClassRooms");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "ClassRoomLanguages");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "ClassRoomExams");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "ClassroomEmployees");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "ClassRoomAssignments");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "ClassRoomActivities");
        }
    }
}
