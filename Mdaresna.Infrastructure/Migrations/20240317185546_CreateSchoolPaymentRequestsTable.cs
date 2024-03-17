using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateSchoolPaymentRequestsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SchoolRequestId",
                table: "PaymentTransactions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SchoolPaymentRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TransfareCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TransfareDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TransfareAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SchoolId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Approvied = table.Column<bool>(type: "bit", nullable: true),
                    ApproviedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchoolPaymentRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SchoolPaymentRequests_Users_ApproviedById",
                        column: x => x.ApproviedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_SchoolRequestId",
                table: "PaymentTransactions",
                column: "SchoolRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolPaymentRequests_ApproviedById",
                table: "SchoolPaymentRequests",
                column: "ApproviedById");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentTransactions_SchoolPaymentRequests_SchoolRequestId",
                table: "PaymentTransactions",
                column: "SchoolRequestId",
                principalTable: "SchoolPaymentRequests",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PaymentTransactions_SchoolPaymentRequests_SchoolRequestId",
                table: "PaymentTransactions");

            migrationBuilder.DropTable(
                name: "SchoolPaymentRequests");

            migrationBuilder.DropIndex(
                name: "IX_PaymentTransactions_SchoolRequestId",
                table: "PaymentTransactions");

            migrationBuilder.DropColumn(
                name: "SchoolRequestId",
                table: "PaymentTransactions");
        }
    }
}
