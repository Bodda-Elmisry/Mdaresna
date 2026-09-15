using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Platform.Infrastructure.Persistence.PostgreSql.Migrations.Platform
{
    /// <inheritdoc />
    public partial class AddSchoolDatabaseEndpointRegistry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "school_database_endpoints",
                schema: "registry",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SchoolId = table.Column<Guid>(type: "uuid", nullable: false),
                    Purpose = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Provider = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Host = table.Column<string>(type: "character varying(253)", maxLength: 253, nullable: false),
                    Port = table.Column<int>(type: "integer", nullable: false),
                    DatabaseName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    CredentialSecretReference = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    RequireTls = table.Column<bool>(type: "boolean", nullable: false),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Region = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SchemaVersion = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp(3) with time zone", precision: 3, nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp(3) with time zone", precision: 3, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_school_database_endpoints", x => x.Id);
                    table.CheckConstraint("ck_registry_school_database_endpoints_port", "\"Port\" >= 1 AND \"Port\" <= 65535");
                    table.CheckConstraint("ck_registry_school_database_endpoints_provider", "\"Provider\" IN ('PostgreSql', 'SqlServer')");
                    table.CheckConstraint("ck_registry_school_database_endpoints_purpose", "\"Purpose\" IN ('Operational', 'Reporting', 'Archive', 'ReadReplica')");
                    table.CheckConstraint("ck_registry_school_database_endpoints_retired_primary", "\"Status\" <> 'Retired' OR \"IsPrimary\" = FALSE");
                    table.CheckConstraint("ck_registry_school_database_endpoints_status", "\"Status\" IN ('Provisioning', 'Active', 'Unavailable', 'Retired')");
                    table.CheckConstraint("ck_registry_school_database_endpoints_timestamps", "\"CreatedAtUtc\" <= \"UpdatedAtUtc\" AND TRUE AND TRUE");
                    table.CheckConstraint("ck_registry_school_database_endpoints_version", "\"Version\" >= 0");
                    table.ForeignKey(
                        name: "FK_school_database_endpoints_schools_SchoolId",
                        column: x => x.SchoolId,
                        principalSchema: "registry",
                        principalTable: "schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_school_database_endpoints_SchoolId_Host_Port_DatabaseName",
                schema: "registry",
                table: "school_database_endpoints",
                columns: new[] { "SchoolId", "Host", "Port", "DatabaseName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_school_database_endpoints_SchoolId_Purpose_IsPrimary",
                schema: "registry",
                table: "school_database_endpoints",
                columns: new[] { "SchoolId", "Purpose", "IsPrimary" },
                unique: true,
                filter: "\"IsPrimary\" = TRUE");

            migrationBuilder.CreateIndex(
                name: "IX_school_database_endpoints_SchoolId_Purpose_Status",
                schema: "registry",
                table: "school_database_endpoints",
                columns: new[] { "SchoolId", "Purpose", "Status" });

            migrationBuilder.Sql("""
                CREATE TRIGGER set_rowversion BEFORE INSERT OR UPDATE
                ON registry.school_database_endpoints
                FOR EACH ROW EXECUTE FUNCTION platform.set_rowversion();
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "DROP TRIGGER IF EXISTS set_rowversion ON registry.school_database_endpoints;");
            migrationBuilder.DropTable(
                name: "school_database_endpoints",
                schema: "registry");
        }
    }
}
