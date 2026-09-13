using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Platform.Infrastructure.Persistence.Platform.Migrations
{
    /// <inheritdoc />
    public partial class AddPlatformSmsProviders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "platform");

            migrationBuilder.CreateTable(
                name: "sms_providers",
                schema: "platform",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProviderUserName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    EncryptedPassword = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    SenderName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    ApiUrlTemplate = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    MessageCharactersLength = table.Column<int>(type: "int", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    SuccessResponsePrefix = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sms_providers", x => x.Id);
                    table.CheckConstraint("ck_platform_sms_providers_api_url_https", "[ApiUrlTemplate] LIKE 'https://%'");
                    table.CheckConstraint("ck_platform_sms_providers_deleted_inactive", "[IsDeleted] = 0 OR [IsActive] = 0");
                    table.CheckConstraint("ck_platform_sms_providers_message_length", "[MessageCharactersLength] BETWEEN 1 AND 1000");
                    table.CheckConstraint("ck_platform_sms_providers_priority", "[Priority] >= 1");
                    table.CheckConstraint("ck_platform_sms_providers_timestamps", "[CreatedAtUtc] <= [UpdatedAtUtc] AND DATEPART(TZOFFSET, [CreatedAtUtc]) = 0 AND DATEPART(TZOFFSET, [UpdatedAtUtc]) = 0");
                });

            migrationBuilder.CreateIndex(
                name: "IX_sms_providers_IsDeleted_IsActive_Priority",
                schema: "platform",
                table: "sms_providers",
                columns: new[] { "IsDeleted", "IsActive", "Priority" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "sms_providers",
                schema: "platform");
        }
    }
}
