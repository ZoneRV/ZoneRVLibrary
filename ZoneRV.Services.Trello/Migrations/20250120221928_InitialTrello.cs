using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZoneRV.Services.Trello.Migrations
{
    /// <inheritdoc />
    public partial class InitialTrello : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "trello");

            migrationBuilder.CreateTable(
                name: "Actions",
                schema: "trello",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ActionId = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
                    BoardId = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
                    CardId = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
                    DateOffset = table.Column<DateTimeOffset>(type: "datetimeoffset", maxLength: 24, nullable: false),
                    ActionType = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
                    MemberId = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CheckId = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: true),
                    DueDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Actions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VanIds",
                schema: "trello",
                columns: table => new
                {
                    VanName = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
                    Id = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: true),
                    Blocked = table.Column<bool>(type: "bit", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VanIds", x => x.VanName);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Actions_ActionId",
                schema: "trello",
                table: "Actions",
                column: "ActionId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Actions",
                schema: "trello");

            migrationBuilder.DropTable(
                name: "VanIds",
                schema: "trello");
        }
    }
}
