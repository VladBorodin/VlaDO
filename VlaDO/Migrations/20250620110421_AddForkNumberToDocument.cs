using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VlaDO.Migrations
{
    /// <inheritdoc />
    public partial class AddForkNumberToDocument : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ForkNumber",
                table: "Documents",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ForkNumber",
                table: "Documents");
        }
    }
}
