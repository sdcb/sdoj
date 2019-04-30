namespace SdojWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Function : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.QuestionFunction",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        QuestionId = c.Int(nullable: false),
                        Name = c.String(nullable: false, maxLength: 30),
                        ReturnType = c.Int(nullable: false),
                        TimeLimitMs = c.Int(nullable: false),
                        MemoryLimitMb = c.Single(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Question", t => t.Id)
                .Index(t => t.Id)
                .Index(t => t.QuestionId, unique: true);
            
            CreateTable(
                "dbo.FunctionParameter",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        QuestionFunctionId = c.Int(nullable: false),
                        ParameterType = c.Int(nullable: false),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.QuestionFunction", t => t.QuestionFunctionId, cascadeDelete: true)
                .Index(t => t.QuestionFunctionId);
            
            CreateTable(
                "dbo.FunctionData",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FunctionId = c.Int(nullable: false),
                        Arguments = c.Binary(nullable: false),
                        ExpectedReturn = c.Binary(nullable: false),
                        UpdateTime = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.QuestionFunction", t => t.FunctionId, cascadeDelete: true)
                .Index(t => t.FunctionId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.FunctionData", "FunctionId", "dbo.QuestionFunction");
            DropForeignKey("dbo.QuestionFunction", "Id", "dbo.Question");
            DropForeignKey("dbo.FunctionParameter", "QuestionFunctionId", "dbo.QuestionFunction");
            DropIndex("dbo.FunctionData", new[] { "FunctionId" });
            DropIndex("dbo.FunctionParameter", new[] { "QuestionFunctionId" });
            DropIndex("dbo.QuestionFunction", new[] { "QuestionId" });
            DropIndex("dbo.QuestionFunction", new[] { "Id" });
            DropTable("dbo.FunctionData");
            DropTable("dbo.FunctionParameter");
            DropTable("dbo.QuestionFunction");
        }
    }
}
