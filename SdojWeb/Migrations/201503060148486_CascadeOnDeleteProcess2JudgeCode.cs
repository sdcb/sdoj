namespace SdojWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CascadeOnDeleteProcess2JudgeCode : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Process2JudgeCode", "QuestionId", "dbo.Question");
            AddForeignKey("dbo.Process2JudgeCode", "QuestionId", "dbo.Question", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Process2JudgeCode", "QuestionId", "dbo.Question");
            AddForeignKey("dbo.Process2JudgeCode", "QuestionId", "dbo.Question", "Id");
        }
    }
}
