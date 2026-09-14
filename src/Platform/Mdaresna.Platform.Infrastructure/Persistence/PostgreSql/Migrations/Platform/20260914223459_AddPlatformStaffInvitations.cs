using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Platform.Infrastructure.Persistence.PostgreSql.Migrations.Platform
{
    /// <inheritdoc />
    public partial class AddPlatformStaffInvitations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "staff_invitation_challenges",
                schema: "access",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_staff_invitation_challenges", x => x.UserId);
                    table.CheckConstraint("ck_access_staff_invitation_attempts", "\"SendCount\" >= 0 AND \"FailedAttempts\" >= 0");
                    table.ForeignKey(
                        name: "FK_staff_invitation_challenges_local_users_UserId",
                        column: x => x.UserId,
                        principalSchema: "access",
                        principalTable: "local_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
            migrationBuilder.Sql("""
                CREATE TRIGGER set_rowversion BEFORE INSERT OR UPDATE
                ON access.staff_invitation_challenges
                FOR EACH ROW EXECUTE FUNCTION platform.set_rowversion();
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS set_rowversion ON access.staff_invitation_challenges;");
            migrationBuilder.DropTable(
                name: "staff_invitation_challenges",
                schema: "access");
        }
    }
}
