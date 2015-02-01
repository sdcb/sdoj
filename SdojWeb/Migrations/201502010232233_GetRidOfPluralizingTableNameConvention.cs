namespace SdojWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class GetRidOfPluralizingTableNameConvention : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.QuestionDatas", newName: "QuestionData");
            RenameTable(name: "dbo.Questions", newName: "Question");
            RenameTable(name: "dbo.Users", newName: "User");
            RenameTable(name: "dbo.Roles", newName: "Role");
            RenameTable(name: "dbo.Solutions", newName: "Solution");
            RenameTable(name: "dbo.SolutionLocks", newName: "SolutionLock");
            RenameTable(name: "dbo.UserClaims", newName: "UserClaim");
            RenameTable(name: "dbo.UserLogins", newName: "UserLogin");
        }
        
        public override void Down()
        {
            RenameTable(name: "dbo.UserLogin", newName: "UserLogins");
            RenameTable(name: "dbo.UserClaim", newName: "UserClaims");
            RenameTable(name: "dbo.SolutionLock", newName: "SolutionLocks");
            RenameTable(name: "dbo.Solution", newName: "Solutions");
            RenameTable(name: "dbo.Role", newName: "Roles");
            RenameTable(name: "dbo.User", newName: "Users");
            RenameTable(name: "dbo.Question", newName: "Questions");
            RenameTable(name: "dbo.QuestionData", newName: "QuestionDatas");
        }
    }
}
