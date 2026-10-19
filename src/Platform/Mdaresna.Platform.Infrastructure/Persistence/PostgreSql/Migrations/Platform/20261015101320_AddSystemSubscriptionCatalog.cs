using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Platform.Infrastructure.Persistence.PostgreSql.Migrations.Platform
{
    /// <inheritdoc />
    public partial class AddSystemSubscriptionCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "school_systems",
                schema: "billing",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    NameAr = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NameEn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CapacityMetric = table.Column<int>(type: "integer", nullable: false),
                    CapabilityCodes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_school_systems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "system_plans",
                schema: "billing",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SystemId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    NameAr = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NameEn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsArchived = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                    LatestRevision = table.Column<int>(type: "integer", nullable: false),
                    PublishedRevision = table.Column<int>(type: "integer", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_system_plans", x => x.Id);
                    table.CheckConstraint("ck_system_plan_order", "\"SortOrder\" > 0");
                    table.ForeignKey(
                        name: "FK_system_plans_school_systems_SystemId",
                        column: x => x.SystemId,
                        principalSchema: "billing",
                        principalTable: "school_systems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "system_plan_revisions",
                schema: "billing",
                columns: table => new
                {
                    PlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    Revision = table.Column<int>(type: "integer", nullable: false),
                    Capacity = table.Column<int>(type: "integer", nullable: false),
                    BaseUnitPrice = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    AddOnUnitPrice = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    BaseAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_system_plan_revisions", x => new { x.PlanId, x.Revision });
                    table.CheckConstraint("ck_system_plan_revision_capacity", "\"Capacity\" BETWEEN 1 AND 1000000");
                    table.CheckConstraint("ck_system_plan_revision_prices", "\"BaseUnitPrice\" > 0 AND \"AddOnUnitPrice\" > 0 AND \"BaseAmount\" > 0");
                    table.ForeignKey(
                        name: "FK_system_plan_revisions_system_plans_PlanId",
                        column: x => x.PlanId,
                        principalSchema: "billing",
                        principalTable: "system_plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "school_system_subscriptions",
                schema: "billing",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SchoolId = table.Column<Guid>(type: "uuid", nullable: false),
                    SystemId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlanRevision = table.Column<int>(type: "integer", nullable: false),
                    BaseCapacity = table.Column<int>(type: "integer", nullable: false),
                    AddOnCapacity = table.Column<int>(type: "integer", nullable: false),
                    BaseAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    AddOnUnitPrice = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    ValidFromUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ValidUntilUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                    IsSuspended = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_school_system_subscriptions", x => x.Id);
                    table.CheckConstraint("ck_school_subscription_capacity", "\"BaseCapacity\" > 0 AND \"AddOnCapacity\" >= 0");
                    table.CheckConstraint("ck_school_subscription_period", "\"ValidUntilUtc\" > \"ValidFromUtc\"");
                    table.ForeignKey(
                        name: "FK_school_system_subscriptions_school_systems_SystemId",
                        column: x => x.SystemId,
                        principalSchema: "billing",
                        principalTable: "school_systems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_school_system_subscriptions_system_plan_revisions_PlanId_Pl~",
                        columns: x => new { x.PlanId, x.PlanRevision },
                        principalSchema: "billing",
                        principalTable: "system_plan_revisions",
                        principalColumns: new[] { "PlanId", "Revision" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "subscription_quotes",
                schema: "billing",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SchoolId = table.Column<Guid>(type: "uuid", nullable: false),
                    SystemId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubscriptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Kind = table.Column<int>(type: "integer", nullable: false),
                    SourceVersion = table.Column<long>(type: "bigint", nullable: false),
                    CatalogVersion = table.Column<long>(type: "bigint", nullable: false),
                    PlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlanRevision = table.Column<int>(type: "integer", nullable: false),
                    BaseCapacity = table.Column<int>(type: "integer", nullable: false),
                    AdditionalQuantity = table.Column<int>(type: "integer", nullable: false),
                    ResultAddOnCapacity = table.Column<int>(type: "integer", nullable: false),
                    BaseAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    AddOnUnitPrice = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    AmountDue = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    ValidFromUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ValidUntilUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ExpiresAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PaymentRequestId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                    AppliedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subscription_quotes", x => x.Id);
                    table.CheckConstraint("ck_subscription_quote_amount", "\"AmountDue\" >= 0 AND \"BaseAmount\" > 0");
                    table.CheckConstraint("ck_subscription_quote_capacity", "\"BaseCapacity\" > 0 AND \"ResultAddOnCapacity\" >= 0");
                    table.ForeignKey(
                        name: "FK_subscription_quotes_school_platform_payment_requests_Paymen~",
                        column: x => x.PaymentRequestId,
                        principalSchema: "billing",
                        principalTable: "school_platform_payment_requests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_subscription_quotes_system_plan_revisions_PlanId_PlanRevisi~",
                        columns: x => new { x.PlanId, x.PlanRevision },
                        principalSchema: "billing",
                        principalTable: "system_plan_revisions",
                        principalColumns: new[] { "PlanId", "Revision" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_school_system_subscriptions_PlanId_PlanRevision",
                schema: "billing",
                table: "school_system_subscriptions",
                columns: new[] { "PlanId", "PlanRevision" });

            migrationBuilder.CreateIndex(
                name: "IX_school_system_subscriptions_SchoolId_SystemId",
                schema: "billing",
                table: "school_system_subscriptions",
                columns: new[] { "SchoolId", "SystemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_school_system_subscriptions_SystemId",
                schema: "billing",
                table: "school_system_subscriptions",
                column: "SystemId");

            migrationBuilder.CreateIndex(
                name: "IX_school_systems_Code",
                schema: "billing",
                table: "school_systems",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_subscription_quotes_PaymentRequestId",
                schema: "billing",
                table: "subscription_quotes",
                column: "PaymentRequestId",
                unique: true,
                filter: "\"PaymentRequestId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_subscription_quotes_PlanId_PlanRevision",
                schema: "billing",
                table: "subscription_quotes",
                columns: new[] { "PlanId", "PlanRevision" });

            migrationBuilder.CreateIndex(
                name: "IX_subscription_quotes_SchoolId_ExpiresAtUtc",
                schema: "billing",
                table: "subscription_quotes",
                columns: new[] { "SchoolId", "ExpiresAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_system_plans_SystemId_Code",
                schema: "billing",
                table: "system_plans",
                columns: new[] { "SystemId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_system_plans_SystemId_SortOrder",
                schema: "billing",
                table: "system_plans",
                columns: new[] { "SystemId", "SortOrder" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "school_system_subscriptions",
                schema: "billing");

            migrationBuilder.DropTable(
                name: "subscription_quotes",
                schema: "billing");

            migrationBuilder.DropTable(
                name: "system_plan_revisions",
                schema: "billing");

            migrationBuilder.DropTable(
                name: "system_plans",
                schema: "billing");

            migrationBuilder.DropTable(
                name: "school_systems",
                schema: "billing");
        }
    }
}
