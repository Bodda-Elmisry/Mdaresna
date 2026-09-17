using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Platform.Infrastructure.Persistence.Platform.Migrations
{
    /// <inheritdoc />
    public partial class AddSchoolDatabaseMigrationControl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastMigrationCompletedAtUtc",
                schema: "registry",
                table: "school_database_endpoints",
                type: "datetimeoffset(3)",
                precision: 3,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastMigrationError",
                schema: "registry",
                table: "school_database_endpoints",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastMigrationOperationId",
                schema: "registry",
                table: "school_database_endpoints",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastMigrationRequestedAtUtc",
                schema: "registry",
                table: "school_database_endpoints",
                type: "datetimeoffset(3)",
                precision: 3,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MigrationStatus",
                schema: "registry",
                table: "school_database_endpoints",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "NeverRun");

            migrationBuilder.InsertData(
                schema: "access",
                table: "permissions",
                columns: new[] { "Code", "Description" },
                values: new object[] { "platform.schools.migrations.execute", null });

            migrationBuilder.Sql("""
                INSERT INTO [access].[role_permissions] ([RoleId], [PermissionCode])
                SELECT [Id], N'platform.schools.migrations.execute'
                FROM [access].[roles]
                WHERE [Key] = N'app-manager'
                  AND NOT EXISTS (
                    SELECT 1 FROM [access].[role_permissions]
                    WHERE [RoleId] = [roles].[Id]
                      AND [PermissionCode] = N'platform.schools.migrations.execute');
                """);

            migrationBuilder.CreateIndex(
                name: "IX_school_database_endpoints_MigrationStatus_LastMigrationRequestedAtUtc",
                schema: "registry",
                table: "school_database_endpoints",
                columns: new[] { "MigrationStatus", "LastMigrationRequestedAtUtc" });

            migrationBuilder.AddCheckConstraint(
                name: "ck_registry_school_database_endpoints_migration_status",
                schema: "registry",
                table: "school_database_endpoints",
                sql: "[MigrationStatus] IN (N'NeverRun', N'Pending', N'Succeeded', N'Failed')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM [access].[role_permissions]
                WHERE [PermissionCode] = N'platform.schools.migrations.execute';
                """);

            migrationBuilder.DropIndex(
                name: "IX_school_database_endpoints_MigrationStatus_LastMigrationRequestedAtUtc",
                schema: "registry",
                table: "school_database_endpoints");

            migrationBuilder.DropCheckConstraint(
                name: "ck_registry_school_database_endpoints_migration_status",
                schema: "registry",
                table: "school_database_endpoints");

            migrationBuilder.DeleteData(
                schema: "access",
                table: "permissions",
                keyColumn: "Code",
                keyValue: "platform.schools.migrations.execute");

            migrationBuilder.DropColumn(
                name: "LastMigrationCompletedAtUtc",
                schema: "registry",
                table: "school_database_endpoints");

            migrationBuilder.DropColumn(
                name: "LastMigrationError",
                schema: "registry",
                table: "school_database_endpoints");

            migrationBuilder.DropColumn(
                name: "LastMigrationOperationId",
                schema: "registry",
                table: "school_database_endpoints");

            migrationBuilder.DropColumn(
                name: "LastMigrationRequestedAtUtc",
                schema: "registry",
                table: "school_database_endpoints");

            migrationBuilder.DropColumn(
                name: "MigrationStatus",
                schema: "registry",
                table: "school_database_endpoints");
        }
    }
}
