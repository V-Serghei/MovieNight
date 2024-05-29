namespace MovieNight.BusinessLogic.Migrations.Movie
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Movie : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CastMemDbTables",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        Role = c.String(nullable: false),
                        ImageUrl = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.MovieDbTables",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Title = c.String(nullable: false, maxLength: 500),
                        Category = c.Int(nullable: false),
                        PosterImage = c.String(),
                        Quote = c.String(maxLength: 500),
                        Description = c.String(nullable: false),
                        ProductionYear = c.DateTime(nullable: false),
                        Country = c.String(nullable: false),
                        Genres = c.String(),
                        Location = c.String(nullable: false, maxLength: 500),
                        Director = c.String(nullable: false),
                        Duration = c.DateTime(nullable: false),
                        MovieNightGrade = c.Single(nullable: false),
                        Certificate = c.String(nullable: false, maxLength: 30),
                        ProductionCompany = c.String(),
                        Budget = c.String(),
                        GrossWorldwide = c.String(),
                        Language = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.BookmarkDbTables",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        MovieId = c.Int(nullable: false),
                        UserId = c.Int(nullable: false),
                        TimeAdd = c.DateTime(nullable: false),
                        BookmarkTimeOf = c.Boolean(nullable: false),
                        BookMark = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.MovieDbTables", t => t.MovieId, cascadeDelete: true)
                .ForeignKey("dbo.UserDbTables", t => t.UserId, cascadeDelete: true)
                .Index(t => t.MovieId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.UserDbTables",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserName = c.String(nullable: false, maxLength: 30),
                        Password = c.String(nullable: false, maxLength: 50),
                        Email = c.String(nullable: false, maxLength: 30),
                        LastLoginDate = c.DateTime(nullable: false),
                        LastIp = c.String(),
                        Role = c.Int(nullable: false),
                        Checkbox = c.Boolean(nullable: false),
                        Salt = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.UserAchievementDbTables",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        AchievementId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AchievementDbTables", t => t.AchievementId, cascadeDelete: true)
                .ForeignKey("dbo.UserDbTables", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.AchievementId);
            
            CreateTable(
                "dbo.AchievementDbTables",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        AchievementType = c.Int(nullable: false),
                        ImgA = c.String(),
                        Description = c.String(),
                        Condition = c.String(),
                        AdditionalRecords = c.String(),
                        SuccessСount = c.Int(nullable: false),
                        ProgressСount = c.Int(nullable: false),
                        Unlocked = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.FriendsDbTables",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IdUser = c.Int(nullable: false),
                        IdFriend = c.Int(nullable: false),
                        Friend_Id = c.Int(nullable: false),
                        User_Id = c.Int(nullable: false),
                        UserDbTable_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.UserDbTables", t => t.Friend_Id, cascadeDelete: true)
                .ForeignKey("dbo.UserDbTables", t => t.User_Id, cascadeDelete: true)
                .ForeignKey("dbo.UserDbTables", t => t.UserDbTable_Id)
                .Index(t => t.Friend_Id)
                .Index(t => t.User_Id)
                .Index(t => t.UserDbTable_Id);
            
            CreateTable(
                "dbo.PEdBdTables",
                c => new
                    {
                        UserDbTableId = c.Int(nullable: false),
                        FirstName = c.String(),
                        LastName = c.String(),
                        AboutMe = c.String(),
                        Gender = c.String(),
                        Avatar = c.String(),
                        DataBirth = c.DateTime(nullable: false),
                        PhoneNumber = c.String(),
                        Country = c.String(),
                        Quote = c.String(),
                        YPICOBSBYF = c.Boolean(nullable: false),
                        SEOBIAY = c.Boolean(nullable: false),
                        HYBH = c.Boolean(nullable: false),
                        HMG = c.Boolean(nullable: false),
                        Facebook = c.String(),
                        Twitter = c.String(),
                        Instagram = c.String(),
                        GitHab = c.String(),
                    })
                .PrimaryKey(t => t.UserDbTableId)
                .ForeignKey("dbo.UserDbTables", t => t.UserDbTableId)
                .Index(t => t.UserDbTableId);
            
            CreateTable(
                "dbo.ViewListDbTables",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        MovieId = c.Int(nullable: false),
                        UserValues = c.Int(nullable: false),
                        Title = c.String(),
                        ReviewDate = c.DateTime(nullable: false),
                        UserComment = c.String(),
                        UserViewCount = c.Int(nullable: false),
                        TimeSpent = c.DateTime(nullable: false),
                        Category = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.MovieDbTables", t => t.MovieId, cascadeDelete: true)
                .ForeignKey("dbo.UserDbTables", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.MovieId);
            
            CreateTable(
                "dbo.InterestingFactDbTables",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FactName = c.String(nullable: false, maxLength: 200),
                        FactText = c.String(nullable: false, maxLength: 1500),
                        MovieId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.MovieDbTables", t => t.MovieId, cascadeDelete: true)
                .Index(t => t.MovieId);
            
            CreateTable(
                "dbo.MovieCardDbTables",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Title = c.String(nullable: false, maxLength: 500),
                        ImageUrl = c.String(nullable: false),
                        Description = c.String(),
                        MovieId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.MovieDbTables", t => t.MovieId, cascadeDelete: true)
                .Index(t => t.MovieId);
            
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
            
            CreateTable(
                "dbo.MovieCastMembers",
                c => new
                    {
                        MovieId = c.Int(nullable: false),
                        CastMemberId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.MovieId, t.CastMemberId })
                .ForeignKey("dbo.MovieDbTables", t => t.MovieId, cascadeDelete: true)
                .ForeignKey("dbo.CastMemDbTables", t => t.CastMemberId, cascadeDelete: true)
                .Index(t => t.MovieId)
                .Index(t => t.CastMemberId);
            
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
