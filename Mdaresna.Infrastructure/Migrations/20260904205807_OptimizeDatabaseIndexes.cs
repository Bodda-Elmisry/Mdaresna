using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class OptimizeDatabaseIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserDevices_UserId",
                table: "UserDevices");

            migrationBuilder.DropIndex(
                name: "IX_studentNotes_StudentId",
                table: "studentNotes");

            migrationBuilder.DropIndex(
                name: "IX_StudentAttendances_StudentId",
                table: "StudentAttendances");

            migrationBuilder.DropIndex(
                name: "IX_SchoolYears_SchoolId",
                table: "SchoolYears");

            migrationBuilder.DropIndex(
                name: "IX_SchoolPosts_SchoolId",
                table: "SchoolPosts");

            migrationBuilder.DropIndex(
                name: "IX_SchoolPostReports_PostId",
                table: "SchoolPostReports");

            migrationBuilder.AlterColumn<string>(
                name: "FcmToken",
                table: "UserDevices",
                type: "nvarchar(440)",
                maxLength: 440,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "DeviceId",
                table: "UserDevices",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_UserDevices_DeviceId",
                table: "UserDevices",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_UserDevices_FcmToken",
                table: "UserDevices",
                column: "FcmToken");

            migrationBuilder.CreateIndex(
                name: "IX_UserDevices_UserId_FcmToken",
                table: "UserDevices",
                columns: new[] { "UserId", "FcmToken" });

            migrationBuilder.CreateIndex(
                name: "IX_studentNotes_StudentId_Date_Active",
                table: "studentNotes",
                columns: new[] { "StudentId", "Date" },
                filter: "[Deleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_StudentAttendances_StudentId_Date_Active",
                table: "StudentAttendances",
                columns: new[] { "StudentId", "Date" },
                filter: "[Deleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolYears_SchoolId_Active_Completed",
                table: "SchoolYears",
                columns: new[] { "SchoolId", "IsActive", "Compleated" },
                filter: "[Deleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolPosts_Feed",
                table: "SchoolPosts",
                columns: new[] { "SchoolId", "ModerationStatus", "Visibility", "LastModifyDate", "PostDate" },
                filter: "[Deleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolPostReports_PostId_Active",
                table: "SchoolPostReports",
                column: "PostId",
                filter: "[Deleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserDevices_DeviceId",
                table: "UserDevices");

            migrationBuilder.DropIndex(
                name: "IX_UserDevices_FcmToken",
                table: "UserDevices");

            migrationBuilder.DropIndex(
                name: "IX_UserDevices_UserId_FcmToken",
                table: "UserDevices");

            migrationBuilder.DropIndex(
                name: "IX_studentNotes_StudentId_Date_Active",
                table: "studentNotes");

            migrationBuilder.DropIndex(
                name: "IX_StudentAttendances_StudentId_Date_Active",
                table: "StudentAttendances");

            migrationBuilder.DropIndex(
                name: "IX_SchoolYears_SchoolId_Active_Completed",
                table: "SchoolYears");

            migrationBuilder.DropIndex(
                name: "IX_SchoolPosts_Feed",
                table: "SchoolPosts");

            migrationBuilder.DropIndex(
                name: "IX_SchoolPostReports_PostId_Active",
                table: "SchoolPostReports");

            migrationBuilder.AlterColumn<string>(
                name: "FcmToken",
                table: "UserDevices",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(440)",
                oldMaxLength: 440);

            migrationBuilder.AlterColumn<string>(
                name: "DeviceId",
                table: "UserDevices",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450);

            migrationBuilder.CreateIndex(
                name: "IX_UserDevices_UserId",
                table: "UserDevices",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_studentNotes_StudentId",
                table: "studentNotes",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentAttendances_StudentId",
                table: "StudentAttendances",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolYears_SchoolId",
                table: "SchoolYears",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolPosts_SchoolId",
                table: "SchoolPosts",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolPostReports_PostId",
                table: "SchoolPostReports",
                column: "PostId");
        }
    }
}
