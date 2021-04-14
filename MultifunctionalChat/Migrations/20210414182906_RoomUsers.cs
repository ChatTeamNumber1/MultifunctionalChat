using Microsoft.EntityFrameworkCore.Migrations;

namespace MultifunctionalChat.Migrations
{
    public partial class RoomUsers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RoomUsers",
                columns: table => new
                {
                    RoomsId = table.Column<int>(type: "integer", nullable: false),
                    UsersId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoomUser", x => new { x.RoomsId, x.UsersId });
                    table.ForeignKey(
                        name: "FK_RoomUser_Rooms_RoomsId",
                        column: x => x.RoomsId,
                        principalTable: "Rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoomUser_Users_UsersId",
                        column: x => x.UsersId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RoomUser_UsersId",
                table: "RoomUsers",
                column: "UsersId");

            migrationBuilder.InsertData(
                table: "RoomUsers",
                columns: new[] { "UsersId", "RoomsId" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 2, 1 },
                    { 3, 1 },
                    { 1, 2 },
                    { 2, 2 },
                    { 3, 2 },
                    { 1, 3 },
                    { 3, 3 }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RoomUsers");
        }
    }
}
