using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Platform.Infrastructure.Persistence.PostgreSql.Migrations.Platform
{
    /// <inheritdoc />
    public partial class AddPostgreSqlSchoolDatabaseMigrationControl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<System.DateTimeOffset>(name: "LastMigrationCompletedAtUtc",
                schema: "registry", table: "school_database_endpoints",
                type: "timestamp with time zone", precision: 3, nullable: true);
            migrationBuilder.AddColumn<string>(name: "LastMigrationError",
                schema: "registry", table: "school_database_endpoints",
                type: "character varying(2000)", maxLength: 2000, nullable: true);
            migrationBuilder.AddColumn<System.Guid>(name: "LastMigrationOperationId",
                schema: "registry", table: "school_database_endpoints", type: "uuid", nullable: true);
            migrationBuilder.AddColumn<System.DateTimeOffset>(name: "LastMigrationRequestedAtUtc",
                schema: "registry", table: "school_database_endpoints",
                type: "timestamp with time zone", precision: 3, nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "MigrationStatus",
                schema: "registry",
                table: "school_database_endpoints",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "NeverRun");
            migrationBuilder.InsertData(schema: "access", table: "permissions",
                columns: new[] { "Code", "Description" },
                values: new object[] { "platform.schools.migrations.execute", null });
            migrationBuilder.Sql("""
                INSERT INTO access.role_permissions ("RoleId", "PermissionCode")
                SELECT r."Id", 'platform.schools.migrations.execute'
                FROM access.roles r
                WHERE r."Key" = 'app-manager'
                  AND NOT EXISTS (
                    SELECT 1 FROM access.role_permissions rp
                    WHERE rp."RoleId" = r."Id"
                      AND rp."PermissionCode" = 'platform.schools.migrations.execute');
                """);
            migrationBuilder.CreateIndex(
                name: "IX_school_database_endpoints_MigrationStatus_LastMigrationRequestedAtUtc",
                schema: "registry", table: "school_database_endpoints",
                columns: new[] { "MigrationStatus", "LastMigrationRequestedAtUtc" });
            migrationBuilder.AddCheckConstraint(
                name: "ck_registry_school_database_endpoints_migration_status",
                schema: "registry", table: "school_database_endpoints",
                sql: "\"MigrationStatus\" IN ('NeverRun', 'Pending', 'Succeeded', 'Failed')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "DELETE FROM access.role_permissions WHERE \"PermissionCode\" = 'platform.schools.migrations.execute';");
            migrationBuilder.DropIndex(
                name: "IX_school_database_endpoints_MigrationStatus_LastMigrationRequestedAtUtc",
                schema: "registry", table: "school_database_endpoints");
            migrationBuilder.DropCheckConstraint(
                name: "ck_registry_school_database_endpoints_migration_status",
                schema: "registry", table: "school_database_endpoints");
            migrationBuilder.DeleteData(schema: "access", table: "permissions",
                keyColumn: "Code", keyValue: "platform.schools.migrations.execute");
            migrationBuilder.DropColumn(name: "LastMigrationCompletedAtUtc", schema: "registry", table: "school_database_endpoints");
            migrationBuilder.DropColumn(name: "LastMigrationError", schema: "registry", table: "school_database_endpoints");
            migrationBuilder.DropColumn(name: "LastMigrationOperationId", schema: "registry", table: "school_database_endpoints");
            migrationBuilder.DropColumn(name: "LastMigrationRequestedAtUtc", schema: "registry", table: "school_database_endpoints");
            migrationBuilder.DropColumn(name: "MigrationStatus", schema: "registry", table: "school_database_endpoints");
        }
    }
}
