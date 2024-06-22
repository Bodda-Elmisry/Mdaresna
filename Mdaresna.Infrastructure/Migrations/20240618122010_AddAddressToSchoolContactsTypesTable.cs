using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAddressToSchoolContactsTypesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "SchoolContactTypes",
                columns: new[] { "Id", "CreateDate", "Description", "IconUrl", "LastModifyDate", "Name" },
                values: new object[] { new Guid("3851e877-81ec-4e74-a9ee-ab29265e873f"), null, null, null, null, "Address" });

            migrationBuilder.Sql("Update SchoolContactTypes set CreateDate = GETDATE(), LastModifyDate = GETDATE() where Id = '3851e877-81ec-4e74-a9ee-ab29265e873f'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "SchoolContactTypes",
                keyColumn: "Id",
                keyValue: new Guid("3851e877-81ec-4e74-a9ee-ab29265e873f"));
        }
    }
}
