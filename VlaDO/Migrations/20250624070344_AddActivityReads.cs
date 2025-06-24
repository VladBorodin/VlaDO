using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VlaDO.Migrations
{
    /// <inheritdoc />
    public partial class AddActivityReads : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsRead",
                table: "Activities");

            migrationBuilder.CreateTable(
                name: "ActivityReads",
                columns: table => new
                {
                    ActivityId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityReads", x => new { x.ActivityId, x.UserId });
                    table.ForeignKey(
                        name: "FK_ActivityReads_Activities_ActivityId",
                        column: x => x.ActivityId,
                        principalTable: "Activities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ActivityReads_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActivityReads_UserId",
                table: "ActivityReads",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActivityReads");

            migrationBuilder.AddColumn<bool>(
                name: "IsRead",
                table: "Activities",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }
    }
}
