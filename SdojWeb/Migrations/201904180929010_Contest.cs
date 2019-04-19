namespace SdojWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Contest : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ContestQuestion",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ContestId = c.Int(nullable: false),
                        QuestionId = c.Int(nullable: false),
                        Rank = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Contest", t => t.ContestId, cascadeDelete: true)
                .ForeignKey("dbo.Question", t => t.QuestionId, cascadeDelete: true)
                .Index(t => t.ContestId)
                .Index(t => t.QuestionId);
            
            CreateTable(
                "dbo.Contest",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 30),
                        Public = c.Boolean(nullable: false),
                        Duration = c.Time(nullable: false, precision: 7),
                        CreateTime = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        UpdateTime = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        StartTime = c.DateTime(precision: 7, storeType: "datetime2"),
                        CompleteTime = c.DateTime(precision: 7, storeType: "datetime2"),
                        CreateUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.CreateUserId)
                .Index(t => t.CreateUserId);
            
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
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ContestQuestion", "QuestionId", "dbo.Question");
            DropForeignKey("dbo.ContestQuestion", "ContestId", "dbo.Contest");
            DropForeignKey("dbo.Contest", "CreateUserId", "dbo.User");
            DropForeignKey("dbo.ContestUser", "UserId", "dbo.User");
            DropForeignKey("dbo.ContestUser", "ContestId", "dbo.Contest");
            DropIndex("dbo.ContestUser", new[] { "UserId" });
            DropIndex("dbo.ContestUser", new[] { "ContestId" });
            DropIndex("dbo.Contest", new[] { "CreateUserId" });
            DropIndex("dbo.ContestQuestion", new[] { "QuestionId" });
            DropIndex("dbo.ContestQuestion", new[] { "ContestId" });
            DropTable("dbo.ContestUser");
            DropTable("dbo.Contest");
            DropTable("dbo.ContestQuestion");
        }
    }
}
