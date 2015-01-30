namespace SdojWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class IsSampleToQuestionData : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Questions", "SampleDataId", "dbo.QuestionDatas");
            DropIndex("dbo.Questions", new[] { "SampleDataId" });
            AddColumn("dbo.QuestionDatas", "IsSample", c => c.Boolean(nullable: false));
            DropColumn("dbo.Questions", "SampleDataId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Questions", "SampleDataId", c => c.Int());
            DropColumn("dbo.QuestionDatas", "IsSample");
            CreateIndex("dbo.Questions", "SampleDataId");
            AddForeignKey("dbo.Questions", "SampleDataId", "dbo.QuestionDatas", "Id");
        }
    }
}
