using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Platform.Infrastructure.Persistence.PostgreSql.Migrations.Identity
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
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql(@"
                WITH ranked AS (
                    SELECT ""Id"", ROW_NUMBER() OVER (
                        PARTITION BY ""AccountId"", ""Type""
                        ORDER BY ""CreatedAtUtc"", ""Id"") AS rn
                    FROM identity.login_identifiers
                    WHERE ""Type"" IN ('Phone', 'Email') AND ""SchoolId"" IS NULL
                )
                UPDATE identity.login_identifiers AS li SET ""IsPrimary"" = TRUE
                FROM ranked WHERE li.""Id"" = ranked.""Id"" AND ranked.rn = 1;");

            migrationBuilder.CreateIndex(
                name: "IX_login_identifiers_AccountId_Type",
                schema: "identity",
                table: "login_identifiers",
                columns: new[] { "AccountId", "Type" },
                unique: true,
                filter: "\"IsPrimary\" = TRUE");

            migrationBuilder.AddCheckConstraint(
                name: "ck_identity_login_identifiers_primary_scope",
                schema: "identity",
                table: "login_identifiers",
                sql: "\"IsPrimary\" = FALSE OR (\"Type\" IN ('Email', 'Phone') AND \"SchoolId\" IS NULL)");
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
