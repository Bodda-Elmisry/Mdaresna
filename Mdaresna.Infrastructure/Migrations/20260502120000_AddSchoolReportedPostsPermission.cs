using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Infrastructure.Migrations
{
    [DbContext(typeof(Mdaresna.Infrastructure.Data.AppDbContext))]
    [Migration("20260502120000_AddSchoolReportedPostsPermission")]
    public partial class AddSchoolReportedPostsPermission : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF NOT EXISTS (
    SELECT 1
    FROM Permissions
    WHERE Id = '2E6D4D17-1E34-4F7D-A8D8-1E4B1C737BB4'
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
        '2E6D4D17-1E34-4F7D-A8D8-1E4B1C737BB4',
        0,
        0,
        0,
        'View reported posts for the selected school',
        N'عرض المنشورات المبلغ عنها للمدرسة المحددة',
        'ShowSchoolReportedPosts',
        'Show School Reported Posts',
        N'عرض المنشورات المبلغ عنها للمدرسة',
        1
    )
END

IF NOT EXISTS (
    SELECT 1
    FROM RolePermissions
    WHERE PermissionId = '2E6D4D17-1E34-4F7D-A8D8-1E4B1C737BB4'
      AND RoleId = '4B8A99FE-B759-4C18-9500-8052C3D7AC73'
)
BEGIN
    INSERT INTO RolePermissions (PermissionId, RoleId, CreateDate, LastModifyDate)
    VALUES ('2E6D4D17-1E34-4F7D-A8D8-1E4B1C737BB4', '4B8A99FE-B759-4C18-9500-8052C3D7AC73', NULL, NULL)
END
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DELETE FROM RolePermissions
WHERE PermissionId = '2E6D4D17-1E34-4F7D-A8D8-1E4B1C737BB4'
  AND RoleId = '4B8A99FE-B759-4C18-9500-8052C3D7AC73';

DELETE FROM Permissions
WHERE Id = '2E6D4D17-1E34-4F7D-A8D8-1E4B1C737BB4';
");
        }
    }
}
