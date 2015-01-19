namespace SdojWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class QuestionGroupMigration : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.QuestionGroupItem",
                c => new
                    {
                        QuestionGroupId = c.Int(nullable: false),
                        QuestionId = c.Int(nullable: false),
                        QuestionName = c.String(maxLength: 20),
                        Order = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.QuestionGroupId, t.QuestionId })
                .ForeignKey("dbo.Questions", t => t.QuestionId, cascadeDelete: true)
                .ForeignKey("dbo.QuestionGroup", t => t.QuestionGroupId, cascadeDelete: true)
                .Index(t => t.QuestionGroupId)
                .Index(t => t.QuestionId);
            
            CreateTable(
                "dbo.QuestionGroup",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 20),
                        Description = c.String(maxLength: 4000),
                        CreateTime = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        ModifyTime = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        CreateUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.CreateUserId, cascadeDelete: true)
                .Index(t => t.CreateUserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.QuestionGroupItem", "QuestionGroupId", "dbo.QuestionGroup");
            DropForeignKey("dbo.QuestionGroup", "CreateUserId", "dbo.Users");
            DropForeignKey("dbo.QuestionGroupItem", "QuestionId", "dbo.Questions");
            DropIndex("dbo.QuestionGroup", new[] { "CreateUserId" });
            DropIndex("dbo.QuestionGroupItem", new[] { "QuestionId" });
            DropIndex("dbo.QuestionGroupItem", new[] { "QuestionGroupId" });
            DropTable("dbo.QuestionGroup");
            DropTable("dbo.QuestionGroupItem");
        }
    }
}
