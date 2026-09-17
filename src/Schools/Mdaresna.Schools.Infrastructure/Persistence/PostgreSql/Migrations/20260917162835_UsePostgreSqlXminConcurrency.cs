using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Schools.Infrastructure.Persistence.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class UsePostgreSqlXminConcurrency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RowVersion",
                schema: "school",
                table: "school_information");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                schema: "school",
                table: "persons");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                schema: "school",
                table: "local_users");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                schema: "school",
                table: "local_user_credentials");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                schema: "school",
                table: "local_roles");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                schema: "school",
                table: "school_information",
                type: "bytea",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                schema: "school",
                table: "persons",
                type: "bytea",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                schema: "school",
                table: "local_users",
                type: "bytea",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                schema: "school",
                table: "local_user_credentials",
                type: "bytea",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                schema: "school",
                table: "local_roles",
                type: "bytea",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);
        }
    }
}
