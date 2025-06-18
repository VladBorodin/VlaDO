using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VlaDO.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentTokenUserRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "ExpiresAt",
                table: "DocumentTokens",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "TEXT");

            migrationBuilder.CreateTable(
                name: "UserContact",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ContactId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserContact", x => new { x.UserId, x.ContactId });
                    table.ForeignKey(
                        name: "FK_UserContact_Users_ContactId",
                        column: x => x.ContactId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserContact_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentTokens_UserId",
                table: "DocumentTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserContact_ContactId",
                table: "UserContact",
                column: "ContactId");

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentTokens_Users_UserId",
                table: "DocumentTokens",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DocumentTokens_Users_UserId",
                table: "DocumentTokens");

            migrationBuilder.DropTable(
                name: "UserContact");

            migrationBuilder.DropIndex(
                name: "IX_DocumentTokens_UserId",
                table: "DocumentTokens");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ExpiresAt",
                table: "DocumentTokens",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldNullable: true);
        }
    }
}
