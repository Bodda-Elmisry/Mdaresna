using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSchoolRelationToSchoolPaymentRequestTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_SchoolPaymentRequests_SchoolId",
                table: "SchoolPaymentRequests",
                column: "SchoolId");

            migrationBuilder.AddForeignKey(
                name: "FK_SchoolPaymentRequests_Schools_SchoolId",
                table: "SchoolPaymentRequests",
                column: "SchoolId",
                principalTable: "Schools",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SchoolPaymentRequests_Schools_SchoolId",
                table: "SchoolPaymentRequests");

            migrationBuilder.DropIndex(
                name: "IX_SchoolPaymentRequests_SchoolId",
                table: "SchoolPaymentRequests");
        }
    }
}
