using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Mdaresna.Platform.Infrastructure.Persistence.PostgreSql.Migrations.Platform
{
    /// <inheritdoc />
    public partial class InitialPostgreSqlPlatform : Migration
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

            migrationBuilder.EnsureSchema(
                name: "billing");

            migrationBuilder.EnsureSchema(
                name: "platform");

            migrationBuilder.CreateTable(
                name: "audit_entries",
                schema: "operations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    Action = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    ResourceType = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    ResourceId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    OccurredAtUtc = table.Column<DateTimeOffset>(type: "timestamp(3) with time zone", precision: 3, nullable: false),
                    IpAddress = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CorrelationId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    MetadataJson = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_entries", x => x.Id);
                    table.CheckConstraint("ck_operations_audit_entries_occurred_utc", "TRUE");
                });

            migrationBuilder.CreateTable(
                name: "inbox_messages",
                schema: "messaging",
                columns: table => new
                {
                    MessageId = table.Column<Guid>(type: "uuid", nullable: false),
                    Consumer = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    MessageType = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    SchemaVersion = table.Column<int>(type: "integer", nullable: false),
                    ReceivedAtUtc = table.Column<DateTimeOffset>(type: "timestamp(3) with time zone", precision: 3, nullable: false),
                    ProcessedAtUtc = table.Column<DateTimeOffset>(type: "timestamp(3) with time zone", precision: 3, nullable: true),
                    AttemptCount = table.Column<int>(type: "integer", nullable: false),
                    LastError = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inbox_messages", x => new { x.Consumer, x.MessageId });
                    table.CheckConstraint("ck_messaging_platform_inbox_timestamps", "TRUE AND (\"ProcessedAtUtc\" IS NULL OR (\"ProcessedAtUtc\" >= \"ReceivedAtUtc\" AND TRUE))");
                });

            migrationBuilder.CreateTable(
                name: "outbox_messages",
                schema: "messaging",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MessageType = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    SchemaVersion = table.Column<int>(type: "integer", nullable: false),
                    PayloadJson = table.Column<string>(type: "text", nullable: false),
                    OccurredAtUtc = table.Column<DateTimeOffset>(type: "timestamp(3) with time zone", precision: 3, nullable: false),
                    ProcessedAtUtc = table.Column<DateTimeOffset>(type: "timestamp(3) with time zone", precision: 3, nullable: true),
                    NextAttemptAtUtc = table.Column<DateTimeOffset>(type: "timestamp(3) with time zone", precision: 3, nullable: true),
                    AttemptCount = table.Column<int>(type: "integer", nullable: false),
                    LastError = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CausationId = table.Column<Guid>(type: "uuid", nullable: true),
                    TraceParent = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_outbox_messages", x => x.Id);
                    table.CheckConstraint("ck_messaging_platform_outbox_timestamps", "TRUE AND (\"ProcessedAtUtc\" IS NULL OR (\"ProcessedAtUtc\" >= \"OccurredAtUtc\" AND TRUE)) AND (\"NextAttemptAtUtc\" IS NULL OR (\"NextAttemptAtUtc\" >= \"OccurredAtUtc\" AND TRUE))");
                });

            migrationBuilder.CreateTable(
                name: "permissions",
                schema: "access",
                columns: table => new
                {
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
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
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsSystem = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp(3) with time zone", precision: 3, nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp(3) with time zone", precision: 3, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.Id);
                    table.CheckConstraint("ck_access_roles_timestamps", "\"CreatedAtUtc\" <= \"UpdatedAtUtc\" AND TRUE AND TRUE");
                    table.CheckConstraint("ck_access_roles_version", "\"Version\" >= 0");
                });

            migrationBuilder.CreateTable(
                name: "sms_providers",
                schema: "platform",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProviderUserName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    EncryptedPassword = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    SenderName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    ApiUrlTemplate = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    MessageCharactersLength = table.Column<int>(type: "integer", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    SuccessResponsePrefix = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp(3) with time zone", precision: 3, nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp(3) with time zone", precision: 3, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sms_providers", x => x.Id);
                    table.CheckConstraint("ck_platform_sms_providers_api_url_https", "\"ApiUrlTemplate\" LIKE 'https://%'");
                    table.CheckConstraint("ck_platform_sms_providers_deleted_inactive", "\"IsDeleted\" = FALSE OR \"IsActive\" = FALSE");
                    table.CheckConstraint("ck_platform_sms_providers_message_length", "\"MessageCharactersLength\" BETWEEN 1 AND 1000");
                    table.CheckConstraint("ck_platform_sms_providers_priority", "\"Priority\" >= 1");
                    table.CheckConstraint("ck_platform_sms_providers_timestamps", "\"CreatedAtUtc\" <= \"UpdatedAtUtc\" AND TRUE AND TRUE");
                });

            migrationBuilder.CreateTable(
                name: "tenants",
                schema: "registry",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    LegalName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SuspensionReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp(3) with time zone", precision: 3, nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp(3) with time zone", precision: 3, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenants", x => x.Id);
                    table.CheckConstraint("ck_registry_tenants_status", "\"Status\" IN ('Draft', 'Active', 'Suspended', 'Closed')");
                    table.CheckConstraint("ck_registry_tenants_status_reason", "(\"Status\" IN ('Suspended', 'Closed') AND \"SuspensionReason\" IS NOT NULL AND length(btrim(\"SuspensionReason\")) > 0) OR (\"Status\" IN ('Draft', 'Active') AND \"SuspensionReason\" IS NULL)");
                    table.CheckConstraint("ck_registry_tenants_timestamps", "\"CreatedAtUtc\" <= \"UpdatedAtUtc\" AND TRUE AND TRUE");
                    table.CheckConstraint("ck_registry_tenants_version", "\"Version\" >= 0");
                });

            migrationBuilder.CreateTable(
                name: "unit_types",
                schema: "billing",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp(3) with time zone", precision: 3, nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp(3) with time zone", precision: 3, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_unit_types", x => x.Id);
                    table.CheckConstraint("ck_billing_unit_type_code", "length(\"Code\") BETWEEN 3 AND 32 AND \"Code\" = UPPER(\"Code\")");
                    table.CheckConstraint("ck_billing_unit_type_currency", "\"Currency\" ~ '^[A-Z]{3}$'");
                    table.CheckConstraint("ck_billing_unit_type_id", "\"Id\" <> '00000000-0000-0000-0000-000000000000'");
                    table.CheckConstraint("ck_billing_unit_type_name", "length(btrim(\"DisplayName\")) > 0");
                    table.CheckConstraint("ck_billing_unit_type_price", "\"UnitPrice\" > 0");
                    table.CheckConstraint("ck_billing_unit_type_timestamps", "\"CreatedAtUtc\" <= \"UpdatedAtUtc\" AND TRUE AND TRUE");
                    table.CheckConstraint("ck_billing_unit_type_version", "\"Version\" >= 0");
                });

            migrationBuilder.CreateTable(
                name: "role_assignments",
                schema: "access",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedByAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedAtUtc = table.Column<DateTimeOffset>(type: "timestamp(3) with time zone", precision: 3, nullable: false),
                    RevokedByAccountId = table.Column<Guid>(type: "uuid", nullable: true),
                    RevokedAtUtc = table.Column<DateTimeOffset>(type: "timestamp(3) with time zone", precision: 3, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_role_assignments", x => x.Id);
                    table.CheckConstraint("ck_access_role_assignments_revocation", "(\"RevokedAtUtc\" IS NULL AND \"RevokedByAccountId\" IS NULL) OR (\"RevokedAtUtc\" IS NOT NULL AND \"RevokedByAccountId\" IS NOT NULL AND \"RevokedAtUtc\" >= \"AssignedAtUtc\")");
                    table.CheckConstraint("ck_access_role_assignments_timestamps", "TRUE AND (\"RevokedAtUtc\" IS NULL OR TRUE)");
                    table.CheckConstraint("ck_access_role_assignments_version", "\"Version\" >= 0");
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
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    PermissionCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
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
                name: "sms_logs",
                schema: "platform",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceSystem = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    MessageTypeCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SchoolId = table.Column<Guid>(type: "uuid", nullable: true),
                    SourceMessageId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProviderId = table.Column<Guid>(type: "uuid", nullable: true),
                    RecipientEncrypted = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    RecipientMasked = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    MessageEncrypted = table.Column<string>(type: "text", nullable: false),
                    ResponseEncrypted = table.Column<string>(type: "text", nullable: true),
                    HttpStatusCode = table.Column<int>(type: "integer", nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    FailureReason = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp(3) with time zone", precision: 3, nullable: false),
                    CompletedAtUtc = table.Column<DateTimeOffset>(type: "timestamp(3) with time zone", precision: 3, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sms_logs", x => x.Id);
                    table.CheckConstraint("ck_platform_sms_logs_completion", "(\"Status\" = 'Pending' AND \"CompletedAtUtc\" IS NULL) OR (\"Status\" <> 'Pending' AND \"CompletedAtUtc\" IS NOT NULL)");
                    table.CheckConstraint("ck_platform_sms_logs_http_status", "\"HttpStatusCode\" IS NULL OR \"HttpStatusCode\" BETWEEN 100 AND 599");
                    table.CheckConstraint("ck_platform_sms_logs_school_scope", "\"SourceSystem\" <> 'schools' OR \"SchoolId\" IS NOT NULL");
                    table.CheckConstraint("ck_platform_sms_logs_source_system", "\"SourceSystem\" IN ('platform', 'schools', 'family')");
                    table.CheckConstraint("ck_platform_sms_logs_status", "\"Status\" IN ('Pending', 'Accepted', 'Rejected', 'Failed')");
                    table.CheckConstraint("ck_platform_sms_logs_timestamps", "TRUE AND (\"CompletedAtUtc\" IS NULL OR (\"CompletedAtUtc\" >= \"CreatedAtUtc\" AND TRUE))");
                    table.ForeignKey(
                        name: "FK_sms_logs_sms_providers_ProviderId",
                        column: x => x.ProviderId,
                        principalSchema: "platform",
                        principalTable: "sms_providers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "external_id_mappings",
                schema: "registry",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceSystem = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EntityType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ExternalId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    InternalId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp(3) with time zone", precision: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_external_id_mappings", x => x.Id);
                    table.CheckConstraint("ck_registry_external_id_mappings_created_utc", "TRUE");
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
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp(3) with time zone", precision: 3, nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp(3) with time zone", precision: 3, nullable: false),
                    UpdatedByAccountId = table.Column<Guid>(type: "uuid", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_feature_flags", x => x.Id);
                    table.CheckConstraint("ck_operations_feature_flags_timestamps", "\"CreatedAtUtc\" <= \"UpdatedAtUtc\" AND TRUE AND TRUE");
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
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    RegistrationRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SchoolType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    DeploymentMode = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    RequestedByAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProvisioningOperationId = table.Column<Guid>(type: "uuid", nullable: true),
                    StatusReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp(3) with time zone", precision: 3, nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp(3) with time zone", precision: 3, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_schools", x => x.Id);
                    table.UniqueConstraint("AK_schools_TenantId_Id", x => new { x.TenantId, x.Id });
                    table.CheckConstraint("ck_registry_schools_deployment_matches_type", "\"DeploymentMode\" <> 'GovernmentOnPremises' OR \"SchoolType\" = 'Government'");
                    table.CheckConstraint("ck_registry_schools_deployment_mode", "\"DeploymentMode\" IN ('SharedSaaS', 'DedicatedCloud', 'GovernmentOnPremises')");
                    table.CheckConstraint("ck_registry_schools_provisioning_operation", "(\"Status\" IN ('Draft', 'PendingVerification', 'Approved') AND \"ProvisioningOperationId\" IS NULL) OR (\"Status\" IN ('Provisioning', 'ProvisioningFailed', 'Active', 'Suspended') AND \"ProvisioningOperationId\" IS NOT NULL AND \"ProvisioningOperationId\" <> '00000000-0000-0000-0000-000000000000') OR (\"Status\" = 'Closed' AND (\"ProvisioningOperationId\" IS NULL OR \"ProvisioningOperationId\" <> '00000000-0000-0000-0000-000000000000'))");
                    table.CheckConstraint("ck_registry_schools_school_is_tenant", "\"Id\" = \"TenantId\"");
                    table.CheckConstraint("ck_registry_schools_status", "\"Status\" IN ('Draft', 'PendingVerification', 'Approved', 'Provisioning', 'ProvisioningFailed', 'Active', 'Suspended', 'Closed')");
                    table.CheckConstraint("ck_registry_schools_status_reason", "(\"Status\" IN ('ProvisioningFailed', 'Suspended', 'Closed') AND \"StatusReason\" IS NOT NULL AND length(btrim(\"StatusReason\")) > 0) OR (\"Status\" IN ('Draft', 'PendingVerification', 'Approved', 'Provisioning', 'Active') AND \"StatusReason\" IS NULL)");
                    table.CheckConstraint("ck_registry_schools_timestamps", "\"CreatedAtUtc\" <= \"UpdatedAtUtc\" AND TRUE AND TRUE");
                    table.CheckConstraint("ck_registry_schools_type", "\"SchoolType\" IN ('Private', 'Government')");
                    table.CheckConstraint("ck_registry_schools_version", "\"Version\" >= 0");
                    table.ForeignKey(
                        name: "FK_schools_tenants_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "registry",
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "school_platform_payment_requests",
                schema: "billing",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    SchoolId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    TransferReference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    RequestedByAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestedAtUtc = table.Column<DateTimeOffset>(type: "timestamp(3) with time zone", precision: 3, nullable: false),
                    ReviewedByAccountId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewedAtUtc = table.Column<DateTimeOffset>(type: "timestamp(3) with time zone", precision: 3, nullable: true),
                    ReviewNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_school_platform_payment_requests", x => x.Id);
                    table.UniqueConstraint("AK_school_platform_payment_requests_Id_TenantId_SchoolId_Amou~1", x => new { x.Id, x.TenantId, x.SchoolId, x.Amount, x.Currency, x.TransferReference });
                    table.UniqueConstraint("AK_school_platform_payment_requests_Id_TenantId_SchoolId_Amoun~", x => new { x.Id, x.TenantId, x.SchoolId, x.Amount, x.Currency });
                    table.CheckConstraint("ck_billing_payment_request_amount", "\"Amount\" > 0");
                    table.CheckConstraint("ck_billing_payment_request_currency", "\"Currency\" ~ '^[A-Z]{3}$'");
                    table.CheckConstraint("ck_billing_payment_request_identifiers", "\"Id\" <> '00000000-0000-0000-0000-000000000000' AND \"RequestedByAccountId\" <> '00000000-0000-0000-0000-000000000000' AND (\"ReviewedByAccountId\" IS NULL OR \"ReviewedByAccountId\" <> '00000000-0000-0000-0000-000000000000')");
                    table.CheckConstraint("ck_billing_payment_request_review", "(\"Status\" = 'Pending' AND \"ReviewedByAccountId\" IS NULL AND \"ReviewedAtUtc\" IS NULL AND \"ReviewNote\" IS NULL) OR (\"Status\" IN ('Approved', 'Rejected') AND \"ReviewedByAccountId\" IS NOT NULL AND \"ReviewedAtUtc\" IS NOT NULL AND \"ReviewedAtUtc\" >= \"RequestedAtUtc\" AND \"ReviewedByAccountId\" <> \"RequestedByAccountId\" AND (\"Status\" = 'Approved' OR (\"ReviewNote\" IS NOT NULL AND length(btrim(\"ReviewNote\")) > 0)))");
                    table.CheckConstraint("ck_billing_payment_request_status", "\"Status\" IN ('Pending', 'Approved', 'Rejected')");
                    table.CheckConstraint("ck_billing_payment_request_timestamps", "TRUE AND (\"ReviewedAtUtc\" IS NULL OR TRUE)");
                    table.CheckConstraint("ck_billing_payment_request_version", "\"Version\" >= 0");
                    table.ForeignKey(
                        name: "FK_school_platform_payment_requests_schools_TenantId_SchoolId",
                        columns: x => new { x.TenantId, x.SchoolId },
                        principalSchema: "registry",
                        principalTable: "schools",
                        principalColumns: new[] { "TenantId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "school_platform_payment_ledger",
                schema: "billing",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PaymentRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    SchoolId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    TransferReference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PostedByAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    PostedAtUtc = table.Column<DateTimeOffset>(type: "timestamp(3) with time zone", precision: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_school_platform_payment_ledger", x => x.Id);
                    table.CheckConstraint("ck_billing_payment_ledger_amount", "\"Amount\" > 0");
                    table.CheckConstraint("ck_billing_payment_ledger_currency", "\"Currency\" ~ '^[A-Z]{3}$'");
                    table.CheckConstraint("ck_billing_payment_ledger_identifiers", "\"Id\" <> '00000000-0000-0000-0000-000000000000' AND \"PaymentRequestId\" <> '00000000-0000-0000-0000-000000000000' AND \"PostedByAccountId\" <> '00000000-0000-0000-0000-000000000000'");
                    table.CheckConstraint("ck_billing_payment_ledger_posted_utc", "TRUE");
                    table.ForeignKey(
                        name: "FK_school_platform_payment_ledger_school_platform_payment_requ~",
                        columns: x => new { x.PaymentRequestId, x.TenantId, x.SchoolId, x.Amount, x.Currency, x.TransferReference },
                        principalSchema: "billing",
                        principalTable: "school_platform_payment_requests",
                        principalColumns: new[] { "Id", "TenantId", "SchoolId", "Amount", "Currency", "TransferReference" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "unit_purchase_intents",
                schema: "billing",
                columns: table => new
                {
                    PaymentRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    SchoolId = table.Column<Guid>(type: "uuid", nullable: false),
                    UnitTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    UnitTypeVersion = table.Column<long>(type: "bigint", nullable: false),
                    UnitTypeCode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    UnitTypeName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PaymentMethodCode = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    TransferOccurredAtUtc = table.Column<DateTimeOffset>(type: "timestamp(3) with time zone", precision: 3, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp(3) with time zone", precision: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_unit_purchase_intents", x => x.PaymentRequestId);
                    table.UniqueConstraint("AK_unit_purchase_intents_PaymentRequestId_TenantId_SchoolId_Un~", x => new { x.PaymentRequestId, x.TenantId, x.SchoolId, x.UnitTypeId, x.UnitTypeCode, x.UnitTypeName, x.UnitPrice, x.Currency, x.Quantity, x.Amount, x.PaymentMethodCode, x.TransferOccurredAtUtc });
                    table.CheckConstraint("ck_billing_unit_purchase_intent_code", "length(\"UnitTypeCode\") BETWEEN 3 AND 32 AND \"UnitTypeCode\" = UPPER(\"UnitTypeCode\")");
                    table.CheckConstraint("ck_billing_unit_purchase_intent_created_utc", "TRUE AND TRUE AND \"TransferOccurredAtUtc\" <= \"CreatedAtUtc\"");
                    table.CheckConstraint("ck_billing_unit_purchase_intent_currency", "\"Currency\" ~ '^[A-Z]{3}$'");
                    table.CheckConstraint("ck_billing_unit_purchase_intent_ids", "\"PaymentRequestId\" <> '00000000-0000-0000-0000-000000000000' AND \"UnitTypeId\" <> '00000000-0000-0000-0000-000000000000'");
                    table.CheckConstraint("ck_billing_unit_purchase_intent_name", "length(btrim(\"UnitTypeName\")) > 0");
                    table.CheckConstraint("ck_billing_unit_purchase_intent_payment_method", "length(\"PaymentMethodCode\") BETWEEN 3 AND 40 AND \"PaymentMethodCode\" = LOWER(\"PaymentMethodCode\")");
                    table.CheckConstraint("ck_billing_unit_purchase_intent_price", "\"UnitPrice\" > 0 AND \"Amount\" > 0 AND \"Amount\" = \"UnitPrice\" * \"Quantity\"");
                    table.CheckConstraint("ck_billing_unit_purchase_intent_quantity", "\"Quantity\" BETWEEN 1 AND 1000000");
                    table.CheckConstraint("ck_billing_unit_purchase_intent_school", "\"SchoolId\" = \"TenantId\"");
                    table.CheckConstraint("ck_billing_unit_purchase_intent_version", "\"UnitTypeVersion\" >= 0");
                    table.ForeignKey(
                        name: "FK_unit_purchase_intents_school_platform_payment_requests_Paym~",
                        columns: x => new { x.PaymentRequestId, x.TenantId, x.SchoolId, x.Amount, x.Currency },
                        principalSchema: "billing",
                        principalTable: "school_platform_payment_requests",
                        principalColumns: new[] { "Id", "TenantId", "SchoolId", "Amount", "Currency" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_unit_purchase_intents_unit_types_UnitTypeId",
                        column: x => x.UnitTypeId,
                        principalSchema: "billing",
                        principalTable: "unit_types",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "unit_grants",
                schema: "billing",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PaymentRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    SchoolId = table.Column<Guid>(type: "uuid", nullable: false),
                    UnitTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    UnitTypeCode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    UnitTypeName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    TransferReference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PaymentMethodCode = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    TransferOccurredAtUtc = table.Column<DateTimeOffset>(type: "timestamp(3) with time zone", precision: 3, nullable: false),
                    GrantedAtUtc = table.Column<DateTimeOffset>(type: "timestamp(3) with time zone", precision: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_unit_grants", x => x.Id);
                    table.CheckConstraint("ck_billing_unit_grant_currency", "\"Currency\" ~ '^[A-Z]{3}$'");
                    table.CheckConstraint("ck_billing_unit_grant_granted_utc", "TRUE AND TRUE AND \"TransferOccurredAtUtc\" <= \"GrantedAtUtc\"");
                    table.CheckConstraint("ck_billing_unit_grant_ids", "\"Id\" <> '00000000-0000-0000-0000-000000000000' AND \"PaymentRequestId\" <> '00000000-0000-0000-0000-000000000000' AND \"UnitTypeId\" <> '00000000-0000-0000-0000-000000000000'");
                    table.CheckConstraint("ck_billing_unit_grant_name", "length(btrim(\"UnitTypeName\")) > 0");
                    table.CheckConstraint("ck_billing_unit_grant_payment_method", "length(\"PaymentMethodCode\") BETWEEN 3 AND 40 AND \"PaymentMethodCode\" = LOWER(\"PaymentMethodCode\")");
                    table.CheckConstraint("ck_billing_unit_grant_price", "\"UnitPrice\" > 0 AND \"Amount\" > 0 AND \"Amount\" = \"UnitPrice\" * \"Quantity\"");
                    table.CheckConstraint("ck_billing_unit_grant_quantity", "\"Quantity\" BETWEEN 1 AND 1000000");
                    table.CheckConstraint("ck_billing_unit_grant_school", "\"SchoolId\" = \"TenantId\"");
                    table.ForeignKey(
                        name: "FK_unit_grants_school_platform_payment_requests_PaymentRequest~",
                        columns: x => new { x.PaymentRequestId, x.TenantId, x.SchoolId, x.Amount, x.Currency, x.TransferReference },
                        principalSchema: "billing",
                        principalTable: "school_platform_payment_requests",
                        principalColumns: new[] { "Id", "TenantId", "SchoolId", "Amount", "Currency", "TransferReference" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_unit_grants_unit_purchase_intents_PaymentRequestId_TenantId~",
                        columns: x => new { x.PaymentRequestId, x.TenantId, x.SchoolId, x.UnitTypeId, x.UnitTypeCode, x.UnitTypeName, x.UnitPrice, x.Currency, x.Quantity, x.Amount, x.PaymentMethodCode, x.TransferOccurredAtUtc },
                        principalSchema: "billing",
                        principalTable: "unit_purchase_intents",
                        principalColumns: new[] { "PaymentRequestId", "TenantId", "SchoolId", "UnitTypeId", "UnitTypeCode", "UnitTypeName", "UnitPrice", "Currency", "Quantity", "Amount", "PaymentMethodCode", "TransferOccurredAtUtc" },
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
                filter: "\"TenantId\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_external_id_mappings_TenantId",
                schema: "registry",
                table: "external_id_mappings",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_external_id_mappings_TenantId_SourceSystem_EntityType_Exter~",
                schema: "registry",
                table: "external_id_mappings",
                columns: new[] { "TenantId", "SourceSystem", "EntityType", "ExternalId" },
                unique: true,
                filter: "\"TenantId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_feature_flags_Key",
                schema: "operations",
                table: "feature_flags",
                column: "Key",
                unique: true,
                filter: "\"TenantId\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_feature_flags_TenantId_Key",
                schema: "operations",
                table: "feature_flags",
                columns: new[] { "TenantId", "Key" },
                unique: true,
                filter: "\"TenantId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_inbox_messages_ProcessedAtUtc_ReceivedAtUtc",
                schema: "messaging",
                table: "inbox_messages",
                columns: new[] { "ProcessedAtUtc", "ReceivedAtUtc" },
                filter: "\"ProcessedAtUtc\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_outbox_messages_ProcessedAtUtc_NextAttemptAtUtc",
                schema: "messaging",
                table: "outbox_messages",
                columns: new[] { "ProcessedAtUtc", "NextAttemptAtUtc" },
                filter: "\"ProcessedAtUtc\" IS NULL");

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
                filter: "\"RevokedAtUtc\" IS NULL");

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
                name: "IX_school_platform_payment_ledger_PaymentRequestId",
                schema: "billing",
                table: "school_platform_payment_ledger",
                column: "PaymentRequestId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_school_platform_payment_ledger_PaymentRequestId_TenantId_Sc~",
                schema: "billing",
                table: "school_platform_payment_ledger",
                columns: new[] { "PaymentRequestId", "TenantId", "SchoolId", "Amount", "Currency", "TransferReference" });

            migrationBuilder.CreateIndex(
                name: "IX_school_platform_payment_ledger_SchoolId_PostedAtUtc",
                schema: "billing",
                table: "school_platform_payment_ledger",
                columns: new[] { "SchoolId", "PostedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_school_platform_payment_requests_SchoolId_TransferReference",
                schema: "billing",
                table: "school_platform_payment_requests",
                columns: new[] { "SchoolId", "TransferReference" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_school_platform_payment_requests_TenantId_SchoolId",
                schema: "billing",
                table: "school_platform_payment_requests",
                columns: new[] { "TenantId", "SchoolId" });

            migrationBuilder.CreateIndex(
                name: "IX_school_platform_payment_requests_TenantId_Status_RequestedA~",
                schema: "billing",
                table: "school_platform_payment_requests",
                columns: new[] { "TenantId", "Status", "RequestedAtUtc" });

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
                filter: "\"ProvisioningOperationId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_schools_RegistrationRequestId",
                schema: "registry",
                table: "schools",
                column: "RegistrationRequestId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_schools_TenantId",
                schema: "registry",
                table: "schools",
                column: "TenantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_schools_TenantId_Status",
                schema: "registry",
                table: "schools",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_sms_logs_CreatedAtUtc",
                schema: "platform",
                table: "sms_logs",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_sms_logs_ProviderId_CreatedAtUtc",
                schema: "platform",
                table: "sms_logs",
                columns: new[] { "ProviderId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_sms_logs_SourceSystem_SchoolId_CreatedAtUtc",
                schema: "platform",
                table: "sms_logs",
                columns: new[] { "SourceSystem", "SchoolId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_sms_logs_SourceSystem_SourceMessageId",
                schema: "platform",
                table: "sms_logs",
                columns: new[] { "SourceSystem", "SourceMessageId" },
                unique: true,
                filter: "\"SourceMessageId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_sms_providers_IsDeleted_IsActive_Priority",
                schema: "platform",
                table: "sms_providers",
                columns: new[] { "IsDeleted", "IsActive", "Priority" });

            migrationBuilder.CreateIndex(
                name: "IX_tenants_Status",
                schema: "registry",
                table: "tenants",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_unit_grants_PaymentRequestId",
                schema: "billing",
                table: "unit_grants",
                column: "PaymentRequestId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_unit_grants_PaymentRequestId_TenantId_SchoolId_Amount_Curre~",
                schema: "billing",
                table: "unit_grants",
                columns: new[] { "PaymentRequestId", "TenantId", "SchoolId", "Amount", "Currency", "TransferReference" });

            migrationBuilder.CreateIndex(
                name: "IX_unit_grants_PaymentRequestId_TenantId_SchoolId_UnitTypeId_U~",
                schema: "billing",
                table: "unit_grants",
                columns: new[] { "PaymentRequestId", "TenantId", "SchoolId", "UnitTypeId", "UnitTypeCode", "UnitTypeName", "UnitPrice", "Currency", "Quantity", "Amount", "PaymentMethodCode", "TransferOccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_unit_grants_SchoolId_GrantedAtUtc",
                schema: "billing",
                table: "unit_grants",
                columns: new[] { "SchoolId", "GrantedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_unit_purchase_intents_PaymentRequestId_TenantId_SchoolId_Am~",
                schema: "billing",
                table: "unit_purchase_intents",
                columns: new[] { "PaymentRequestId", "TenantId", "SchoolId", "Amount", "Currency" });

            migrationBuilder.CreateIndex(
                name: "IX_unit_purchase_intents_SchoolId_CreatedAtUtc",
                schema: "billing",
                table: "unit_purchase_intents",
                columns: new[] { "SchoolId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_unit_purchase_intents_UnitTypeId",
                schema: "billing",
                table: "unit_purchase_intents",
                column: "UnitTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_unit_types_Code",
                schema: "billing",
                table: "unit_types",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_unit_types_IsActive",
                schema: "billing",
                table: "unit_types",
                column: "IsActive");

            migrationBuilder.Sql("""
                CREATE FUNCTION platform.set_rowversion() RETURNS trigger
                LANGUAGE plpgsql AS $body$
                BEGIN
                    NEW."RowVersion" := uuid_send(gen_random_uuid());
                    RETURN NEW;
                END
                $body$;

                DO $body$
                DECLARE target record;
                BEGIN
                    FOR target IN
                        SELECT table_schema, table_name
                        FROM information_schema.columns
                        WHERE column_name = 'RowVersion'
                          AND table_schema IN ('platform', 'access', 'registry', 'operations', 'messaging', 'billing')
                    LOOP
                        EXECUTE format(
                            'CREATE TRIGGER set_rowversion BEFORE INSERT OR UPDATE ON %I.%I '
                            || 'FOR EACH ROW EXECUTE FUNCTION platform.set_rowversion()',
                            target.table_schema, target.table_name);
                    END LOOP;
                END
                $body$;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS platform.set_rowversion() CASCADE;");
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
                name: "school_platform_payment_ledger",
                schema: "billing");

            migrationBuilder.DropTable(
                name: "sms_logs",
                schema: "platform");

            migrationBuilder.DropTable(
                name: "unit_grants",
                schema: "billing");

            migrationBuilder.DropTable(
                name: "permissions",
                schema: "access");

            migrationBuilder.DropTable(
                name: "roles",
                schema: "access");

            migrationBuilder.DropTable(
                name: "sms_providers",
                schema: "platform");

            migrationBuilder.DropTable(
                name: "unit_purchase_intents",
                schema: "billing");

            migrationBuilder.DropTable(
                name: "school_platform_payment_requests",
                schema: "billing");

            migrationBuilder.DropTable(
                name: "unit_types",
                schema: "billing");

            migrationBuilder.DropTable(
                name: "schools",
                schema: "registry");

            migrationBuilder.DropTable(
                name: "tenants",
                schema: "registry");
        }
    }
}
