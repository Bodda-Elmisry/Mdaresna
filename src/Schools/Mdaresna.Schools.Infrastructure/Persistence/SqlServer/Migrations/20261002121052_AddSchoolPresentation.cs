using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Schools.Infrastructure.Persistence.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class AddSchoolPresentation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "school_profile_images",
                schema: "school",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SchoolInformationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsLogo = table.Column<bool>(type: "bit", nullable: false),
                    CaptionAr = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CaptionEn = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_school_profile_images", x => x.Id);
                    table.ForeignKey(
                        name: "FK_school_profile_images_documents_DocumentId",
                        column: x => x.DocumentId,
                        principalSchema: "school",
                        principalTable: "documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_school_profile_images_school_information_SchoolInformationId",
                        column: x => x.SchoolInformationId,
                        principalSchema: "school",
                        principalTable: "school_information",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "school_presentations",
                schema: "school",
                columns: table => new
                {
                    SchoolInformationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TaglineAr = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    TaglineEn = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    AboutAr = table.Column<string>(type: "nvarchar(max)", maxLength: 10000, nullable: false),
                    AboutEn = table.Column<string>(type: "nvarchar(max)", maxLength: 10000, nullable: false),
                    VisionAr = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: false),
                    VisionEn = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: false),
                    MissionAr = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: false),
                    MissionEn = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: false),
                    GoalsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Website = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ContactAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ContactPhone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CoverImageId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CoverPosition = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    Revision = table.Column<int>(type: "int", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_school_presentations", x => x.SchoolInformationId);
                    table.ForeignKey(
                        name: "FK_school_presentations_school_information_SchoolInformationId",
                        column: x => x.SchoolInformationId,
                        principalSchema: "school",
                        principalTable: "school_information",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_school_presentations_school_profile_images_CoverImageId",
                        column: x => x.CoverImageId,
                        principalSchema: "school",
                        principalTable: "school_profile_images",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                schema: "school",
                table: "local_permissions",
                columns: new[] { "Id", "Code", "CreatedAtUtc", "DisplayNameAr", "DisplayNameEn", "IsActive", "Module", "UpdatedAtUtc" },
                values: new object[] { new Guid("7d0640b1-113c-4206-b0ae-6c04fe0eed01"), "school.profile.manage", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "إدارة تعريف المدرسة وصورها", "Manage school presentation and images", true, "profile", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) });

            migrationBuilder.InsertData(
                schema: "school",
                table: "local_role_permissions",
                columns: new[] { "PermissionId", "RoleId", "GrantedAtUtc", "GrantedByUserId" },
                values: new object[] { new Guid("7d0640b1-113c-4206-b0ae-6c04fe0eed01"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null });

            migrationBuilder.CreateIndex(
                name: "IX_school_presentations_CoverImageId",
                schema: "school",
                table: "school_presentations",
                column: "CoverImageId");

            migrationBuilder.CreateIndex(
                name: "IX_school_profile_images_DocumentId",
                schema: "school",
                table: "school_profile_images",
                column: "DocumentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_school_profile_images_SchoolInformationId_SortOrder",
                schema: "school",
                table: "school_profile_images",
                columns: new[] { "SchoolInformationId", "SortOrder" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "school_presentations",
                schema: "school");

            migrationBuilder.DropTable(
                name: "school_profile_images",
                schema: "school");

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_role_permissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("7d0640b1-113c-4206-b0ae-6c04fe0eed01"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713") });

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_permissions",
                keyColumn: "Id",
                keyValue: new Guid("7d0640b1-113c-4206-b0ae-6c04fe0eed01"));
        }
    }
}
