using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Platform.Infrastructure.Persistence.Platform.Migrations
{
    /// <inheritdoc />
    public partial class AddPlatformSmsLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "sms_logs",
                schema: "platform",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProviderId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RecipientEncrypted = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    RecipientMasked = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    MessageEncrypted = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ResponseEncrypted = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HttpStatusCode = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    FailureReason = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: false),
                    CompletedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sms_logs", x => x.Id);
                    table.CheckConstraint("ck_platform_sms_logs_completion", "([Status] = 'Pending' AND [CompletedAtUtc] IS NULL) OR ([Status] <> 'Pending' AND [CompletedAtUtc] IS NOT NULL)");
                    table.CheckConstraint("ck_platform_sms_logs_http_status", "[HttpStatusCode] IS NULL OR [HttpStatusCode] BETWEEN 100 AND 599");
                    table.CheckConstraint("ck_platform_sms_logs_status", "[Status] IN ('Pending', 'Accepted', 'Rejected', 'Failed')");
                    table.CheckConstraint("ck_platform_sms_logs_timestamps", "DATEPART(TZOFFSET, [CreatedAtUtc]) = 0 AND ([CompletedAtUtc] IS NULL OR ([CompletedAtUtc] >= [CreatedAtUtc] AND DATEPART(TZOFFSET, [CompletedAtUtc]) = 0))");
                    table.ForeignKey(
                        name: "FK_sms_logs_sms_providers_ProviderId",
                        column: x => x.ProviderId,
                        principalSchema: "platform",
                        principalTable: "sms_providers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_sms_logs_CreatedAtUtc",
                schema: "platform",
                table: "sms_logs",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_sms_logs_ProviderId_CreatedAtUtc",
                schema: "platform",
                table: "sms_logs",
                columns: new[] { "ProviderId", "CreatedAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "sms_logs",
                schema: "platform");
        }
    }
}
