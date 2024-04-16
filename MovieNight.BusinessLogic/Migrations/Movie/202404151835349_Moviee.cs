namespace MovieNight.BusinessLogic.Migrations.Movie
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Moviee : DbMigration
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
                "dbo.ViewListDbTables",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        MovieId = c.Int(nullable: false),
                        UserValues = c.Int(nullable: false),
                        ReviewDate = c.DateTime(nullable: false),
                        UserComment = c.String(),
                        UserViewCount = c.Int(nullable: false),
                        TimeSpent = c.Time(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.MovieDbTables", t => t.MovieId, cascadeDelete: true)
                .ForeignKey("dbo.UserDbTables", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.MovieId);
            
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
            DropForeignKey("dbo.ViewListDbTables", "UserId", "dbo.UserDbTables");
            DropForeignKey("dbo.PEdBdTables", "UserDbTableId", "dbo.UserDbTables");
            DropForeignKey("dbo.ViewListDbTables", "MovieId", "dbo.MovieDbTables");
            DropForeignKey("dbo.MovieCardDbTables", "MovieId", "dbo.MovieDbTables");
            DropForeignKey("dbo.InterestingFactDbTables", "MovieId", "dbo.MovieDbTables");
            DropForeignKey("dbo.MovieCastMembers", "CastMemberId", "dbo.CastMemDbTables");
            DropForeignKey("dbo.MovieCastMembers", "MovieId", "dbo.MovieDbTables");
            DropIndex("dbo.MovieCastMembers", new[] { "CastMemberId" });
            DropIndex("dbo.MovieCastMembers", new[] { "MovieId" });
            DropIndex("dbo.PEdBdTables", new[] { "UserDbTableId" });
            DropIndex("dbo.ViewListDbTables", new[] { "MovieId" });
            DropIndex("dbo.ViewListDbTables", new[] { "UserId" });
            DropIndex("dbo.MovieCardDbTables", new[] { "MovieId" });
            DropIndex("dbo.InterestingFactDbTables", new[] { "MovieId" });
            DropTable("dbo.MovieCastMembers");
            DropTable("dbo.PEdBdTables");
            DropTable("dbo.UserDbTables");
            DropTable("dbo.ViewListDbTables");
            DropTable("dbo.MovieCardDbTables");
            DropTable("dbo.InterestingFactDbTables");
            DropTable("dbo.MovieDbTables");
            DropTable("dbo.CastMemDbTables");
        }
    }
}
