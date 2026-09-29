using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Mdaresna.Schools.Infrastructure.Persistence.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class AddSchoolHomework : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "homework_assignments",
                schema: "school",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClassSectionSubjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    DeliveryMode = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Instructions = table.Column<string>(type: "character varying(6000)", maxLength: 6000, nullable: false),
                    CurriculumSubjectBookId = table.Column<Guid>(type: "uuid", nullable: true),
                    BookReference = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    DueAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    TimeZoneIdSnapshot = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TotalScore = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    AllowLateSubmission = table.Column<bool>(type: "boolean", nullable: false),
                    MaximumAttempts = table.Column<int>(type: "integer", nullable: false),
                    AllowUnsubmitBeforeDue = table.Column<bool>(type: "boolean", nullable: false),
                    ShowCorrectAnswersAfter = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    ShuffleQuestions = table.Column<bool>(type: "boolean", nullable: false),
                    ShuffleOptions = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PublishedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PublishedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClosedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_homework_assignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_homework_assignments_class_section_subjects_ClassSectionSub~",
                        column: x => x.ClassSectionSubjectId,
                        principalSchema: "school",
                        principalTable: "class_section_subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_homework_assignments_curriculum_subject_books_CurriculumSub~",
                        column: x => x.CurriculumSubjectBookId,
                        principalSchema: "school",
                        principalTable: "curriculum_subject_books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_homework_assignments_local_users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalSchema: "school",
                        principalTable: "local_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_homework_assignments_local_users_PublishedByUserId",
                        column: x => x.PublishedByUserId,
                        principalSchema: "school",
                        principalTable: "local_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "homework_questions",
                schema: "school",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HomeworkAssignmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    Prompt = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false),
                    MaxScore = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    ModelAnswer = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Explanation = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_homework_questions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_homework_questions_homework_assignments_HomeworkAssignmentId",
                        column: x => x.HomeworkAssignmentId,
                        principalSchema: "school",
                        principalTable: "homework_assignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "student_homework",
                schema: "school",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HomeworkAssignmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentEnrollmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CurrentAttemptId = table.Column<Guid>(type: "uuid", nullable: true),
                    FinalScore = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    TeacherFeedback = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    GradedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    GradedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ExcuseReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_student_homework", x => x.Id);
                    table.ForeignKey(
                        name: "FK_student_homework_homework_assignments_HomeworkAssignmentId",
                        column: x => x.HomeworkAssignmentId,
                        principalSchema: "school",
                        principalTable: "homework_assignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_student_homework_local_users_GradedByUserId",
                        column: x => x.GradedByUserId,
                        principalSchema: "school",
                        principalTable: "local_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_student_homework_student_enrollments_StudentEnrollmentId",
                        column: x => x.StudentEnrollmentId,
                        principalSchema: "school",
                        principalTable: "student_enrollments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "homework_question_blanks",
                schema: "school",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HomeworkQuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Token = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    MaxScore = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    IgnoreCase = table.Column<bool>(type: "boolean", nullable: false),
                    IgnoreDiacritics = table.Column<bool>(type: "boolean", nullable: false),
                    CollapseWhitespace = table.Column<bool>(type: "boolean", nullable: false),
                    SendUnmatchedToManualReview = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_homework_question_blanks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_homework_question_blanks_homework_questions_HomeworkQuestio~",
                        column: x => x.HomeworkQuestionId,
                        principalSchema: "school",
                        principalTable: "homework_questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "homework_question_options",
                schema: "school",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HomeworkQuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Text = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsCorrect = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_homework_question_options", x => x.Id);
                    table.ForeignKey(
                        name: "FK_homework_question_options_homework_questions_HomeworkQuesti~",
                        column: x => x.HomeworkQuestionId,
                        principalSchema: "school",
                        principalTable: "homework_questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "homework_audits",
                schema: "school",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HomeworkAssignmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentHomeworkId = table.Column<Guid>(type: "uuid", nullable: true),
                    Action = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PayloadJson = table.Column<string>(type: "text", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_homework_audits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_homework_audits_homework_assignments_HomeworkAssignmentId",
                        column: x => x.HomeworkAssignmentId,
                        principalSchema: "school",
                        principalTable: "homework_assignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_homework_audits_local_users_ActorUserId",
                        column: x => x.ActorUserId,
                        principalSchema: "school",
                        principalTable: "local_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_homework_audits_student_homework_StudentHomeworkId",
                        column: x => x.StudentHomeworkId,
                        principalSchema: "school",
                        principalTable: "student_homework",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "homework_submission_attempts",
                schema: "school",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentHomeworkId = table.Column<Guid>(type: "uuid", nullable: false),
                    AttemptNumber = table.Column<int>(type: "integer", nullable: false),
                    Channel = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SubmittedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    SubmittedByActorType = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: true),
                    SubmittedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    AutoScore = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    FinalScore = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_homework_submission_attempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_homework_submission_attempts_student_homework_StudentHomewo~",
                        column: x => x.StudentHomeworkId,
                        principalSchema: "school",
                        principalTable: "student_homework",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "homework_blank_accepted_answers",
                schema: "school",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HomeworkQuestionBlankId = table.Column<Guid>(type: "uuid", nullable: false),
                    Answer = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    NormalizedAnswer = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_homework_blank_accepted_answers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_homework_blank_accepted_answers_homework_question_blanks_Ho~",
                        column: x => x.HomeworkQuestionBlankId,
                        principalSchema: "school",
                        principalTable: "homework_question_blanks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "homework_student_answers",
                schema: "school",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HomeworkSubmissionAttemptId = table.Column<Guid>(type: "uuid", nullable: false),
                    HomeworkQuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    TextAnswer = table.Column<string>(type: "character varying(6000)", maxLength: 6000, nullable: true),
                    GradingStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Score = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    TeacherFeedback = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    GradedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    GradedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_homework_student_answers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_homework_student_answers_homework_questions_HomeworkQuestio~",
                        column: x => x.HomeworkQuestionId,
                        principalSchema: "school",
                        principalTable: "homework_questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_homework_student_answers_homework_submission_attempts_Homew~",
                        column: x => x.HomeworkSubmissionAttemptId,
                        principalSchema: "school",
                        principalTable: "homework_submission_attempts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_homework_student_answers_local_users_GradedByUserId",
                        column: x => x.GradedByUserId,
                        principalSchema: "school",
                        principalTable: "local_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "homework_student_blank_answers",
                schema: "school",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HomeworkStudentAnswerId = table.Column<Guid>(type: "uuid", nullable: false),
                    HomeworkQuestionBlankId = table.Column<Guid>(type: "uuid", nullable: false),
                    Answer = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    NormalizedAnswer = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    IsMatched = table.Column<bool>(type: "boolean", nullable: true),
                    Score = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    IsManualOverride = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_homework_student_blank_answers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_homework_student_blank_answers_homework_question_blanks_Hom~",
                        column: x => x.HomeworkQuestionBlankId,
                        principalSchema: "school",
                        principalTable: "homework_question_blanks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_homework_student_blank_answers_homework_student_answers_Hom~",
                        column: x => x.HomeworkStudentAnswerId,
                        principalSchema: "school",
                        principalTable: "homework_student_answers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "homework_student_selected_options",
                schema: "school",
                columns: table => new
                {
                    HomeworkStudentAnswerId = table.Column<Guid>(type: "uuid", nullable: false),
                    HomeworkQuestionOptionId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_homework_student_selected_options", x => new { x.HomeworkStudentAnswerId, x.HomeworkQuestionOptionId });
                    table.ForeignKey(
                        name: "FK_homework_student_selected_options_homework_question_options~",
                        column: x => x.HomeworkQuestionOptionId,
                        principalSchema: "school",
                        principalTable: "homework_question_options",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_homework_student_selected_options_homework_student_answers_~",
                        column: x => x.HomeworkStudentAnswerId,
                        principalSchema: "school",
                        principalTable: "homework_student_answers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "school",
                table: "local_permissions",
                columns: new[] { "Id", "Code", "CreatedAtUtc", "DisplayNameAr", "DisplayNameEn", "IsActive", "Module", "UpdatedAtUtc" },
                values: new object[,]
                {
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd23"), "school.homework.view", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "عرض الواجبات", "View homework", true, "homework", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd24"), "school.homework.manage", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "إدارة الواجبات", "Manage homework", true, "homework", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd25"), "school.homework.publish", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "نشر وإغلاق الواجبات", "Publish and close homework", true, "homework", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd26"), "school.homework.grade", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "تقييم واجبات الطلاب", "Grade student homework", true, "homework", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) }
                });

            migrationBuilder.InsertData(
                schema: "school",
                table: "local_role_permissions",
                columns: new[] { "PermissionId", "RoleId", "GrantedAtUtc", "GrantedByUserId" },
                values: new object[,]
                {
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd23"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd24"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd25"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd26"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_homework_assignments_ClassSectionSubjectId_Status_DueAtUtc",
                schema: "school",
                table: "homework_assignments",
                columns: new[] { "ClassSectionSubjectId", "Status", "DueAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_homework_assignments_CreatedByUserId",
                schema: "school",
                table: "homework_assignments",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_homework_assignments_CurriculumSubjectBookId",
                schema: "school",
                table: "homework_assignments",
                column: "CurriculumSubjectBookId");

            migrationBuilder.CreateIndex(
                name: "IX_homework_assignments_PublishedByUserId",
                schema: "school",
                table: "homework_assignments",
                column: "PublishedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_homework_audits_ActorUserId",
                schema: "school",
                table: "homework_audits",
                column: "ActorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_homework_audits_HomeworkAssignmentId_CreatedAtUtc",
                schema: "school",
                table: "homework_audits",
                columns: new[] { "HomeworkAssignmentId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_homework_audits_StudentHomeworkId",
                schema: "school",
                table: "homework_audits",
                column: "StudentHomeworkId");

            migrationBuilder.CreateIndex(
                name: "IX_homework_blank_accepted_answers_HomeworkQuestionBlankId_Sor~",
                schema: "school",
                table: "homework_blank_accepted_answers",
                columns: new[] { "HomeworkQuestionBlankId", "SortOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_homework_question_blanks_HomeworkQuestionId_SortOrder",
                schema: "school",
                table: "homework_question_blanks",
                columns: new[] { "HomeworkQuestionId", "SortOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_homework_question_options_HomeworkQuestionId_SortOrder",
                schema: "school",
                table: "homework_question_options",
                columns: new[] { "HomeworkQuestionId", "SortOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_homework_questions_HomeworkAssignmentId_SortOrder",
                schema: "school",
                table: "homework_questions",
                columns: new[] { "HomeworkAssignmentId", "SortOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_homework_student_answers_GradedByUserId",
                schema: "school",
                table: "homework_student_answers",
                column: "GradedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_homework_student_answers_HomeworkQuestionId",
                schema: "school",
                table: "homework_student_answers",
                column: "HomeworkQuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_homework_student_answers_HomeworkSubmissionAttemptId_Homewo~",
                schema: "school",
                table: "homework_student_answers",
                columns: new[] { "HomeworkSubmissionAttemptId", "HomeworkQuestionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_homework_student_blank_answers_HomeworkQuestionBlankId",
                schema: "school",
                table: "homework_student_blank_answers",
                column: "HomeworkQuestionBlankId");

            migrationBuilder.CreateIndex(
                name: "IX_homework_student_blank_answers_HomeworkStudentAnswerId_Home~",
                schema: "school",
                table: "homework_student_blank_answers",
                columns: new[] { "HomeworkStudentAnswerId", "HomeworkQuestionBlankId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_homework_student_selected_options_HomeworkQuestionOptionId",
                schema: "school",
                table: "homework_student_selected_options",
                column: "HomeworkQuestionOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_homework_submission_attempts_StudentHomeworkId_AttemptNumber",
                schema: "school",
                table: "homework_submission_attempts",
                columns: new[] { "StudentHomeworkId", "AttemptNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_student_homework_GradedByUserId",
                schema: "school",
                table: "student_homework",
                column: "GradedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_student_homework_HomeworkAssignmentId_StudentEnrollmentId",
                schema: "school",
                table: "student_homework",
                columns: new[] { "HomeworkAssignmentId", "StudentEnrollmentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_student_homework_StudentEnrollmentId",
                schema: "school",
                table: "student_homework",
                column: "StudentEnrollmentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "homework_audits",
                schema: "school");

            migrationBuilder.DropTable(
                name: "homework_blank_accepted_answers",
                schema: "school");

            migrationBuilder.DropTable(
                name: "homework_student_blank_answers",
                schema: "school");

            migrationBuilder.DropTable(
                name: "homework_student_selected_options",
                schema: "school");

            migrationBuilder.DropTable(
                name: "homework_question_blanks",
                schema: "school");

            migrationBuilder.DropTable(
                name: "homework_question_options",
                schema: "school");

            migrationBuilder.DropTable(
                name: "homework_student_answers",
                schema: "school");

            migrationBuilder.DropTable(
                name: "homework_questions",
                schema: "school");

            migrationBuilder.DropTable(
                name: "homework_submission_attempts",
                schema: "school");

            migrationBuilder.DropTable(
                name: "student_homework",
                schema: "school");

            migrationBuilder.DropTable(
                name: "homework_assignments",
                schema: "school");

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_role_permissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd23"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713") });

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_role_permissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd24"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713") });

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_role_permissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd25"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713") });

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_role_permissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd26"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713") });

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_permissions",
                keyColumn: "Id",
                keyValue: new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd23"));

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_permissions",
                keyColumn: "Id",
                keyValue: new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd24"));

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_permissions",
                keyColumn: "Id",
                keyValue: new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd25"));

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_permissions",
                keyColumn: "Id",
                keyValue: new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd26"));
        }
    }
}
