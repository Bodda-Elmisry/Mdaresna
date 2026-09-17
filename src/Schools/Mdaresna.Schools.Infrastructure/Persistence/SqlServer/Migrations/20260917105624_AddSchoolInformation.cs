using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Schools.Infrastructure.Persistence.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class AddSchoolInformation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "school_information",
                schema: "school",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlatformSchoolReferenceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlatformTenantReferenceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlatformRegistrationRequestReferenceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SchoolType = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    DeploymentMode = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PrimaryPhone = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    PlatformUnitTypeReferenceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UnitTypeCode = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    UnitTypeName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    OwnerPlatformAccountReferenceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlatformCreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ActivatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_school_information", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_school_information_Code",
                schema: "school",
                table: "school_information",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_school_information_PlatformSchoolReferenceId",
                schema: "school",
                table: "school_information",
                column: "PlatformSchoolReferenceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_school_information_PlatformTenantReferenceId",
                schema: "school",
                table: "school_information",
                column: "PlatformTenantReferenceId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "school_information",
                schema: "school");
        }
    }
}
