using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Schools.Infrastructure.Persistence.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class AddSchoolNotificationEventKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EventKey",
                schema: "school",
                table: "school_user_notifications",
                type: "character varying(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_school_user_notifications_RecipientUserId_EventKey",
                schema: "school",
                table: "school_user_notifications",
                columns: new[] { "RecipientUserId", "EventKey" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_school_user_notifications_RecipientUserId_EventKey",
                schema: "school",
                table: "school_user_notifications");

            migrationBuilder.DropColumn(
                name: "EventKey",
                schema: "school",
                table: "school_user_notifications");
        }
    }
}
