using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZoneRV.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUnneededLocationType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Type",
                schema: "production",
                table: "WorkspaceLocation",
                newName: "LocationType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LocationType",
                schema: "production",
                table: "WorkspaceLocation",
                newName: "Type");
        }
    }
}
