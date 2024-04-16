namespace MovieNight.BusinessLogic.Migrations.User
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Movie : DbMigration
    {
        public override void Up()
        {
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
                        MovieDbTable_Id = c.Int(),
                        UserDbTable_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.MovieDbTables", t => t.MovieDbTable_Id)
                .ForeignKey("dbo.MovieDbTables", t => t.MovieId, cascadeDelete: true)
                .ForeignKey("dbo.UserDbTables", t => t.UserId, cascadeDelete: true)
                .ForeignKey("dbo.UserDbTables", t => t.UserDbTable_Id)
                .Index(t => t.UserId)
                .Index(t => t.MovieId)
                .Index(t => t.MovieDbTable_Id)
                .Index(t => t.UserDbTable_Id);
            
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
                "dbo.CastMemDbTableMovieDbTables",
                c => new
                    {
                        CastMemDbTable_Id = c.Int(nullable: false),
                        MovieDbTable_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.CastMemDbTable_Id, t.MovieDbTable_Id })
                .ForeignKey("dbo.CastMemDbTables", t => t.CastMemDbTable_Id, cascadeDelete: true)
                .ForeignKey("dbo.MovieDbTables", t => t.MovieDbTable_Id, cascadeDelete: true)
                .Index(t => t.CastMemDbTable_Id)
                .Index(t => t.MovieDbTable_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ViewListDbTables", "UserDbTable_Id", "dbo.UserDbTables");
            DropForeignKey("dbo.ViewListDbTables", "UserId", "dbo.UserDbTables");
            DropForeignKey("dbo.ViewListDbTables", "MovieId", "dbo.MovieDbTables");
            DropForeignKey("dbo.ViewListDbTables", "MovieDbTable_Id", "dbo.MovieDbTables");
            DropForeignKey("dbo.MovieCardDbTables", "MovieId", "dbo.MovieDbTables");
            DropForeignKey("dbo.InterestingFactDbTables", "MovieId", "dbo.MovieDbTables");
            DropForeignKey("dbo.CastMemDbTableMovieDbTables", "MovieDbTable_Id", "dbo.MovieDbTables");
            DropForeignKey("dbo.CastMemDbTableMovieDbTables", "CastMemDbTable_Id", "dbo.CastMemDbTables");
            DropForeignKey("dbo.PEdBdTables", "UserDbTableId", "dbo.UserDbTables");
            DropIndex("dbo.CastMemDbTableMovieDbTables", new[] { "MovieDbTable_Id" });
            DropIndex("dbo.CastMemDbTableMovieDbTables", new[] { "CastMemDbTable_Id" });
            DropIndex("dbo.MovieCardDbTables", new[] { "MovieId" });
            DropIndex("dbo.InterestingFactDbTables", new[] { "MovieId" });
            DropIndex("dbo.ViewListDbTables", new[] { "UserDbTable_Id" });
            DropIndex("dbo.ViewListDbTables", new[] { "MovieDbTable_Id" });
            DropIndex("dbo.ViewListDbTables", new[] { "MovieId" });
            DropIndex("dbo.ViewListDbTables", new[] { "UserId" });
            DropIndex("dbo.PEdBdTables", new[] { "UserDbTableId" });
            DropTable("dbo.CastMemDbTableMovieDbTables");
            DropTable("dbo.MovieCardDbTables");
            DropTable("dbo.InterestingFactDbTables");
            DropTable("dbo.CastMemDbTables");
            DropTable("dbo.MovieDbTables");
            DropTable("dbo.ViewListDbTables");
            DropTable("dbo.UserDbTables");
            DropTable("dbo.PEdBdTables");
        }
    }
}
