using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToursAndTravelsManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddIsDomesticToDestination : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDomestic",
                table: "Destinations",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDomestic",
                table: "Destinations");
        }
    }
}
