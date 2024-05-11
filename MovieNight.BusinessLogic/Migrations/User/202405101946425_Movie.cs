namespace MovieNight.BusinessLogic.Migrations.User
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Movie : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.FriendsDbTables",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IdUser = c.Int(nullable: false),
                        IdFriend = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.UserDbTables", t => t.IdFriend)
                .ForeignKey("dbo.UserDbTables", t => t.IdUser)
                .Index(t => t.IdUser)
                .Index(t => t.IdFriend);
            
            CreateTable(
                "dbo.MailDbTables",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        SenderId = c.Int(nullable: false),
                        RecipientId = c.Int(nullable: false),
                        Theme = c.String(),
                        Message = c.String(nullable: false),
                        Date = c.DateTime(nullable: false),
                        IsStarred = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.UserDbTables", t => t.RecipientId)
                .ForeignKey("dbo.UserDbTables", t => t.SenderId)
                .Index(t => t.SenderId)
                .Index(t => t.RecipientId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.MailDbTables", "SenderId", "dbo.UserDbTables");
            DropForeignKey("dbo.MailDbTables", "RecipientId", "dbo.UserDbTables");
            DropForeignKey("dbo.FriendsDbTables", "IdUser", "dbo.UserDbTables");
            DropForeignKey("dbo.FriendsDbTables", "IdFriend", "dbo.UserDbTables");
            DropIndex("dbo.MailDbTables", new[] { "RecipientId" });
            DropIndex("dbo.MailDbTables", new[] { "SenderId" });
            DropIndex("dbo.FriendsDbTables", new[] { "IdFriend" });
            DropIndex("dbo.FriendsDbTables", new[] { "IdUser" });
            DropTable("dbo.MailDbTables");
            DropTable("dbo.FriendsDbTables");
        }
    }
}
