using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Schools.Infrastructure.Persistence.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class AddSchoolSubscriptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "subscription_states",
                schema: "school",
                columns: table => new
                {
                    SystemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubscriptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SystemCode = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    CapacityMetric = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
                    Revision = table.Column<long>(type: "bigint", nullable: false),
                    LocalVersion = table.Column<long>(type: "bigint", nullable: false),
                    PlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BaseCapacity = table.Column<int>(type: "int", nullable: false),
                    AddOnCapacity = table.Column<int>(type: "int", nullable: false),
                    UsedCapacity = table.Column<int>(type: "int", nullable: false),
                    IsSuspended = table.Column<bool>(type: "bit", nullable: false),
                    IsEnforced = table.Column<bool>(type: "bit", nullable: false),
                    ValidFromUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ValidUntilUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    SyncedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CapabilityCodes = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subscription_states", x => x.SystemId);
                });

            migrationBuilder.CreateTable(
                name: "capacity_allocations",
                schema: "school",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SystemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_capacity_allocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_capacity_allocations_subscription_states_SystemId",
                        column: x => x.SystemId,
                        principalSchema: "school",
                        principalTable: "subscription_states",
                        principalColumn: "SystemId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "capacity_movements",
                schema: "school",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SystemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ActorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    OccurredAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_capacity_movements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_capacity_movements_subscription_states_SystemId",
                        column: x => x.SystemId,
                        principalSchema: "school",
                        principalTable: "subscription_states",
                        principalColumn: "SystemId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_capacity_allocations_SystemId_SubjectId",
                schema: "school",
                table: "capacity_allocations",
                columns: new[] { "SystemId", "SubjectId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_capacity_movements_SystemId_OccurredAtUtc",
                schema: "school",
                table: "capacity_movements",
                columns: new[] { "SystemId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_subscription_states_SubscriptionId",
                schema: "school",
                table: "subscription_states",
                column: "SubscriptionId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "capacity_allocations",
                schema: "school");

            migrationBuilder.DropTable(
                name: "capacity_movements",
                schema: "school");

            migrationBuilder.DropTable(
                name: "subscription_states",
                schema: "school");
        }
    }
}
