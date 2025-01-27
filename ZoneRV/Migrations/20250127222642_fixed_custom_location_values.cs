using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZoneRV.Migrations
{
    /// <inheritdoc />
    public partial class fixed_custom_location_values : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AreaOfOrigin_Line_LineId",
                schema: "production",
                table: "AreaOfOrigin");

            migrationBuilder.DropForeignKey(
                name: "FK_LocationInventoryName_Line_LineId",
                schema: "production",
                table: "LocationInventoryName");

            migrationBuilder.DropForeignKey(
                name: "FK_LocationInventoryName_Location_LocationId",
                schema: "production",
                table: "LocationInventoryName");

            migrationBuilder.DropIndex(
                name: "IX_LocationInventoryName_LineId",
                schema: "production",
                table: "LocationInventoryName");

            migrationBuilder.DropColumn(
                name: "LineId",
                schema: "production",
                table: "LocationInventoryName");

            migrationBuilder.AlterColumn<int>(
                name: "LocationId",
                schema: "production",
                table: "LocationInventoryName",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "LineId",
                schema: "production",
                table: "AreaOfOrigin",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AreaOfOrigin_Line_LineId",
                schema: "production",
                table: "AreaOfOrigin",
                column: "LineId",
                principalSchema: "production",
                principalTable: "Line",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LocationInventoryName_Location_LocationId",
                schema: "production",
                table: "LocationInventoryName",
                column: "LocationId",
                principalSchema: "production",
                principalTable: "Location",
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
                name: "FK_LocationInventoryName_Location_LocationId",
                schema: "production",
                table: "LocationInventoryName");

            migrationBuilder.AlterColumn<int>(
                name: "LocationId",
                schema: "production",
                table: "LocationInventoryName",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "LineId",
                schema: "production",
                table: "LocationInventoryName",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "LineId",
                schema: "production",
                table: "AreaOfOrigin",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_LocationInventoryName_LineId",
                schema: "production",
                table: "LocationInventoryName",
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
                name: "FK_LocationInventoryName_Line_LineId",
                schema: "production",
                table: "LocationInventoryName",
                column: "LineId",
                principalSchema: "production",
                principalTable: "Line",
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
        }
    }
}
