﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Mdaresna.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addIdToUserRoleTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserRoles",
                table: "UserRoles");

            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73"), new Guid("d4c7be15-c9b6-4d83-8516-aff52c94f963") });

            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { new Guid("92d00b28-9d25-4bd2-a587-6c22a3a07a92"), new Guid("d4c7be15-c9b6-4d83-8516-aff52c94f963") });

            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { new Guid("228ae7f5-c704-4660-aeb0-0e1f43112ae1"), new Guid("de36f342-fe3c-46c3-bdfc-bb3fcf2ec7e4") });

            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { new Guid("92d00b28-9d25-4bd2-a587-6c22a3a07a92"), new Guid("de36f342-fe3c-46c3-bdfc-bb3fcf2ec7e4") });

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "UserRoles",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserRoles",
                table: "UserRoles",
                columns: new[] { "Id", "RoleId", "UserId" });

            migrationBuilder.InsertData(
                table: "UserRoles",
                columns: new[] { "Id", "RoleId", "UserId", "CreateDate", "LastModifyDate", "SchoolId" },
                values: new object[,]
                {
                    { new Guid("87c582e3-7fdc-4235-be0e-2663925bc9ee"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73"), new Guid("d4c7be15-c9b6-4d83-8516-aff52c94f963"), null, null, null },
                    { new Guid("a65cba57-baeb-42df-89f6-88ef7e3d2b66"), new Guid("92d00b28-9d25-4bd2-a587-6c22a3a07a92"), new Guid("d4c7be15-c9b6-4d83-8516-aff52c94f963"), null, null, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_UserId",
                table: "UserRoles",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserRoles",
                table: "UserRoles");

            migrationBuilder.DropIndex(
                name: "IX_UserRoles_UserId",
                table: "UserRoles");

            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumns: new[] { "Id", "RoleId", "UserId" },
                keyColumnTypes: new[] { "uniqueidentifier", "uniqueidentifier", "uniqueidentifier" },
                keyValues: new object[] { new Guid("87c582e3-7fdc-4235-be0e-2663925bc9ee"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73"), new Guid("d4c7be15-c9b6-4d83-8516-aff52c94f963") });

            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumns: new[] { "Id", "RoleId", "UserId" },
                keyColumnTypes: new[] { "uniqueidentifier", "uniqueidentifier", "uniqueidentifier" },
                keyValues: new object[] { new Guid("a65cba57-baeb-42df-89f6-88ef7e3d2b66"), new Guid("92d00b28-9d25-4bd2-a587-6c22a3a07a92"), new Guid("d4c7be15-c9b6-4d83-8516-aff52c94f963") });

            migrationBuilder.DropColumn(
                name: "Id",
                table: "UserRoles");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserRoles",
                table: "UserRoles",
                columns: new[] { "UserId", "RoleId" });

            migrationBuilder.InsertData(
                table: "UserRoles",
                columns: new[] { "RoleId", "UserId", "CreateDate", "LastModifyDate", "SchoolId" },
                values: new object[,]
                {
                    { new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73"), new Guid("d4c7be15-c9b6-4d83-8516-aff52c94f963"), null, null, null },
                    { new Guid("92d00b28-9d25-4bd2-a587-6c22a3a07a92"), new Guid("d4c7be15-c9b6-4d83-8516-aff52c94f963"), null, null, null },
                    { new Guid("228ae7f5-c704-4660-aeb0-0e1f43112ae1"), new Guid("de36f342-fe3c-46c3-bdfc-bb3fcf2ec7e4"), null, null, null },
                    { new Guid("92d00b28-9d25-4bd2-a587-6c22a3a07a92"), new Guid("de36f342-fe3c-46c3-bdfc-bb3fcf2ec7e4"), null, null, null }
                });
        }
    }
}
