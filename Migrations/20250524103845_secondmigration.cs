using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace XLeadServer.Migrations
{
    /// <inheritdoc />
    public partial class secondmigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Deals_DealStages_DealStageId",
                table: "Deals");

            migrationBuilder.DropIndex(
                name: "IX_Deals_DealStageId",
                table: "Deals");

            migrationBuilder.DropColumn(
                name: "DealStageId",
                table: "Deals");

            migrationBuilder.DropColumn(
                name: "StageId",
                table: "Deals");

            migrationBuilder.DropColumn(
                name: "data",
                table: "Accounts");

            migrationBuilder.AddColumn<long>(
                name: "DealId",
                table: "DealStages",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_DealStages_DealId",
                table: "DealStages",
                column: "DealId");

            migrationBuilder.AddForeignKey(
                name: "FK_DealStages_Deals_DealId",
                table: "DealStages",
                column: "DealId",
                principalTable: "Deals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DealStages_Deals_DealId",
                table: "DealStages");

            migrationBuilder.DropIndex(
                name: "IX_DealStages_DealId",
                table: "DealStages");

            migrationBuilder.DropColumn(
                name: "DealId",
                table: "DealStages");

            migrationBuilder.AddColumn<long>(
                name: "DealStageId",
                table: "Deals",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "StageId",
                table: "Deals",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "data",
                table: "Accounts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Deals_DealStageId",
                table: "Deals",
                column: "DealStageId");

            migrationBuilder.AddForeignKey(
                name: "FK_Deals_DealStages_DealStageId",
                table: "Deals",
                column: "DealStageId",
                principalTable: "DealStages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
