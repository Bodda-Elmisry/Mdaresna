using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Infrastructure.Migrations
{
    [DbContext(typeof(Mdaresna.Infrastructure.Data.AppDbContext))]
    [Migration("20260409110000_AddStopSchoolsActivationPermission")]
    public partial class AddStopSchoolsActivationPermission : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Id = 'DDA9E1F1-00A0-4A68-84E7-8E24B9A6C7F1')
BEGIN
    INSERT INTO Permissions (Id, Name, Name_AR, [Key], Description, Description_AR, SchoolPermission, AppPermission, AllowedToMapToClassroom, CreateDate, LastModifyDate, Deleted)
    VALUES ('DDA9E1F1-00A0-4A68-84E7-8E24B9A6C7F1', 'Stop Schools Activation', N'إيقاف تفعيل المدارس', 'StopSchoolsActivation', 'Stop Schools Activation', N'إيقاف تفعيل المدارس', 0, 1, 0, NULL, NULL, 0)
END
IF NOT EXISTS (SELECT 1 FROM RolePermissions WHERE PermissionId = 'DDA9E1F1-00A0-4A68-84E7-8E24B9A6C7F1' AND RoleId = '228AE7F5-C704-4660-AEB0-0E1F43112AE1')
BEGIN
    INSERT INTO RolePermissions (PermissionId, RoleId, CreateDate, LastModifyDate)
    VALUES ('DDA9E1F1-00A0-4A68-84E7-8E24B9A6C7F1', '228AE7F5-C704-4660-AEB0-0E1F43112AE1', NULL, NULL)
END
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DELETE FROM RolePermissions WHERE PermissionId = 'DDA9E1F1-00A0-4A68-84E7-8E24B9A6C7F1' AND RoleId = '228AE7F5-C704-4660-AEB0-0E1F43112AE1';
DELETE FROM Permissions WHERE Id = 'DDA9E1F1-00A0-4A68-84E7-8E24B9A6C7F1';
");
        }
    }
}
