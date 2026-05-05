using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Infrastructure.Migrations
{
    [DbContext(typeof(Mdaresna.Infrastructure.Data.AppDbContext))]
    [Migration("20260425123000_MoveShowReportedPostsToApplicationManager")]
    public partial class MoveShowReportedPostsToApplicationManager : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
UPDATE Permissions
SET SchoolPermission = 0,
    AppPermission = 1
WHERE Id = 'A44F8B6E-7B16-4497-9C4B-8E55EAF4E7D4';

DELETE FROM RolePermissions
WHERE PermissionId = 'A44F8B6E-7B16-4497-9C4B-8E55EAF4E7D4'
  AND RoleId = '4B8A99FE-B759-4C18-9500-8052C3D7AC73';

IF NOT EXISTS (
    SELECT 1
    FROM RolePermissions
    WHERE PermissionId = 'A44F8B6E-7B16-4497-9C4B-8E55EAF4E7D4'
      AND RoleId = '228AE7F5-C704-4660-AEB0-0E1F43112AE1'
)
BEGIN
    INSERT INTO RolePermissions (PermissionId, RoleId, CreateDate, LastModifyDate)
    VALUES ('A44F8B6E-7B16-4497-9C4B-8E55EAF4E7D4', '228AE7F5-C704-4660-AEB0-0E1F43112AE1', NULL, NULL)
END
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
UPDATE Permissions
SET SchoolPermission = 1,
    AppPermission = 0
WHERE Id = 'A44F8B6E-7B16-4497-9C4B-8E55EAF4E7D4';

DELETE FROM RolePermissions
WHERE PermissionId = 'A44F8B6E-7B16-4497-9C4B-8E55EAF4E7D4'
  AND RoleId = '228AE7F5-C704-4660-AEB0-0E1F43112AE1';

IF NOT EXISTS (
    SELECT 1
    FROM RolePermissions
    WHERE PermissionId = 'A44F8B6E-7B16-4497-9C4B-8E55EAF4E7D4'
      AND RoleId = '4B8A99FE-B759-4C18-9500-8052C3D7AC73'
)
BEGIN
    INSERT INTO RolePermissions (PermissionId, RoleId, CreateDate, LastModifyDate)
    VALUES ('A44F8B6E-7B16-4497-9C4B-8E55EAF4E7D4', '4B8A99FE-B759-4C18-9500-8052C3D7AC73', NULL, NULL)
END
");
        }
    }
}
