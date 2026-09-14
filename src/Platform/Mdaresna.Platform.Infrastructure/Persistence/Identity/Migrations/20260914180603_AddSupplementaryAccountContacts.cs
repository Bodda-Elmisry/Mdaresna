using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Platform.Infrastructure.Persistence.Identity.Migrations
{
    /// <inheritdoc />
    public partial class AddSupplementaryAccountContacts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "account_contacts",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    NormalizedValue = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_account_contacts", x => x.Id);
                    table.CheckConstraint("ck_identity_account_contacts_type", "[Type] IN (N'Phone', N'Email', N'Address')");
                    table.CheckConstraint("ck_identity_account_contacts_value", "[Value] <> N'' AND [NormalizedValue] <> N''");
                    table.ForeignKey(
                        name: "FK_account_contacts_accounts_AccountId",
                        column: x => x.AccountId,
                        principalSchema: "identity",
                        principalTable: "accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_account_contacts_AccountId",
                schema: "identity",
                table: "account_contacts",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_account_contacts_AccountId_Type_NormalizedValue",
                schema: "identity",
                table: "account_contacts",
                columns: new[] { "AccountId", "Type", "NormalizedValue" },
                unique: true,
                filter: "[Type] IN (N'Phone', N'Email')");

            migrationBuilder.Sql("INSERT INTO [identity].[account_contacts] " +
                "([Id], [AccountId], [Type], [Value], [NormalizedValue], [CreatedAtUtc], [UpdatedAtUtc]) " +
                "SELECT [Id], [AccountId], N'Address', [Value], [Value], [CreatedAtUtc], [UpdatedAtUtc] " +
                "FROM [identity].[account_addresses]");

            migrationBuilder.DropTable(name: "account_addresses", schema: "identity");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "account_addresses",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_account_addresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_account_addresses_accounts_AccountId",
                        column: x => x.AccountId,
                        principalSchema: "identity",
                        principalTable: "accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_account_addresses_AccountId",
                schema: "identity",
                table: "account_addresses",
                column: "AccountId");

            migrationBuilder.Sql("INSERT INTO [identity].[account_addresses] " +
                "([Id], [AccountId], [Value], [CreatedAtUtc], [UpdatedAtUtc]) " +
                "SELECT [Id], [AccountId], [Value], [CreatedAtUtc], [UpdatedAtUtc] " +
                "FROM [identity].[account_contacts] WHERE [Type] = N'Address'");

            migrationBuilder.DropTable(name: "account_contacts", schema: "identity");
        }
    }
}
