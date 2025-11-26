using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Friends.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialFriends : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Movies",
                columns: table => new
                {
                    IdUser = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IdFriend = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    KindOfFriendship = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Movies", x => new { x.IdFriend, x.IdUser });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Movies");
        }
    }
}
