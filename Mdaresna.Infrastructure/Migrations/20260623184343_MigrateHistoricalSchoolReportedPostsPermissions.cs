using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MigrateHistoricalSchoolReportedPostsPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. RolePermissions: ShowReportedPosts -> ShowSchoolReportedPosts (except Application Manager)
            migrationBuilder.Sql(@"
DELETE rp
FROM RolePermissions rp
WHERE rp.PermissionId = 'A44F8B6E-7B16-4497-9C4B-8E55EAF4E7D4'
  AND rp.RoleId <> '228AE7F5-C704-4660-AEB0-0E1F43112AE1'
  AND EXISTS (
      SELECT 1
      FROM RolePermissions rp2
      WHERE rp2.RoleId = rp.RoleId
        AND rp2.PermissionId = '2E6D4D17-1E34-4F7D-A8D8-1E4B1C737BB4'
  );

UPDATE rp
SET rp.PermissionId = '2E6D4D17-1E34-4F7D-A8D8-1E4B1C737BB4'
FROM RolePermissions rp
WHERE rp.PermissionId = 'A44F8B6E-7B16-4497-9C4B-8E55EAF4E7D4'
  AND rp.RoleId <> '228AE7F5-C704-4660-AEB0-0E1F43112AE1';
");

            // 2. RolePermissions: ShowReportedUsers -> ShowSchoolReportedUsers (except Application Manager)
            migrationBuilder.Sql(@"
DELETE rp
FROM RolePermissions rp
WHERE rp.PermissionId = '4C1E2D42-8F6C-4B8A-9A80-6A8F6D034D5A'
  AND rp.RoleId <> '228AE7F5-C704-4660-AEB0-0E1F43112AE1'
  AND EXISTS (
      SELECT 1
      FROM RolePermissions rp2
      WHERE rp2.RoleId = rp.RoleId
        AND rp2.PermissionId = 'EA7E7A5D-64C0-4AE6-B8B1-70A7D7E4F66C'
  );

UPDATE rp
SET rp.PermissionId = 'EA7E7A5D-64C0-4AE6-B8B1-70A7D7E4F66C'
FROM RolePermissions rp
WHERE rp.PermissionId = '4C1E2D42-8F6C-4B8A-9A80-6A8F6D034D5A'
  AND rp.RoleId <> '228AE7F5-C704-4660-AEB0-0E1F43112AE1';
");

            // 3. userPermissions: ShowReportedPosts -> ShowSchoolReportedPosts
            migrationBuilder.Sql(@"
DELETE up
FROM userPermissions up
WHERE up.PermissionId = 'A44F8B6E-7B16-4497-9C4B-8E55EAF4E7D4'
  AND up.SchoolId IS NOT NULL
  AND EXISTS (
      SELECT 1
      FROM userPermissions up2
      WHERE up2.UserId = up.UserId
        AND up2.SchoolId = up.SchoolId
        AND up2.PermissionId = '2E6D4D17-1E34-4F7D-A8D8-1E4B1C737BB4'
  );

UPDATE userPermissions
SET PermissionId = '2E6D4D17-1E34-4F7D-A8D8-1E4B1C737BB4'
WHERE PermissionId = 'A44F8B6E-7B16-4497-9C4B-8E55EAF4E7D4'
  AND SchoolId IS NOT NULL;
");

            // 4. userPermissions: ShowReportedUsers -> ShowSchoolReportedUsers
            migrationBuilder.Sql(@"
DELETE up
FROM userPermissions up
WHERE up.PermissionId = '4C1E2D42-8F6C-4B8A-9A80-6A8F6D034D5A'
  AND up.SchoolId IS NOT NULL
  AND EXISTS (
      SELECT 1
      FROM userPermissions up2
      WHERE up2.UserId = up.UserId
        AND up2.SchoolId = up.SchoolId
        AND up2.PermissionId = 'EA7E7A5D-64C0-4AE6-B8B1-70A7D7E4F66C'
  );

UPDATE userPermissions
SET PermissionId = 'EA7E7A5D-64C0-4AE6-B8B1-70A7D7E4F66C'
WHERE PermissionId = '4C1E2D42-8F6C-4B8A-9A80-6A8F6D034D5A'
  AND SchoolId IS NOT NULL;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // 1. RolePermissions: ShowSchoolReportedPosts -> ShowReportedPosts (except Application Manager)
            migrationBuilder.Sql(@"
DELETE rp
FROM RolePermissions rp
WHERE rp.PermissionId = '2E6D4D17-1E34-4F7D-A8D8-1E4B1C737BB4'
  AND rp.RoleId <> '228AE7F5-C704-4660-AEB0-0E1F43112AE1'
  AND EXISTS (
      SELECT 1
      FROM RolePermissions rp2
      WHERE rp2.RoleId = rp.RoleId
        AND rp2.PermissionId = 'A44F8B6E-7B16-4497-9C4B-8E55EAF4E7D4'
  );

UPDATE rp
SET rp.PermissionId = 'A44F8B6E-7B16-4497-9C4B-8E55EAF4E7D4'
FROM RolePermissions rp
WHERE rp.PermissionId = '2E6D4D17-1E34-4F7D-A8D8-1E4B1C737BB4'
  AND rp.RoleId <> '228AE7F5-C704-4660-AEB0-0E1F43112AE1';
");

            // 2. RolePermissions: ShowSchoolReportedUsers -> ShowReportedUsers (except Application Manager)
            migrationBuilder.Sql(@"
DELETE rp
FROM RolePermissions rp
WHERE rp.PermissionId = 'EA7E7A5D-64C0-4AE6-B8B1-70A7D7E4F66C'
  AND rp.RoleId <> '228AE7F5-C704-4660-AEB0-0E1F43112AE1'
  AND EXISTS (
      SELECT 1
      FROM RolePermissions rp2
      WHERE rp2.RoleId = rp.RoleId
        AND rp2.PermissionId = '4C1E2D42-8F6C-4B8A-9A80-6A8F6D034D5A'
  );

UPDATE rp
SET rp.PermissionId = '4C1E2D42-8F6C-4B8A-9A80-6A8F6D034D5A'
FROM RolePermissions rp
WHERE rp.PermissionId = 'EA7E7A5D-64C0-4AE6-B8B1-70A7D7E4F66C'
  AND rp.RoleId <> '228AE7F5-C704-4660-AEB0-0E1F43112AE1';
");

            // 3. userPermissions: ShowSchoolReportedPosts -> ShowReportedPosts
            migrationBuilder.Sql(@"
DELETE up
FROM userPermissions up
WHERE up.PermissionId = '2E6D4D17-1E34-4F7D-A8D8-1E4B1C737BB4'
  AND up.SchoolId IS NOT NULL
  AND EXISTS (
      SELECT 1
      FROM userPermissions up2
      WHERE up2.UserId = up.UserId
        AND up2.SchoolId = up.SchoolId
        AND up2.PermissionId = 'A44F8B6E-7B16-4497-9C4B-8E55EAF4E7D4'
  );

UPDATE userPermissions
SET PermissionId = 'A44F8B6E-7B16-4497-9C4B-8E55EAF4E7D4'
WHERE PermissionId = '2E6D4D17-1E34-4F7D-A8D8-1E4B1C737BB4'
  AND SchoolId IS NOT NULL;
");

            // 4. userPermissions: ShowSchoolReportedUsers -> ShowReportedUsers
            migrationBuilder.Sql(@"
DELETE up
FROM userPermissions up
WHERE up.PermissionId = 'EA7E7A5D-64C0-4AE6-B8B1-70A7D7E4F66C'
  AND up.SchoolId IS NOT NULL
  AND EXISTS (
      SELECT 1
      FROM userPermissions up2
      WHERE up2.UserId = up.UserId
        AND up2.SchoolId = up.SchoolId
        AND up2.PermissionId = '4C1E2D42-8F6C-4B8A-9A80-6A8F6D034D5A'
  );

UPDATE userPermissions
SET PermissionId = '4C1E2D42-8F6C-4B8A-9A80-6A8F6D034D5A'
WHERE PermissionId = 'EA7E7A5D-64C0-4AE6-B8B1-70A7D7E4F66C'
  AND SchoolId IS NOT NULL;
");
        }
    }
}
