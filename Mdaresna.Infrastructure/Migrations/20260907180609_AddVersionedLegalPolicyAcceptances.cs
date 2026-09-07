using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Mdaresna.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVersionedLegalPolicyAcceptances : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LegalPolicyVersions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Version = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TitleAr = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    TitleEn = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    PrivacyPolicyAr = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PrivacyPolicyEn = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UgcTermsAr = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UgcTermsEn = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    EffectiveDateUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifyDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LegalPolicyVersions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserLegalPolicyAcceptances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LegalPolicyVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AcceptedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AcceptedIpAddress = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    AcceptedUserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RevokedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RevokedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RevocationReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifyDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLegalPolicyAcceptances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserLegalPolicyAcceptances_LegalPolicyVersions_LegalPolicyVersionId",
                        column: x => x.LegalPolicyVersionId,
                        principalTable: "LegalPolicyVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserLegalPolicyAcceptances_Users_RevokedByUserId",
                        column: x => x.RevokedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserLegalPolicyAcceptances_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "LegalPolicyVersions",
                columns: new[] { "Id", "CreateDate", "CreatedByUserId", "EffectiveDateUtc", "IsActive", "LastModifyDate", "PrivacyPolicyAr", "PrivacyPolicyEn", "TitleAr", "TitleEn", "UgcTermsAr", "UgcTermsEn", "Version" },
                values: new object[] { new Guid("e302c5a7-39ad-4a59-9593-e80e544fc246"), new DateTime(2026, 5, 22, 0, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2026, 5, 22, 0, 0, 0, 0, DateTimeKind.Utc), true, new DateTime(2026, 5, 22, 0, 0, 0, 0, DateTimeKind.Utc), "", "", "سياسة الخصوصية وشروط الاستخدام", "Privacy Policy and Terms of Use", "", "", "2026-05-22" });

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "AppPermission", "CreateDate", "Description", "Description_AR", "Key", "LastModifyDate", "Name", "Name_AR", "SchoolPermission" },
                values: new object[,]
                {
                    { new Guid("312718aa-9407-4e1d-a210-54e97d05a403"), true, null, "Revoke a user's active legal policy acceptance", "إلغاء موافقة مستخدم النشطة على سياسة قانونية", "RevokeLegalPolicyAcceptance", null, "Revoke Legal Policy Acceptance", "إلغاء موافقة مستخدم على سياسة", false },
                    { new Guid("792dc31e-28ac-44c1-ac44-9f5e283c03ca"), true, null, "View user legal policy acceptance history", "عرض سجل موافقات المستخدمين على السياسات القانونية", "ViewLegalPolicyAcceptances", null, "View Legal Policy Acceptances", "عرض موافقات المستخدمين على السياسات", false },
                    { new Guid("d1d0cfdd-9bed-4cf9-8894-82fa1280498e"), true, null, "Delete an unused legal policy draft", "حذف مسودة سياسة قانونية غير مستخدمة", "DeleteLegalPolicy", null, "Delete Legal Policy", "حذف سياسة قانونية", false },
                    { new Guid("e563525f-3553-4fbf-86d9-9d3cee16b440"), true, null, "Activate a legal policy and require users to accept it", "تفعيل سياسة قانونية وإلزام المستخدمين بالموافقة عليها", "ActivateLegalPolicy", null, "Activate Legal Policy", "تفعيل سياسة قانونية", false },
                    { new Guid("e5b3f685-9b46-4086-b4b2-446dce9183e7"), true, null, "View legal policies ordered by creation date", "عرض السياسات القانونية بترتيب تاريخ الإنشاء", "ViewLegalPolicies", null, "View Legal Policies", "عرض السياسات القانونية", false },
                    { new Guid("fcca6951-e719-418f-91a9-b9ed598b73da"), true, null, "Create a new legal policy draft", "إنشاء مسودة سياسة قانونية جديدة", "CreateLegalPolicy", null, "Create Legal Policy", "إنشاء سياسة قانونية", false }
                });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "PermissionId", "RoleId", "CreateDate", "LastModifyDate" },
                values: new object[,]
                {
                    { new Guid("312718aa-9407-4e1d-a210-54e97d05a403"), new Guid("228ae7f5-c704-4660-aeb0-0e1f43112ae1"), null, null },
                    { new Guid("792dc31e-28ac-44c1-ac44-9f5e283c03ca"), new Guid("228ae7f5-c704-4660-aeb0-0e1f43112ae1"), null, null },
                    { new Guid("d1d0cfdd-9bed-4cf9-8894-82fa1280498e"), new Guid("228ae7f5-c704-4660-aeb0-0e1f43112ae1"), null, null },
                    { new Guid("e563525f-3553-4fbf-86d9-9d3cee16b440"), new Guid("228ae7f5-c704-4660-aeb0-0e1f43112ae1"), null, null },
                    { new Guid("e5b3f685-9b46-4086-b4b2-446dce9183e7"), new Guid("228ae7f5-c704-4660-aeb0-0e1f43112ae1"), null, null },
                    { new Guid("fcca6951-e719-418f-91a9-b9ed598b73da"), new Guid("228ae7f5-c704-4660-aeb0-0e1f43112ae1"), null, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_LegalPolicyVersions_IsActive",
                table: "LegalPolicyVersions",
                column: "IsActive",
                unique: true,
                filter: "[IsActive] = 1 AND [Deleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_LegalPolicyVersions_Version",
                table: "LegalPolicyVersions",
                column: "Version",
                unique: true,
                filter: "[Deleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_UserLegalPolicyAcceptances_AcceptedAtUtc",
                table: "UserLegalPolicyAcceptances",
                column: "AcceptedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_UserLegalPolicyAcceptances_LegalPolicyVersionId",
                table: "UserLegalPolicyAcceptances",
                column: "LegalPolicyVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserLegalPolicyAcceptances_RevokedByUserId",
                table: "UserLegalPolicyAcceptances",
                column: "RevokedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserLegalPolicyAcceptances_UserId_LegalPolicyVersionId",
                table: "UserLegalPolicyAcceptances",
                columns: new[] { "UserId", "LegalPolicyVersionId" },
                unique: true,
                filter: "[RevokedAtUtc] IS NULL AND [Deleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserLegalPolicyAcceptances");

            migrationBuilder.DropTable(
                name: "LegalPolicyVersions");

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("312718aa-9407-4e1d-a210-54e97d05a403"), new Guid("228ae7f5-c704-4660-aeb0-0e1f43112ae1") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("792dc31e-28ac-44c1-ac44-9f5e283c03ca"), new Guid("228ae7f5-c704-4660-aeb0-0e1f43112ae1") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("d1d0cfdd-9bed-4cf9-8894-82fa1280498e"), new Guid("228ae7f5-c704-4660-aeb0-0e1f43112ae1") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("e563525f-3553-4fbf-86d9-9d3cee16b440"), new Guid("228ae7f5-c704-4660-aeb0-0e1f43112ae1") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("e5b3f685-9b46-4086-b4b2-446dce9183e7"), new Guid("228ae7f5-c704-4660-aeb0-0e1f43112ae1") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("fcca6951-e719-418f-91a9-b9ed598b73da"), new Guid("228ae7f5-c704-4660-aeb0-0e1f43112ae1") });

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("312718aa-9407-4e1d-a210-54e97d05a403"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("792dc31e-28ac-44c1-ac44-9f5e283c03ca"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("d1d0cfdd-9bed-4cf9-8894-82fa1280498e"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("e563525f-3553-4fbf-86d9-9d3cee16b440"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("e5b3f685-9b46-4086-b4b2-446dce9183e7"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("fcca6951-e719-418f-91a9-b9ed598b73da"));
        }
    }
}
