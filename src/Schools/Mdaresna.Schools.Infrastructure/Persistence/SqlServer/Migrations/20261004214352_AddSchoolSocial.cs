using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Mdaresna.Schools.Infrastructure.Persistence.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class AddSchoolSocial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "social_posts",
                schema: "school",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AuthorUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(max)", maxLength: 10000, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CommentsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    Revision = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    PublishedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_social_posts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_social_posts_local_users_AuthorUserId",
                        column: x => x.AuthorUserId,
                        principalSchema: "school",
                        principalTable: "local_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "social_reviewers",
                schema: "school",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_social_reviewers", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_social_reviewers_local_users_UserId",
                        column: x => x.UserId,
                        principalSchema: "school",
                        principalTable: "local_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "social_settings",
                schema: "school",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    RequireApproval = table.Column<bool>(type: "bit", nullable: false),
                    Revision = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_social_settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "social_audits",
                schema: "school",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PostId = table.Column<long>(type: "bigint", nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SnapshotJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_social_audits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_social_audits_local_users_ActorUserId",
                        column: x => x.ActorUserId,
                        principalSchema: "school",
                        principalTable: "local_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_social_audits_social_posts_PostId",
                        column: x => x.PostId,
                        principalSchema: "school",
                        principalTable: "social_posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "social_comments",
                schema: "school",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PostId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Revision = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    EditedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_social_comments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_social_comments_local_users_UserId",
                        column: x => x.UserId,
                        principalSchema: "school",
                        principalTable: "local_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_social_comments_social_posts_PostId",
                        column: x => x.PostId,
                        principalSchema: "school",
                        principalTable: "social_posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "social_likes",
                schema: "school",
                columns: table => new
                {
                    PostId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_social_likes", x => new { x.PostId, x.UserId });
                    table.ForeignKey(
                        name: "FK_social_likes_local_users_UserId",
                        column: x => x.UserId,
                        principalSchema: "school",
                        principalTable: "local_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_social_likes_social_posts_PostId",
                        column: x => x.PostId,
                        principalSchema: "school",
                        principalTable: "social_posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "social_media",
                schema: "school",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PostId = table.Column<long>(type: "bigint", nullable: false),
                    DocumentVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_social_media", x => x.Id);
                    table.ForeignKey(
                        name: "FK_social_media_document_versions_DocumentVersionId",
                        column: x => x.DocumentVersionId,
                        principalSchema: "school",
                        principalTable: "document_versions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_social_media_social_posts_PostId",
                        column: x => x.PostId,
                        principalSchema: "school",
                        principalTable: "social_posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "social_reports",
                schema: "school",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PostId = table.Column<long>(type: "bigint", nullable: false),
                    CommentId = table.Column<long>(type: "bigint", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TargetKey = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Details = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    EvidenceJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ResolvedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ResolvedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Resolution = table.Column<string>(type: "nvarchar(1100)", maxLength: 1100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_social_reports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_social_reports_local_users_ResolvedByUserId",
                        column: x => x.ResolvedByUserId,
                        principalSchema: "school",
                        principalTable: "local_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_social_reports_local_users_UserId",
                        column: x => x.UserId,
                        principalSchema: "school",
                        principalTable: "local_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_social_reports_social_comments_CommentId",
                        column: x => x.CommentId,
                        principalSchema: "school",
                        principalTable: "social_comments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_social_reports_social_posts_PostId",
                        column: x => x.PostId,
                        principalSchema: "school",
                        principalTable: "social_posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                schema: "school",
                table: "local_permissions",
                columns: new[] { "Id", "Code", "CreatedAtUtc", "DisplayNameAr", "DisplayNameEn", "IsActive", "Module", "UpdatedAtUtc" },
                values: new object[,]
                {
                    { new Guid("e04c0679-6900-49e1-b350-e48f9c100001"), "school.social.view", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "عرض مجتمع المدرسة", "View school social", true, "social", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("e04c0679-6900-49e1-b350-e48f9c100002"), "school.social.create", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "إنشاء منشورات المدرسة", "Create school posts", true, "social", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("e04c0679-6900-49e1-b350-e48f9c100003"), "school.social.publish", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "نشر منشورات المدرسة", "Publish school posts", true, "social", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("e04c0679-6900-49e1-b350-e48f9c100004"), "school.social.edit_own", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "تعديل منشوراتي وتعليقاتي", "Edit own posts and comments", true, "social", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("e04c0679-6900-49e1-b350-e48f9c100005"), "school.social.delete_own", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "حذف منشوراتي وتعليقاتي", "Delete own posts and comments", true, "social", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("e04c0679-6900-49e1-b350-e48f9c100006"), "school.social.manage", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "إدارة منشورات وتعليقات الجميع", "Manage all posts and comments", true, "social", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("e04c0679-6900-49e1-b350-e48f9c100007"), "school.social.delete_any", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "حذف منشورات الجميع", "Delete any school post", true, "social", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("e04c0679-6900-49e1-b350-e48f9c100008"), "school.social.moderate", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "مراجعة المنشورات والبلاغات", "Review posts and reports", true, "social", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("e04c0679-6900-49e1-b350-e48f9c100009"), "school.social.interact", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "الإعجاب والتعليق", "Like and comment", true, "social", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("e04c0679-6900-49e1-b350-e48f9c100010"), "school.social.settings", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "ضبط إعدادات مجتمع المدرسة", "Configure school social", true, "social", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) }
                });

            migrationBuilder.InsertData(
                schema: "school",
                table: "social_settings",
                columns: new[] { "Id", "RequireApproval", "Revision" },
                values: new object[] { 1, false, 1 });

            migrationBuilder.InsertData(
                schema: "school",
                table: "local_role_permissions",
                columns: new[] { "PermissionId", "RoleId", "GrantedAtUtc", "GrantedByUserId" },
                values: new object[,]
                {
                    { new Guid("e04c0679-6900-49e1-b350-e48f9c100001"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { new Guid("e04c0679-6900-49e1-b350-e48f9c100002"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { new Guid("e04c0679-6900-49e1-b350-e48f9c100003"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { new Guid("e04c0679-6900-49e1-b350-e48f9c100004"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { new Guid("e04c0679-6900-49e1-b350-e48f9c100005"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { new Guid("e04c0679-6900-49e1-b350-e48f9c100006"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { new Guid("e04c0679-6900-49e1-b350-e48f9c100007"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { new Guid("e04c0679-6900-49e1-b350-e48f9c100008"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { new Guid("e04c0679-6900-49e1-b350-e48f9c100009"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { new Guid("e04c0679-6900-49e1-b350-e48f9c100010"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_social_audits_ActorUserId",
                schema: "school",
                table: "social_audits",
                column: "ActorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_social_audits_PostId_Id",
                schema: "school",
                table: "social_audits",
                columns: new[] { "PostId", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_social_comments_PostId_Id",
                schema: "school",
                table: "social_comments",
                columns: new[] { "PostId", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_social_comments_UserId",
                schema: "school",
                table: "social_comments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_social_likes_UserId",
                schema: "school",
                table: "social_likes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_social_media_DocumentVersionId",
                schema: "school",
                table: "social_media",
                column: "DocumentVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_social_media_PostId_SortOrder",
                schema: "school",
                table: "social_media",
                columns: new[] { "PostId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_social_posts_AuthorUserId_Id",
                schema: "school",
                table: "social_posts",
                columns: new[] { "AuthorUserId", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_social_posts_Status_Id",
                schema: "school",
                table: "social_posts",
                columns: new[] { "Status", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_social_reports_CommentId",
                schema: "school",
                table: "social_reports",
                column: "CommentId");

            migrationBuilder.CreateIndex(
                name: "IX_social_reports_PostId_ResolvedAtUtc",
                schema: "school",
                table: "social_reports",
                columns: new[] { "PostId", "ResolvedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_social_reports_PostId_UserId_TargetKey",
                schema: "school",
                table: "social_reports",
                columns: new[] { "PostId", "UserId", "TargetKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_social_reports_ResolvedByUserId",
                schema: "school",
                table: "social_reports",
                column: "ResolvedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_social_reports_UserId",
                schema: "school",
                table: "social_reports",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "social_audits",
                schema: "school");

            migrationBuilder.DropTable(
                name: "social_likes",
                schema: "school");

            migrationBuilder.DropTable(
                name: "social_media",
                schema: "school");

            migrationBuilder.DropTable(
                name: "social_reports",
                schema: "school");

            migrationBuilder.DropTable(
                name: "social_reviewers",
                schema: "school");

            migrationBuilder.DropTable(
                name: "social_settings",
                schema: "school");

            migrationBuilder.DropTable(
                name: "social_comments",
                schema: "school");

            migrationBuilder.DropTable(
                name: "social_posts",
                schema: "school");

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_role_permissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("e04c0679-6900-49e1-b350-e48f9c100001"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713") });

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_role_permissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("e04c0679-6900-49e1-b350-e48f9c100002"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713") });

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_role_permissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("e04c0679-6900-49e1-b350-e48f9c100003"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713") });

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_role_permissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("e04c0679-6900-49e1-b350-e48f9c100004"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713") });

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_role_permissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("e04c0679-6900-49e1-b350-e48f9c100005"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713") });

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_role_permissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("e04c0679-6900-49e1-b350-e48f9c100006"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713") });

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_role_permissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("e04c0679-6900-49e1-b350-e48f9c100007"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713") });

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_role_permissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("e04c0679-6900-49e1-b350-e48f9c100008"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713") });

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_role_permissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("e04c0679-6900-49e1-b350-e48f9c100009"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713") });

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_role_permissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("e04c0679-6900-49e1-b350-e48f9c100010"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713") });

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_permissions",
                keyColumn: "Id",
                keyValue: new Guid("e04c0679-6900-49e1-b350-e48f9c100001"));

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_permissions",
                keyColumn: "Id",
                keyValue: new Guid("e04c0679-6900-49e1-b350-e48f9c100002"));

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_permissions",
                keyColumn: "Id",
                keyValue: new Guid("e04c0679-6900-49e1-b350-e48f9c100003"));

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_permissions",
                keyColumn: "Id",
                keyValue: new Guid("e04c0679-6900-49e1-b350-e48f9c100004"));

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_permissions",
                keyColumn: "Id",
                keyValue: new Guid("e04c0679-6900-49e1-b350-e48f9c100005"));

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_permissions",
                keyColumn: "Id",
                keyValue: new Guid("e04c0679-6900-49e1-b350-e48f9c100006"));

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_permissions",
                keyColumn: "Id",
                keyValue: new Guid("e04c0679-6900-49e1-b350-e48f9c100007"));

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_permissions",
                keyColumn: "Id",
                keyValue: new Guid("e04c0679-6900-49e1-b350-e48f9c100008"));

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_permissions",
                keyColumn: "Id",
                keyValue: new Guid("e04c0679-6900-49e1-b350-e48f9c100009"));

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_permissions",
                keyColumn: "Id",
                keyValue: new Guid("e04c0679-6900-49e1-b350-e48f9c100010"));
        }
    }
}
