using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Platform.Infrastructure.Persistence.Platform.Migrations
{
    /// <inheritdoc />
    public partial class AddGlobalStudentRegistry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "global_students",
                schema: "registry",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentCode = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NormalizedName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: false),
                    NationalId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    BirthCertificateNumber = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_global_students", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_global_students_BirthCertificateNumber",
                schema: "registry",
                table: "global_students",
                column: "BirthCertificateNumber",
                unique: true,
                filter: "[BirthCertificateNumber] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_global_students_NationalId",
                schema: "registry",
                table: "global_students",
                column: "NationalId",
                unique: true,
                filter: "[NationalId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_global_students_NormalizedName_DateOfBirth",
                schema: "registry",
                table: "global_students",
                columns: new[] { "NormalizedName", "DateOfBirth" });

            migrationBuilder.CreateIndex(
                name: "IX_global_students_StudentCode",
                schema: "registry",
                table: "global_students",
                column: "StudentCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "global_students",
                schema: "registry");
        }
    }
}
