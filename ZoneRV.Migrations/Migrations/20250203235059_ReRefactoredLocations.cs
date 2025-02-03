using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZoneRV.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class ReRefactoredLocations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LocationInventoryName_LineLocation_LineLocationId",
                schema: "production",
                table: "LocationInventoryName");

            migrationBuilder.DropTable(
                name: "LineLocationCustomName",
                schema: "production");

            migrationBuilder.DropTable(
                name: "LineLocation",
                schema: "production");

            migrationBuilder.RenameColumn(
                name: "LineLocationId",
                schema: "production",
                table: "LocationInventoryName",
                newName: "OrderedLineLocationId");

            migrationBuilder.RenameIndex(
                name: "IX_LocationInventoryName_LineLocationId",
                schema: "production",
                table: "LocationInventoryName",
                newName: "IX_LocationInventoryName_OrderedLineLocationId");

            migrationBuilder.CreateTable(
                name: "OrderedLineLocations",
                schema: "production",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LineId = table.Column<int>(type: "int", nullable: false),
                    LocationId = table.Column<int>(type: "int", nullable: false),
                    Order = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ServiceType = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderedLineLocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderedLineLocations_Line_LineId",
                        column: x => x.LineId,
                        principalSchema: "production",
                        principalTable: "Line",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderedLineLocations_WorkspaceLocation_LocationId",
                        column: x => x.LocationId,
                        principalSchema: "production",
                        principalTable: "WorkspaceLocation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LocationCustomName",
                schema: "production",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceType = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
                    CustomName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    LineLocationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationCustomName", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocationCustomName_OrderedLineLocations_LineLocationId",
                        column: x => x.LineLocationId,
                        principalSchema: "production",
                        principalTable: "OrderedLineLocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LocationCustomName_LineLocationId",
                schema: "production",
                table: "LocationCustomName",
                column: "LineLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderedLineLocations_LineId",
                schema: "production",
                table: "OrderedLineLocations",
                column: "LineId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderedLineLocations_LocationId",
                schema: "production",
                table: "OrderedLineLocations",
                column: "LocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_LocationInventoryName_OrderedLineLocations_OrderedLineLocationId",
                schema: "production",
                table: "LocationInventoryName",
                column: "OrderedLineLocationId",
                principalSchema: "production",
                principalTable: "OrderedLineLocations",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LocationInventoryName_OrderedLineLocations_OrderedLineLocationId",
                schema: "production",
                table: "LocationInventoryName");

            migrationBuilder.DropTable(
                name: "LocationCustomName",
                schema: "production");

            migrationBuilder.DropTable(
                name: "OrderedLineLocations",
                schema: "production");

            migrationBuilder.RenameColumn(
                name: "OrderedLineLocationId",
                schema: "production",
                table: "LocationInventoryName",
                newName: "LineLocationId");

            migrationBuilder.RenameIndex(
                name: "IX_LocationInventoryName_OrderedLineLocationId",
                schema: "production",
                table: "LocationInventoryName",
                newName: "IX_LocationInventoryName_LineLocationId");

            migrationBuilder.CreateTable(
                name: "LineLocation",
                schema: "production",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LineId = table.Column<int>(type: "int", nullable: false),
                    WorkspaceLocationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LineLocation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LineLocation_Line_LineId",
                        column: x => x.LineId,
                        principalSchema: "production",
                        principalTable: "Line",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LineLocation_WorkspaceLocation_WorkspaceLocationId",
                        column: x => x.WorkspaceLocationId,
                        principalSchema: "production",
                        principalTable: "WorkspaceLocation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LineLocationCustomName",
                schema: "production",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LineLocationId = table.Column<int>(type: "int", nullable: false),
                    CustomName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Order = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ServiceType = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LineLocationCustomName", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LineLocationCustomName_LineLocation_LineLocationId",
                        column: x => x.LineLocationId,
                        principalSchema: "production",
                        principalTable: "LineLocation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LineLocation_LineId",
                schema: "production",
                table: "LineLocation",
                column: "LineId");

            migrationBuilder.CreateIndex(
                name: "IX_LineLocation_WorkspaceLocationId",
                schema: "production",
                table: "LineLocation",
                column: "WorkspaceLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_LineLocationCustomName_LineLocationId",
                schema: "production",
                table: "LineLocationCustomName",
                column: "LineLocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_LocationInventoryName_LineLocation_LineLocationId",
                schema: "production",
                table: "LocationInventoryName",
                column: "LineLocationId",
                principalSchema: "production",
                principalTable: "LineLocation",
                principalColumn: "Id");
        }
    }
}
