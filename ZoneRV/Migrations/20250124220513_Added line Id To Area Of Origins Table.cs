using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZoneRV.Migrations
{
    /// <inheritdoc />
    public partial class AddedlineIdToAreaOfOriginsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LocationCustomName_Line_LineId",
                schema: "production",
                table: "LocationCustomName");

            migrationBuilder.DropForeignKey(
                name: "FK_LocationCustomName_Location_ProductionLocationId",
                schema: "production",
                table: "LocationCustomName");

            migrationBuilder.DropForeignKey(
                name: "FK_LocationInventoryName_Location_ProductionLocationId",
                schema: "production",
                table: "LocationInventoryName");

            migrationBuilder.DropForeignKey(
                name: "FK_Model_Line_ProductionLineId",
                schema: "production",
                table: "Model");

            migrationBuilder.DropIndex(
                name: "IX_Model_ProductionLineId",
                schema: "production",
                table: "Model");

            migrationBuilder.DropIndex(
                name: "IX_LocationCustomName_ProductionLocationId",
                schema: "production",
                table: "LocationCustomName");

            migrationBuilder.DropColumn(
                name: "ProductionLineId",
                schema: "production",
                table: "Model");

            migrationBuilder.DropColumn(
                name: "ProductionLocationId",
                schema: "production",
                table: "LocationCustomName");

            migrationBuilder.RenameColumn(
                name: "ProductionLocationId",
                schema: "production",
                table: "LocationInventoryName",
                newName: "LocationId");

            migrationBuilder.RenameIndex(
                name: "IX_LocationInventoryName_ProductionLocationId",
                schema: "production",
                table: "LocationInventoryName",
                newName: "IX_LocationInventoryName_LocationId");

            migrationBuilder.RenameColumn(
                name: "LineId",
                schema: "production",
                table: "LocationCustomName",
                newName: "LocationId");

            migrationBuilder.RenameIndex(
                name: "IX_LocationCustomName_LineId",
                schema: "production",
                table: "LocationCustomName",
                newName: "IX_LocationCustomName_LocationId");

            migrationBuilder.AddColumn<int>(
                name: "LineId",
                schema: "production",
                table: "AreaOfOrigin",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Model_LineId",
                schema: "production",
                table: "Model",
                column: "LineId");

            migrationBuilder.CreateIndex(
                name: "IX_AreaOfOrigin_LineId",
                schema: "production",
                table: "AreaOfOrigin",
                column: "LineId");

            migrationBuilder.AddForeignKey(
                name: "FK_AreaOfOrigin_Line_LineId",
                schema: "production",
                table: "AreaOfOrigin",
                column: "LineId",
                principalSchema: "production",
                principalTable: "Line",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LocationCustomName_Location_LocationId",
                schema: "production",
                table: "LocationCustomName",
                column: "LocationId",
                principalSchema: "production",
                principalTable: "Location",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LocationInventoryName_Location_LocationId",
                schema: "production",
                table: "LocationInventoryName",
                column: "LocationId",
                principalSchema: "production",
                principalTable: "Location",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Model_Line_LineId",
                schema: "production",
                table: "Model",
                column: "LineId",
                principalSchema: "production",
                principalTable: "Line",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AreaOfOrigin_Line_LineId",
                schema: "production",
                table: "AreaOfOrigin");

            migrationBuilder.DropForeignKey(
                name: "FK_LocationCustomName_Location_LocationId",
                schema: "production",
                table: "LocationCustomName");

            migrationBuilder.DropForeignKey(
                name: "FK_LocationInventoryName_Location_LocationId",
                schema: "production",
                table: "LocationInventoryName");

            migrationBuilder.DropForeignKey(
                name: "FK_Model_Line_LineId",
                schema: "production",
                table: "Model");

            migrationBuilder.DropIndex(
                name: "IX_Model_LineId",
                schema: "production",
                table: "Model");

            migrationBuilder.DropIndex(
                name: "IX_AreaOfOrigin_LineId",
                schema: "production",
                table: "AreaOfOrigin");

            migrationBuilder.DropColumn(
                name: "LineId",
                schema: "production",
                table: "AreaOfOrigin");

            migrationBuilder.RenameColumn(
                name: "LocationId",
                schema: "production",
                table: "LocationInventoryName",
                newName: "ProductionLocationId");

            migrationBuilder.RenameIndex(
                name: "IX_LocationInventoryName_LocationId",
                schema: "production",
                table: "LocationInventoryName",
                newName: "IX_LocationInventoryName_ProductionLocationId");

            migrationBuilder.RenameColumn(
                name: "LocationId",
                schema: "production",
                table: "LocationCustomName",
                newName: "LineId");

            migrationBuilder.RenameIndex(
                name: "IX_LocationCustomName_LocationId",
                schema: "production",
                table: "LocationCustomName",
                newName: "IX_LocationCustomName_LineId");

            migrationBuilder.AddColumn<int>(
                name: "ProductionLineId",
                schema: "production",
                table: "Model",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ProductionLocationId",
                schema: "production",
                table: "LocationCustomName",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Model_ProductionLineId",
                schema: "production",
                table: "Model",
                column: "ProductionLineId");

            migrationBuilder.CreateIndex(
                name: "IX_LocationCustomName_ProductionLocationId",
                schema: "production",
                table: "LocationCustomName",
                column: "ProductionLocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_LocationCustomName_Line_LineId",
                schema: "production",
                table: "LocationCustomName",
                column: "LineId",
                principalSchema: "production",
                principalTable: "Line",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LocationCustomName_Location_ProductionLocationId",
                schema: "production",
                table: "LocationCustomName",
                column: "ProductionLocationId",
                principalSchema: "production",
                principalTable: "Location",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LocationInventoryName_Location_ProductionLocationId",
                schema: "production",
                table: "LocationInventoryName",
                column: "ProductionLocationId",
                principalSchema: "production",
                principalTable: "Location",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Model_Line_ProductionLineId",
                schema: "production",
                table: "Model",
                column: "ProductionLineId",
                principalSchema: "production",
                principalTable: "Line",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
