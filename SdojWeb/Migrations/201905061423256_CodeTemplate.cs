namespace SdojWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CodeTemplate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CodeTemplate",
                c => new
                    {
                        Language = c.Int(nullable: false),
                        Template = c.String(),
                    })
                .PrimaryKey(t => t.Language);
            
            CreateTable(
                "dbo.QuestionCodeTemplate",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        QuestionId = c.Int(nullable: false),
                        Language = c.Int(nullable: false),
                        Template = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Question", t => t.QuestionId, cascadeDelete: true)
                .Index(t => t.QuestionId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.QuestionCodeTemplate", "QuestionId", "dbo.Question");
            DropIndex("dbo.QuestionCodeTemplate", new[] { "QuestionId" });
            DropTable("dbo.QuestionCodeTemplate");
            DropTable("dbo.CodeTemplate");
        }
    }
}
