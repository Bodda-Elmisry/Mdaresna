using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Platform.Infrastructure.Persistence.Platform.Migrations
{
    /// <inheritdoc />
    public partial class AddSmsLogSourceMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MessageTypeCode",
                schema: "platform",
                table: "sms_logs",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SchoolId",
                schema: "platform",
                table: "sms_logs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SourceMessageId",
                schema: "platform",
                table: "sms_logs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceSystem",
                schema: "platform",
                table: "sms_logs",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: true);

            // The existing Platform sender only emitted first-owner OTPs.
            // Backfill before enforcing NOT NULL, without leaving defaults that
            // might silently misclassify future SMS rows.
            migrationBuilder.Sql(
                "UPDATE [platform].[sms_logs] SET [SourceSystem] = N'platform', " +
                "[MessageTypeCode] = N'otp' WHERE [SourceSystem] IS NULL " +
                "OR [MessageTypeCode] IS NULL");

            migrationBuilder.AlterColumn<string>(
                name: "MessageTypeCode",
                schema: "platform",
                table: "sms_logs",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SourceSystem",
                schema: "platform",
                table: "sms_logs",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(16)",
                oldMaxLength: 16,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_sms_logs_SourceSystem_SchoolId_CreatedAtUtc",
                schema: "platform",
                table: "sms_logs",
                columns: new[] { "SourceSystem", "SchoolId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_sms_logs_SourceSystem_SourceMessageId",
                schema: "platform",
                table: "sms_logs",
                columns: new[] { "SourceSystem", "SourceMessageId" },
                unique: true,
                filter: "[SourceMessageId] IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "ck_platform_sms_logs_school_scope",
                schema: "platform",
                table: "sms_logs",
                sql: "[SourceSystem] <> 'schools' OR [SchoolId] IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "ck_platform_sms_logs_source_system",
                schema: "platform",
                table: "sms_logs",
                sql: "[SourceSystem] IN ('platform', 'schools', 'family')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_sms_logs_SourceSystem_SchoolId_CreatedAtUtc",
                schema: "platform",
                table: "sms_logs");

            migrationBuilder.DropIndex(
                name: "IX_sms_logs_SourceSystem_SourceMessageId",
                schema: "platform",
                table: "sms_logs");

            migrationBuilder.DropCheckConstraint(
                name: "ck_platform_sms_logs_school_scope",
                schema: "platform",
                table: "sms_logs");

            migrationBuilder.DropCheckConstraint(
                name: "ck_platform_sms_logs_source_system",
                schema: "platform",
                table: "sms_logs");

            migrationBuilder.DropColumn(
                name: "MessageTypeCode",
                schema: "platform",
                table: "sms_logs");

            migrationBuilder.DropColumn(
                name: "SchoolId",
                schema: "platform",
                table: "sms_logs");

            migrationBuilder.DropColumn(
                name: "SourceMessageId",
                schema: "platform",
                table: "sms_logs");

            migrationBuilder.DropColumn(
                name: "SourceSystem",
                schema: "platform",
                table: "sms_logs");
        }
    }
}
