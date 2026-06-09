using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Infrastructure.Migrations
{
    [DbContext(typeof(Mdaresna.Infrastructure.Data.AppDbContext))]
    [Migration("20260609214127_AddReviewSchoolMonthReportPermission")]
    public partial class AddReviewSchoolMonthReportPermission : Migration
    {
        private const string PermissionId = "5F3FF85F-D4BB-46BE-B0E6-942C10A87B4A";
        private const string SchoolManagerRoleId = "4B8A99FE-B759-4C18-9500-8052C3D7AC73";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
IF NOT EXISTS (
    SELECT 1
    FROM Permissions
    WHERE Id = '{PermissionId}'
)
BEGIN
    INSERT INTO Permissions (
        Id,
        AllowedToMapToClassroom,
        AppPermission,
        Deleted,
        Description,
        Description_AR,
        [Key],
        [Name],
        Name_AR,
        SchoolPermission
    )
    VALUES (
        '{PermissionId}',
        0,
        0,
        0,
        'Review school monthly report',
        N'مراجعة التقرير الشهري للمدرسة',
        'ReviewSchoolMonthReport',
        'Review School Month Report',
        N'مراجعة التقرير الشهري للمدرسة',
        1
    )
END

IF NOT EXISTS (
    SELECT 1
    FROM RolePermissions
    WHERE PermissionId = '{PermissionId}'
      AND RoleId = '{SchoolManagerRoleId}'
)
BEGIN
    INSERT INTO RolePermissions (PermissionId, RoleId, CreateDate, LastModifyDate)
    VALUES ('{PermissionId}', '{SchoolManagerRoleId}', NULL, NULL)
END
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
DELETE FROM RolePermissions
WHERE PermissionId = '{PermissionId}'
  AND RoleId = '{SchoolManagerRoleId}';

DELETE FROM Permissions
WHERE Id = '{PermissionId}';
");
        }
    }
}
