using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailLoginIdentifier : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NormalizedEmail",
                table: "Users",
                type: "nvarchar(320)",
                maxLength: 320,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("d4c7be15-c9b6-4d83-8516-aff52c94f963"),
                column: "NormalizedEmail",
                value: null);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("de36f342-fe3c-46c3-bdfc-bb3fcf2ec7e4"),
                column: "NormalizedEmail",
                value: null);

            migrationBuilder.Sql(
                """
                UPDATE [Users]
                SET [NormalizedEmail] = LOWER(LTRIM(RTRIM([Email])))
                WHERE [Deleted] = 0
                  AND [Email] IS NOT NULL
                  AND LEN(LTRIM(RTRIM([Email]))) BETWEEN 1 AND 320;

                ;WITH [DuplicateEmails] AS
                (
                    SELECT [NormalizedEmail]
                    FROM [Users]
                    WHERE [Deleted] = 0 AND [NormalizedEmail] IS NOT NULL
                    GROUP BY [NormalizedEmail]
                    HAVING COUNT(*) > 1
                )
                UPDATE [Users]
                SET [NormalizedEmail] = NULL
                WHERE [Deleted] = 0
                  AND [NormalizedEmail] IN (SELECT [NormalizedEmail] FROM [DuplicateEmails]);
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Users_NormalizedEmail",
                table: "Users",
                column: "NormalizedEmail",
                unique: true,
                filter: "[NormalizedEmail] IS NOT NULL AND [Deleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Users_PhoneNumber",
                table: "Users",
                column: "PhoneNumber");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_NormalizedEmail",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_PhoneNumber",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "NormalizedEmail",
                table: "Users");
        }
    }
}
