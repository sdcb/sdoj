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
            AddColumn("dbo.Questions", "QuestionType", c => c.Byte(nullable: false));
            Sql("UPDATE                                                              " +
                "    [dbo].[QuestionDatas]                                           " +
                "SET                                                                 " +
                "    [IsSample] = 1                                                  " +
                "FROM                                                                " +
                "    [dbo].[QuestionDatas] data                                      " +
                "JOIN                                                                " +
                "    [dbo].[Questions] question ON data.[QuestionId] = question.[Id] " +
                "WHERE                                                               " +
                "    question.[SampleDataId] = data.[Id]                             ");
            DropColumn("dbo.Questions", "SampleDataId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Questions", "SampleDataId", c => c.Int());
            Sql("UPDATE                                                              " +
                "    [dbo].[Questions]                                               " +
                "SET                                                                 " +
                "    [SampleDataId] = data.Id                                        " +
                "FROM                                                                " +
                "    [dbo].[Questions] question                                      " +
                "JOIN                                                                " +
                "    [dbo].[QuestionDatas] data ON data.[QuestionId] = question.[Id] " +
                "WHERE                                                               " +
                "    data.[IsSample] = 1                                             ");
            DropColumn("dbo.Questions", "QuestionType");
            DropColumn("dbo.QuestionDatas", "IsSample");
            CreateIndex("dbo.Questions", "SampleDataId");
            AddForeignKey("dbo.Questions", "SampleDataId", "dbo.QuestionDatas", "Id");
        }
    }
}
