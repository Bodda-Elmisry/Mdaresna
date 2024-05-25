using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDefaultData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "CoinsTypes",
                columns: new[] { "Id", "Name", "Note", "Value" },
                values: new object[] { new Guid("aa619624-c134-4e20-867b-e635a5a3b609"), "Standerd", null, 1m });

            migrationBuilder.InsertData(
                table: "SchoolTypes",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[] { new Guid("04d490ad-0994-404e-9d8b-8ecf504811f3"), "Standerd Schools", "Standerd" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "CoinsTypes",
                keyColumn: "Id",
                keyValue: new Guid("aa619624-c134-4e20-867b-e635a5a3b609"));

            migrationBuilder.DeleteData(
                table: "SchoolTypes",
                keyColumn: "Id",
                keyValue: new Guid("04d490ad-0994-404e-9d8b-8ecf504811f3"));
        }
    }
}
