using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ChatServer.Data.Migrations
{
    public partial class CreateChatMessagesTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChatMessages",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Author = table.Column<string>(maxLength: 256, nullable: true),
                    Text = table.Column<string>(maxLength: 256, nullable: true),
                    DateTime = table.Column<DateTime>(nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatMessages", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChatMessages");
        }
    }
}
