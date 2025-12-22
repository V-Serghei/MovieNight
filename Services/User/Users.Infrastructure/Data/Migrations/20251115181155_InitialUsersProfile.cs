using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Users.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialUsersProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserProfiles",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    FirstName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    AboutMe = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Quote = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    Gender = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: true),
                    DateOfBirth = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Country = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Facebook = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Twitter = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Instagram = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    GitHub = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    PersonalInfoFriendsOnly = table.Column<bool>(type: "bit", nullable: false),
                    ShowOnlyBasicInfo = table.Column<bool>(type: "bit", nullable: false),
                    HideBrowsingHistory = table.Column<bool>(type: "bit", nullable: false),
                    HideGrades = table.Column<bool>(type: "bit", nullable: false),
                    AvatarMediaId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProfiles", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_UserProfiles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserProfiles");
        }
    }
}
