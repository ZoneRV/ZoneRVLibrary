using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZoneRV.Migrations
{
    /// <inheritdoc />
    public partial class addingSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "production");

            migrationBuilder.RenameTable(
                name: "Model",
                newName: "Model",
                newSchema: "production");

            migrationBuilder.RenameTable(
                name: "LocationInventoryName",
                newName: "LocationInventoryName",
                newSchema: "production");

            migrationBuilder.RenameTable(
                name: "LocationCustomName",
                newName: "LocationCustomName",
                newSchema: "production");

            migrationBuilder.RenameTable(
                name: "Location",
                newName: "Location",
                newSchema: "production");

            migrationBuilder.RenameTable(
                name: "Line",
                newName: "Line",
                newSchema: "production");

            migrationBuilder.RenameTable(
                name: "AreaOfOrigin",
                newName: "AreaOfOrigin",
                newSchema: "production");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "Model",
                schema: "production",
                newName: "Model");

            migrationBuilder.RenameTable(
                name: "LocationInventoryName",
                schema: "production",
                newName: "LocationInventoryName");

            migrationBuilder.RenameTable(
                name: "LocationCustomName",
                schema: "production",
                newName: "LocationCustomName");

            migrationBuilder.RenameTable(
                name: "Location",
                schema: "production",
                newName: "Location");

            migrationBuilder.RenameTable(
                name: "Line",
                schema: "production",
                newName: "Line");

            migrationBuilder.RenameTable(
                name: "AreaOfOrigin",
                schema: "production",
                newName: "AreaOfOrigin");
        }
    }
}
