using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarbonKickers.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTransportModeIsActive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "TransportModes",
                type: "boolean",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "TransportModes");
        }
    }
}
