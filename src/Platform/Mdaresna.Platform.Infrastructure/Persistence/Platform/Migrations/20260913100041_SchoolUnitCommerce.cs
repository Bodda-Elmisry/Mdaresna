using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Platform.Infrastructure.Persistence.Platform.Migrations
{
    /// <inheritdoc />
    public partial class SchoolUnitCommerce : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddUniqueConstraint(
                name: "AK_school_platform_payment_requests_Id_TenantId_SchoolId_Amount_Currency",
                schema: "billing",
                table: "school_platform_payment_requests",
                columns: new[] { "Id", "TenantId", "SchoolId", "Amount", "Currency" });

            migrationBuilder.CreateTable(
                name: "unit_types",
                schema: "billing",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_unit_types", x => x.Id);
                    table.CheckConstraint("ck_billing_unit_type_code", "LEN([Code]) BETWEEN 3 AND 32 AND [Code] COLLATE Latin1_General_BIN2 = UPPER([Code])");
                    table.CheckConstraint("ck_billing_unit_type_currency", "[Currency] COLLATE Latin1_General_BIN2 LIKE '[A-Z][A-Z][A-Z]'");
                    table.CheckConstraint("ck_billing_unit_type_id", "[Id] <> '00000000-0000-0000-0000-000000000000'");
                    table.CheckConstraint("ck_billing_unit_type_name", "LEN(LTRIM(RTRIM([DisplayName]))) > 0");
                    table.CheckConstraint("ck_billing_unit_type_price", "[UnitPrice] > 0");
                    table.CheckConstraint("ck_billing_unit_type_timestamps", "[CreatedAtUtc] <= [UpdatedAtUtc] AND DATEPART(TZOFFSET, [CreatedAtUtc]) = 0 AND DATEPART(TZOFFSET, [UpdatedAtUtc]) = 0");
                    table.CheckConstraint("ck_billing_unit_type_version", "[Version] >= 0");
                });

            migrationBuilder.CreateTable(
                name: "unit_purchase_intents",
                schema: "billing",
                columns: table => new
                {
                    PaymentRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SchoolId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UnitTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UnitTypeVersion = table.Column<long>(type: "bigint", nullable: false),
                    UnitTypeCode = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    UnitTypeName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PaymentMethodCode = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    TransferOccurredAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_unit_purchase_intents", x => x.PaymentRequestId);
                    table.UniqueConstraint("AK_unit_purchase_intents_PaymentRequestId_TenantId_SchoolId_UnitTypeId_UnitTypeCode_UnitTypeName_UnitPrice_Currency_Quantity_Am~", x => new { x.PaymentRequestId, x.TenantId, x.SchoolId, x.UnitTypeId, x.UnitTypeCode, x.UnitTypeName, x.UnitPrice, x.Currency, x.Quantity, x.Amount, x.PaymentMethodCode, x.TransferOccurredAtUtc });
                    table.CheckConstraint("ck_billing_unit_purchase_intent_code", "LEN([UnitTypeCode]) BETWEEN 3 AND 32 AND [UnitTypeCode] COLLATE Latin1_General_BIN2 = UPPER([UnitTypeCode])");
                    table.CheckConstraint("ck_billing_unit_purchase_intent_created_utc", "DATEPART(TZOFFSET, [CreatedAtUtc]) = 0 AND DATEPART(TZOFFSET, [TransferOccurredAtUtc]) = 0 AND [TransferOccurredAtUtc] <= [CreatedAtUtc]");
                    table.CheckConstraint("ck_billing_unit_purchase_intent_currency", "[Currency] COLLATE Latin1_General_BIN2 LIKE '[A-Z][A-Z][A-Z]'");
                    table.CheckConstraint("ck_billing_unit_purchase_intent_ids", "[PaymentRequestId] <> '00000000-0000-0000-0000-000000000000' AND [UnitTypeId] <> '00000000-0000-0000-0000-000000000000'");
                    table.CheckConstraint("ck_billing_unit_purchase_intent_name", "LEN(LTRIM(RTRIM([UnitTypeName]))) > 0");
                    table.CheckConstraint("ck_billing_unit_purchase_intent_payment_method", "LEN([PaymentMethodCode]) BETWEEN 3 AND 40 AND [PaymentMethodCode] COLLATE Latin1_General_BIN2 = LOWER([PaymentMethodCode])");
                    table.CheckConstraint("ck_billing_unit_purchase_intent_price", "[UnitPrice] > 0 AND [Amount] > 0 AND [Amount] = [UnitPrice] * [Quantity]");
                    table.CheckConstraint("ck_billing_unit_purchase_intent_quantity", "[Quantity] BETWEEN 1 AND 1000000");
                    table.CheckConstraint("ck_billing_unit_purchase_intent_school", "[SchoolId] = [TenantId]");
                    table.CheckConstraint("ck_billing_unit_purchase_intent_version", "[UnitTypeVersion] >= 0");
                    table.ForeignKey(
                        name: "FK_unit_purchase_intents_school_platform_payment_requests_PaymentRequestId_TenantId_SchoolId_Amount_Currency",
                        columns: x => new { x.PaymentRequestId, x.TenantId, x.SchoolId, x.Amount, x.Currency },
                        principalSchema: "billing",
                        principalTable: "school_platform_payment_requests",
                        principalColumns: new[] { "Id", "TenantId", "SchoolId", "Amount", "Currency" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_unit_purchase_intents_unit_types_UnitTypeId",
                        column: x => x.UnitTypeId,
                        principalSchema: "billing",
                        principalTable: "unit_types",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "unit_grants",
                schema: "billing",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaymentRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SchoolId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UnitTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UnitTypeCode = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    UnitTypeName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    TransferReference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PaymentMethodCode = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    TransferOccurredAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: false),
                    GrantedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_unit_grants", x => x.Id);
                    table.CheckConstraint("ck_billing_unit_grant_currency", "[Currency] COLLATE Latin1_General_BIN2 LIKE '[A-Z][A-Z][A-Z]'");
                    table.CheckConstraint("ck_billing_unit_grant_granted_utc", "DATEPART(TZOFFSET, [GrantedAtUtc]) = 0 AND DATEPART(TZOFFSET, [TransferOccurredAtUtc]) = 0 AND [TransferOccurredAtUtc] <= [GrantedAtUtc]");
                    table.CheckConstraint("ck_billing_unit_grant_ids", "[Id] <> '00000000-0000-0000-0000-000000000000' AND [PaymentRequestId] <> '00000000-0000-0000-0000-000000000000' AND [UnitTypeId] <> '00000000-0000-0000-0000-000000000000'");
                    table.CheckConstraint("ck_billing_unit_grant_name", "LEN(LTRIM(RTRIM([UnitTypeName]))) > 0");
                    table.CheckConstraint("ck_billing_unit_grant_payment_method", "LEN([PaymentMethodCode]) BETWEEN 3 AND 40 AND [PaymentMethodCode] COLLATE Latin1_General_BIN2 = LOWER([PaymentMethodCode])");
                    table.CheckConstraint("ck_billing_unit_grant_price", "[UnitPrice] > 0 AND [Amount] > 0 AND [Amount] = [UnitPrice] * [Quantity]");
                    table.CheckConstraint("ck_billing_unit_grant_quantity", "[Quantity] BETWEEN 1 AND 1000000");
                    table.CheckConstraint("ck_billing_unit_grant_school", "[SchoolId] = [TenantId]");
                    table.ForeignKey(
                        name: "FK_unit_grants_school_platform_payment_requests_PaymentRequestId_TenantId_SchoolId_Amount_Currency_TransferReference",
                        columns: x => new { x.PaymentRequestId, x.TenantId, x.SchoolId, x.Amount, x.Currency, x.TransferReference },
                        principalSchema: "billing",
                        principalTable: "school_platform_payment_requests",
                        principalColumns: new[] { "Id", "TenantId", "SchoolId", "Amount", "Currency", "TransferReference" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_unit_grants_unit_purchase_intents_PaymentRequestId_TenantId_SchoolId_UnitTypeId_UnitTypeCode_UnitTypeName_UnitPrice_Currency~",
                        columns: x => new { x.PaymentRequestId, x.TenantId, x.SchoolId, x.UnitTypeId, x.UnitTypeCode, x.UnitTypeName, x.UnitPrice, x.Currency, x.Quantity, x.Amount, x.PaymentMethodCode, x.TransferOccurredAtUtc },
                        principalSchema: "billing",
                        principalTable: "unit_purchase_intents",
                        principalColumns: new[] { "PaymentRequestId", "TenantId", "SchoolId", "UnitTypeId", "UnitTypeCode", "UnitTypeName", "UnitPrice", "Currency", "Quantity", "Amount", "PaymentMethodCode", "TransferOccurredAtUtc" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_unit_grants_PaymentRequestId",
                schema: "billing",
                table: "unit_grants",
                column: "PaymentRequestId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_unit_grants_PaymentRequestId_TenantId_SchoolId_Amount_Currency_TransferReference",
                schema: "billing",
                table: "unit_grants",
                columns: new[] { "PaymentRequestId", "TenantId", "SchoolId", "Amount", "Currency", "TransferReference" });

            migrationBuilder.CreateIndex(
                name: "IX_unit_grants_PaymentRequestId_TenantId_SchoolId_UnitTypeId_UnitTypeCode_UnitTypeName_UnitPrice_Currency_Quantity_Amount_Payme~",
                schema: "billing",
                table: "unit_grants",
                columns: new[] { "PaymentRequestId", "TenantId", "SchoolId", "UnitTypeId", "UnitTypeCode", "UnitTypeName", "UnitPrice", "Currency", "Quantity", "Amount", "PaymentMethodCode", "TransferOccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_unit_grants_SchoolId_GrantedAtUtc",
                schema: "billing",
                table: "unit_grants",
                columns: new[] { "SchoolId", "GrantedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_unit_purchase_intents_PaymentRequestId_TenantId_SchoolId_Amount_Currency",
                schema: "billing",
                table: "unit_purchase_intents",
                columns: new[] { "PaymentRequestId", "TenantId", "SchoolId", "Amount", "Currency" });

            migrationBuilder.CreateIndex(
                name: "IX_unit_purchase_intents_SchoolId_CreatedAtUtc",
                schema: "billing",
                table: "unit_purchase_intents",
                columns: new[] { "SchoolId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_unit_purchase_intents_UnitTypeId",
                schema: "billing",
                table: "unit_purchase_intents",
                column: "UnitTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_unit_types_Code",
                schema: "billing",
                table: "unit_types",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_unit_types_IsActive",
                schema: "billing",
                table: "unit_types",
                column: "IsActive");

            // No legacy payment rows are changed or inferred to be unit purchases.
            migrationBuilder.Sql("""
                CREATE OR ALTER TRIGGER [billing].[tr_billing_unit_purchase_intent_append_only]
                ON [billing].[unit_purchase_intents]
                AFTER UPDATE, DELETE
                AS
                BEGIN
                    SET NOCOUNT ON;
                    IF EXISTS (SELECT 1 FROM deleted)
                        THROW 51030, 'Unit purchase intents are append-only.', 1;
                END
                """);

            migrationBuilder.Sql("""
                CREATE OR ALTER TRIGGER [billing].[tr_billing_unit_purchase_intent_pending_insert]
                ON [billing].[unit_purchase_intents]
                AFTER INSERT
                AS
                BEGIN
                    SET NOCOUNT ON;
                    IF EXISTS (
                        SELECT 1
                        FROM inserted AS intent
                        LEFT JOIN [billing].[school_platform_payment_requests] AS request
                            ON request.[Id] = intent.[PaymentRequestId]
                        WHERE request.[Id] IS NULL OR request.[Status] <> N'Pending'
                    )
                        THROW 51031, 'A unit purchase intent requires a pending payment request.', 1;
                END
                """);

            migrationBuilder.Sql("""
                CREATE OR ALTER TRIGGER [billing].[tr_billing_unit_grant_approved_insert]
                ON [billing].[unit_grants]
                AFTER INSERT
                AS
                BEGIN
                    SET NOCOUNT ON;
                    IF EXISTS (
                        SELECT 1
                        FROM inserted AS grantRow
                        LEFT JOIN [billing].[school_platform_payment_requests] AS request
                            ON request.[Id] = grantRow.[PaymentRequestId]
                        WHERE request.[Id] IS NULL
                            OR request.[Status] <> N'Approved'
                            OR request.[ReviewedAtUtc] IS NULL
                            OR grantRow.[GrantedAtUtc] < request.[ReviewedAtUtc]
                    )
                        THROW 51032, 'A unit grant requires its matching approved payment request.', 1;
                END
                """);

            migrationBuilder.Sql("""
                CREATE OR ALTER TRIGGER [billing].[tr_billing_unit_grant_append_only]
                ON [billing].[unit_grants]
                AFTER UPDATE, DELETE
                AS
                BEGIN
                    SET NOCOUNT ON;
                    IF EXISTS (SELECT 1 FROM deleted)
                        THROW 51033, 'Unit grants are append-only.', 1;
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "unit_grants",
                schema: "billing");

            migrationBuilder.DropTable(
                name: "unit_purchase_intents",
                schema: "billing");

            migrationBuilder.DropTable(
                name: "unit_types",
                schema: "billing");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_school_platform_payment_requests_Id_TenantId_SchoolId_Amount_Currency",
                schema: "billing",
                table: "school_platform_payment_requests");
        }
    }
}
