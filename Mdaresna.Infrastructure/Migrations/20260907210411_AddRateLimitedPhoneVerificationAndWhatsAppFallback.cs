using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Mdaresna.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRateLimitedPhoneVerificationAndWhatsAppFallback : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VerificationChallenges",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Purpose = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CodeHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    ExpiresAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VerifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LockedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FailedVerificationAttempts = table.Column<int>(type: "int", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifyDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VerificationChallenges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VerificationChallenges_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VerificationDeliveryAttempts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VerificationChallengeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Purpose = table.Column<int>(type: "int", nullable: false),
                    Channel = table.Column<int>(type: "int", nullable: false),
                    IsSuccess = table.Column<bool>(type: "bit", nullable: false),
                    ProviderResponse = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SentByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifyDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VerificationDeliveryAttempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VerificationDeliveryAttempts_Users_SentByUserId",
                        column: x => x.SentByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VerificationDeliveryAttempts_VerificationChallenges_VerificationChallengeId",
                        column: x => x.VerificationChallengeId,
                        principalTable: "VerificationChallenges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WhatsAppVerificationRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VerificationChallengeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    RequestedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PreparedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PreparedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SentConfirmedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SentConfirmedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifyDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WhatsAppVerificationRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WhatsAppVerificationRequests_Users_PreparedByUserId",
                        column: x => x.PreparedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WhatsAppVerificationRequests_Users_SentConfirmedByUserId",
                        column: x => x.SentConfirmedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WhatsAppVerificationRequests_VerificationChallenges_VerificationChallengeId",
                        column: x => x.VerificationChallengeId,
                        principalTable: "VerificationChallenges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "AppPermission", "CreateDate", "Description", "Description_AR", "Key", "LastModifyDate", "Name", "Name_AR", "SchoolPermission" },
                values: new object[,]
                {
                    { new Guid("a13d7c8e-4f2a-4b91-8d6e-1c5a7b9e2031"), true, null, "View manual WhatsApp verification requests", "عرض طلبات إرسال أكواد التحقق يدويًا عبر واتساب", "ViewWhatsAppVerificationRequests", null, "View WhatsApp Verification Requests", "عرض طلبات إرسال أكواد التحقق عبر واتساب", false },
                    { new Guid("b24e8d9f-5a3b-4c02-9e7f-2d6b8c0f3142"), true, null, "Generate a verification code and open its WhatsApp message", "إنشاء كود تحقق وتجهيز رسالته للإرسال عبر واتساب", "PrepareWhatsAppVerificationMessage", null, "Prepare WhatsApp Verification Message", "تجهيز رسالة التحقق عبر واتساب", false },
                    { new Guid("c35f9ea0-6b4c-4d13-af80-3e7c9d104253"), true, null, "Confirm that a manual WhatsApp verification message was sent", "تأكيد إرسال رسالة التحقق يدويًا عبر واتساب", "ConfirmWhatsAppVerificationSent", null, "Confirm WhatsApp Verification Sent", "تأكيد إرسال رسالة التحقق عبر واتساب", false }
                });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "PermissionId", "RoleId", "CreateDate", "LastModifyDate" },
                values: new object[,]
                {
                    { new Guid("a13d7c8e-4f2a-4b91-8d6e-1c5a7b9e2031"), new Guid("228ae7f5-c704-4660-aeb0-0e1f43112ae1"), null, null },
                    { new Guid("b24e8d9f-5a3b-4c02-9e7f-2d6b8c0f3142"), new Guid("228ae7f5-c704-4660-aeb0-0e1f43112ae1"), null, null },
                    { new Guid("c35f9ea0-6b4c-4d13-af80-3e7c9d104253"), new Guid("228ae7f5-c704-4660-aeb0-0e1f43112ae1"), null, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_VerificationChallenges_PhoneNumber_Purpose_CreateDate",
                table: "VerificationChallenges",
                columns: new[] { "PhoneNumber", "Purpose", "CreateDate" });

            migrationBuilder.CreateIndex(
                name: "IX_VerificationChallenges_Status_ExpiresAtUtc",
                table: "VerificationChallenges",
                columns: new[] { "Status", "ExpiresAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_VerificationChallenges_UserId",
                table: "VerificationChallenges",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_VerificationDeliveryAttempts_PhoneNumber_Purpose_Channel_CreateDate",
                table: "VerificationDeliveryAttempts",
                columns: new[] { "PhoneNumber", "Purpose", "Channel", "CreateDate" });

            migrationBuilder.CreateIndex(
                name: "IX_VerificationDeliveryAttempts_SentByUserId",
                table: "VerificationDeliveryAttempts",
                column: "SentByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_VerificationDeliveryAttempts_VerificationChallengeId",
                table: "VerificationDeliveryAttempts",
                column: "VerificationChallengeId");

            migrationBuilder.CreateIndex(
                name: "IX_WhatsAppVerificationRequests_PreparedByUserId",
                table: "WhatsAppVerificationRequests",
                column: "PreparedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WhatsAppVerificationRequests_SentConfirmedByUserId",
                table: "WhatsAppVerificationRequests",
                column: "SentConfirmedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WhatsAppVerificationRequests_Status_RequestedAtUtc",
                table: "WhatsAppVerificationRequests",
                columns: new[] { "Status", "RequestedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_WhatsAppVerificationRequests_VerificationChallengeId",
                table: "WhatsAppVerificationRequests",
                column: "VerificationChallengeId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VerificationDeliveryAttempts");

            migrationBuilder.DropTable(
                name: "WhatsAppVerificationRequests");

            migrationBuilder.DropTable(
                name: "VerificationChallenges");

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("a13d7c8e-4f2a-4b91-8d6e-1c5a7b9e2031"), new Guid("228ae7f5-c704-4660-aeb0-0e1f43112ae1") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("b24e8d9f-5a3b-4c02-9e7f-2d6b8c0f3142"), new Guid("228ae7f5-c704-4660-aeb0-0e1f43112ae1") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("c35f9ea0-6b4c-4d13-af80-3e7c9d104253"), new Guid("228ae7f5-c704-4660-aeb0-0e1f43112ae1") });

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("a13d7c8e-4f2a-4b91-8d6e-1c5a7b9e2031"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("b24e8d9f-5a3b-4c02-9e7f-2d6b8c0f3142"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("c35f9ea0-6b4c-4d13-af80-3e7c9d104253"));
        }
    }
}
