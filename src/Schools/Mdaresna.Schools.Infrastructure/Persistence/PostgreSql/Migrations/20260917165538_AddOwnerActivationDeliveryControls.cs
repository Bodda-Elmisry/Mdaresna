using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Schools.Infrastructure.Persistence.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class AddOwnerActivationDeliveryControls : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastSentAtUtc",
                schema: "school",
                table: "local_user_activation_challenges",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SendCount",
                schema: "school",
                table: "local_user_activation_challenges",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SendWindowStartUtc",
                schema: "school",
                table: "local_user_activation_challenges",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.Sql(
                "UPDATE school.local_user_activation_challenges SET \"ConsumedAtUtc\" = CURRENT_TIMESTAMP WHERE \"ConsumedAtUtc\" IS NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastSentAtUtc",
                schema: "school",
                table: "local_user_activation_challenges");

            migrationBuilder.DropColumn(
                name: "SendCount",
                schema: "school",
                table: "local_user_activation_challenges");

            migrationBuilder.DropColumn(
                name: "SendWindowStartUtc",
                schema: "school",
                table: "local_user_activation_challenges");
        }
    }
}
