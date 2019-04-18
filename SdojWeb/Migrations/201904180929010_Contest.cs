namespace SdojWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Contest : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ContestUser",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ContestId = c.Int(nullable: false),
                        UserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Contest", t => t.ContestId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.UserId, cascadeDelete: true)
                .Index(t => t.ContestId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.Contest",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 30),
                        Public = c.Boolean(nullable: false),
                        CreateTime = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        UpdateTime = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        StartTime = c.DateTime(precision: 7, storeType: "datetime2"),
                        CompleteTime = c.DateTime(precision: 7, storeType: "datetime2"),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ContestQuestion",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ContestId = c.Int(nullable: false),
                        QuestionId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Contest", t => t.ContestId, cascadeDelete: true)
                .ForeignKey("dbo.Question", t => t.QuestionId, cascadeDelete: true)
                .Index(t => t.ContestId)
                .Index(t => t.QuestionId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ContestUser", "UserId", "dbo.User");
            DropForeignKey("dbo.ContestUser", "ContestId", "dbo.Contest");
            DropForeignKey("dbo.ContestQuestion", "QuestionId", "dbo.Question");
            DropForeignKey("dbo.ContestQuestion", "ContestId", "dbo.Contest");
            DropIndex("dbo.ContestQuestion", new[] { "QuestionId" });
            DropIndex("dbo.ContestQuestion", new[] { "ContestId" });
            DropIndex("dbo.ContestUser", new[] { "UserId" });
            DropIndex("dbo.ContestUser", new[] { "ContestId" });
            DropTable("dbo.ContestQuestion");
            DropTable("dbo.Contest");
            DropTable("dbo.ContestUser");
        }
    }
}
