using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SparshaERP.Migrations
{
    /// <inheritdoc />
    public partial class AddExtraCharges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "OtherCharge",
                table: "Quotations",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SubTotal",
                table: "Quotations",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TaxAmount",
                table: "Quotations",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TransportCharge",
                table: "Quotations",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OtherCharge",
                table: "Quotations");

            migrationBuilder.DropColumn(
                name: "SubTotal",
                table: "Quotations");

            migrationBuilder.DropColumn(
                name: "TaxAmount",
                table: "Quotations");

            migrationBuilder.DropColumn(
                name: "TransportCharge",
                table: "Quotations");
        }
    }
}
