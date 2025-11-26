using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bookmark.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialMovieRatings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Bookmarks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MovieId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Kind = table.Column<int>(type: "int", nullable: false),
                    MovieTitle = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    MovieYear = table.Column<int>(type: "int", nullable: false),
                    MovieDuration = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MoviePosterImage = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    WatchedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bookmarks", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bookmarks_Kind_ExpiresAt",
                table: "Bookmarks",
                columns: new[] { "Kind", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Bookmarks_UserId_Kind_CreatedAt",
                table: "Bookmarks",
                columns: new[] { "UserId", "Kind", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Bookmarks_UserId_MovieId_Kind",
                table: "Bookmarks",
                columns: new[] { "UserId", "MovieId", "Kind" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Bookmarks");
        }
    }
}
