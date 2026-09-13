using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Platform.Infrastructure.Persistence.Identity.Migrations
{
    /// <inheritdoc />
    public partial class InitialIdentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "identity");

            migrationBuilder.EnsureSchema(
                name: "messaging");

            migrationBuilder.CreateTable(
                name: "accounts",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PreferredLocale = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TimeZone = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_accounts", x => x.Id);
                    table.CheckConstraint("ck_identity_accounts_status", "[Status] IN (N'PendingVerification', N'Active', N'Locked', N'Disabled')");
                    table.CheckConstraint("ck_identity_accounts_timestamps", "[CreatedAtUtc] <= [UpdatedAtUtc] AND DATEPART(TZOFFSET, [CreatedAtUtc]) = 0 AND DATEPART(TZOFFSET, [UpdatedAtUtc]) = 0");
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
                    table.CheckConstraint("ck_messaging_identity_outbox_nullable_utc", "([ProcessedAtUtc] IS NULL OR DATEPART(TZOFFSET, [ProcessedAtUtc]) = 0) AND ([NextAttemptAtUtc] IS NULL OR DATEPART(TZOFFSET, [NextAttemptAtUtc]) = 0)");
                    table.CheckConstraint("ck_messaging_identity_outbox_occurred_utc", "DATEPART(TZOFFSET, [OccurredAtUtc]) = 0");
                });

            migrationBuilder.CreateTable(
                name: "security_events",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EventType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Succeeded = table.Column<bool>(type: "bit", nullable: false),
                    OccurredAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    MetadataJson = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_security_events", x => x.Id);
                    table.CheckConstraint("ck_identity_security_events_occurred_utc", "DATEPART(TZOFFSET, [OccurredAtUtc]) = 0");
                });

            migrationBuilder.CreateTable(
                name: "login_identifiers",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    NormalizedValue = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: false),
                    DisplayValue = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: false),
                    SchoolId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsVerified = table.Column<bool>(type: "bit", nullable: false),
                    VerifiedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_login_identifiers", x => x.Id);
                    table.CheckConstraint("ck_identity_login_identifiers_scope", "([Type] IN (N'Email', N'Phone') AND [SchoolId] IS NULL) OR ([Type] IN (N'SchoolUsername', N'StudentCode') AND [SchoolId] IS NOT NULL)");
                    table.CheckConstraint("ck_identity_login_identifiers_timestamps", "DATEPART(TZOFFSET, [CreatedAtUtc]) = 0 AND ([VerifiedAtUtc] IS NULL OR ([VerifiedAtUtc] >= [CreatedAtUtc] AND DATEPART(TZOFFSET, [VerifiedAtUtc]) = 0))");
                    table.CheckConstraint("ck_identity_login_identifiers_verification", "([IsVerified] = 1 AND [VerifiedAtUtc] IS NOT NULL) OR ([IsVerified] = 0 AND [VerifiedAtUtc] IS NULL)");
                    table.ForeignKey(
                        name: "FK_login_identifiers_accounts_AccountId",
                        column: x => x.AccountId,
                        principalSchema: "identity",
                        principalTable: "accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "mfa_methods",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    SecretReference = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DestinationHint = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: false),
                    LastUsedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mfa_methods", x => x.Id);
                    table.CheckConstraint("ck_identity_mfa_methods_authenticator_secret", "[Type] <> N'Authenticator' OR ([SecretReference] IS NOT NULL AND LEN(LTRIM(RTRIM([SecretReference]))) > 0)");
                    table.CheckConstraint("ck_identity_mfa_methods_primary_enabled", "[IsPrimary] = 0 OR [IsEnabled] = 1");
                    table.CheckConstraint("ck_identity_mfa_methods_timestamps", "DATEPART(TZOFFSET, [CreatedAtUtc]) = 0 AND ([LastUsedAtUtc] IS NULL OR ([LastUsedAtUtc] >= [CreatedAtUtc] AND DATEPART(TZOFFSET, [LastUsedAtUtc]) = 0))");
                    table.CheckConstraint("ck_identity_mfa_methods_type", "[Type] IN (N'Authenticator', N'Email', N'Sms')");
                    table.ForeignKey(
                        name: "FK_mfa_methods_accounts_AccountId",
                        column: x => x.AccountId,
                        principalSchema: "identity",
                        principalTable: "accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "password_credentials",
                schema: "identity",
                columns: table => new
                {
                    AccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    HashingAlgorithm = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    HashingVersion = table.Column<int>(type: "int", nullable: false),
                    SecurityStamp = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FailedSignInCount = table.Column<int>(type: "int", nullable: false),
                    LockoutEndUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: true),
                    MustChangePassword = table.Column<bool>(type: "bit", nullable: false),
                    ChangedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_password_credentials", x => x.AccountId);
                    table.CheckConstraint("ck_identity_password_credentials_failed_count", "[FailedSignInCount] >= 0");
                    table.CheckConstraint("ck_identity_password_credentials_hashing_version", "[HashingVersion] > 0");
                    table.CheckConstraint("ck_identity_password_credentials_timestamps", "DATEPART(TZOFFSET, [ChangedAtUtc]) = 0 AND ([LockoutEndUtc] IS NULL OR DATEPART(TZOFFSET, [LockoutEndUtc]) = 0)");
                    table.ForeignKey(
                        name: "FK_password_credentials_accounts_AccountId",
                        column: x => x.AccountId,
                        principalSchema: "identity",
                        principalTable: "accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sessions",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RefreshTokenHash = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: false),
                    ExpiresAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: false),
                    LastSeenAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: true),
                    RevokedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: true),
                    ReplacedBySessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RevocationReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sessions", x => x.Id);
                    table.UniqueConstraint("AK_sessions_AccountId_Id", x => new { x.AccountId, x.Id });
                    table.CheckConstraint("ck_identity_sessions_expiry", "[ExpiresAtUtc] > [CreatedAtUtc]");
                    table.CheckConstraint("ck_identity_sessions_replacement_not_self", "[ReplacedBySessionId] IS NULL OR [ReplacedBySessionId] <> [Id]");
                    table.CheckConstraint("ck_identity_sessions_replacement_requires_revocation", "[ReplacedBySessionId] IS NULL OR [RevokedAtUtc] IS NOT NULL");
                    table.CheckConstraint("ck_identity_sessions_timestamps", "DATEPART(TZOFFSET, [CreatedAtUtc]) = 0 AND DATEPART(TZOFFSET, [ExpiresAtUtc]) = 0 AND ([LastSeenAtUtc] IS NULL OR ([LastSeenAtUtc] >= [CreatedAtUtc] AND DATEPART(TZOFFSET, [LastSeenAtUtc]) = 0)) AND ([RevokedAtUtc] IS NULL OR ([RevokedAtUtc] >= [CreatedAtUtc] AND DATEPART(TZOFFSET, [RevokedAtUtc]) = 0))");
                    table.ForeignKey(
                        name: "FK_sessions_accounts_AccountId",
                        column: x => x.AccountId,
                        principalSchema: "identity",
                        principalTable: "accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_sessions_sessions_AccountId_ReplacedBySessionId",
                        columns: x => new { x.AccountId, x.ReplacedBySessionId },
                        principalSchema: "identity",
                        principalTable: "sessions",
                        principalColumns: new[] { "AccountId", "Id" });
                });

            migrationBuilder.CreateIndex(
                name: "IX_accounts_Status",
                schema: "identity",
                table: "accounts",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_login_identifiers_AccountId",
                schema: "identity",
                table: "login_identifiers",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_login_identifiers_SchoolId_Type_NormalizedValue",
                schema: "identity",
                table: "login_identifiers",
                columns: new[] { "SchoolId", "Type", "NormalizedValue" },
                unique: true,
                filter: "[SchoolId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_login_identifiers_Type_NormalizedValue",
                schema: "identity",
                table: "login_identifiers",
                columns: new[] { "Type", "NormalizedValue" },
                unique: true,
                filter: "[SchoolId] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_mfa_methods_AccountId",
                schema: "identity",
                table: "mfa_methods",
                column: "AccountId",
                unique: true,
                filter: "[IsPrimary] = 1 AND [IsEnabled] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_mfa_methods_AccountId_Type",
                schema: "identity",
                table: "mfa_methods",
                columns: new[] { "AccountId", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_outbox_messages_ProcessedAtUtc_NextAttemptAtUtc",
                schema: "messaging",
                table: "outbox_messages",
                columns: new[] { "ProcessedAtUtc", "NextAttemptAtUtc" },
                filter: "[ProcessedAtUtc] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_security_events_AccountId_OccurredAtUtc",
                schema: "identity",
                table: "security_events",
                columns: new[] { "AccountId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_security_events_EventType_OccurredAtUtc",
                schema: "identity",
                table: "security_events",
                columns: new[] { "EventType", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_sessions_AccountId_ExpiresAtUtc",
                schema: "identity",
                table: "sessions",
                columns: new[] { "AccountId", "ExpiresAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_sessions_AccountId_ReplacedBySessionId",
                schema: "identity",
                table: "sessions",
                columns: new[] { "AccountId", "ReplacedBySessionId" });

            migrationBuilder.CreateIndex(
                name: "IX_sessions_RefreshTokenHash",
                schema: "identity",
                table: "sessions",
                column: "RefreshTokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_sessions_ReplacedBySessionId",
                schema: "identity",
                table: "sessions",
                column: "ReplacedBySessionId",
                unique: true,
                filter: "[ReplacedBySessionId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "login_identifiers",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "mfa_methods",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "outbox_messages",
                schema: "messaging");

            migrationBuilder.DropTable(
                name: "password_credentials",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "security_events",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "sessions",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "accounts",
                schema: "identity");
        }
    }
}
