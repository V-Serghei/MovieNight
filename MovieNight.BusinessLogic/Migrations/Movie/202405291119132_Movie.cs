namespace MovieNight.BusinessLogic.Migrations.Movie
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Movie : DbMigration
    {
        public override void Up()
        {
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
            
            AddColumn("dbo.BookmarkDbTables", "BookMark", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UserAchievementDbTables", "UserId", "dbo.UserDbTables");
            DropForeignKey("dbo.UserAchievementDbTables", "AchievementId", "dbo.AchievementDbTables");
            DropIndex("dbo.UserAchievementDbTables", new[] { "AchievementId" });
            DropIndex("dbo.UserAchievementDbTables", new[] { "UserId" });
            DropColumn("dbo.BookmarkDbTables", "BookMark");
            DropTable("dbo.AchievementDbTables");
            DropTable("dbo.UserAchievementDbTables");
        }
    }
}
