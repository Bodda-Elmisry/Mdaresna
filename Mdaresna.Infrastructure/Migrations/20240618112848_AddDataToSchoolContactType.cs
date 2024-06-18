using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Mdaresna.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDataToSchoolContactType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "SchoolContactTypes",
                columns: new[] { "Id", "CreateDate", "Description", "IconUrl", "LastModifyDate", "Name" },
                values: new object[,]
                {
                    { new Guid("04415888-fe5c-4c91-a8aa-b6a8d1383c08"), null, null, null, null, "Mobile" },
                    { new Guid("5160b1a7-b5ff-4807-a3e0-94fd99579407"), null, null, null, null, "Email" },
                    { new Guid("7b962cc1-db7b-489f-b75e-a478fb932e00"), null, null, null, null, "Phone" },
                    { new Guid("a3fdcfa4-0c57-416b-91a9-51e8601e7d0c"), null, null, null, null, "Fax" }
                });

            migrationBuilder.Sql("Update SchoolContactTypes set CreateDate = GETDATE(), LastModifyDate = GETDATE()");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "SchoolContactTypes",
                keyColumn: "Id",
                keyValue: new Guid("04415888-fe5c-4c91-a8aa-b6a8d1383c08"));

            migrationBuilder.DeleteData(
                table: "SchoolContactTypes",
                keyColumn: "Id",
                keyValue: new Guid("5160b1a7-b5ff-4807-a3e0-94fd99579407"));

            migrationBuilder.DeleteData(
                table: "SchoolContactTypes",
                keyColumn: "Id",
                keyValue: new Guid("7b962cc1-db7b-489f-b75e-a478fb932e00"));

            migrationBuilder.DeleteData(
                table: "SchoolContactTypes",
                keyColumn: "Id",
                keyValue: new Guid("a3fdcfa4-0c57-416b-91a9-51e8601e7d0c"));
        }
    }
}
