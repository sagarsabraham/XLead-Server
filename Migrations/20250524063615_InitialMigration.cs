using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace XLeadServer.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Deals_Accounts_AccountId",
                table: "Deals");

            migrationBuilder.DropForeignKey(
                name: "FK_Deals_Countries_CountryId",
                table: "Deals");

            migrationBuilder.DropForeignKey(
                name: "FK_Deals_DUs_DuId",
                table: "Deals");

            migrationBuilder.DropForeignKey(
                name: "FK_Deals_Domains_DomainId",
                table: "Deals");

            migrationBuilder.DropForeignKey(
                name: "FK_Deals_Regions_RegionId",
                table: "Deals");

            migrationBuilder.DropForeignKey(
                name: "FK_Deals_RevenueTypes_RevenueTypeId",
                table: "Deals");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RevenueTypes",
                table: "RevenueTypes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Regions",
                table: "Regions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DUs",
                table: "DUs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Domains",
                table: "Domains");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Countries",
                table: "Countries");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Accounts",
                table: "Accounts");

            migrationBuilder.RenameTable(
                name: "RevenueTypes",
                newName: "RevenueType");

            migrationBuilder.RenameTable(
                name: "Regions",
                newName: "Region");

            migrationBuilder.RenameTable(
                name: "DUs",
                newName: "DU");

            migrationBuilder.RenameTable(
                name: "Domains",
                newName: "Domain");

            migrationBuilder.RenameTable(
                name: "Countries",
                newName: "Country");

            migrationBuilder.RenameTable(
                name: "Accounts",
                newName: "Account");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RevenueType",
                table: "RevenueType",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Region",
                table: "Region",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DU",
                table: "DU",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Domain",
                table: "Domain",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Country",
                table: "Country",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Account",
                table: "Account",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Deals_Account_AccountId",
                table: "Deals",
                column: "AccountId",
                principalTable: "Account",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Deals_Country_CountryId",
                table: "Deals",
                column: "CountryId",
                principalTable: "Country",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Deals_DU_DuId",
                table: "Deals",
                column: "DuId",
                principalTable: "DU",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Deals_Domain_DomainId",
                table: "Deals",
                column: "DomainId",
                principalTable: "Domain",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Deals_Region_RegionId",
                table: "Deals",
                column: "RegionId",
                principalTable: "Region",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Deals_RevenueType_RevenueTypeId",
                table: "Deals",
                column: "RevenueTypeId",
                principalTable: "RevenueType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Deals_Account_AccountId",
                table: "Deals");

            migrationBuilder.DropForeignKey(
                name: "FK_Deals_Country_CountryId",
                table: "Deals");

            migrationBuilder.DropForeignKey(
                name: "FK_Deals_DU_DuId",
                table: "Deals");

            migrationBuilder.DropForeignKey(
                name: "FK_Deals_Domain_DomainId",
                table: "Deals");

            migrationBuilder.DropForeignKey(
                name: "FK_Deals_Region_RegionId",
                table: "Deals");

            migrationBuilder.DropForeignKey(
                name: "FK_Deals_RevenueType_RevenueTypeId",
                table: "Deals");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RevenueType",
                table: "RevenueType");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Region",
                table: "Region");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DU",
                table: "DU");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Domain",
                table: "Domain");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Country",
                table: "Country");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Account",
                table: "Account");

            migrationBuilder.RenameTable(
                name: "RevenueType",
                newName: "RevenueTypes");

            migrationBuilder.RenameTable(
                name: "Region",
                newName: "Regions");

            migrationBuilder.RenameTable(
                name: "DU",
                newName: "DUs");

            migrationBuilder.RenameTable(
                name: "Domain",
                newName: "Domains");

            migrationBuilder.RenameTable(
                name: "Country",
                newName: "Countries");

            migrationBuilder.RenameTable(
                name: "Account",
                newName: "Accounts");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RevenueTypes",
                table: "RevenueTypes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Regions",
                table: "Regions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DUs",
                table: "DUs",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Domains",
                table: "Domains",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Countries",
                table: "Countries",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Accounts",
                table: "Accounts",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Deals_Accounts_AccountId",
                table: "Deals",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Deals_Countries_CountryId",
                table: "Deals",
                column: "CountryId",
                principalTable: "Countries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Deals_DUs_DuId",
                table: "Deals",
                column: "DuId",
                principalTable: "DUs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Deals_Domains_DomainId",
                table: "Deals",
                column: "DomainId",
                principalTable: "Domains",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Deals_Regions_RegionId",
                table: "Deals",
                column: "RegionId",
                principalTable: "Regions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Deals_RevenueTypes_RevenueTypeId",
                table: "Deals",
                column: "RevenueTypeId",
                principalTable: "RevenueTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
