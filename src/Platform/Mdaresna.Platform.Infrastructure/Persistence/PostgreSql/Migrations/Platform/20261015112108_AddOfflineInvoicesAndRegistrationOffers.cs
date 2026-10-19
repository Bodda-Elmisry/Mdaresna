using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Platform.Infrastructure.Persistence.PostgreSql.Migrations.Platform
{
    /// <inheritdoc />
    public partial class AddOfflineInvoicesAndRegistrationOffers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_subscription_quotes_PaymentRequestId",
                schema: "billing",
                table: "subscription_quotes");

            migrationBuilder.AddColumn<Guid>(
                name: "InvoiceId",
                schema: "billing",
                table: "subscription_quotes",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "invoices",
                schema: "billing",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Number = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    SchoolId = table.Column<Guid>(type: "uuid", nullable: false),
                    RegistrationRequestId = table.Column<Guid>(type: "uuid", nullable: true),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    PaymentStatus = table.Column<int>(type: "integer", nullable: false),
                    CurrentPaymentRequestId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PaidAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_invoices", x => x.Id);
                    table.CheckConstraint("ck_invoice_amount", "\"Amount\" > 0");
                });

            migrationBuilder.CreateTable(
                name: "registration_subscription_offers",
                schema: "billing",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SchoolId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    QuoteIdsJson = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_registration_subscription_offers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "invoice_lines",
                schema: "billing",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuoteId = table.Column<Guid>(type: "uuid", nullable: false),
                    SystemNameAr = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SystemNameEn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PlanNameAr = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PlanNameEn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_invoice_lines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_invoice_lines_invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalSchema: "billing",
                        principalTable: "invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_invoice_lines_subscription_quotes_QuoteId",
                        column: x => x.QuoteId,
                        principalSchema: "billing",
                        principalTable: "subscription_quotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "invoice_receipts",
                schema: "billing",
                columns: table => new
                {
                    PaymentRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Method = table.Column<int>(type: "integer", nullable: false),
                    Reference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RepresentativeName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ReceivedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RecordedByAccountId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_invoice_receipts", x => x.PaymentRequestId);
                    table.ForeignKey(
                        name: "FK_invoice_receipts_invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalSchema: "billing",
                        principalTable: "invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_invoice_receipts_school_platform_payment_requests_PaymentRe~",
                        column: x => x.PaymentRequestId,
                        principalSchema: "billing",
                        principalTable: "school_platform_payment_requests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_subscription_quotes_InvoiceId",
                schema: "billing",
                table: "subscription_quotes",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_subscription_quotes_PaymentRequestId",
                schema: "billing",
                table: "subscription_quotes",
                column: "PaymentRequestId",
                filter: "\"PaymentRequestId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_invoice_lines_InvoiceId",
                schema: "billing",
                table: "invoice_lines",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_invoice_lines_QuoteId",
                schema: "billing",
                table: "invoice_lines",
                column: "QuoteId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_invoice_receipts_InvoiceId",
                schema: "billing",
                table: "invoice_receipts",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_invoices_Number",
                schema: "billing",
                table: "invoices",
                column: "Number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_invoices_RegistrationRequestId",
                schema: "billing",
                table: "invoices",
                column: "RegistrationRequestId",
                unique: true,
                filter: "\"RegistrationRequestId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_invoices_SchoolId_CreatedAtUtc",
                schema: "billing",
                table: "invoices",
                columns: new[] { "SchoolId", "CreatedAtUtc" });

            migrationBuilder.AddForeignKey(
                name: "FK_subscription_quotes_invoices_InvoiceId",
                schema: "billing",
                table: "subscription_quotes",
                column: "InvoiceId",
                principalSchema: "billing",
                principalTable: "invoices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_subscription_quotes_invoices_InvoiceId",
                schema: "billing",
                table: "subscription_quotes");

            migrationBuilder.DropTable(
                name: "invoice_lines",
                schema: "billing");

            migrationBuilder.DropTable(
                name: "invoice_receipts",
                schema: "billing");

            migrationBuilder.DropTable(
                name: "registration_subscription_offers",
                schema: "billing");

            migrationBuilder.DropTable(
                name: "invoices",
                schema: "billing");

            migrationBuilder.DropIndex(
                name: "IX_subscription_quotes_InvoiceId",
                schema: "billing",
                table: "subscription_quotes");

            migrationBuilder.DropIndex(
                name: "IX_subscription_quotes_PaymentRequestId",
                schema: "billing",
                table: "subscription_quotes");

            migrationBuilder.DropColumn(
                name: "InvoiceId",
                schema: "billing",
                table: "subscription_quotes");

            migrationBuilder.CreateIndex(
                name: "IX_subscription_quotes_PaymentRequestId",
                schema: "billing",
                table: "subscription_quotes",
                column: "PaymentRequestId",
                unique: true,
                filter: "\"PaymentRequestId\" IS NOT NULL");
        }
    }
}
