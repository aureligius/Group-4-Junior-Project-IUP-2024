using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarbonKickers.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBaselineTripCalculation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ActualDistanceIsEstimated",
                table: "TripLogs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ActualRouteMode",
                table: "TripLogs",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "BaselineCo2EmittedKg",
                table: "TripLogs",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<bool>(
                name: "BaselineDistanceIsEstimated",
                table: "TripLogs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<double>(
                name: "BaselineDistanceKm",
                table: "TripLogs",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "BaselineRouteMode",
                table: "TripLogs",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActualDistanceIsEstimated",
                table: "TripLogs");

            migrationBuilder.DropColumn(
                name: "ActualRouteMode",
                table: "TripLogs");

            migrationBuilder.DropColumn(
                name: "BaselineCo2EmittedKg",
                table: "TripLogs");

            migrationBuilder.DropColumn(
                name: "BaselineDistanceIsEstimated",
                table: "TripLogs");

            migrationBuilder.DropColumn(
                name: "BaselineDistanceKm",
                table: "TripLogs");

            migrationBuilder.DropColumn(
                name: "BaselineRouteMode",
                table: "TripLogs");
        }
    }
}
