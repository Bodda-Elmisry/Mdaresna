using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Platform.Infrastructure.Persistence.Platform.Migrations
{
    /// <inheritdoc />
    public partial class AddSchoolRoutingVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "RoutingVersion",
                schema: "registry",
                table: "schools",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RoutingVersion",
                schema: "registry",
                table: "schools");
        }
    }
}
