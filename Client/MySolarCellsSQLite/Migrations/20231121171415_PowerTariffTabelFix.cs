using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MySolarCellsSQLite.Migrations
{
    /// <inheritdoc />
    public partial class PowerTariffTabelFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DayTimeAllDay",
                table: "PowerTariffParameters");

            migrationBuilder.DropColumn(
                name: "PeroidAllYear",
                table: "PowerTariffParameters");

            migrationBuilder.DropColumn(
                name: "WeekDayAllDays",
                table: "PowerTariffParameters");

            migrationBuilder.RenameColumn(
                name: "weekday",
                table: "PowerTariffParameters",
                newName: "Weekday");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Weekday",
                table: "PowerTariffParameters",
                newName: "weekday");

            migrationBuilder.AddColumn<bool>(
                name: "DayTimeAllDay",
                table: "PowerTariffParameters",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PeroidAllYear",
                table: "PowerTariffParameters",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "WeekDayAllDays",
                table: "PowerTariffParameters",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }
    }
}
