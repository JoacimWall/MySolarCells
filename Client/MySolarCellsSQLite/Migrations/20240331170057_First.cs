using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MySolarCellsSQLite.Migrations
{
    /// <inheritdoc />
    public partial class First : Migration
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
                    ProductionSoldProfit = table.Column<double>(type: "REAL", nullable: false),
                    ProductionOwnUseProfit = table.Column<double>(type: "REAL", nullable: false),
                    PurchasedCost = table.Column<double>(type: "REAL", nullable: false),
                    BatteryUsedProfit = table.Column<double>(type: "REAL", nullable: false),
                    UnitPriceBuy = table.Column<double>(type: "REAL", nullable: false),
                    UnitPriceVatBuy = table.Column<double>(type: "REAL", nullable: false),
                    UnitPriceSold = table.Column<double>(type: "REAL", nullable: false),
                    UnitPriceVatSold = table.Column<double>(type: "REAL", nullable: false),
                    PriceLevel = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    ElectricitySupplierProductionSold = table.Column<int>(type: "INTEGER", nullable: false),
                    InverterTypeProductionOwnUse = table.Column<int>(type: "INTEGER", nullable: false),
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
                    ProdCompensationElectricityLowLoad = table.Column<double>(type: "REAL", nullable: false),
                    TransferFee = table.Column<double>(type: "REAL", nullable: false),
                    TaxReduction = table.Column<double>(type: "REAL", nullable: false),
                    EnergyTax = table.Column<double>(type: "REAL", nullable: false),
                    TotalInstallKwhPanels = table.Column<double>(type: "REAL", nullable: false),
                    FixedPriceKwh = table.Column<double>(type: "REAL", nullable: false),
                    UseSpotPrice = table.Column<bool>(type: "INTEGER", nullable: false),
                    ElectricitySupplierId = table.Column<int>(type: "INTEGER", nullable: false)
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
                    Name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    EnergyUnit = table.Column<string>(type: "TEXT", maxLength: 5, nullable: false),
                    CurrencyUnit = table.Column<string>(type: "TEXT", maxLength: 5, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Home", x => x.HomeId);
                });

            migrationBuilder.CreateTable(
                name: "InvestmentAndLon",
                columns: table => new
                {
                    InvestmentAndLoanId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Description = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
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
                name: "Log",
                columns: table => new
                {
                    LogId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LogTitle = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    LogDetails = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    LogTyp = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Log", x => x.LogId);
                });

            migrationBuilder.CreateTable(
                name: "PowerTariffParameters",
                columns: table => new
                {
                    PowerTariffParametersId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FromDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    AmountOfPeaksToUse = table.Column<int>(type: "INTEGER", nullable: false),
                    UsePeaksFromSameDay = table.Column<bool>(type: "INTEGER", nullable: false),
                    PricePerKwh = table.Column<double>(type: "REAL", nullable: false),
                    PeriodMonthStart = table.Column<int>(type: "INTEGER", nullable: false),
                    PeriodMonthEnd = table.Column<int>(type: "INTEGER", nullable: false),
                    Weekend = table.Column<bool>(type: "INTEGER", nullable: false),
                    Weekday = table.Column<bool>(type: "INTEGER", nullable: false),
                    DayTimeStart = table.Column<int>(type: "INTEGER", nullable: false),
                    DayTimeEnd = table.Column<int>(type: "INTEGER", nullable: false),
                    ElectricitySupplierId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PowerTariffParameters", x => x.PowerTariffParametersId);
                });

            migrationBuilder.CreateTable(
                name: "Preferences",
                columns: table => new
                {
                    PreferencesId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    DateValue = table.Column<DateTime>(type: "TEXT", nullable: false),
                    StringValue = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    IntValue = table.Column<int>(type: "INTEGER", nullable: false),
                    DoubleValue = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Preferences", x => x.PreferencesId);
                });

            migrationBuilder.CreateTable(
                name: "SavingEstimateParameters",
                columns: table => new
                {
                    SavingEstimateParametersId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FromDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    RealDevelopmentElectricityPrice = table.Column<double>(type: "REAL", nullable: false),
                    PanelDegradationPerYear = table.Column<double>(type: "REAL", nullable: false),
                    HomeId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SavingEstimateParameters", x => x.SavingEstimateParametersId);
                });

            migrationBuilder.CreateTable(
                name: "ElectricitySupplier",
                columns: table => new
                {
                    ElectricitySupplierId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    FromDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SubSystemEntityId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    ElectricitySupplierType = table.Column<int>(type: "INTEGER", nullable: false),
                    ImportOnlySpotPrice = table.Column<bool>(type: "INTEGER", nullable: false),
                    ApiKey = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    HomeId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ElectricitySupplier", x => x.ElectricitySupplierId);
                    table.ForeignKey(
                        name: "FK_ElectricitySupplier_Home_HomeId",
                        column: x => x.HomeId,
                        principalTable: "Home",
                        principalColumn: "HomeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Inverter",
                columns: table => new
                {
                    InverterId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    FromDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SubSystemEntityId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    InverterTyp = table.Column<int>(type: "INTEGER", nullable: false),
                    UserName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Password = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    ApiUrl = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    ApiKey = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    HomeId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inverter", x => x.InverterId);
                    table.ForeignKey(
                        name: "FK_Inverter_Home_HomeId",
                        column: x => x.HomeId,
                        principalTable: "Home",
                        principalColumn: "HomeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InvestmentAndLonInterest",
                columns: table => new
                {
                    InvestmentAndLoanInterestId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Description = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Interest = table.Column<float>(type: "REAL", nullable: false),
                    FromDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Amortization = table.Column<int>(type: "INTEGER", nullable: false),
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
                name: "IX_ElectricitySupplier_HomeId",
                table: "ElectricitySupplier",
                column: "HomeId");

            migrationBuilder.CreateIndex(
                name: "IX_Energy_Timestamp",
                table: "Energy",
                column: "Timestamp",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EnergyCalculationParameter_FromDate",
                table: "EnergyCalculationParameter",
                column: "FromDate",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Inverter_HomeId",
                table: "Inverter",
                column: "HomeId");

            migrationBuilder.CreateIndex(
                name: "IX_InvestmentAndLon_FromDate",
                table: "InvestmentAndLon",
                column: "FromDate",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InvestmentAndLonInterest_FromDate_InvestmentAndLoanId",
                table: "InvestmentAndLonInterest",
                columns: new[] { "FromDate", "InvestmentAndLoanId" },
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
                name: "ElectricitySupplier");

            migrationBuilder.DropTable(
                name: "Energy");

            migrationBuilder.DropTable(
                name: "EnergyCalculationParameter");

            migrationBuilder.DropTable(
                name: "Inverter");

            migrationBuilder.DropTable(
                name: "InvestmentAndLonInterest");

            migrationBuilder.DropTable(
                name: "Log");

            migrationBuilder.DropTable(
                name: "PowerTariffParameters");

            migrationBuilder.DropTable(
                name: "Preferences");

            migrationBuilder.DropTable(
                name: "SavingEstimateParameters");

            migrationBuilder.DropTable(
                name: "Home");

            migrationBuilder.DropTable(
                name: "InvestmentAndLon");
        }
    }
}
