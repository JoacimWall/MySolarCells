using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MySolarCellsSQLite.Migrations
{
    /// <inheritdoc />
    public partial class Add_PowerTariffParameters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                    PeroidAllYear = table.Column<bool>(type: "INTEGER", nullable: false),
                    PeriodMonthStart = table.Column<int>(type: "INTEGER", nullable: false),
                    PeriodMonthEnd = table.Column<int>(type: "INTEGER", nullable: false),
                    WeekDayAllDays = table.Column<bool>(type: "INTEGER", nullable: false),
                    Weekend = table.Column<bool>(type: "INTEGER", nullable: false),
                    weekday = table.Column<bool>(type: "INTEGER", nullable: false),
                    DayTimeAllDay = table.Column<bool>(type: "INTEGER", nullable: false),
                    DayTimeStart = table.Column<int>(type: "INTEGER", nullable: false),
                    DayTimeEnd = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PowerTariffParameters", x => x.PowerTariffParametersId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PowerTariffParameters");
        }
    }
}
