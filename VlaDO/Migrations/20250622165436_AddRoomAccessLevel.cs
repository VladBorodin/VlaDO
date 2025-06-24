using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VlaDO.Migrations
{
    /// <inheritdoc />
    public partial class AddRoomAccessLevel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Rooms_OwnerId",
                table: "Rooms");

            migrationBuilder.AddColumn<int>(
                name: "AccessLevel",
                table: "Rooms",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_OwnerId_Title",
                table: "Rooms",
                columns: new[] { "OwnerId", "Title" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Rooms_OwnerId_Title",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "AccessLevel",
                table: "Rooms");

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_OwnerId",
                table: "Rooms",
                column: "OwnerId");
        }
    }
}
