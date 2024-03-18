using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSchoolRelationToSchoolPostsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_SchoolPosts_SchoolId",
                table: "SchoolPosts",
                column: "SchoolId");

            migrationBuilder.AddForeignKey(
                name: "FK_SchoolPosts_Schools_SchoolId",
                table: "SchoolPosts",
                column: "SchoolId",
                principalTable: "Schools",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SchoolPosts_Schools_SchoolId",
                table: "SchoolPosts");

            migrationBuilder.DropIndex(
                name: "IX_SchoolPosts_SchoolId",
                table: "SchoolPosts");
        }
    }
}
