namespace SdojWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Question2ndProcessDriveCode : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.QuestionProcessJudgeCode",
                c => new
                    {
                        QuestoinId = c.Int(nullable: false),
                        Code = c.String(nullable: false),
                        Language = c.Int(nullable: false),
                        RunTimes = c.Short(nullable: false),
                        TimeLimitMs = c.Int(nullable: false),
                        MemoryLimitMb = c.Single(nullable: false),
                        UpdateTime = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                    })
                .PrimaryKey(t => t.QuestoinId)
                .ForeignKey("dbo.Question", t => t.QuestoinId)
                .Index(t => t.QuestoinId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.QuestionProcessJudgeCode", "QuestoinId", "dbo.Question");
            DropIndex("dbo.QuestionProcessJudgeCode", new[] { "QuestoinId" });
            DropTable("dbo.QuestionProcessJudgeCode");
        }
    }
}
