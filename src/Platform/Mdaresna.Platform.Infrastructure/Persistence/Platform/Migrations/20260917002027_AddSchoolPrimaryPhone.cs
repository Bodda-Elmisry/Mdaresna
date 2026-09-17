using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Platform.Infrastructure.Persistence.Platform.Migrations
{
    /// <inheritdoc />
    public partial class AddSchoolPrimaryPhone : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PrimaryPhone",
                schema: "registry",
                table: "schools",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrimaryPhone",
                schema: "registry",
                table: "schools");
        }
    }
}
