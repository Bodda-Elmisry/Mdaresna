using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Platform.Infrastructure.Persistence.Platform.Migrations
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
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PersonId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_local_users", x => x.Id);
                    table.CheckConstraint("ck_access_local_users_identifiers", "[Id] <> '00000000-0000-0000-0000-000000000000' AND [PersonId] <> '00000000-0000-0000-0000-000000000000'");
                    table.CheckConstraint("ck_access_local_users_status", "[Status] IN (N'PendingActivation', N'Active', N'Disabled')");
                });

            migrationBuilder.CreateTable(
                name: "local_credentials",
                schema: "access",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                    table.PrimaryKey("PK_local_credentials", x => x.UserId);
                    table.CheckConstraint("ck_access_local_credentials_failed_count", "[FailedSignInCount] >= 0");
                    table.CheckConstraint("ck_access_local_credentials_hashing_version", "[HashingVersion] > 0");
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "local_credentials",
                schema: "access");

            migrationBuilder.DropTable(
                name: "local_users",
                schema: "access");
        }
    }
}
