using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Platform.Infrastructure.Persistence.Platform.Migrations
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
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SchoolId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Purpose = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Provider = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Host = table.Column<string>(type: "nvarchar(253)", maxLength: 253, nullable: false),
                    Port = table.Column<int>(type: "int", nullable: false),
                    DatabaseName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    CredentialSecretReference = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    RequireTls = table.Column<bool>(type: "bit", nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Region = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SchemaVersion = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_school_database_endpoints", x => x.Id);
                    table.CheckConstraint("ck_registry_school_database_endpoints_port", "[Port] >= 1 AND [Port] <= 65535");
                    table.CheckConstraint("ck_registry_school_database_endpoints_provider", "[Provider] IN (N'PostgreSql', N'SqlServer')");
                    table.CheckConstraint("ck_registry_school_database_endpoints_purpose", "[Purpose] IN (N'Operational', N'Reporting', N'Archive', N'ReadReplica')");
                    table.CheckConstraint("ck_registry_school_database_endpoints_retired_primary", "[Status] <> N'Retired' OR [IsPrimary] = 0");
                    table.CheckConstraint("ck_registry_school_database_endpoints_status", "[Status] IN (N'Provisioning', N'Active', N'Unavailable', N'Retired')");
                    table.CheckConstraint("ck_registry_school_database_endpoints_timestamps", "[CreatedAtUtc] <= [UpdatedAtUtc] AND DATEPART(TZOFFSET, [CreatedAtUtc]) = 0 AND DATEPART(TZOFFSET, [UpdatedAtUtc]) = 0");
                    table.CheckConstraint("ck_registry_school_database_endpoints_version", "[Version] >= 0");
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
                filter: "[IsPrimary] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_school_database_endpoints_SchoolId_Purpose_Status",
                schema: "registry",
                table: "school_database_endpoints",
                columns: new[] { "SchoolId", "Purpose", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "school_database_endpoints",
                schema: "registry");
        }
    }
}
