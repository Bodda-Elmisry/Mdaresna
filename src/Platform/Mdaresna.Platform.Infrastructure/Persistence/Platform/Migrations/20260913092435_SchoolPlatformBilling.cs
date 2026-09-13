using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Platform.Infrastructure.Persistence.Platform.Migrations
{
    /// <inheritdoc />
    public partial class SchoolPlatformBilling : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "billing");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_schools_TenantId_Id",
                schema: "registry",
                table: "schools",
                columns: new[] { "TenantId", "Id" });

            migrationBuilder.CreateTable(
                name: "school_platform_payment_requests",
                schema: "billing",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SchoolId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    TransferReference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    RequestedByAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: false),
                    ReviewedByAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReviewedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: true),
                    ReviewNote = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_school_platform_payment_requests", x => x.Id);
                    table.CheckConstraint("ck_billing_payment_request_amount", "[Amount] > 0");
                    table.CheckConstraint("ck_billing_payment_request_currency", "[Currency] COLLATE Latin1_General_BIN2 LIKE '[A-Z][A-Z][A-Z]'");
                    table.CheckConstraint("ck_billing_payment_request_identifiers", "[Id] <> '00000000-0000-0000-0000-000000000000' AND [RequestedByAccountId] <> '00000000-0000-0000-0000-000000000000' AND ([ReviewedByAccountId] IS NULL OR [ReviewedByAccountId] <> '00000000-0000-0000-0000-000000000000')");
                    table.CheckConstraint("ck_billing_payment_request_review", "([Status] = N'Pending' AND [ReviewedByAccountId] IS NULL AND [ReviewedAtUtc] IS NULL AND [ReviewNote] IS NULL) OR ([Status] IN (N'Approved', N'Rejected') AND [ReviewedByAccountId] IS NOT NULL AND [ReviewedAtUtc] IS NOT NULL AND [ReviewedAtUtc] >= [RequestedAtUtc] AND [ReviewedByAccountId] <> [RequestedByAccountId] AND ([Status] = N'Approved' OR ([ReviewNote] IS NOT NULL AND LEN(LTRIM(RTRIM([ReviewNote]))) > 0)))");
                    table.CheckConstraint("ck_billing_payment_request_status", "[Status] IN (N'Pending', N'Approved', N'Rejected')");
                    table.CheckConstraint("ck_billing_payment_request_timestamps", "DATEPART(TZOFFSET, [RequestedAtUtc]) = 0 AND ([ReviewedAtUtc] IS NULL OR DATEPART(TZOFFSET, [ReviewedAtUtc]) = 0)");
                    table.CheckConstraint("ck_billing_payment_request_version", "[Version] >= 0");
                    table.ForeignKey(
                        name: "FK_school_platform_payment_requests_schools_TenantId_SchoolId",
                        columns: x => new { x.TenantId, x.SchoolId },
                        principalSchema: "registry",
                        principalTable: "schools",
                        principalColumns: new[] { "TenantId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "school_platform_payment_ledger",
                schema: "billing",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaymentRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SchoolId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    TransferReference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PostedByAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PostedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_school_platform_payment_ledger", x => x.Id);
                    table.CheckConstraint("ck_billing_payment_ledger_amount", "[Amount] > 0");
                    table.CheckConstraint("ck_billing_payment_ledger_currency", "[Currency] COLLATE Latin1_General_BIN2 LIKE '[A-Z][A-Z][A-Z]'");
                    table.CheckConstraint("ck_billing_payment_ledger_identifiers", "[Id] <> '00000000-0000-0000-0000-000000000000' AND [PaymentRequestId] <> '00000000-0000-0000-0000-000000000000' AND [PostedByAccountId] <> '00000000-0000-0000-0000-000000000000'");
                    table.CheckConstraint("ck_billing_payment_ledger_posted_utc", "DATEPART(TZOFFSET, [PostedAtUtc]) = 0");
                    table.ForeignKey(
                        name: "FK_school_platform_payment_ledger_school_platform_payment_requests_PaymentRequestId",
                        column: x => x.PaymentRequestId,
                        principalSchema: "billing",
                        principalTable: "school_platform_payment_requests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_school_platform_payment_ledger_PaymentRequestId",
                schema: "billing",
                table: "school_platform_payment_ledger",
                column: "PaymentRequestId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_school_platform_payment_ledger_SchoolId_PostedAtUtc",
                schema: "billing",
                table: "school_platform_payment_ledger",
                columns: new[] { "SchoolId", "PostedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_school_platform_payment_requests_SchoolId_TransferReference",
                schema: "billing",
                table: "school_platform_payment_requests",
                columns: new[] { "SchoolId", "TransferReference" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_school_platform_payment_requests_TenantId_SchoolId",
                schema: "billing",
                table: "school_platform_payment_requests",
                columns: new[] { "TenantId", "SchoolId" });

            migrationBuilder.CreateIndex(
                name: "IX_school_platform_payment_requests_TenantId_Status_RequestedAtUtc",
                schema: "billing",
                table: "school_platform_payment_requests",
                columns: new[] { "TenantId", "Status", "RequestedAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "school_platform_payment_ledger",
                schema: "billing");

            migrationBuilder.DropTable(
                name: "school_platform_payment_requests",
                schema: "billing");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_schools_TenantId_Id",
                schema: "registry",
                table: "schools");
        }
    }
}
