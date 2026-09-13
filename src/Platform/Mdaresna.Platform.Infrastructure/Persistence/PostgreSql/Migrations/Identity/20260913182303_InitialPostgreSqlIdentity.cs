using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Platform.Infrastructure.Persistence.PostgreSql.Migrations.Identity
{
    /// <inheritdoc />
    public partial class InitialPostgreSqlIdentity : Migration
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
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PreferredLocale = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    TimeZone = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp(3) with time zone", precision: 3, nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp(3) with time zone", precision: 3, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_accounts", x => x.Id);
                    table.CheckConstraint("ck_identity_accounts_status", "\"Status\" IN ('PendingVerification', 'Active', 'Locked', 'Disabled')");
                    table.CheckConstraint("ck_identity_accounts_timestamps", "\"CreatedAtUtc\" <= \"UpdatedAtUtc\" AND TRUE AND TRUE");
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
                    table.CheckConstraint("ck_messaging_identity_outbox_nullable_utc", "(\"ProcessedAtUtc\" IS NULL OR TRUE) AND (\"NextAttemptAtUtc\" IS NULL OR TRUE)");
                    table.CheckConstraint("ck_messaging_identity_outbox_occurred_utc", "TRUE");
                });

            migrationBuilder.CreateTable(
                name: "security_events",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: true),
                    EventType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Succeeded = table.Column<bool>(type: "boolean", nullable: false),
                    OccurredAtUtc = table.Column<DateTimeOffset>(type: "timestamp(3) with time zone", precision: 3, nullable: false),
                    IpAddress = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    MetadataJson = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_security_events", x => x.Id);
                    table.CheckConstraint("ck_identity_security_events_occurred_utc", "TRUE");
                });

            migrationBuilder.CreateTable(
                name: "account_activation_challenges",
                schema: "identity",
                columns: table => new
                {
                    AccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    CodeHash = table.Column<byte[]>(type: "bytea", maxLength: 32, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp(3) with time zone", precision: 3, nullable: false),
                    ExpiresAtUtc = table.Column<DateTimeOffset>(type: "timestamp(3) with time zone", precision: 3, nullable: false),
                    ConsumedAtUtc = table.Column<DateTimeOffset>(type: "timestamp(3) with time zone", precision: 3, nullable: true),
                    LastSentAtUtc = table.Column<DateTimeOffset>(type: "timestamp(3) with time zone", precision: 3, nullable: false),
                    SendWindowStartUtc = table.Column<DateTimeOffset>(type: "timestamp(3) with time zone", precision: 3, nullable: false),
                    SendCount = table.Column<int>(type: "integer", nullable: false),
                    FailedAttempts = table.Column<int>(type: "integer", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_account_activation_challenges", x => x.AccountId);
                    table.CheckConstraint("ck_identity_activation_challenge_counts", "\"SendCount\" >= 0 AND \"FailedAttempts\" >= 0");
                    table.CheckConstraint("ck_identity_activation_challenge_expiry", "\"ExpiresAtUtc\" > \"CreatedAtUtc\"");
                    table.ForeignKey(
                        name: "FK_account_activation_challenges_accounts_AccountId",
                        column: x => x.AccountId,
                        principalSchema: "identity",
                        principalTable: "accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "account_password_reset_challenges",
                schema: "identity",
                columns: table => new
                {
                    AccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    CodeHash = table.Column<byte[]>(type: "bytea", maxLength: 32, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp(3) with time zone", precision: 3, nullable: false),
                    ExpiresAtUtc = table.Column<DateTimeOffset>(type: "timestamp(3) with time zone", precision: 3, nullable: false),
                    ConsumedAtUtc = table.Column<DateTimeOffset>(type: "timestamp(3) with time zone", precision: 3, nullable: true),
                    LastSentAtUtc = table.Column<DateTimeOffset>(type: "timestamp(3) with time zone", precision: 3, nullable: false),
                    SendWindowStartUtc = table.Column<DateTimeOffset>(type: "timestamp(3) with time zone", precision: 3, nullable: false),
                    SendCount = table.Column<int>(type: "integer", nullable: false),
                    FailedAttempts = table.Column<int>(type: "integer", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_account_password_reset_challenges", x => x.AccountId);
                    table.CheckConstraint("ck_identity_password_reset_challenge_counts", "\"SendCount\" >= 0 AND \"FailedAttempts\" >= 0");
                    table.CheckConstraint("ck_identity_password_reset_challenge_expiry", "\"ExpiresAtUtc\" > \"CreatedAtUtc\"");
                    table.ForeignKey(
                        name: "FK_account_password_reset_challenges_accounts_AccountId",
                        column: x => x.AccountId,
                        principalSchema: "identity",
                        principalTable: "accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "login_identifiers",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    NormalizedValue = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    DisplayValue = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    SchoolId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false),
                    VerifiedAtUtc = table.Column<DateTimeOffset>(type: "timestamp(3) with time zone", precision: 3, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp(3) with time zone", precision: 3, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_login_identifiers", x => x.Id);
                    table.CheckConstraint("ck_identity_login_identifiers_scope", "(\"Type\" IN ('Email', 'Phone') AND \"SchoolId\" IS NULL) OR (\"Type\" IN ('SchoolUsername', 'StudentCode') AND \"SchoolId\" IS NOT NULL)");
                    table.CheckConstraint("ck_identity_login_identifiers_timestamps", "TRUE AND (\"VerifiedAtUtc\" IS NULL OR (\"VerifiedAtUtc\" >= \"CreatedAtUtc\" AND TRUE))");
                    table.CheckConstraint("ck_identity_login_identifiers_verification", "(\"IsVerified\" = TRUE AND \"VerifiedAtUtc\" IS NOT NULL) OR (\"IsVerified\" = FALSE AND \"VerifiedAtUtc\" IS NULL)");
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
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SecretReference = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DestinationHint = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp(3) with time zone", precision: 3, nullable: false),
                    LastUsedAtUtc = table.Column<DateTimeOffset>(type: "timestamp(3) with time zone", precision: 3, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mfa_methods", x => x.Id);
                    table.CheckConstraint("ck_identity_mfa_methods_authenticator_secret", "\"Type\" <> 'Authenticator' OR (\"SecretReference\" IS NOT NULL AND length(btrim(\"SecretReference\")) > 0)");
                    table.CheckConstraint("ck_identity_mfa_methods_primary_enabled", "\"IsPrimary\" = FALSE OR \"IsEnabled\" = TRUE");
                    table.CheckConstraint("ck_identity_mfa_methods_timestamps", "TRUE AND (\"LastUsedAtUtc\" IS NULL OR (\"LastUsedAtUtc\" >= \"CreatedAtUtc\" AND TRUE))");
                    table.CheckConstraint("ck_identity_mfa_methods_type", "\"Type\" IN ('Authenticator', 'Email', 'Sms')");
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
                    AccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    HashingAlgorithm = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    HashingVersion = table.Column<int>(type: "integer", nullable: false),
                    SecurityStamp = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FailedSignInCount = table.Column<int>(type: "integer", nullable: false),
                    LockoutEndUtc = table.Column<DateTimeOffset>(type: "timestamp(3) with time zone", precision: 3, nullable: true),
                    MustChangePassword = table.Column<bool>(type: "boolean", nullable: false),
                    ChangedAtUtc = table.Column<DateTimeOffset>(type: "timestamp(3) with time zone", precision: 3, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_password_credentials", x => x.AccountId);
                    table.CheckConstraint("ck_identity_password_credentials_failed_count", "\"FailedSignInCount\" >= 0");
                    table.CheckConstraint("ck_identity_password_credentials_hashing_version", "\"HashingVersion\" > 0");
                    table.CheckConstraint("ck_identity_password_credentials_timestamps", "TRUE AND (\"LockoutEndUtc\" IS NULL OR TRUE)");
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
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    RefreshTokenHash = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp(3) with time zone", precision: 3, nullable: false),
                    ExpiresAtUtc = table.Column<DateTimeOffset>(type: "timestamp(3) with time zone", precision: 3, nullable: false),
                    LastSeenAtUtc = table.Column<DateTimeOffset>(type: "timestamp(3) with time zone", precision: 3, nullable: true),
                    RevokedAtUtc = table.Column<DateTimeOffset>(type: "timestamp(3) with time zone", precision: 3, nullable: true),
                    ReplacedBySessionId = table.Column<Guid>(type: "uuid", nullable: true),
                    RevocationReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IpAddress = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sessions", x => x.Id);
                    table.UniqueConstraint("AK_sessions_AccountId_Id", x => new { x.AccountId, x.Id });
                    table.CheckConstraint("ck_identity_sessions_expiry", "\"ExpiresAtUtc\" > \"CreatedAtUtc\"");
                    table.CheckConstraint("ck_identity_sessions_replacement_not_self", "\"ReplacedBySessionId\" IS NULL OR \"ReplacedBySessionId\" <> \"Id\"");
                    table.CheckConstraint("ck_identity_sessions_replacement_requires_revocation", "\"ReplacedBySessionId\" IS NULL OR \"RevokedAtUtc\" IS NOT NULL");
                    table.CheckConstraint("ck_identity_sessions_timestamps", "TRUE AND TRUE AND (\"LastSeenAtUtc\" IS NULL OR (\"LastSeenAtUtc\" >= \"CreatedAtUtc\" AND TRUE)) AND (\"RevokedAtUtc\" IS NULL OR (\"RevokedAtUtc\" >= \"CreatedAtUtc\" AND TRUE))");
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
                filter: "\"SchoolId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_login_identifiers_Type_NormalizedValue",
                schema: "identity",
                table: "login_identifiers",
                columns: new[] { "Type", "NormalizedValue" },
                unique: true,
                filter: "\"SchoolId\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_mfa_methods_AccountId",
                schema: "identity",
                table: "mfa_methods",
                column: "AccountId",
                unique: true,
                filter: "\"IsPrimary\" = TRUE AND \"IsEnabled\" = TRUE");

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
                filter: "\"ProcessedAtUtc\" IS NULL");

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
                filter: "\"ReplacedBySessionId\" IS NOT NULL");

            migrationBuilder.Sql("""
                CREATE FUNCTION identity.set_rowversion() RETURNS trigger
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
                          AND table_schema IN ('identity', 'messaging')
                    LOOP
                        EXECUTE format(
                            'CREATE TRIGGER set_rowversion BEFORE INSERT OR UPDATE ON %I.%I '
                            || 'FOR EACH ROW EXECUTE FUNCTION identity.set_rowversion()',
                            target.table_schema, target.table_name);
                    END LOOP;
                END
                $body$;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS identity.set_rowversion() CASCADE;");
            migrationBuilder.DropTable(
                name: "account_activation_challenges",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "account_password_reset_challenges",
                schema: "identity");

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
