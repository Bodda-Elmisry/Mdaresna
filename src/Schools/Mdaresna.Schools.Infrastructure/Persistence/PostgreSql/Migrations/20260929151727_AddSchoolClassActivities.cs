using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Mdaresna.Schools.Infrastructure.Persistence.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class AddSchoolClassActivities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "class_activities",
                schema: "school",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClassSectionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClassSectionSubjectId = table.Column<Guid>(type: "uuid", nullable: true),
                    Scope = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Category = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    Status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    AudienceMode = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Details = table.Column<string>(type: "character varying(6000)", maxLength: 6000, nullable: false),
                    ActivityDate = table.Column<DateOnly>(type: "date", nullable: false),
                    StartsAt = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    EndsAt = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    Location = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    IsGraded = table.Column<bool>(type: "boolean", nullable: false),
                    TotalScore = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    TimeZoneIdSnapshot = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PublishedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompletedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CancelledByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    PublishedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CompletedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CancelledAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CancellationReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_class_activities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_class_activities_class_section_subjects_ClassSectionSubject~",
                        column: x => x.ClassSectionSubjectId,
                        principalSchema: "school",
                        principalTable: "class_section_subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_class_activities_class_sections_ClassSectionId",
                        column: x => x.ClassSectionId,
                        principalSchema: "school",
                        principalTable: "class_sections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_class_activities_local_users_CancelledByUserId",
                        column: x => x.CancelledByUserId,
                        principalSchema: "school",
                        principalTable: "local_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_class_activities_local_users_CompletedByUserId",
                        column: x => x.CompletedByUserId,
                        principalSchema: "school",
                        principalTable: "local_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_class_activities_local_users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalSchema: "school",
                        principalTable: "local_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_class_activities_local_users_PublishedByUserId",
                        column: x => x.PublishedByUserId,
                        principalSchema: "school",
                        principalTable: "local_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "class_activity_audience_students",
                schema: "school",
                columns: table => new
                {
                    ClassActivityId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentEnrollmentId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_class_activity_audience_students", x => new { x.ClassActivityId, x.StudentEnrollmentId });
                    table.ForeignKey(
                        name: "FK_class_activity_audience_students_class_activities_ClassActi~",
                        column: x => x.ClassActivityId,
                        principalSchema: "school",
                        principalTable: "class_activities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_class_activity_audience_students_student_enrollments_Studen~",
                        column: x => x.StudentEnrollmentId,
                        principalSchema: "school",
                        principalTable: "student_enrollments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "class_activity_audits",
                schema: "school",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClassActivityId = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    PayloadJson = table.Column<string>(type: "text", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_class_activity_audits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_class_activity_audits_class_activities_ClassActivityId",
                        column: x => x.ClassActivityId,
                        principalSchema: "school",
                        principalTable: "class_activities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_class_activity_audits_local_users_ActorUserId",
                        column: x => x.ActorUserId,
                        principalSchema: "school",
                        principalTable: "local_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "class_activity_participants",
                schema: "school",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClassActivityId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentEnrollmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    Score = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    Note = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    EvaluatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    EvaluatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_class_activity_participants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_class_activity_participants_class_activities_ClassActivityId",
                        column: x => x.ClassActivityId,
                        principalSchema: "school",
                        principalTable: "class_activities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_class_activity_participants_local_users_EvaluatedByUserId",
                        column: x => x.EvaluatedByUserId,
                        principalSchema: "school",
                        principalTable: "local_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_class_activity_participants_student_enrollments_StudentEnro~",
                        column: x => x.StudentEnrollmentId,
                        principalSchema: "school",
                        principalTable: "student_enrollments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                schema: "school",
                table: "local_permissions",
                columns: new[] { "Id", "Code", "CreatedAtUtc", "DisplayNameAr", "DisplayNameEn", "IsActive", "Module", "UpdatedAtUtc" },
                values: new object[,]
                {
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd27"), "school.activities.view", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "عرض أنشطة الفصل", "View class activities", true, "activities", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd28"), "school.activities.manage", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "إدارة أنشطة الفصل", "Manage class activities", true, "activities", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd29"), "school.activities.publish", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "نشر وإلغاء أنشطة الفصل", "Publish and cancel class activities", true, "activities", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd2a"), "school.activities.grade", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "تسجيل مشاركة وتقييم الأنشطة", "Record class activity participation and grades", true, "activities", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd2b"), "school.activities.reopen", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "إعادة فتح تقييم أنشطة الفصل", "Reopen class activity evaluation", true, "activities", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) }
                });

            migrationBuilder.InsertData(
                schema: "school",
                table: "local_role_permissions",
                columns: new[] { "PermissionId", "RoleId", "GrantedAtUtc", "GrantedByUserId" },
                values: new object[,]
                {
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd27"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd28"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd29"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd2a"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd2b"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_class_activities_CancelledByUserId",
                schema: "school",
                table: "class_activities",
                column: "CancelledByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_class_activities_ClassSectionId_Status_ActivityDate",
                schema: "school",
                table: "class_activities",
                columns: new[] { "ClassSectionId", "Status", "ActivityDate" });

            migrationBuilder.CreateIndex(
                name: "IX_class_activities_ClassSectionSubjectId_ActivityDate",
                schema: "school",
                table: "class_activities",
                columns: new[] { "ClassSectionSubjectId", "ActivityDate" });

            migrationBuilder.CreateIndex(
                name: "IX_class_activities_CompletedByUserId",
                schema: "school",
                table: "class_activities",
                column: "CompletedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_class_activities_CreatedByUserId",
                schema: "school",
                table: "class_activities",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_class_activities_PublishedByUserId",
                schema: "school",
                table: "class_activities",
                column: "PublishedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_class_activity_audience_students_StudentEnrollmentId",
                schema: "school",
                table: "class_activity_audience_students",
                column: "StudentEnrollmentId");

            migrationBuilder.CreateIndex(
                name: "IX_class_activity_audits_ActorUserId",
                schema: "school",
                table: "class_activity_audits",
                column: "ActorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_class_activity_audits_ClassActivityId_CreatedAtUtc",
                schema: "school",
                table: "class_activity_audits",
                columns: new[] { "ClassActivityId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_class_activity_participants_ClassActivityId_StudentEnrollme~",
                schema: "school",
                table: "class_activity_participants",
                columns: new[] { "ClassActivityId", "StudentEnrollmentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_class_activity_participants_EvaluatedByUserId",
                schema: "school",
                table: "class_activity_participants",
                column: "EvaluatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_class_activity_participants_StudentEnrollmentId",
                schema: "school",
                table: "class_activity_participants",
                column: "StudentEnrollmentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "class_activity_audience_students",
                schema: "school");

            migrationBuilder.DropTable(
                name: "class_activity_audits",
                schema: "school");

            migrationBuilder.DropTable(
                name: "class_activity_participants",
                schema: "school");

            migrationBuilder.DropTable(
                name: "class_activities",
                schema: "school");

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_role_permissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd27"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713") });

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_role_permissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd28"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713") });

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_role_permissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd29"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713") });

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_role_permissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd2a"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713") });

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_role_permissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd2b"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713") });

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_permissions",
                keyColumn: "Id",
                keyValue: new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd27"));

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_permissions",
                keyColumn: "Id",
                keyValue: new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd28"));

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_permissions",
                keyColumn: "Id",
                keyValue: new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd29"));

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_permissions",
                keyColumn: "Id",
                keyValue: new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd2a"));

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_permissions",
                keyColumn: "Id",
                keyValue: new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd2b"));
        }
    }
}
