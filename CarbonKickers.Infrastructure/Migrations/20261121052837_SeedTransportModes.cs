using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CarbonKickers.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedTransportModes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ModeName",
                table: "TransportModes",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.InsertData(
                table: "TransportModes",
                columns: new[] { "Id", "EmissionFactorPerKm", "ModeName" },
                values: new object[,]
                {
                    { 1, 0.0, "Walk" },
                    { 2, 0.0, "Bicycle" },
                    { 3, 0.088999999999999996, "Bus" },
                    { 4, 0.10299999999999999, "Motorcycle" },
                    { 5, 0.192, "Car" },
                    { 6, 0.041000000000000002, "Train" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_TransportModes_ModeName",
                table: "TransportModes",
                column: "ModeName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TransportModes_ModeName",
                table: "TransportModes");

            migrationBuilder.DeleteData(
                table: "TransportModes",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "TransportModes",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "TransportModes",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "TransportModes",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "TransportModes",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "TransportModes",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.AlterColumn<string>(
                name: "ModeName",
                table: "TransportModes",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);
        }
    }
}
