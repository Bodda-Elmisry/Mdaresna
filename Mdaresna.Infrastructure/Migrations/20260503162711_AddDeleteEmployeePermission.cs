using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Infrastructure.Migrations
{
    public partial class AddDeleteEmployeePermission : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF NOT EXISTS (
    SELECT 1
    FROM Permissions
    WHERE Id = '5ED6B95D-CD84-4B2C-BEB8-88686933EA78'
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
        '5ED6B95D-CD84-4B2C-BEB8-88686933EA78',
        0,
        0,
        0,
        'Delete Employee',
        N'حذف الموظف',
        'DeleteEmployee',
        'Delete Employee',
        N'حذف الموظف',
        1
    )
END

IF NOT EXISTS (
    SELECT 1
    FROM RolePermissions
    WHERE PermissionId = '5ED6B95D-CD84-4B2C-BEB8-88686933EA78'
      AND RoleId = '4B8A99FE-B759-4C18-9500-8052C3D7AC73'
)
BEGIN
    INSERT INTO RolePermissions (PermissionId, RoleId, CreateDate, LastModifyDate)
    VALUES ('5ED6B95D-CD84-4B2C-BEB8-88686933EA78', '4B8A99FE-B759-4C18-9500-8052C3D7AC73', NULL, NULL)
END
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DELETE FROM RolePermissions
WHERE PermissionId = '5ED6B95D-CD84-4B2C-BEB8-88686933EA78'
  AND RoleId = '4B8A99FE-B759-4C18-9500-8052C3D7AC73';

DELETE FROM Permissions
WHERE Id = '5ED6B95D-CD84-4B2C-BEB8-88686933EA78';
");
        }
    }
}
