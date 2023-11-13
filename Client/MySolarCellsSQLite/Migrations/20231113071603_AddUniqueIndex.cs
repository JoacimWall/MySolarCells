using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MySolarCellsSQLite.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_InvestmentAndLonInterest_FromDate_InvestmentAndLoanId",
                table: "InvestmentAndLonInterest",
                columns: new[] { "FromDate", "InvestmentAndLoanId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InvestmentAndLon_FromDate",
                table: "InvestmentAndLon",
                column: "FromDate",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EnergyCalculationParameter_FromDate",
                table: "EnergyCalculationParameter",
                column: "FromDate",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InvestmentAndLonInterest_FromDate_InvestmentAndLoanId",
                table: "InvestmentAndLonInterest");

            migrationBuilder.DropIndex(
                name: "IX_InvestmentAndLon_FromDate",
                table: "InvestmentAndLon");

            migrationBuilder.DropIndex(
                name: "IX_EnergyCalculationParameter_FromDate",
                table: "EnergyCalculationParameter");
        }
    }
}
