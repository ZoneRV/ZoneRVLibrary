using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZoneRV.Api.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "production");

            migrationBuilder.CreateTable(
                name: "Line",
                schema: "production",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Line", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AreaOfOrigin",
                schema: "production",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
                    LineId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AreaOfOrigin", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AreaOfOrigin_Line_LineId",
                        column: x => x.LineId,
                        principalSchema: "production",
                        principalTable: "Line",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Location",
                schema: "production",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LineId = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    Order = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    BayNumber = table.Column<int>(type: "int", nullable: true)
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
                name: "Model",
                schema: "production",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LineId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    Prefix = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Model", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Model_Line_LineId",
                        column: x => x.LineId,
                        principalSchema: "production",
                        principalTable: "Line",
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
                    LocationId = table.Column<int>(type: "int", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "LocationInventoryName",
                schema: "production",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceType = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
                    CustomName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    LocationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationInventoryName", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocationInventoryName_Location_LocationId",
                        column: x => x.LocationId,
                        principalSchema: "production",
                        principalTable: "Location",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AreaOfOrigin_LineId",
                schema: "production",
                table: "AreaOfOrigin",
                column: "LineId");

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

            migrationBuilder.CreateIndex(
                name: "IX_LocationInventoryName_LocationId",
                schema: "production",
                table: "LocationInventoryName",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Model_LineId",
                schema: "production",
                table: "Model",
                column: "LineId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AreaOfOrigin",
                schema: "production");

            migrationBuilder.DropTable(
                name: "LocationCustomName",
                schema: "production");

            migrationBuilder.DropTable(
                name: "LocationInventoryName",
                schema: "production");

            migrationBuilder.DropTable(
                name: "Model",
                schema: "production");

            migrationBuilder.DropTable(
                name: "Location",
                schema: "production");

            migrationBuilder.DropTable(
                name: "Line",
                schema: "production");
        }
    }
}
