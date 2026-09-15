using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Platform.Infrastructure.Persistence.PostgreSql.Migrations.Platform
{
    /// <inheritdoc />
    public partial class AddPlatformNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "notifications",
                schema: "messaging",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    TitleAr = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TitleEn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    BodyAr = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    BodyEn = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    ActionUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DataJson = table.Column<string>(type: "text", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp(3) with time zone", precision: 3, nullable: false),
                    ExpiresAtUtc = table.Column<DateTimeOffset>(type: "timestamp(3) with time zone", precision: 3, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notifications", x => x.Id);
                    table.CheckConstraint("ck_messaging_notifications_timestamps", "TRUE AND (\"ExpiresAtUtc\" IS NULL OR (\"ExpiresAtUtc\" > \"CreatedAtUtc\" AND TRUE))");
                });

            migrationBuilder.CreateTable(
                name: "user_devices",
                schema: "messaging",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    InstallationId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    FcmToken = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    Platform = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    LanguageCode = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    DeviceName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp(3) with time zone", precision: 3, nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp(3) with time zone", precision: 3, nullable: false),
                    LastSeenAtUtc = table.Column<DateTimeOffset>(type: "timestamp(3) with time zone", precision: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_devices", x => x.Id);
                    table.CheckConstraint("ck_messaging_user_devices_language", "\"LanguageCode\" IN ('ar','en')");
                    table.CheckConstraint("ck_messaging_user_devices_platform", "\"Platform\" IN ('android','ios','web')");
                    table.CheckConstraint("ck_messaging_user_devices_timestamps", "\"CreatedAtUtc\" <= \"UpdatedAtUtc\" AND \"UpdatedAtUtc\" <= \"LastSeenAtUtc\" AND TRUE AND TRUE AND TRUE");
                });

            migrationBuilder.CreateTable(
                name: "notification_deliveries",
                schema: "messaging",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NotificationId = table.Column<Guid>(type: "uuid", nullable: false),
                    DeviceId = table.Column<Guid>(type: "uuid", nullable: false),
                    FcmTokenSnapshot = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    Status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    AttemptCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp(3) with time zone", precision: 3, nullable: false),
                    NextAttemptAtUtc = table.Column<DateTimeOffset>(type: "timestamp(3) with time zone", precision: 3, nullable: true),
                    SentAtUtc = table.Column<DateTimeOffset>(type: "timestamp(3) with time zone", precision: 3, nullable: true),
                    LastError = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notification_deliveries", x => x.Id);
                    table.CheckConstraint("ck_messaging_notification_deliveries_attempts", "\"AttemptCount\" >= 0");
                    table.CheckConstraint("ck_messaging_notification_deliveries_status", "\"Status\" IN ('Pending','Sent','Failed','Skipped')");
                    table.ForeignKey(
                        name: "FK_notification_deliveries_notifications_NotificationId",
                        column: x => x.NotificationId,
                        principalSchema: "messaging",
                        principalTable: "notifications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "notification_recipients",
                schema: "messaging",
                columns: table => new
                {
                    NotificationId = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp(3) with time zone", precision: 3, nullable: false),
                    ReadAtUtc = table.Column<DateTimeOffset>(type: "timestamp(3) with time zone", precision: 3, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notification_recipients", x => new { x.NotificationId, x.AccountId });
                    table.CheckConstraint("ck_messaging_notification_recipients_timestamps", "TRUE AND (\"ReadAtUtc\" IS NULL OR (\"ReadAtUtc\" >= \"CreatedAtUtc\" AND TRUE))");
                    table.ForeignKey(
                        name: "FK_notification_recipients_notifications_NotificationId",
                        column: x => x.NotificationId,
                        principalSchema: "messaging",
                        principalTable: "notifications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_notification_deliveries_NotificationId_DeviceId",
                schema: "messaging",
                table: "notification_deliveries",
                columns: new[] { "NotificationId", "DeviceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_notification_deliveries_Status_NextAttemptAtUtc_CreatedAtUtc",
                schema: "messaging",
                table: "notification_deliveries",
                columns: new[] { "Status", "NextAttemptAtUtc", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_notification_recipients_AccountId_ReadAtUtc_CreatedAtUtc",
                schema: "messaging",
                table: "notification_recipients",
                columns: new[] { "AccountId", "ReadAtUtc", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_notifications_CreatedAtUtc",
                schema: "messaging",
                table: "notifications",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_user_devices_AccountId_InstallationId",
                schema: "messaging",
                table: "user_devices",
                columns: new[] { "AccountId", "InstallationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_devices_AccountId_LastSeenAtUtc",
                schema: "messaging",
                table: "user_devices",
                columns: new[] { "AccountId", "LastSeenAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_user_devices_FcmToken",
                schema: "messaging",
                table: "user_devices",
                column: "FcmToken",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "notification_deliveries",
                schema: "messaging");

            migrationBuilder.DropTable(
                name: "notification_recipients",
                schema: "messaging");

            migrationBuilder.DropTable(
                name: "user_devices",
                schema: "messaging");

            migrationBuilder.DropTable(
                name: "notifications",
                schema: "messaging");
        }
    }
}
