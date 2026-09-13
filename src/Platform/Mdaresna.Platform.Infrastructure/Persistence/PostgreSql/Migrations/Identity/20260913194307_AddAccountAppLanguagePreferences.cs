using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Platform.Infrastructure.Persistence.PostgreSql.Migrations.Identity
{
    /// <inheritdoc />
    public partial class AddAccountAppLanguagePreferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "account_app_language_preferences",
                schema: "identity",
                columns: table => new
                {
                    AccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    AppCode = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    LanguageCode = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp(3) with time zone", precision: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_account_app_language_preferences", x => new { x.AccountId, x.AppCode });
                    table.CheckConstraint("ck_identity_app_language_app_code", "\"AppCode\" IN ('platform', 'schools', 'family')");
                    table.CheckConstraint("ck_identity_app_language_language_code", "\"LanguageCode\" IN ('ar', 'en')");
                    table.CheckConstraint("ck_identity_app_language_updated_at_utc", "TRUE");
                    table.ForeignKey(
                        name: "FK_account_app_language_preferences_accounts_AccountId",
                        column: x => x.AccountId,
                        principalSchema: "identity",
                        principalTable: "accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "account_app_language_preferences",
                schema: "identity");
        }
    }
}
