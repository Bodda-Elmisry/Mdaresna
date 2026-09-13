using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Mdaresna.Platform.Infrastructure.Persistence.Platform.Migrations
{
    /// <inheritdoc />
    public partial class InitialPlatform : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "operations");

            migrationBuilder.EnsureSchema(
                name: "registry");

            migrationBuilder.EnsureSchema(
                name: "messaging");

            migrationBuilder.EnsureSchema(
                name: "access");

            migrationBuilder.CreateTable(
                name: "audit_entries",
                schema: "operations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Action = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ResourceType = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ResourceId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    OccurredAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CorrelationId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MetadataJson = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_entries", x => x.Id);
                    table.CheckConstraint("ck_operations_audit_entries_occurred_utc", "DATEPART(TZOFFSET, [OccurredAtUtc]) = 0");
                });

            migrationBuilder.CreateTable(
                name: "inbox_messages",
                schema: "messaging",
                columns: table => new
                {
                    MessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Consumer = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MessageType = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    SchemaVersion = table.Column<int>(type: "int", nullable: false),
                    ReceivedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: false),
                    ProcessedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: true),
                    AttemptCount = table.Column<int>(type: "int", nullable: false),
                    LastError = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inbox_messages", x => new { x.Consumer, x.MessageId });
                    table.CheckConstraint("ck_messaging_platform_inbox_timestamps", "DATEPART(TZOFFSET, [ReceivedAtUtc]) = 0 AND ([ProcessedAtUtc] IS NULL OR ([ProcessedAtUtc] >= [ReceivedAtUtc] AND DATEPART(TZOFFSET, [ProcessedAtUtc]) = 0))");
                });

            migrationBuilder.CreateTable(
                name: "outbox_messages",
                schema: "messaging",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MessageType = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    SchemaVersion = table.Column<int>(type: "int", nullable: false),
                    PayloadJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OccurredAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: false),
                    ProcessedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: true),
                    NextAttemptAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: true),
                    AttemptCount = table.Column<int>(type: "int", nullable: false),
                    LastError = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    CorrelationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CausationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TraceParent = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_outbox_messages", x => x.Id);
                    table.CheckConstraint("ck_messaging_platform_outbox_timestamps", "DATEPART(TZOFFSET, [OccurredAtUtc]) = 0 AND ([ProcessedAtUtc] IS NULL OR ([ProcessedAtUtc] >= [OccurredAtUtc] AND DATEPART(TZOFFSET, [ProcessedAtUtc]) = 0)) AND ([NextAttemptAtUtc] IS NULL OR ([NextAttemptAtUtc] >= [OccurredAtUtc] AND DATEPART(TZOFFSET, [NextAttemptAtUtc]) = 0))");
                });

            migrationBuilder.CreateTable(
                name: "permissions",
                schema: "access",
                columns: table => new
                {
                    Code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_permissions", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                schema: "access",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsSystem = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.Id);
                    table.CheckConstraint("ck_access_roles_timestamps", "[CreatedAtUtc] <= [UpdatedAtUtc] AND DATEPART(TZOFFSET, [CreatedAtUtc]) = 0 AND DATEPART(TZOFFSET, [UpdatedAtUtc]) = 0");
                    table.CheckConstraint("ck_access_roles_version", "[Version] >= 0");
                });

            migrationBuilder.CreateTable(
                name: "tenants",
                schema: "registry",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    LegalName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    SuspensionReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenants", x => x.Id);
                    table.CheckConstraint("ck_registry_tenants_status", "[Status] IN (N'Draft', N'Active', N'Suspended', N'Closed')");
                    table.CheckConstraint("ck_registry_tenants_status_reason", "([Status] IN (N'Suspended', N'Closed') AND [SuspensionReason] IS NOT NULL AND LEN(LTRIM(RTRIM([SuspensionReason]))) > 0) OR ([Status] IN (N'Draft', N'Active') AND [SuspensionReason] IS NULL)");
                    table.CheckConstraint("ck_registry_tenants_timestamps", "[CreatedAtUtc] <= [UpdatedAtUtc] AND DATEPART(TZOFFSET, [CreatedAtUtc]) = 0 AND DATEPART(TZOFFSET, [UpdatedAtUtc]) = 0");
                    table.CheckConstraint("ck_registry_tenants_version", "[Version] >= 0");
                });

            migrationBuilder.CreateTable(
                name: "role_assignments",
                schema: "access",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssignedByAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssignedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: false),
                    RevokedByAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RevokedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_role_assignments", x => x.Id);
                    table.CheckConstraint("ck_access_role_assignments_revocation", "([RevokedAtUtc] IS NULL AND [RevokedByAccountId] IS NULL) OR ([RevokedAtUtc] IS NOT NULL AND [RevokedByAccountId] IS NOT NULL AND [RevokedAtUtc] >= [AssignedAtUtc])");
                    table.CheckConstraint("ck_access_role_assignments_timestamps", "DATEPART(TZOFFSET, [AssignedAtUtc]) = 0 AND ([RevokedAtUtc] IS NULL OR DATEPART(TZOFFSET, [RevokedAtUtc]) = 0)");
                    table.CheckConstraint("ck_access_role_assignments_version", "[Version] >= 0");
                    table.ForeignKey(
                        name: "FK_role_assignments_roles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "access",
                        principalTable: "roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "role_permissions",
                schema: "access",
                columns: table => new
                {
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PermissionCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_role_permissions", x => new { x.RoleId, x.PermissionCode });
                    table.ForeignKey(
                        name: "FK_role_permissions_permissions_PermissionCode",
                        column: x => x.PermissionCode,
                        principalSchema: "access",
                        principalTable: "permissions",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_role_permissions_roles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "access",
                        principalTable: "roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "external_id_mappings",
                schema: "registry",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceSystem = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ExternalId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    InternalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_external_id_mappings", x => x.Id);
                    table.CheckConstraint("ck_registry_external_id_mappings_created_utc", "DATEPART(TZOFFSET, [CreatedAtUtc]) = 0");
                    table.ForeignKey(
                        name: "FK_external_id_mappings_tenants_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "registry",
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "feature_flags",
                schema: "operations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: false),
                    UpdatedByAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_feature_flags", x => x.Id);
                    table.CheckConstraint("ck_operations_feature_flags_timestamps", "[CreatedAtUtc] <= [UpdatedAtUtc] AND DATEPART(TZOFFSET, [CreatedAtUtc]) = 0 AND DATEPART(TZOFFSET, [UpdatedAtUtc]) = 0");
                    table.ForeignKey(
                        name: "FK_feature_flags_tenants_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "registry",
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "schools",
                schema: "registry",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RegistrationRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SchoolType = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    DeploymentMode = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    RequestedByAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProvisioningOperationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StatusReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_schools", x => x.Id);
                    table.CheckConstraint("ck_registry_schools_deployment_matches_type", "[DeploymentMode] <> N'GovernmentOnPremises' OR [SchoolType] = N'Government'");
                    table.CheckConstraint("ck_registry_schools_deployment_mode", "[DeploymentMode] IN (N'SharedSaaS', N'DedicatedCloud', N'GovernmentOnPremises')");
                    table.CheckConstraint("ck_registry_schools_provisioning_operation", "([Status] IN (N'Draft', N'PendingVerification', N'Approved') AND [ProvisioningOperationId] IS NULL) OR ([Status] IN (N'Provisioning', N'ProvisioningFailed', N'Active', N'Suspended') AND [ProvisioningOperationId] IS NOT NULL AND [ProvisioningOperationId] <> '00000000-0000-0000-0000-000000000000') OR ([Status] = N'Closed' AND ([ProvisioningOperationId] IS NULL OR [ProvisioningOperationId] <> '00000000-0000-0000-0000-000000000000'))");
                    table.CheckConstraint("ck_registry_schools_status", "[Status] IN (N'Draft', N'PendingVerification', N'Approved', N'Provisioning', N'ProvisioningFailed', N'Active', N'Suspended', N'Closed')");
                    table.CheckConstraint("ck_registry_schools_status_reason", "([Status] IN (N'ProvisioningFailed', N'Suspended', N'Closed') AND [StatusReason] IS NOT NULL AND LEN(LTRIM(RTRIM([StatusReason]))) > 0) OR ([Status] IN (N'Draft', N'PendingVerification', N'Approved', N'Provisioning', N'Active') AND [StatusReason] IS NULL)");
                    table.CheckConstraint("ck_registry_schools_timestamps", "[CreatedAtUtc] <= [UpdatedAtUtc] AND DATEPART(TZOFFSET, [CreatedAtUtc]) = 0 AND DATEPART(TZOFFSET, [UpdatedAtUtc]) = 0");
                    table.CheckConstraint("ck_registry_schools_type", "[SchoolType] IN (N'Private', N'Government')");
                    table.CheckConstraint("ck_registry_schools_version", "[Version] >= 0");
                    table.ForeignKey(
                        name: "FK_schools_tenants_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "registry",
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                schema: "access",
                table: "permissions",
                columns: new[] { "Code", "Description" },
                values: new object[,]
                {
                    { "platform.access.manage", null },
                    { "platform.audit.read", null },
                    { "platform.billing.manage", null },
                    { "platform.billing.read", null },
                    { "platform.deployments.manage", null },
                    { "platform.payments.approve", null },
                    { "platform.schools.activate", null },
                    { "platform.schools.manage", null },
                    { "platform.schools.read", null },
                    { "platform.support.manage", null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_audit_entries_AccountId_OccurredAtUtc",
                schema: "operations",
                table: "audit_entries",
                columns: new[] { "AccountId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_audit_entries_ResourceType_ResourceId",
                schema: "operations",
                table: "audit_entries",
                columns: new[] { "ResourceType", "ResourceId" });

            migrationBuilder.CreateIndex(
                name: "IX_audit_entries_TenantId_OccurredAtUtc",
                schema: "operations",
                table: "audit_entries",
                columns: new[] { "TenantId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_external_id_mappings_EntityType_InternalId",
                schema: "registry",
                table: "external_id_mappings",
                columns: new[] { "EntityType", "InternalId" });

            migrationBuilder.CreateIndex(
                name: "IX_external_id_mappings_SourceSystem_EntityType_ExternalId",
                schema: "registry",
                table: "external_id_mappings",
                columns: new[] { "SourceSystem", "EntityType", "ExternalId" },
                unique: true,
                filter: "[TenantId] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_external_id_mappings_TenantId",
                schema: "registry",
                table: "external_id_mappings",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_external_id_mappings_TenantId_SourceSystem_EntityType_ExternalId",
                schema: "registry",
                table: "external_id_mappings",
                columns: new[] { "TenantId", "SourceSystem", "EntityType", "ExternalId" },
                unique: true,
                filter: "[TenantId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_feature_flags_Key",
                schema: "operations",
                table: "feature_flags",
                column: "Key",
                unique: true,
                filter: "[TenantId] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_feature_flags_TenantId_Key",
                schema: "operations",
                table: "feature_flags",
                columns: new[] { "TenantId", "Key" },
                unique: true,
                filter: "[TenantId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_inbox_messages_ProcessedAtUtc_ReceivedAtUtc",
                schema: "messaging",
                table: "inbox_messages",
                columns: new[] { "ProcessedAtUtc", "ReceivedAtUtc" },
                filter: "[ProcessedAtUtc] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_outbox_messages_ProcessedAtUtc_NextAttemptAtUtc",
                schema: "messaging",
                table: "outbox_messages",
                columns: new[] { "ProcessedAtUtc", "NextAttemptAtUtc" },
                filter: "[ProcessedAtUtc] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_role_assignments_AccountId_RevokedAtUtc",
                schema: "access",
                table: "role_assignments",
                columns: new[] { "AccountId", "RevokedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_role_assignments_AccountId_RoleId",
                schema: "access",
                table: "role_assignments",
                columns: new[] { "AccountId", "RoleId" },
                unique: true,
                filter: "[RevokedAtUtc] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_role_assignments_RoleId",
                schema: "access",
                table: "role_assignments",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_role_permissions_PermissionCode",
                schema: "access",
                table: "role_permissions",
                column: "PermissionCode");

            migrationBuilder.CreateIndex(
                name: "IX_roles_IsActive",
                schema: "access",
                table: "roles",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_roles_Key",
                schema: "access",
                table: "roles",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_schools_Code",
                schema: "registry",
                table: "schools",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_schools_ProvisioningOperationId",
                schema: "registry",
                table: "schools",
                column: "ProvisioningOperationId",
                unique: true,
                filter: "[ProvisioningOperationId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_schools_RegistrationRequestId",
                schema: "registry",
                table: "schools",
                column: "RegistrationRequestId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_schools_TenantId_Status",
                schema: "registry",
                table: "schools",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_tenants_Status",
                schema: "registry",
                table: "tenants",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_entries",
                schema: "operations");

            migrationBuilder.DropTable(
                name: "external_id_mappings",
                schema: "registry");

            migrationBuilder.DropTable(
                name: "feature_flags",
                schema: "operations");

            migrationBuilder.DropTable(
                name: "inbox_messages",
                schema: "messaging");

            migrationBuilder.DropTable(
                name: "outbox_messages",
                schema: "messaging");

            migrationBuilder.DropTable(
                name: "role_assignments",
                schema: "access");

            migrationBuilder.DropTable(
                name: "role_permissions",
                schema: "access");

            migrationBuilder.DropTable(
                name: "schools",
                schema: "registry");

            migrationBuilder.DropTable(
                name: "permissions",
                schema: "access");

            migrationBuilder.DropTable(
                name: "roles",
                schema: "access");

            migrationBuilder.DropTable(
                name: "tenants",
                schema: "registry");
        }
    }
}
