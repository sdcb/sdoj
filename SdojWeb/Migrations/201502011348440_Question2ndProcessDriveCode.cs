namespace SdojWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Question2ndProcessDriveCode : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Process2JudgeCode",
                c => new
                    {
                        QuestionId = c.Int(nullable: false),
                        Code = c.String(nullable: false),
                        Language = c.Int(nullable: false),
                        RunTimes = c.Short(nullable: false),
                        TimeLimitMs = c.Int(nullable: false),
                        MemoryLimitMb = c.Single(nullable: false),
                        UpdateTime = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                    })
                .PrimaryKey(t => t.QuestionId)
                .ForeignKey("dbo.Question", t => t.QuestionId)
                .Index(t => t.QuestionId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Process2JudgeCode", "QuestionId", "dbo.Question");
            DropIndex("dbo.Process2JudgeCode", new[] { "QuestionId" });
            DropTable("dbo.Process2JudgeCode");
        }
    }
}
