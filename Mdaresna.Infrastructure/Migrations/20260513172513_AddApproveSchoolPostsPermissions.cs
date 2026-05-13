using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddApproveSchoolPostsPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("5b4c93d4-22e3-4d89-bdb8-6b6ec2f7e840"),
                columns: new[] { "Description", "Description_AR", "Name_AR" },
                values: new object[] { "Delete school post", "حذف منشور المدرسه", "حذف منشورات المدرسه" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("8a01a316-9151-4bb7-8b0e-a87e5ee7e367"),
                columns: new[] { "Description_AR", "Name_AR" },
                values: new object[] { "حذف منشور المدرسه", "خذف المنشور" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("a44f8b6e-7b16-4497-9c4b-8e55eaf4e7d4"),
                columns: new[] { "Description_AR", "Name_AR" },
                values: new object[] { "عرض منشورات المدارس المبلغ عنها مع عدد البلاغات", "عرض المنشورات المبلغ عنها" });

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "AppPermission", "CreateDate", "Description", "Description_AR", "Key", "LastModifyDate", "Name", "Name_AR", "SchoolPermission" },
                values: new object[] { new Guid("99542671-e575-43f7-9c67-5290d9cf4578"), false, null, "Approve school post", "الموافقه على منشورات المدرسه", "ApproveSchoolPost", null, "Approve School Post", "الموافقه على المنشورات", true });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "PermissionId", "RoleId", "CreateDate", "LastModifyDate" },
                values: new object[] { new Guid("99542671-e575-43f7-9c67-5290d9cf4578"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73"), null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("99542671-e575-43f7-9c67-5290d9cf4578"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73") });

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("99542671-e575-43f7-9c67-5290d9cf4578"));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("5b4c93d4-22e3-4d89-bdb8-6b6ec2f7e840"),
                columns: new[] { "Description", "Description_AR", "Name_AR" },
                values: new object[] { "Delete school post (school manager)", "Delete school post (school manager)", "Delete School Post" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("8a01a316-9151-4bb7-8b0e-a87e5ee7e367"),
                columns: new[] { "Description_AR", "Name_AR" },
                values: new object[] { "Delete school post", "Delete Post" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("a44f8b6e-7b16-4497-9c4b-8e55eaf4e7d4"),
                columns: new[] { "Description_AR", "Name_AR" },
                values: new object[] { "View reported school posts with report counts", "Show Reported Posts" });
        }
    }
}
