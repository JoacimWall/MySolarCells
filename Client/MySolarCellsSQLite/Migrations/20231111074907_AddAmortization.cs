using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MySolarCellsSQLite.Migrations
{
    /// <inheritdoc />
    public partial class AddAmortization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Amortization",
                table: "InvestmentAndLonInterest",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Amortization",
                table: "InvestmentAndLonInterest");
        }
    }
}
