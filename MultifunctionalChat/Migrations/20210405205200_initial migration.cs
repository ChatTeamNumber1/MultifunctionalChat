using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MultifunctionalChat.Controllers;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace MultifunctionalChat.Migrations
{
    public partial class initialmigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "serial", nullable: false),
                    Text = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    RoomId = table.Column<int>(type: "integer", nullable: false),
                    MessageDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RoomMembers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "serial", nullable: false),
                    RoomId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoomMembers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Rooms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "serial", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    OwnerId = table.Column<int>(type: "integer", nullable: false),
                    IsPublic = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rooms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "serial", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Login = table.Column<string>(type: "text", nullable: true),
                    Password = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });



            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Name", "Login", "Password" },
                values: new object[,]
                {
                    { 1, "Главный", "1", AccountController.EncryptPassword("Главный") },
                    { 2, "Сергей", "Сергей", AccountController.EncryptPassword("Сергей") },
                    { 3, "Наталья", "Наталья", AccountController.EncryptPassword("Наталья") }
                });

            migrationBuilder.InsertData(
                table: "Rooms",
                columns: new[] { "Id", "Name", "OwnerId", "IsPublic" },
                values: new object[,]
                {
                    { 1, "Общий чат", 1, true },
                    { 2, "Кружок по интересам", 2, true },
                    { 3, "Всемирный заговор", 1, false }
                });

            migrationBuilder.InsertData(
                table: "RoomMembers",
                columns: new[] { "Id", "UserId", "RoomId" },
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

            migrationBuilder.InsertData(
                table: "Messages",
                columns: new[] { "Id", "UserId", "Text", "MessageDate", "RoomId" },
                values: new object[,]
                {
                    { 1, 1, "Решаем задачу", DateTime.Now, 1 },
                    { 2, 2, "Принято", DateTime.Now, 1 },
                    { 3, 3, "Хорошо", DateTime.Now, 1 },
                    { 4, 1, "Всем привет", DateTime.Now, 2 },
                    { 5, 1, "Всем привет", DateTime.Now, 3 },
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.DropTable(
                name: "RoomMembers");

            migrationBuilder.DropTable(
                name: "Rooms");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
