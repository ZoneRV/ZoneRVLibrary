using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZoneRV.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class RemoveServieTypeFromOrderedLocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ServiceType",
                schema: "production",
                table: "OrderedLineLocations");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ServiceType",
                schema: "production",
                table: "OrderedLineLocations",
                type: "nvarchar(24)",
                maxLength: 24,
                nullable: false,
                defaultValue: "");
        }
    }
}
