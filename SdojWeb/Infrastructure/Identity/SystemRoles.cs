namespace SdojWeb.Infrastructure.Identity
{
    public class SystemRoles
    {
        public const string UserAdmin = "用户管理员";

        public const string QuestionAdmin = "题目管理员";

        public const string QuestionCreator = "题目作者";

        public const string QuestionAdminOrCreator = QuestionAdmin + "," + QuestionCreator;

        public const string Judger = "评测人员";

        public const string SolutionViewer = "可看答案";
    }
}