namespace SdojWeb.Infrastructure.Identity
{
    public class SystemRoles
    {
        // user.
        public const string UserAdmin = "用户管理员";

        // question.
        public const string QuestionAdmin = "题目管理员";

        public const string QuestionCreator = "题目作者";

        public const string QuestionAdminOrCreator = QuestionAdmin + "," + QuestionCreator;

        // question group.
        public const string QuestionGroupAdmin = "题目组管理员";

        public const string QuestionGroupCreator = "题目组作者";

        public const string QuestionGroupAdminOrCreator = QuestionGroupAdmin + "," + QuestionGroupCreator;

        // contest
        public const string ContestCreator = "考试作者";

        public const string ContestAdmin = "考试管理员";

        public const string ContestAdminOrCreator = ContestAdmin + "," + ContestCreator;

        // judger.
        public const string Judger = "评测人员";

        // solution viewer.
        public const string SolutionViewer = "可看答案";
    }
}