using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class ManyStrategiesV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MembershipLevel",
                table: "Reward",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "QuantityAvailable",
                table: "Reward",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MembershipLevel",
                table: "Reward");

            migrationBuilder.DropColumn(
                name: "QuantityAvailable",
                table: "Reward");
        }
    }
}
