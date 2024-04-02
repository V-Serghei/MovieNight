namespace MovieNight.BusinessLogic.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialMigration : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PEdBdTables",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
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
                        Skype = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.UserDbTables", t => t.Id)
                .Index(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.PEdBdTables", "Id", "dbo.UserDbTables");
            DropIndex("dbo.PEdBdTables", new[] { "Id" });
            DropTable("dbo.PEdBdTables");
        }
    }
}
