using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace MultifunctionalChat.Migrations
{
    public partial class Roles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RoleId",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "serial", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    ImageAddress = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Name", "ImageAddress" },
                values: new object[,]
                {
                    { 1, "Администратор", "https://drive.google.com/uc?export=view&id=12Fk7iS2hRwLFiOLYfX8ynAzEi2EJ9uw4"},
                    { 2, "Модератор", "https://drive.google.com/uc?export=view&id=1aSVPQjKiG0ryGFUF2g2DpKn0yLy1MCPO"},
                    { 3, "Пользователь", "https://drive.google.com/uc?export=view&id=113g5m-BHZkoBPAaj1yWztXpQUG0t6DSf"},
                    { 4, "Заблокированный пользователь", "https://drive.google.com/uc?export=view&id=1yXaC2hF-xTM2KXzyf8cuRZB7Kn4ZnUYx"},
                    { 5, "Владелец комнаты", "https://drive.google.com/uc?export=view&id=15LtbyhmvYjGxh-b7r-SjIBdUq6QBjl-C" }
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "RoleId",
                value: 1);
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "RoleId",
                value: 2);
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                column: "RoleId",
                value: 3);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropColumn(
                name: "RoleId",
                table: "Users");
        }
    }
}
