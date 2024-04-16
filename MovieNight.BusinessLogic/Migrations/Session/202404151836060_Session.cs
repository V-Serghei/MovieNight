namespace MovieNight.BusinessLogic.Migrations.Session
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Session : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.SessionCookies",
                c => new
                    {
                        SessionId = c.Int(nullable: false, identity: true),
                        UserName = c.String(nullable: false, maxLength: 30),
                        Email = c.String(),
                        CookieString = c.String(nullable: false),
                        ExpireTime = c.DateTime(nullable: false),
                        UserDeviceInformation = c.String(),
                    })
                .PrimaryKey(t => t.SessionId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.SessionCookies");
        }
    }
}
