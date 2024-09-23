using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentTypeToSchoolPeymnetRequestesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PaymentTypeId",
                table: "SchoolPaymentRequests",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_SchoolPaymentRequests_PaymentTypeId",
                table: "SchoolPaymentRequests",
                column: "PaymentTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_SchoolPaymentRequests_PaymentTypes_PaymentTypeId",
                table: "SchoolPaymentRequests",
                column: "PaymentTypeId",
                principalTable: "PaymentTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SchoolPaymentRequests_PaymentTypes_PaymentTypeId",
                table: "SchoolPaymentRequests");

            migrationBuilder.DropIndex(
                name: "IX_SchoolPaymentRequests_PaymentTypeId",
                table: "SchoolPaymentRequests");

            migrationBuilder.DropColumn(
                name: "PaymentTypeId",
                table: "SchoolPaymentRequests");
        }
    }
}
