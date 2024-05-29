namespace MovieNight.BusinessLogic.Migrations.Movie
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Movie : DbMigration
    {
        public override void Up()
        {
           
            
            
            
           
            
            
           
            
           
            
           
            
           
            
            
            
            
            
            CreateTable(
                "dbo.ReviewDbTables",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FilmId = c.Int(nullable: false),
                        UserId = c.Int(nullable: false),
                        Text = c.String(nullable: false),
                        Date = c.DateTime(nullable: false),
                        ReviewType = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.MovieDbTables", t => t.FilmId)
                .ForeignKey("dbo.UserDbTables", t => t.UserId)
                .Index(t => t.FilmId)
                .Index(t => t.UserId);
            
           
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ReviewDbTables", "UserId", "dbo.UserDbTables");
            DropForeignKey("dbo.ReviewDbTables", "FilmId", "dbo.MovieDbTables");
            DropForeignKey("dbo.MovieCardDbTables", "MovieId", "dbo.MovieDbTables");
            DropForeignKey("dbo.InterestingFactDbTables", "MovieId", "dbo.MovieDbTables");
            DropForeignKey("dbo.MovieCastMembers", "CastMemberId", "dbo.CastMemDbTables");
            DropForeignKey("dbo.MovieCastMembers", "MovieId", "dbo.MovieDbTables");
            DropForeignKey("dbo.ViewListDbTables", "UserId", "dbo.UserDbTables");
            DropForeignKey("dbo.ViewListDbTables", "MovieId", "dbo.MovieDbTables");
            DropForeignKey("dbo.PEdBdTables", "UserDbTableId", "dbo.UserDbTables");
            DropForeignKey("dbo.FriendsDbTables", "UserDbTable_Id", "dbo.UserDbTables");
            DropForeignKey("dbo.FriendsDbTables", "User_Id", "dbo.UserDbTables");
            DropForeignKey("dbo.FriendsDbTables", "Friend_Id", "dbo.UserDbTables");
            DropForeignKey("dbo.BookmarkDbTables", "UserId", "dbo.UserDbTables");
            DropForeignKey("dbo.UserAchievementDbTables", "UserId", "dbo.UserDbTables");
            DropForeignKey("dbo.UserAchievementDbTables", "AchievementId", "dbo.AchievementDbTables");
            DropForeignKey("dbo.BookmarkDbTables", "MovieId", "dbo.MovieDbTables");
            DropIndex("dbo.MovieCastMembers", new[] { "CastMemberId" });
            DropIndex("dbo.MovieCastMembers", new[] { "MovieId" });
            DropIndex("dbo.ReviewDbTables", new[] { "UserId" });
            DropIndex("dbo.ReviewDbTables", new[] { "FilmId" });
            DropIndex("dbo.MovieCardDbTables", new[] { "MovieId" });
            DropIndex("dbo.InterestingFactDbTables", new[] { "MovieId" });
            DropIndex("dbo.ViewListDbTables", new[] { "MovieId" });
            DropIndex("dbo.ViewListDbTables", new[] { "UserId" });
            DropIndex("dbo.PEdBdTables", new[] { "UserDbTableId" });
            DropIndex("dbo.FriendsDbTables", new[] { "UserDbTable_Id" });
            DropIndex("dbo.FriendsDbTables", new[] { "User_Id" });
            DropIndex("dbo.FriendsDbTables", new[] { "Friend_Id" });
            DropIndex("dbo.UserAchievementDbTables", new[] { "AchievementId" });
            DropIndex("dbo.UserAchievementDbTables", new[] { "UserId" });
            DropIndex("dbo.BookmarkDbTables", new[] { "UserId" });
            DropIndex("dbo.BookmarkDbTables", new[] { "MovieId" });
            DropTable("dbo.MovieCastMembers");
            DropTable("dbo.ReviewDbTables");
            DropTable("dbo.MovieCardDbTables");
            DropTable("dbo.InterestingFactDbTables");
            DropTable("dbo.ViewListDbTables");
            DropTable("dbo.PEdBdTables");
            DropTable("dbo.FriendsDbTables");
            DropTable("dbo.AchievementDbTables");
            DropTable("dbo.UserAchievementDbTables");
            DropTable("dbo.UserDbTables");
            DropTable("dbo.BookmarkDbTables");
            DropTable("dbo.MovieDbTables");
            DropTable("dbo.CastMemDbTables");
        }
    }
}
