using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Mdaresna.Schools.Infrastructure.Persistence.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentLessonNotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "student_lesson_notes",
                schema: "school",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClassSectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentEnrollmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WeeklyTimetableSlotId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LessonDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
                    NoteText = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    RatingLevel = table.Column<int>(type: "int", nullable: true),
                    RequiresFollowUp = table.Column<bool>(type: "bit", nullable: false),
                    Visibility = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_student_lesson_notes", x => x.Id);
                    table.CheckConstraint("CK_student_lesson_notes_rating", "\"RatingLevel\" IS NULL OR (\"RatingLevel\" >= 1 AND \"RatingLevel\" <= 5)");
                    table.ForeignKey(
                        name: "FK_student_lesson_notes_class_sections_ClassSectionId",
                        column: x => x.ClassSectionId,
                        principalSchema: "school",
                        principalTable: "class_sections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_student_lesson_notes_local_users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalSchema: "school",
                        principalTable: "local_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_student_lesson_notes_local_users_DeletedByUserId",
                        column: x => x.DeletedByUserId,
                        principalSchema: "school",
                        principalTable: "local_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_student_lesson_notes_local_users_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalSchema: "school",
                        principalTable: "local_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_student_lesson_notes_student_enrollments_StudentEnrollmentId",
                        column: x => x.StudentEnrollmentId,
                        principalSchema: "school",
                        principalTable: "student_enrollments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_student_lesson_notes_weekly_timetable_slots_WeeklyTimetableSlotId",
                        column: x => x.WeeklyTimetableSlotId,
                        principalSchema: "school",
                        principalTable: "weekly_timetable_slots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "student_lesson_note_audits",
                schema: "school",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentLessonNoteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SnapshotJson = table.Column<string>(type: "nvarchar(max)", maxLength: 6000, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_student_lesson_note_audits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_student_lesson_note_audits_local_users_ActorUserId",
                        column: x => x.ActorUserId,
                        principalSchema: "school",
                        principalTable: "local_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_student_lesson_note_audits_student_lesson_notes_StudentLessonNoteId",
                        column: x => x.StudentLessonNoteId,
                        principalSchema: "school",
                        principalTable: "student_lesson_notes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "school",
                table: "local_permissions",
                columns: new[] { "Id", "Code", "CreatedAtUtc", "DisplayNameAr", "DisplayNameEn", "IsActive", "Module", "UpdatedAtUtc" },
                values: new object[,]
                {
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd3d"), "school.lesson_notes.view", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "عرض ملاحظات الطلاب داخل الحصة", "View student lesson notes", true, "lesson_notes", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd3e"), "school.lesson_notes.manage", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "إدارة ملاحظات الطلاب داخل الحصة", "Manage student lesson notes", true, "lesson_notes", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) }
                });

            migrationBuilder.InsertData(
                schema: "school",
                table: "local_role_permissions",
                columns: new[] { "PermissionId", "RoleId", "GrantedAtUtc", "GrantedByUserId" },
                values: new object[,]
                {
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd3d"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd3e"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_student_lesson_note_audits_ActorUserId",
                schema: "school",
                table: "student_lesson_note_audits",
                column: "ActorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_student_lesson_note_audits_StudentLessonNoteId_CreatedAtUtc",
                schema: "school",
                table: "student_lesson_note_audits",
                columns: new[] { "StudentLessonNoteId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_student_lesson_notes_ClassSectionId_LessonDate_WeeklyTimetableSlotId",
                schema: "school",
                table: "student_lesson_notes",
                columns: new[] { "ClassSectionId", "LessonDate", "WeeklyTimetableSlotId" });

            migrationBuilder.CreateIndex(
                name: "IX_student_lesson_notes_CreatedByUserId",
                schema: "school",
                table: "student_lesson_notes",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_student_lesson_notes_DeletedByUserId",
                schema: "school",
                table: "student_lesson_notes",
                column: "DeletedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_student_lesson_notes_StudentEnrollmentId_LessonDate",
                schema: "school",
                table: "student_lesson_notes",
                columns: new[] { "StudentEnrollmentId", "LessonDate" });

            migrationBuilder.CreateIndex(
                name: "IX_student_lesson_notes_UpdatedByUserId",
                schema: "school",
                table: "student_lesson_notes",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_student_lesson_notes_WeeklyTimetableSlotId",
                schema: "school",
                table: "student_lesson_notes",
                column: "WeeklyTimetableSlotId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "student_lesson_note_audits",
                schema: "school");

            migrationBuilder.DropTable(
                name: "student_lesson_notes",
                schema: "school");

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_role_permissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd3d"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713") });

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_role_permissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd3e"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713") });

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_permissions",
                keyColumn: "Id",
                keyValue: new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd3d"));

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_permissions",
                keyColumn: "Id",
                keyValue: new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd3e"));
        }
    }
}
