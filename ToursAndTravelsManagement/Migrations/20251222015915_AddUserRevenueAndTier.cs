using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToursAndTravelsManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddUserRevenueAndTier : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MembershipTierId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalRevenue",
                table: "AspNetUsers",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "MembershipTier",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MinRevenue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiscountPercent = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MembershipTier", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_MembershipTierId",
                table: "AspNetUsers",
                column: "MembershipTierId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_MembershipTier_MembershipTierId",
                table: "AspNetUsers",
                column: "MembershipTierId",
                principalTable: "MembershipTier",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_MembershipTier_MembershipTierId",
                table: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "MembershipTier");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_MembershipTierId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "MembershipTierId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "TotalRevenue",
                table: "AspNetUsers");
        }
    }
}
