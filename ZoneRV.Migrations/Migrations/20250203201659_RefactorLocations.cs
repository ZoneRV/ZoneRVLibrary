using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZoneRV.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class RefactorLocations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LocationInventoryName_Location_LocationId",
                schema: "production",
                table: "LocationInventoryName");

            migrationBuilder.DropTable(
                name: "LocationCustomName",
                schema: "production");

            migrationBuilder.DropTable(
                name: "Location",
                schema: "production");

            migrationBuilder.AddColumn<int>(
                name: "LineLocationId",
                schema: "production",
                table: "LocationInventoryName",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WorkspaceId",
                schema: "production",
                table: "Line",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Workspace",
                schema: "production",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Workspace", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkspaceLocation",
                schema: "production",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkspaceId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkspaceLocation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkspaceLocation_Workspace_WorkspaceId",
                        column: x => x.WorkspaceId,
                        principalSchema: "production",
                        principalTable: "Workspace",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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
                    ServiceType = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
                    CustomName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Order = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LineLocationId = table.Column<int>(type: "int", nullable: false)
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
                name: "IX_LocationInventoryName_LineLocationId",
                schema: "production",
                table: "LocationInventoryName",
                column: "LineLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Line_WorkspaceId",
                schema: "production",
                table: "Line",
                column: "WorkspaceId");

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

            migrationBuilder.CreateIndex(
                name: "IX_Workspace_Name",
                schema: "production",
                table: "Workspace",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkspaceLocation_WorkspaceId",
                schema: "production",
                table: "WorkspaceLocation",
                column: "WorkspaceId");

            migrationBuilder.AddForeignKey(
                name: "FK_Line_Workspace_WorkspaceId",
                schema: "production",
                table: "Line",
                column: "WorkspaceId",
                principalSchema: "production",
                principalTable: "Workspace",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LocationInventoryName_LineLocation_LineLocationId",
                schema: "production",
                table: "LocationInventoryName",
                column: "LineLocationId",
                principalSchema: "production",
                principalTable: "LineLocation",
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Line_Workspace_WorkspaceId",
                schema: "production",
                table: "Line");

            migrationBuilder.DropForeignKey(
                name: "FK_LocationInventoryName_LineLocation_LineLocationId",
                schema: "production",
                table: "LocationInventoryName");

            migrationBuilder.DropForeignKey(
                name: "FK_LocationInventoryName_WorkspaceLocation_LocationId",
                schema: "production",
                table: "LocationInventoryName");

            migrationBuilder.DropTable(
                name: "LineLocationCustomName",
                schema: "production");

            migrationBuilder.DropTable(
                name: "LineLocation",
                schema: "production");

            migrationBuilder.DropTable(
                name: "WorkspaceLocation",
                schema: "production");

            migrationBuilder.DropTable(
                name: "Workspace",
                schema: "production");

            migrationBuilder.DropIndex(
                name: "IX_LocationInventoryName_LineLocationId",
                schema: "production",
                table: "LocationInventoryName");

            migrationBuilder.DropIndex(
                name: "IX_Line_WorkspaceId",
                schema: "production",
                table: "Line");

            migrationBuilder.DropColumn(
                name: "LineLocationId",
                schema: "production",
                table: "LocationInventoryName");

            migrationBuilder.DropColumn(
                name: "WorkspaceId",
                schema: "production",
                table: "Line");

            migrationBuilder.CreateTable(
                name: "Location",
                schema: "production",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LineId = table.Column<int>(type: "int", nullable: true),
                    BayNumber = table.Column<int>(type: "int", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Order = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Location", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Location_Line_LineId",
                        column: x => x.LineId,
                        principalSchema: "production",
                        principalTable: "Line",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LocationCustomName",
                schema: "production",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LocationId = table.Column<int>(type: "int", nullable: false),
                    CustomName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ServiceType = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationCustomName", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocationCustomName_Location_LocationId",
                        column: x => x.LocationId,
                        principalSchema: "production",
                        principalTable: "Location",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Location_LineId",
                schema: "production",
                table: "Location",
                column: "LineId");

            migrationBuilder.CreateIndex(
                name: "IX_LocationCustomName_LocationId",
                schema: "production",
                table: "LocationCustomName",
                column: "LocationId");

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
    }
}
