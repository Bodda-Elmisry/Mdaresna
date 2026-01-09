using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Mdaresna.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "AppPermission", "CreateDate", "Description", "Description_AR", "Key", "LastModifyDate", "Name", "Name_AR", "SchoolPermission" },
                values: new object[,]
                {
                    { new Guid("5b4c93d4-22e3-4d89-bdb8-6b6ec2f7e840"), false, null, "Delete school post (school manager)", "Delete school post (school manager)", "DeleteSchoolPost", null, "Delete School Post", "Delete School Post", true },
                    { new Guid("8a01a316-9151-4bb7-8b0e-a87e5ee7e367"), true, null, "Delete school post", "Delete school post", "DeletePost", null, "Delete Post", "Delete Post", false },
                    { new Guid("a44f8b6e-7b16-4497-9c4b-8e55eaf4e7d4"), true, null, "View reported school posts with report counts", "View reported school posts with report counts", "ShowReportedPosts", null, "Show Reported Posts", "Show Reported Posts", false }
                });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "PermissionId", "RoleId", "CreateDate", "LastModifyDate" },
                values: new object[,]
                {
                    { new Guid("8a01a316-9151-4bb7-8b0e-a87e5ee7e367"), new Guid("228ae7f5-c704-4660-aeb0-0e1f43112ae1"), null, null },
                    { new Guid("a44f8b6e-7b16-4497-9c4b-8e55eaf4e7d4"), new Guid("228ae7f5-c704-4660-aeb0-0e1f43112ae1"), null, null },
                    { new Guid("5b4c93d4-22e3-4d89-bdb8-6b6ec2f7e840"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73"), null, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("8a01a316-9151-4bb7-8b0e-a87e5ee7e367"), new Guid("228ae7f5-c704-4660-aeb0-0e1f43112ae1") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("a44f8b6e-7b16-4497-9c4b-8e55eaf4e7d4"), new Guid("228ae7f5-c704-4660-aeb0-0e1f43112ae1") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("5b4c93d4-22e3-4d89-bdb8-6b6ec2f7e840"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73") });

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("5b4c93d4-22e3-4d89-bdb8-6b6ec2f7e840"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("8a01a316-9151-4bb7-8b0e-a87e5ee7e367"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("a44f8b6e-7b16-4497-9c4b-8e55eaf4e7d4"));
        }
    }
}
