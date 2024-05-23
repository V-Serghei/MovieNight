namespace MovieNight.BusinessLogic.Migrations.User
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class YourMigrationName : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.MailDbTables", "IsChecked", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.MailDbTables", "IsChecked");
        }
    }
}
