namespace SdojWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Difficulty : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Question", "Difficulty", c => c.Byte(nullable: false, defaultValue: 10));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Question", "Difficulty");
        }
    }
}
