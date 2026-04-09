using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Infrastructure.Migrations
{
    [DbContext(typeof(Mdaresna.Infrastructure.Data.AppDbContext))]
    [Migration("20260409113000_RenameStopSchoolsActivationPermissionToAllowCompleteSchoolYear")]
    public partial class RenameStopSchoolsActivationPermissionToAllowCompleteSchoolYear : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
UPDATE Permissions
SET
    Name = 'Allow Complete School Year',
    Name_AR = N'السماح بإكمال السنة الدراسية',
    [Key] = 'AllowCompleteSchoolYear',
    Description = 'Allow Complete School Year',
    Description_AR = N'السماح بإكمال السنة الدراسية'
WHERE Id = 'DDA9E1F1-00A0-4A68-84E7-8E24B9A6C7F1'
   OR [Key] = 'StopSchoolsActivation';
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
UPDATE Permissions
SET
    Name = 'Stop Schools Activation',
    Name_AR = N'إيقاف تفعيل المدارس',
    [Key] = 'StopSchoolsActivation',
    Description = 'Stop Schools Activation',
    Description_AR = N'إيقاف تفعيل المدارس'
WHERE Id = 'DDA9E1F1-00A0-4A68-84E7-8E24B9A6C7F1'
   OR [Key] = 'AllowCompleteSchoolYear';
");
        }
    }
}
