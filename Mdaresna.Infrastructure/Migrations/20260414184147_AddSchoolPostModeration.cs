using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Infrastructure.Migrations
{
    [DbContext(typeof(Mdaresna.Infrastructure.Data.AppDbContext))]
    [Migration("20260414184147_AddSchoolPostModeration")]
    public partial class AddSchoolPostModeration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ModerationReason",
                table: "SchoolPosts",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ModerationStatus",
                table: "SchoolPosts",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(
                "UPDATE [SchoolPosts] SET [ModerationStatus] = 1 WHERE [ModerationStatus] IS NULL");

            migrationBuilder.AlterColumn<int>(
                name: "ModerationStatus",
                table: "SchoolPosts",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ModerationReason",
                table: "SchoolPosts");

            migrationBuilder.DropColumn(
                name: "ModerationStatus",
                table: "SchoolPosts");
        }
    }
}
