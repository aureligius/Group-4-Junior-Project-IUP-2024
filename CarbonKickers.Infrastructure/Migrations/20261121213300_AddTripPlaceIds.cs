using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarbonKickers.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTripPlaceIds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DestinationPlaceId",
                table: "TripLogs",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OriginPlaceId",
                table: "TripLogs",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DestinationPlaceId",
                table: "TripLogs");

            migrationBuilder.DropColumn(
                name: "OriginPlaceId",
                table: "TripLogs");
        }
    }
}
