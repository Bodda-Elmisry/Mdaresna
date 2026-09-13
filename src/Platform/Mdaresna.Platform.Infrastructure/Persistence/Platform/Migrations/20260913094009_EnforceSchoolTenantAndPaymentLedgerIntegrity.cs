using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Platform.Infrastructure.Persistence.Platform.Migrations
{
    /// <inheritdoc />
    public partial class EnforceSchoolTenantAndPaymentLedgerIntegrity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Fail safely on existing data instead of silently changing tenant or financial records.
            migrationBuilder.Sql("""
                IF EXISTS (
                    SELECT 1
                    FROM [registry].[schools]
                    WHERE [Id] <> [TenantId]
                ) OR EXISTS (
                    SELECT 1
                    FROM [registry].[schools]
                    GROUP BY [TenantId]
                    HAVING COUNT(*) > 1
                )
                    THROW 51010, 'Existing school IDs do not satisfy one school per tenant.', 1;

                IF EXISTS (
                    SELECT 1
                    FROM [billing].[school_platform_payment_ledger] AS ledger
                    LEFT JOIN [billing].[school_platform_payment_requests] AS request
                        ON request.[Id] = ledger.[PaymentRequestId]
                    WHERE request.[Id] IS NULL
                        OR request.[Status] <> N'Approved'
                        OR request.[TenantId] <> ledger.[TenantId]
                        OR request.[SchoolId] <> ledger.[SchoolId]
                        OR request.[Amount] <> ledger.[Amount]
                        OR request.[Currency] COLLATE Latin1_General_BIN2 <>
                           ledger.[Currency] COLLATE Latin1_General_BIN2
                        OR request.[TransferReference] COLLATE Latin1_General_BIN2 <>
                           ledger.[TransferReference] COLLATE Latin1_General_BIN2
                )
                    THROW 51011, 'Existing school-platform payment ledger rows do not match approved requests.', 1;
                """);

            migrationBuilder.DropForeignKey(
                name: "FK_school_platform_payment_ledger_school_platform_payment_requests_PaymentRequestId",
                schema: "billing",
                table: "school_platform_payment_ledger");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_school_platform_payment_requests_Id_TenantId_SchoolId_Amount_Currency_TransferReference",
                schema: "billing",
                table: "school_platform_payment_requests",
                columns: new[] { "Id", "TenantId", "SchoolId", "Amount", "Currency", "TransferReference" });

            migrationBuilder.CreateIndex(
                name: "IX_schools_TenantId",
                schema: "registry",
                table: "schools",
                column: "TenantId",
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "ck_registry_schools_school_is_tenant",
                schema: "registry",
                table: "schools",
                sql: "[Id] = [TenantId]");

            migrationBuilder.CreateIndex(
                name: "IX_school_platform_payment_ledger_PaymentRequestId_TenantId_SchoolId_Amount_Currency_TransferReference",
                schema: "billing",
                table: "school_platform_payment_ledger",
                columns: new[] { "PaymentRequestId", "TenantId", "SchoolId", "Amount", "Currency", "TransferReference" });

            migrationBuilder.AddForeignKey(
                name: "FK_school_platform_payment_ledger_school_platform_payment_requests_PaymentRequestId_TenantId_SchoolId_Amount_Currency_TransferR~",
                schema: "billing",
                table: "school_platform_payment_ledger",
                columns: new[] { "PaymentRequestId", "TenantId", "SchoolId", "Amount", "Currency", "TransferReference" },
                principalSchema: "billing",
                principalTable: "school_platform_payment_requests",
                principalColumns: new[] { "Id", "TenantId", "SchoolId", "Amount", "Currency", "TransferReference" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.Sql("""
                CREATE OR ALTER TRIGGER [billing].[tr_billing_payment_ledger_approved_insert]
                ON [billing].[school_platform_payment_ledger]
                AFTER INSERT
                AS
                BEGIN
                    SET NOCOUNT ON;
                    IF EXISTS (
                        SELECT 1
                        FROM inserted AS ledger
                        LEFT JOIN [billing].[school_platform_payment_requests] AS request
                            ON request.[Id] = ledger.[PaymentRequestId]
                        WHERE request.[Id] IS NULL
                            OR request.[Status] <> N'Approved'
                            OR request.[TenantId] <> ledger.[TenantId]
                            OR request.[SchoolId] <> ledger.[SchoolId]
                            OR request.[Amount] <> ledger.[Amount]
                            OR request.[Currency] COLLATE Latin1_General_BIN2 <>
                               ledger.[Currency] COLLATE Latin1_General_BIN2
                            OR request.[TransferReference] COLLATE Latin1_General_BIN2 <>
                               ledger.[TransferReference] COLLATE Latin1_General_BIN2
                    )
                        THROW 51012, 'A ledger entry requires a matching approved payment request.', 1;
                END
                """);

            migrationBuilder.Sql("""
                CREATE OR ALTER TRIGGER [billing].[tr_billing_payment_ledger_append_only]
                ON [billing].[school_platform_payment_ledger]
                AFTER UPDATE, DELETE
                AS
                BEGIN
                    SET NOCOUNT ON;
                    IF EXISTS (SELECT 1 FROM deleted)
                        THROW 51013, 'School-platform payment ledger entries are append-only.', 1;
                END
                """);

            migrationBuilder.Sql("""
                CREATE OR ALTER TRIGGER [billing].[tr_billing_payment_request_approved_guard]
                ON [billing].[school_platform_payment_requests]
                AFTER UPDATE, DELETE
                AS
                BEGIN
                    SET NOCOUNT ON;
                    IF EXISTS (
                        SELECT 1
                        FROM deleted AS previous
                        LEFT JOIN inserted AS next ON next.[Id] = previous.[Id]
                        WHERE next.[Id] IS NULL
                            OR previous.[Status] <> N'Pending'
                            OR next.[Status] NOT IN (N'Approved', N'Rejected')
                            OR next.[TenantId] <> previous.[TenantId]
                            OR next.[SchoolId] <> previous.[SchoolId]
                            OR next.[Amount] <> previous.[Amount]
                            OR next.[Currency] COLLATE Latin1_General_BIN2 <>
                               previous.[Currency] COLLATE Latin1_General_BIN2
                            OR next.[TransferReference] COLLATE Latin1_General_BIN2 <>
                               previous.[TransferReference] COLLATE Latin1_General_BIN2
                            OR next.[RequestedByAccountId] <> previous.[RequestedByAccountId]
                            OR next.[RequestedAtUtc] <> previous.[RequestedAtUtc]
                    )
                        THROW 51014, 'A payment request may only transition once from Pending to a reviewed state.', 1;

                    IF EXISTS (
                        SELECT 1
                        FROM inserted AS request
                        JOIN [billing].[school_platform_payment_ledger] AS ledger
                            ON ledger.[PaymentRequestId] = request.[Id]
                        WHERE request.[Status] <> N'Approved'
                    )
                        THROW 51015, 'A payment request with a posted ledger entry must remain Approved.', 1;
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS [billing].[tr_billing_payment_request_approved_guard]");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS [billing].[tr_billing_payment_ledger_append_only]");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS [billing].[tr_billing_payment_ledger_approved_insert]");

            migrationBuilder.DropForeignKey(
                name: "FK_school_platform_payment_ledger_school_platform_payment_requests_PaymentRequestId_TenantId_SchoolId_Amount_Currency_TransferR~",
                schema: "billing",
                table: "school_platform_payment_ledger");

            migrationBuilder.DropIndex(
                name: "IX_schools_TenantId",
                schema: "registry",
                table: "schools");

            migrationBuilder.DropCheckConstraint(
                name: "ck_registry_schools_school_is_tenant",
                schema: "registry",
                table: "schools");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_school_platform_payment_requests_Id_TenantId_SchoolId_Amount_Currency_TransferReference",
                schema: "billing",
                table: "school_platform_payment_requests");

            migrationBuilder.DropIndex(
                name: "IX_school_platform_payment_ledger_PaymentRequestId_TenantId_SchoolId_Amount_Currency_TransferReference",
                schema: "billing",
                table: "school_platform_payment_ledger");

            migrationBuilder.AddForeignKey(
                name: "FK_school_platform_payment_ledger_school_platform_payment_requests_PaymentRequestId",
                schema: "billing",
                table: "school_platform_payment_ledger",
                column: "PaymentRequestId",
                principalSchema: "billing",
                principalTable: "school_platform_payment_requests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
