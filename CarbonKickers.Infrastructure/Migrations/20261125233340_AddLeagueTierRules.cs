using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CarbonKickers.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLeagueTierRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LeagueTierRules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TierName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    MinXp = table.Column<int>(type: "integer", nullable: false),
                    MaxXp = table.Column<int>(type: "integer", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeagueTierRules", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "LeagueTierRules",
                columns: new[] { "Id", "IsActive", "MaxXp", "MinXp", "TierName" },
                values: new object[,]
                {
                    { 1, true, 99, 0, "Bronze" },
                    { 2, true, 299, 100, "Silver" },
                    { 3, true, 699, 300, "Gold" },
                    { 4, true, null, 700, "Diamond" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_LeagueTierRules_TierName",
                table: "LeagueTierRules",
                column: "TierName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LeagueTierRules");
        }
    }
}
