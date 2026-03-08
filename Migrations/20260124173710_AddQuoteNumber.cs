using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SparshaERP.Migrations
{
    /// <inheritdoc />
    public partial class AddQuoteNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "QuoteNo",
                table: "Quotations",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QuoteNo",
                table: "Quotations");
        }
    }
}
