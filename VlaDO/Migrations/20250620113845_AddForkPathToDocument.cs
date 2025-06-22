using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VlaDO.Migrations
{
    /// <inheritdoc />
    public partial class AddForkPathToDocument : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ForkNumber",
                table: "Documents");

            migrationBuilder.AddColumn<string>(
                name: "ForkPath",
                table: "Documents",
                type: "TEXT",
                maxLength: 128,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ForkPath",
                table: "Documents");

            migrationBuilder.AddColumn<int>(
                name: "ForkNumber",
                table: "Documents",
                type: "INTEGER",
                nullable: true);
        }
    }
}
