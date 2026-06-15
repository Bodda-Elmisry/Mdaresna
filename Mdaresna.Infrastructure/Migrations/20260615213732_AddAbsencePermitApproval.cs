using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAbsencePermitApproval : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReportStatus",
                table: "SchoolYearMonths");

            migrationBuilder.AddColumn<Guid>(
                name: "ReviewedById",
                table: "StudentAbsencePermits",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "StudentAbsencePermits",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SupervisorNotes",
                table: "StudentAbsencePermits",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "AppPermission", "CreateDate", "Description", "Description_AR", "Key", "LastModifyDate", "Name", "Name_AR", "SchoolPermission" },
                values: new object[] { new Guid("e593cd01-0ff3-4ae1-9df6-61e9d7c23f5d"), false, null, "Approve or reject student absence permits submitted by parents", "الموافقة أو رفض أعذار غياب الطلاب المقدمة من أولياء الأمور", "ApproveAbsencePermit", null, "Approve Student Absence Permit", "الموافقة على أعذار غياب الطلاب", true });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "PermissionId", "RoleId", "CreateDate", "LastModifyDate" },
                values: new object[] { new Guid("e593cd01-0ff3-4ae1-9df6-61e9d7c23f5d"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73"), null, null });

            migrationBuilder.CreateIndex(
                name: "IX_StudentAbsencePermits_ReviewedById",
                table: "StudentAbsencePermits",
                column: "ReviewedById");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentAbsencePermits_Users_ReviewedById",
                table: "StudentAbsencePermits",
                column: "ReviewedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudentAbsencePermits_Users_ReviewedById",
                table: "StudentAbsencePermits");

            migrationBuilder.DropIndex(
                name: "IX_StudentAbsencePermits_ReviewedById",
                table: "StudentAbsencePermits");

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("e593cd01-0ff3-4ae1-9df6-61e9d7c23f5d"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73") });

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("e593cd01-0ff3-4ae1-9df6-61e9d7c23f5d"));

            migrationBuilder.DropColumn(
                name: "ReviewedById",
                table: "StudentAbsencePermits");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "StudentAbsencePermits");

            migrationBuilder.DropColumn(
                name: "SupervisorNotes",
                table: "StudentAbsencePermits");

            migrationBuilder.AddColumn<int>(
                name: "ReportStatus",
                table: "SchoolYearMonths",
                type: "int",
                nullable: false,
                defaultValue: 1);
        }
    }
}
