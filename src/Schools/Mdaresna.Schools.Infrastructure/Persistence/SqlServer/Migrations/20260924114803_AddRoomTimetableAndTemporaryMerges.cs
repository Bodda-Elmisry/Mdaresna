using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Schools.Infrastructure.Persistence.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class AddRoomTimetableAndTemporaryMerges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AllowRoomSharing",
                schema: "school",
                table: "weekly_timetable_slots",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "RoomId",
                schema: "school",
                table: "weekly_timetable_slots",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "temporary_class_merges",
                schema: "school",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoomId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MergeDate = table.Column<DateOnly>(type: "date", nullable: false),
                    StartsAt = table.Column<TimeOnly>(type: "time", nullable: false),
                    EndsAt = table.Column<TimeOnly>(type: "time", nullable: false),
                    ExpectedStudentCount = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_temporary_class_merges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_temporary_class_merges_school_rooms_RoomId",
                        column: x => x.RoomId,
                        principalSchema: "school",
                        principalTable: "school_rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "temporary_class_merge_sections",
                schema: "school",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TemporaryClassMergeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClassSectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_temporary_class_merge_sections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_temporary_class_merge_sections_class_sections_ClassSectionId",
                        column: x => x.ClassSectionId,
                        principalSchema: "school",
                        principalTable: "class_sections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_temporary_class_merge_sections_temporary_class_merges_TemporaryClassMergeId",
                        column: x => x.TemporaryClassMergeId,
                        principalSchema: "school",
                        principalTable: "temporary_class_merges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_weekly_timetable_slots_RoomId",
                schema: "school",
                table: "weekly_timetable_slots",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_temporary_class_merge_sections_ClassSectionId",
                schema: "school",
                table: "temporary_class_merge_sections",
                column: "ClassSectionId");

            migrationBuilder.CreateIndex(
                name: "IX_temporary_class_merge_sections_TemporaryClassMergeId_ClassSectionId",
                schema: "school",
                table: "temporary_class_merge_sections",
                columns: new[] { "TemporaryClassMergeId", "ClassSectionId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_temporary_class_merges_RoomId_MergeDate_StartsAt_EndsAt",
                schema: "school",
                table: "temporary_class_merges",
                columns: new[] { "RoomId", "MergeDate", "StartsAt", "EndsAt" },
                filter: "[IsDeleted] = 0");

            migrationBuilder.AddForeignKey(
                name: "FK_weekly_timetable_slots_school_rooms_RoomId",
                schema: "school",
                table: "weekly_timetable_slots",
                column: "RoomId",
                principalSchema: "school",
                principalTable: "school_rooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_weekly_timetable_slots_school_rooms_RoomId",
                schema: "school",
                table: "weekly_timetable_slots");

            migrationBuilder.DropTable(
                name: "temporary_class_merge_sections",
                schema: "school");

            migrationBuilder.DropTable(
                name: "temporary_class_merges",
                schema: "school");

            migrationBuilder.DropIndex(
                name: "IX_weekly_timetable_slots_RoomId",
                schema: "school",
                table: "weekly_timetable_slots");

            migrationBuilder.DropColumn(
                name: "AllowRoomSharing",
                schema: "school",
                table: "weekly_timetable_slots");

            migrationBuilder.DropColumn(
                name: "RoomId",
                schema: "school",
                table: "weekly_timetable_slots");
        }
    }
}
