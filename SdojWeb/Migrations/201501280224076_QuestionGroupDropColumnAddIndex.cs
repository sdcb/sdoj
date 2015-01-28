namespace SdojWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class QuestionGroupDropColumnAddIndex : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.QuestionGroup", "Name", unique: true);
            CreateIndex("dbo.QuestionGroup", "ModifyTime");
            DropColumn("dbo.QuestionGroup", "CreateTime");
        }
        
        public override void Down()
        {
            AddColumn("dbo.QuestionGroup", "CreateTime", c => c.DateTime(nullable: false, precision: 7, storeType: "datetime2"));
            DropIndex("dbo.QuestionGroup", new[] { "ModifyTime" });
            DropIndex("dbo.QuestionGroup", new[] { "Name" });
        }
    }
}
