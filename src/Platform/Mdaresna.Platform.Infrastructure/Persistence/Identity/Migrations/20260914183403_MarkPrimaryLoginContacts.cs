using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Platform.Infrastructure.Persistence.Identity.Migrations
{
    /// <inheritdoc />
    public partial class MarkPrimaryLoginContacts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPrimary",
                schema: "identity",
                table: "login_identifiers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql(@"
                WITH ranked AS (
                    SELECT [Id], ROW_NUMBER() OVER (
                        PARTITION BY [AccountId], [Type]
                        ORDER BY [CreatedAtUtc], [Id]) AS rn
                    FROM [identity].[login_identifiers]
                    WHERE [Type] IN (N'Phone', N'Email') AND [SchoolId] IS NULL
                )
                UPDATE li SET [IsPrimary] = 1
                FROM [identity].[login_identifiers] AS li
                INNER JOIN ranked ON ranked.[Id] = li.[Id]
                WHERE ranked.rn = 1;");

            migrationBuilder.CreateIndex(
                name: "IX_login_identifiers_AccountId_Type",
                schema: "identity",
                table: "login_identifiers",
                columns: new[] { "AccountId", "Type" },
                unique: true,
                filter: "[IsPrimary] = 1");

            migrationBuilder.AddCheckConstraint(
                name: "ck_identity_login_identifiers_primary_scope",
                schema: "identity",
                table: "login_identifiers",
                sql: "[IsPrimary] = 0 OR ([Type] IN (N'Email', N'Phone') AND [SchoolId] IS NULL)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_login_identifiers_AccountId_Type",
                schema: "identity",
                table: "login_identifiers");

            migrationBuilder.DropCheckConstraint(
                name: "ck_identity_login_identifiers_primary_scope",
                schema: "identity",
                table: "login_identifiers");

            migrationBuilder.DropColumn(
                name: "IsPrimary",
                schema: "identity",
                table: "login_identifiers");
        }
    }
}
