using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class correctedStrategy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccessRecord_SpecialEvent_SpecialEventId",
                table: "AccessRecord");

            migrationBuilder.DropIndex(
                name: "IX_PointsAward_UserId",
                table: "PointsAward");

            migrationBuilder.CreateTable(
                name: "ScoringStrategyMeta",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScoringStrategyMeta", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PointsAward_StrategyId",
                table: "PointsAward",
                column: "StrategyId");

            migrationBuilder.CreateIndex(
                name: "IX_PointsAward_UserId_At",
                table: "PointsAward",
                columns: new[] { "UserId", "At" });

            migrationBuilder.CreateIndex(
                name: "IX_ScoringStrategyMeta_FilePath_FileName",
                table: "ScoringStrategyMeta",
                columns: new[] { "FilePath", "FileName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScoringStrategyMeta_IsActive",
                table: "ScoringStrategyMeta",
                column: "IsActive",
                unique: true,
                filter: "[IsActive] = 1 AND [IsDeleted] = 0");

            migrationBuilder.AddForeignKey(
                name: "FK_AccessRecord_SpecialEvent_SpecialEventId",
                table: "AccessRecord",
                column: "SpecialEventId",
                principalTable: "SpecialEvent",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_PointsAward_ScoringStrategyMeta_StrategyId",
                table: "PointsAward",
                column: "StrategyId",
                principalTable: "ScoringStrategyMeta",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccessRecord_SpecialEvent_SpecialEventId",
                table: "AccessRecord");

            migrationBuilder.DropForeignKey(
                name: "FK_PointsAward_ScoringStrategyMeta_StrategyId",
                table: "PointsAward");

            migrationBuilder.DropTable(
                name: "ScoringStrategyMeta");

            migrationBuilder.DropIndex(
                name: "IX_PointsAward_StrategyId",
                table: "PointsAward");

            migrationBuilder.DropIndex(
                name: "IX_PointsAward_UserId_At",
                table: "PointsAward");

            migrationBuilder.CreateIndex(
                name: "IX_PointsAward_UserId",
                table: "PointsAward",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_AccessRecord_SpecialEvent_SpecialEventId",
                table: "AccessRecord",
                column: "SpecialEventId",
                principalTable: "SpecialEvent",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
