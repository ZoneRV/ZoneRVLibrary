using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZoneRV.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class FixedLocations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LocationInventoryName_OrderedLineLocations_OrderedLineLocationId",
                schema: "production",
                table: "LocationInventoryName");

            migrationBuilder.DropForeignKey(
                name: "FK_LocationInventoryName_WorkspaceLocation_LocationId",
                schema: "production",
                table: "LocationInventoryName");

            migrationBuilder.DropIndex(
                name: "IX_LocationInventoryName_OrderedLineLocationId",
                schema: "production",
                table: "LocationInventoryName");

            migrationBuilder.DropColumn(
                name: "OrderedLineLocationId",
                schema: "production",
                table: "LocationInventoryName");

            migrationBuilder.AddForeignKey(
                name: "FK_LocationInventoryName_OrderedLineLocations_LocationId",
                schema: "production",
                table: "LocationInventoryName",
                column: "LocationId",
                principalSchema: "production",
                principalTable: "OrderedLineLocations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LocationInventoryName_OrderedLineLocations_LocationId",
                schema: "production",
                table: "LocationInventoryName");

            migrationBuilder.AddColumn<int>(
                name: "OrderedLineLocationId",
                schema: "production",
                table: "LocationInventoryName",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LocationInventoryName_OrderedLineLocationId",
                schema: "production",
                table: "LocationInventoryName",
                column: "OrderedLineLocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_LocationInventoryName_OrderedLineLocations_OrderedLineLocationId",
                schema: "production",
                table: "LocationInventoryName",
                column: "OrderedLineLocationId",
                principalSchema: "production",
                principalTable: "OrderedLineLocations",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LocationInventoryName_WorkspaceLocation_LocationId",
                schema: "production",
                table: "LocationInventoryName",
                column: "LocationId",
                principalSchema: "production",
                principalTable: "WorkspaceLocation",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
