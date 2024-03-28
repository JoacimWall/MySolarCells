using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MySolarCellsSQLite.Migrations
{
    /// <inheritdoc />
    public partial class Refactoring : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SavingEssitmateParameters");

            migrationBuilder.RenameColumn(
                name: "doubleValue",
                table: "Preferences",
                newName: "DoubleValue");

            migrationBuilder.RenameColumn(
                name: "ProdCompensationElectricityLowload",
                table: "EnergyCalculationParameter",
                newName: "ProdCompensationElectricityLowLoad");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SavingEstimateParameters");

            migrationBuilder.RenameColumn(
                name: "DoubleValue",
                table: "Preferences",
                newName: "doubleValue");

            migrationBuilder.RenameColumn(
                name: "ProdCompensationElectricityLowLoad",
                table: "EnergyCalculationParameter",
                newName: "ProdCompensationElectricityLowload");

            migrationBuilder.CreateTable(
                name: "SavingEssitmateParameters",
                columns: table => new
                {
                    SavingEssitmateParametersId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FromDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    HomeId = table.Column<int>(type: "INTEGER", nullable: false),
                    PanelDegradationPerYear = table.Column<double>(type: "REAL", nullable: false),
                    RealDevelopmentElectricityPrice = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SavingEssitmateParameters", x => x.SavingEssitmateParametersId);
                });
        }
    }
}
