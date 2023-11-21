using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MySolarCellsSQLite.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingHomeId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "HomeId",
                table: "SavingEssitmateParameters",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "HomeId",
                table: "PowerTariffParameters",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1);
           // migrationBuilder.Sql("Update SavingEssitmateParameters Set HomeId = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HomeId",
                table: "SavingEssitmateParameters");

            migrationBuilder.DropColumn(
                name: "HomeId",
                table: "PowerTariffParameters");
        }
    }
}
