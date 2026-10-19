using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Platform.Infrastructure.Persistence.PostgreSql.Migrations.Platform
{
    /// <inheritdoc />
    public partial class AddSubscriptionCapabilitySnapshots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CapabilityCodes",
                schema: "billing",
                table: "subscription_quotes",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CapabilityCodes",
                schema: "billing",
                table: "school_system_subscriptions",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CapabilityCodes",
                schema: "billing",
                table: "subscription_quotes");

            migrationBuilder.DropColumn(
                name: "CapabilityCodes",
                schema: "billing",
                table: "school_system_subscriptions");
        }
    }
}
