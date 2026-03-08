using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SparshaERP.Migrations
{
    /// <inheritdoc />
    public partial class AddQuotationCharges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ApplyOther",
                table: "Quotations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ApplyTax",
                table: "Quotations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ApplyTransport",
                table: "Quotations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "TaxPercent",
                table: "Quotations",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApplyOther",
                table: "Quotations");

            migrationBuilder.DropColumn(
                name: "ApplyTax",
                table: "Quotations");

            migrationBuilder.DropColumn(
                name: "ApplyTransport",
                table: "Quotations");

            migrationBuilder.DropColumn(
                name: "TaxPercent",
                table: "Quotations");
        }
    }
}
