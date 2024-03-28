using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MySolarCellsSQLite.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Energy",
                columns: table => new
                {
                    EnergyId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ProductionSold = table.Column<double>(type: "REAL", nullable: false),
                    ProductionOwnUse = table.Column<double>(type: "REAL", nullable: false),
                    Purchased = table.Column<double>(type: "REAL", nullable: false),
                    BatteryCharge = table.Column<double>(type: "REAL", nullable: false),
                    BatteryUsed = table.Column<double>(type: "REAL", nullable: false),
                    Unit = table.Column<string>(type: "TEXT", nullable: false),
                    Currency = table.Column<string>(type: "TEXT", nullable: false),
                    ProductionSoldProfit = table.Column<double>(type: "REAL", nullable: false),
                    ProductionOwnUseProfit = table.Column<double>(type: "REAL", nullable: false),
                    PurchasedCost = table.Column<double>(type: "REAL", nullable: false),
                    BatteryUsedProfit = table.Column<double>(type: "REAL", nullable: false),
                    UnitPriceBuy = table.Column<double>(type: "REAL", nullable: false),
                    UnitPriceVatBuy = table.Column<double>(type: "REAL", nullable: false),
                    UnitPriceSold = table.Column<double>(type: "REAL", nullable: false),
                    UnitPriceVatSold = table.Column<double>(type: "REAL", nullable: false),
                    PriceLevel = table.Column<string>(type: "TEXT", nullable: false),
                    ElectricitySupplierProductionSold = table.Column<int>(type: "INTEGER", nullable: false),
                    InverterTypProductionOwnUse = table.Column<int>(type: "INTEGER", nullable: false),
                    ElectricitySupplierPurchased = table.Column<int>(type: "INTEGER", nullable: false),
                    ProductionSoldSynced = table.Column<bool>(type: "INTEGER", nullable: false),
                    ProductionOwnUseSynced = table.Column<bool>(type: "INTEGER", nullable: false),
                    PurchasedSynced = table.Column<bool>(type: "INTEGER", nullable: false),
                    BatterySynced = table.Column<bool>(type: "INTEGER", nullable: false),
                    HomeId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Energy", x => x.EnergyId);
                });

            migrationBuilder.CreateTable(
                name: "EnergyCalculationParameter",
                columns: table => new
                {
                    EnergyCalculationParameterId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FromDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ProdCompensationElectricityLowload = table.Column<double>(type: "REAL", nullable: false),
                    TransferFee = table.Column<double>(type: "REAL", nullable: false),
                    TaxReduction = table.Column<double>(type: "REAL", nullable: false),
                    EnergyTax = table.Column<double>(type: "REAL", nullable: false),
                    TotalInstallKwhPanels = table.Column<double>(type: "REAL", nullable: false),
                    FixedPriceKwh = table.Column<double>(type: "REAL", nullable: false),
                    UseSpotPrice = table.Column<bool>(type: "INTEGER", nullable: false),
                    HomeId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnergyCalculationParameter", x => x.EnergyCalculationParameterId);
                });

            migrationBuilder.CreateTable(
                name: "Home",
                columns: table => new
                {
                    HomeId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    FromDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SubSystemEntityId = table.Column<string>(type: "TEXT", nullable: false),
                    ElectricitySupplier = table.Column<int>(type: "INTEGER", nullable: false),
                    ImportOnlySpotPrice = table.Column<bool>(type: "INTEGER", nullable: false),
                    ApiKey = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Home", x => x.HomeId);
                });

            migrationBuilder.CreateTable(
                name: "Inverter",
                columns: table => new
                {
                    InverterId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    FromDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SubSystemEntityId = table.Column<string>(type: "TEXT", nullable: false),
                    InverterTyp = table.Column<int>(type: "INTEGER", nullable: false),
                    UserName = table.Column<string>(type: "TEXT", nullable: false),
                    Password = table.Column<string>(type: "TEXT", nullable: false),
                    ApiUrl = table.Column<string>(type: "TEXT", nullable: false),
                    ApiKey = table.Column<string>(type: "TEXT", nullable: false),
                    HomeId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inverter", x => x.InverterId);
                });

            migrationBuilder.CreateTable(
                name: "InvestmentAndLon",
                columns: table => new
                {
                    InvestmentAndLoanId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    FromDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Investment = table.Column<int>(type: "INTEGER", nullable: false),
                    Loan = table.Column<int>(type: "INTEGER", nullable: false),
                    HomeId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvestmentAndLon", x => x.InvestmentAndLoanId);
                });

            migrationBuilder.CreateTable(
                name: "Preferences",
                columns: table => new
                {
                    PreferencesId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    DateValue = table.Column<DateTime>(type: "TEXT", nullable: false),
                    StringValue = table.Column<string>(type: "TEXT", nullable: false),
                    IntValue = table.Column<int>(type: "INTEGER", nullable: false),
                    doubleValue = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Preferences", x => x.PreferencesId);
                });

            migrationBuilder.CreateTable(
                name: "InvestmentAndLonInterest",
                columns: table => new
                {
                    InvestmentAndLoanInterestId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    Interest = table.Column<float>(type: "REAL", nullable: false),
                    FromDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    InvestmentAndLoanId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvestmentAndLonInterest", x => x.InvestmentAndLoanInterestId);
                    table.ForeignKey(
                        name: "FK_InvestmentAndLonInterest_InvestmentAndLon_InvestmentAndLoanId",
                        column: x => x.InvestmentAndLoanId,
                        principalTable: "InvestmentAndLon",
                        principalColumn: "InvestmentAndLoanId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Energy_Timestamp",
                table: "Energy",
                column: "Timestamp",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InvestmentAndLonInterest_InvestmentAndLoanId",
                table: "InvestmentAndLonInterest",
                column: "InvestmentAndLoanId");

            migrationBuilder.CreateIndex(
                name: "IX_Preferences_Name",
                table: "Preferences",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Energy");

            migrationBuilder.DropTable(
                name: "EnergyCalculationParameter");

            migrationBuilder.DropTable(
                name: "Home");

            migrationBuilder.DropTable(
                name: "Inverter");

            migrationBuilder.DropTable(
                name: "InvestmentAndLonInterest");

            migrationBuilder.DropTable(
                name: "Preferences");

            migrationBuilder.DropTable(
                name: "InvestmentAndLon");
        }
    }
}
