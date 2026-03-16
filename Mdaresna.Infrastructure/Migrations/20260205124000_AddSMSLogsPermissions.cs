using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Infrastructure.Migrations
{
    [DbContext(typeof(Mdaresna.Infrastructure.Data.AppDbContext))]
    [Migration("20260205124000_AddSMSLogsPermissions")]
    public partial class AddSMSLogsPermissions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Id = 'C1F5A23E-3B9C-4E57-8AD8-1BBF2F7A2C4D')
BEGIN
    INSERT INTO Permissions (Id, Name, Name_AR, [Key], Description, Description_AR, SchoolPermission, AppPermission, AllowedToMapToClassroom, CreateDate, LastModifyDate, Deleted)
    VALUES ('C1F5A23E-3B9C-4E57-8AD8-1BBF2F7A2C4D', 'View SMS Logs', N'عرض سجل الرسائل', 'ViewSMSLogs', 'View SMS logs', N'عرض سجل الرسائل', 0, 1, 0, NULL, NULL, 0)
END
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Id = '0C7C9F0E-8F3C-4F44-9D83-0C2E5B1C61F1')
BEGIN
    INSERT INTO Permissions (Id, Name, Name_AR, [Key], Description, Description_AR, SchoolPermission, AppPermission, AllowedToMapToClassroom, CreateDate, LastModifyDate, Deleted)
    VALUES ('0C7C9F0E-8F3C-4F44-9D83-0C2E5B1C61F1', 'Export SMS Logs', N'تصدير سجل الرسائل', 'ExportSMSLogs', 'Export SMS logs', N'تصدير سجل الرسائل', 0, 1, 0, NULL, NULL, 0)
END
IF NOT EXISTS (SELECT 1 FROM RolePermissions WHERE PermissionId = 'C1F5A23E-3B9C-4E57-8AD8-1BBF2F7A2C4D' AND RoleId = '228AE7F5-C704-4660-AEB0-0E1F43112AE1')
BEGIN
    INSERT INTO RolePermissions (PermissionId, RoleId, CreateDate, LastModifyDate)
    VALUES ('C1F5A23E-3B9C-4E57-8AD8-1BBF2F7A2C4D', '228AE7F5-C704-4660-AEB0-0E1F43112AE1', NULL, NULL)
END
IF NOT EXISTS (SELECT 1 FROM RolePermissions WHERE PermissionId = '0C7C9F0E-8F3C-4F44-9D83-0C2E5B1C61F1' AND RoleId = '228AE7F5-C704-4660-AEB0-0E1F43112AE1')
BEGIN
    INSERT INTO RolePermissions (PermissionId, RoleId, CreateDate, LastModifyDate)
    VALUES ('0C7C9F0E-8F3C-4F44-9D83-0C2E5B1C61F1', '228AE7F5-C704-4660-AEB0-0E1F43112AE1', NULL, NULL)
END
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DELETE FROM RolePermissions WHERE PermissionId = 'C1F5A23E-3B9C-4E57-8AD8-1BBF2F7A2C4D' AND RoleId = '228AE7F5-C704-4660-AEB0-0E1F43112AE1';
DELETE FROM RolePermissions WHERE PermissionId = '0C7C9F0E-8F3C-4F44-9D83-0C2E5B1C61F1' AND RoleId = '228AE7F5-C704-4660-AEB0-0E1F43112AE1';
DELETE FROM Permissions WHERE Id = 'C1F5A23E-3B9C-4E57-8AD8-1BBF2F7A2C4D';
DELETE FROM Permissions WHERE Id = '0C7C9F0E-8F3C-4F44-9D83-0C2E5B1C61F1';
");
        }
    }
}
