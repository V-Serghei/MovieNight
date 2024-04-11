namespace MovieNight.BusinessLogic.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PendingChanges : DbMigration
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
                        Skype = c.String(),
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
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.PEdBdTables", "UserDbTableId", "dbo.UserDbTables");
            DropIndex("dbo.PEdBdTables", new[] { "UserDbTableId" });
            DropTable("dbo.UserDbTables");
            DropTable("dbo.PEdBdTables");
        }
    }
}
