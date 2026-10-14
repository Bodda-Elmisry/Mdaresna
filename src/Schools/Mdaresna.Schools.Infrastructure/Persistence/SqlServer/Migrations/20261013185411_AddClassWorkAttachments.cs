using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Schools.Infrastructure.Persistence.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class AddClassWorkAttachments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "class_activity_attachments",
                schema: "school",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_class_activity_attachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_class_activity_attachments_class_activities_OwnerId",
                        column: x => x.OwnerId,
                        principalSchema: "school",
                        principalTable: "class_activities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_class_activity_attachments_document_versions_DocumentVersionId",
                        column: x => x.DocumentVersionId,
                        principalSchema: "school",
                        principalTable: "document_versions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "homework_attachments",
                schema: "school",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_homework_attachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_homework_attachments_document_versions_DocumentVersionId",
                        column: x => x.DocumentVersionId,
                        principalSchema: "school",
                        principalTable: "document_versions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_homework_attachments_homework_assignments_OwnerId",
                        column: x => x.OwnerId,
                        principalSchema: "school",
                        principalTable: "homework_assignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "lesson_note_attachments",
                schema: "school",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lesson_note_attachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_lesson_note_attachments_document_versions_DocumentVersionId",
                        column: x => x.DocumentVersionId,
                        principalSchema: "school",
                        principalTable: "document_versions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_lesson_note_attachments_student_lesson_notes_OwnerId",
                        column: x => x.OwnerId,
                        principalSchema: "school",
                        principalTable: "student_lesson_notes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_class_activity_attachments_DocumentVersionId",
                schema: "school",
                table: "class_activity_attachments",
                column: "DocumentVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_class_activity_attachments_OwnerId_DocumentVersionId",
                schema: "school",
                table: "class_activity_attachments",
                columns: new[] { "OwnerId", "DocumentVersionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_homework_attachments_DocumentVersionId",
                schema: "school",
                table: "homework_attachments",
                column: "DocumentVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_homework_attachments_OwnerId_DocumentVersionId",
                schema: "school",
                table: "homework_attachments",
                columns: new[] { "OwnerId", "DocumentVersionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_lesson_note_attachments_DocumentVersionId",
                schema: "school",
                table: "lesson_note_attachments",
                column: "DocumentVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_lesson_note_attachments_OwnerId_DocumentVersionId",
                schema: "school",
                table: "lesson_note_attachments",
                columns: new[] { "OwnerId", "DocumentVersionId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "class_activity_attachments",
                schema: "school");

            migrationBuilder.DropTable(
                name: "homework_attachments",
                schema: "school");

            migrationBuilder.DropTable(
                name: "lesson_note_attachments",
                schema: "school");
        }
    }
}
