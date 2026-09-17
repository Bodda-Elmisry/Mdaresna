using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Mdaresna.Schools.Infrastructure.Persistence.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class InitialSchoolIdentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "school");

            migrationBuilder.CreateTable(
                name: "local_permissions",
                schema: "school",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Module = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DisplayNameAr = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    DisplayNameEn = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_local_permissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "local_roles",
                schema: "school",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DisplayNameAr = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    DisplayNameEn = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    IsSystem = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: false, defaultValue: Array.Empty<byte>())
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_local_roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "persons",
                schema: "school",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    MiddleName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: true),
                    GenderCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: false, defaultValue: Array.Empty<byte>())
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_persons", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "local_role_permissions",
                schema: "school",
                columns: table => new
                {
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    PermissionId = table.Column<Guid>(type: "uuid", nullable: false),
                    GrantedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    GrantedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_local_role_permissions", x => new { x.RoleId, x.PermissionId });
                    table.ForeignKey(
                        name: "FK_local_role_permissions_local_permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalSchema: "school",
                        principalTable: "local_permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_local_role_permissions_local_roles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "school",
                        principalTable: "local_roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "local_users",
                schema: "school",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlatformAccountId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NormalizedUserName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    PermissionsVersion = table.Column<long>(type: "bigint", nullable: false),
                    LastLoginAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: false, defaultValue: Array.Empty<byte>())
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_local_users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_local_users_persons_PersonId",
                        column: x => x.PersonId,
                        principalSchema: "school",
                        principalTable: "persons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "person_contacts",
                schema: "school",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Value = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    NormalizedValue = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_person_contacts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_person_contacts_persons_PersonId",
                        column: x => x.PersonId,
                        principalSchema: "school",
                        principalTable: "persons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "local_user_credentials",
                schema: "school",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    SecurityStamp = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FailedSignInCount = table.Column<int>(type: "integer", nullable: false),
                    LockoutEndUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    MustChangePassword = table.Column<bool>(type: "boolean", nullable: false),
                    ChangedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: false, defaultValue: Array.Empty<byte>())
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_local_user_credentials", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_local_user_credentials_local_users_UserId",
                        column: x => x.UserId,
                        principalSchema: "school",
                        principalTable: "local_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "local_user_roles",
                schema: "school",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AssignedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_local_user_roles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_local_user_roles_local_roles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "school",
                        principalTable: "local_roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_local_user_roles_local_users_UserId",
                        column: x => x.UserId,
                        principalSchema: "school",
                        principalTable: "local_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "local_user_sessions",
                schema: "school",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RefreshTokenHash = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ExpiresAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RevokedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RevocationReason = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_local_user_sessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_local_user_sessions_local_users_UserId",
                        column: x => x.UserId,
                        principalSchema: "school",
                        principalTable: "local_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "school",
                table: "local_permissions",
                columns: new[] { "Id", "Code", "CreatedAtUtc", "DisplayNameAr", "DisplayNameEn", "IsActive", "Module", "UpdatedAtUtc" },
                values: new object[,]
                {
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd01"), "school.dashboard.view", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "عرض لوحة التحكم", "View dashboard", true, "dashboard", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd02"), "school.people.view", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "عرض الأشخاص", "View people", true, "people", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd03"), "school.people.manage", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "إدارة الأشخاص", "Manage people", true, "people", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd04"), "school.users.view", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "عرض المستخدمين", "View users", true, "users", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd05"), "school.users.manage", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "إدارة المستخدمين", "Manage users", true, "users", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd06"), "school.roles.view", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "عرض الأدوار", "View roles", true, "access", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd07"), "school.roles.manage", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "إدارة الأدوار والصلاحيات", "Manage roles and permissions", true, "access", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) }
                });

            migrationBuilder.InsertData(
                schema: "school",
                table: "local_roles",
                columns: new[] { "Id", "Code", "CreatedAtUtc", "DisplayNameAr", "DisplayNameEn", "IsActive", "IsSystem", "UpdatedAtUtc" },
                values: new object[] { new Guid("62f4655a-20af-4acc-bb74-c42733e4f713"), "school-admin", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "مدير المدرسة", "School Admin", true, true, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) });

            migrationBuilder.InsertData(
                schema: "school",
                table: "local_role_permissions",
                columns: new[] { "PermissionId", "RoleId", "GrantedAtUtc", "GrantedByUserId" },
                values: new object[,]
                {
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd01"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd02"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd03"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd04"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd05"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd06"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd07"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_local_permissions_Code",
                schema: "school",
                table: "local_permissions",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_local_role_permissions_PermissionId",
                schema: "school",
                table: "local_role_permissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_local_roles_Code",
                schema: "school",
                table: "local_roles",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_local_user_roles_RoleId",
                schema: "school",
                table: "local_user_roles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_local_user_sessions_RefreshTokenHash",
                schema: "school",
                table: "local_user_sessions",
                column: "RefreshTokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_local_user_sessions_UserId",
                schema: "school",
                table: "local_user_sessions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_local_users_NormalizedUserName",
                schema: "school",
                table: "local_users",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_local_users_PersonId",
                schema: "school",
                table: "local_users",
                column: "PersonId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_person_contacts_PersonId_Type_NormalizedValue",
                schema: "school",
                table: "person_contacts",
                columns: new[] { "PersonId", "Type", "NormalizedValue" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "local_role_permissions",
                schema: "school");

            migrationBuilder.DropTable(
                name: "local_user_credentials",
                schema: "school");

            migrationBuilder.DropTable(
                name: "local_user_roles",
                schema: "school");

            migrationBuilder.DropTable(
                name: "local_user_sessions",
                schema: "school");

            migrationBuilder.DropTable(
                name: "person_contacts",
                schema: "school");

            migrationBuilder.DropTable(
                name: "local_permissions",
                schema: "school");

            migrationBuilder.DropTable(
                name: "local_roles",
                schema: "school");

            migrationBuilder.DropTable(
                name: "local_users",
                schema: "school");

            migrationBuilder.DropTable(
                name: "persons",
                schema: "school");
        }
    }
}
