using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Schools.Infrastructure.Persistence.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class AddExamWorkflowNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "WorkflowDefaultsJson",
                schema: "school",
                table: "exam_policies",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "exam_notification_outbox",
                schema: "school",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventKey = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    NotificationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DeliveredAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Attempts = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exam_notification_outbox", x => x.Id);
                    table.ForeignKey(
                        name: "FK_exam_notification_outbox_school_user_notifications_NotificationId",
                        column: x => x.NotificationId,
                        principalSchema: "school",
                        principalTable: "school_user_notifications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "exam_workflows",
                schema: "school",
                columns: table => new
                {
                    ExamSeriesId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
                    Revision = table.Column<int>(type: "int", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exam_workflows", x => x.ExamSeriesId);
                    table.ForeignKey(
                        name: "FK_exam_workflows_exam_series_ExamSeriesId",
                        column: x => x.ExamSeriesId,
                        principalSchema: "school",
                        principalTable: "exam_series",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "exam_workflow_steps",
                schema: "school",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExamSeriesId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExamPaperId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Stage = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    AssigneeUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BackupUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SupervisorUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DueAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
                    CompletedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    RemindedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    EscalatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ReturnReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exam_workflow_steps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_exam_workflow_steps_exam_papers_ExamPaperId",
                        column: x => x.ExamPaperId,
                        principalSchema: "school",
                        principalTable: "exam_papers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_exam_workflow_steps_exam_workflows_ExamSeriesId",
                        column: x => x.ExamSeriesId,
                        principalSchema: "school",
                        principalTable: "exam_workflows",
                        principalColumn: "ExamSeriesId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_exam_workflow_steps_local_users_AssigneeUserId",
                        column: x => x.AssigneeUserId,
                        principalSchema: "school",
                        principalTable: "local_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_exam_workflow_steps_local_users_BackupUserId",
                        column: x => x.BackupUserId,
                        principalSchema: "school",
                        principalTable: "local_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_exam_workflow_steps_local_users_SupervisorUserId",
                        column: x => x.SupervisorUserId,
                        principalSchema: "school",
                        principalTable: "local_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_exam_notification_outbox_DeliveredAtUtc",
                schema: "school",
                table: "exam_notification_outbox",
                column: "DeliveredAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_exam_notification_outbox_EventKey",
                schema: "school",
                table: "exam_notification_outbox",
                column: "EventKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_exam_notification_outbox_NotificationId",
                schema: "school",
                table: "exam_notification_outbox",
                column: "NotificationId");

            migrationBuilder.CreateIndex(
                name: "IX_exam_workflow_steps_AssigneeUserId_Status_DueAtUtc",
                schema: "school",
                table: "exam_workflow_steps",
                columns: new[] { "AssigneeUserId", "Status", "DueAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_exam_workflow_steps_BackupUserId",
                schema: "school",
                table: "exam_workflow_steps",
                column: "BackupUserId");

            migrationBuilder.CreateIndex(
                name: "IX_exam_workflow_steps_ExamPaperId",
                schema: "school",
                table: "exam_workflow_steps",
                column: "ExamPaperId");

            migrationBuilder.CreateIndex(
                name: "IX_exam_workflow_steps_ExamSeriesId",
                schema: "school",
                table: "exam_workflow_steps",
                column: "ExamSeriesId");

            migrationBuilder.CreateIndex(
                name: "IX_exam_workflow_steps_SupervisorUserId",
                schema: "school",
                table: "exam_workflow_steps",
                column: "SupervisorUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "exam_notification_outbox",
                schema: "school");

            migrationBuilder.DropTable(
                name: "exam_workflow_steps",
                schema: "school");

            migrationBuilder.DropTable(
                name: "exam_workflows",
                schema: "school");

            migrationBuilder.DropColumn(
                name: "WorkflowDefaultsJson",
                schema: "school",
                table: "exam_policies");
        }
    }
}
