using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Schools.Infrastructure.Persistence.SqlServer.Migrations
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
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_school_user_notifications_RecipientUserId_EventKey",
                schema: "school",
                table: "school_user_notifications",
                columns: new[] { "RecipientUserId", "EventKey" },
                unique: true,
                filter: "[EventKey] IS NOT NULL");
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
