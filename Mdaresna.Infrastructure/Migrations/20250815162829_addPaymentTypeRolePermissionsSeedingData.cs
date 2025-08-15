using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Mdaresna.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addPaymentTypeRolePermissionsSeedingData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "PermissionId", "RoleId", "CreateDate", "LastModifyDate" },
                values: new object[,]
                {
                    { new Guid("0a0ab8a7-1d51-4b03-b10a-647a8f90ec24"), new Guid("228ae7f5-c704-4660-aeb0-0e1f43112ae1"), null, null },
                    { new Guid("56d9a5b6-28a6-4e8d-ade6-54ad37c846bd"), new Guid("228ae7f5-c704-4660-aeb0-0e1f43112ae1"), null, null },
                    { new Guid("9c8526e9-9119-43d8-a434-4c4828c8a5d9"), new Guid("228ae7f5-c704-4660-aeb0-0e1f43112ae1"), null, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("0a0ab8a7-1d51-4b03-b10a-647a8f90ec24"), new Guid("228ae7f5-c704-4660-aeb0-0e1f43112ae1") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("56d9a5b6-28a6-4e8d-ade6-54ad37c846bd"), new Guid("228ae7f5-c704-4660-aeb0-0e1f43112ae1") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("9c8526e9-9119-43d8-a434-4c4828c8a5d9"), new Guid("228ae7f5-c704-4660-aeb0-0e1f43112ae1") });
        }
    }
}
