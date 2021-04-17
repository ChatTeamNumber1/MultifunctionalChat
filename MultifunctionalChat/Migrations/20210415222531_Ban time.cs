using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MultifunctionalChat.Migrations
{
    public partial class Bantime : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RoomUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "serial", nullable: false),
                    RoomsId = table.Column<int>(type: "integer", nullable: false),
                    UsersId = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<char>(type: "char", nullable: true),
                    BanStart = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    BanInterval = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoomUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoomUsers_Rooms_RoomsId",
                        column: x => x.RoomsId,
                        principalTable: "Rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoomUsers_Users_UsersId",
                        column: x => x.UsersId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RoomMembers_RoomId",
                table: "RoomMembers",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_RoomMembers_UserId",
                table: "RoomMembers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RoomUsers_RoomsId",
                table: "RoomUsers",
                column: "RoomsId");

            migrationBuilder.AddForeignKey(
                name: "FK_RoomMembers_Rooms_RoomId",
                table: "RoomMembers",
                column: "RoomId",
                principalTable: "Rooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RoomMembers_Users_UserId",
                table: "RoomMembers",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.InsertData(
                table: "RoomUsers",
                columns: new[] { "Id", "UsersId", "RoomsId" },
                values: new object[,]
                {
                    { 1, 1, 1 },
                    { 2, 2, 1 },
                    { 3, 3, 1 },
                    { 4, 1, 2 },
                    { 5, 2, 2 },
                    { 6, 3, 2 },
                    { 7, 1, 3 },
                    { 8, 3, 3 }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RoomMembers_Rooms_RoomId",
                table: "RoomMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_RoomMembers_Users_UserId",
                table: "RoomMembers");

            migrationBuilder.DropTable(
                name: "RoomUsers");

            migrationBuilder.DropIndex(
                name: "IX_RoomMembers_RoomId",
                table: "RoomMembers");

            migrationBuilder.DropIndex(
                name: "IX_RoomMembers_UserId",
                table: "RoomMembers");
        }
    }
}
