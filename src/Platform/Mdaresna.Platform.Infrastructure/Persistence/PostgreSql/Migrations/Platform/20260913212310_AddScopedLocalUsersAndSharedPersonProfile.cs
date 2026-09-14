using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Platform.Infrastructure.Persistence.PostgreSql.Migrations.Platform
{
    /// <inheritdoc />
    public partial class AddScopedLocalUsersAndSharedPersonProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "local_users",
                schema: "access",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NormalizedUserName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp(3) with time zone", precision: 3, nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp(3) with time zone", precision: 3, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_local_users", x => x.Id);
                    table.CheckConstraint("ck_access_local_users_identifiers", "\"Id\" <> '00000000-0000-0000-0000-000000000000' AND \"PersonId\" <> '00000000-0000-0000-0000-000000000000'");
                    table.CheckConstraint("ck_access_local_users_status", "\"Status\" IN ('PendingActivation', 'Active', 'Disabled')");
                });

            migrationBuilder.CreateTable(
                name: "local_credentials",
                schema: "access",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_local_credentials", x => x.UserId);
                    table.CheckConstraint("ck_access_local_credentials_failed_count", "\"FailedSignInCount\" >= 0");
                    table.CheckConstraint("ck_access_local_credentials_hashing_version", "\"HashingVersion\" > 0");
                    table.ForeignKey(
                        name: "FK_local_credentials_local_users_UserId",
                        column: x => x.UserId,
                        principalSchema: "access",
                        principalTable: "local_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_local_users_NormalizedUserName",
                schema: "access",
                table: "local_users",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_local_users_PersonId",
                schema: "access",
                table: "local_users",
                column: "PersonId",
                unique: true);

            migrationBuilder.Sql("""
                CREATE TRIGGER set_rowversion BEFORE INSERT OR UPDATE ON access.local_users
                FOR EACH ROW EXECUTE FUNCTION platform.set_rowversion();
                CREATE TRIGGER set_rowversion BEFORE INSERT OR UPDATE ON access.local_credentials
                FOR EACH ROW EXECUTE FUNCTION platform.set_rowversion();
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP TRIGGER IF EXISTS set_rowversion ON access.local_credentials;
                DROP TRIGGER IF EXISTS set_rowversion ON access.local_users;
                """);
            migrationBuilder.DropTable(
                name: "local_credentials",
                schema: "access");

            migrationBuilder.DropTable(
                name: "local_users",
                schema: "access");
        }
    }
}
