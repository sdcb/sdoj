using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SdojWeb.Models.ContestModels
{
    public class ContestCreateModel
    {
        [Display(Name = "名称"), Required, MaxLength(30)]
        public string Name { get; set; } = $"考试-{DateTime.Now:yyyy-MM-dd HH:mm}";

        [Display(Name = "完全公开")]
        public bool Public { get; set; }

        [Display(Name = "限时")]
        public TimeSpan Duration { get; set; }

        [Display(Name = "题目列表", Description = "填入题目Id, 多个题目逗号(,)分隔...")]
        [NumberList, Required]
        public string QuestionIds { get; set; }

        [Display(Name = "允许用户列表", Description = "填入用户Id, 多个用户逗号(,)分隔...")]
        [OptionalNumberList]
        public string UserIds { get; set; }

        public IEnumerable<int> GetQuestionIds() => QuestionIds
            .Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
            .Select(x => int.Parse(x));

        public IEnumerable<int> GetUserIds() => UserIds?
            .Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
            .Select(x => int.Parse(x)) ?? new List<int>();

        public async Task Validate(ApplicationDbContext db, ModelStateDictionary modelState)
        {
            if (!modelState.IsValid) return;
            ValidateTimeSpan(db, modelState);
            ValidatePublic(db, modelState);
            await ValidateQuestionIds(db, modelState);
            await ValidateUserIds(db, modelState);
        }

        private void ValidatePublic(ApplicationDbContext db, ModelStateDictionary modelState)
        {
            if (!Public && String.IsNullOrWhiteSpace(UserIds))
                modelState.AddModelError(nameof(UserIds), $"非Public时，必须要有用户。");
        }

        private void ValidateTimeSpan(ApplicationDbContext db, ModelStateDictionary modelState)
        {
            if (Duration == TimeSpan.Zero)
                modelState.AddModelError(nameof(Duration), $"时限不能为0。");
        }

        private async Task ValidateQuestionIds(ApplicationDbContext db, ModelStateDictionary modelState)
        {
            int[] questionIds = GetQuestionIds().ToArray();
            int[] existingQuestionIds = await db.Questions.Where(x => questionIds.Contains(x.Id)).Select(x => x.Id).ToArrayAsync();
            int[] notExistingQuestionIds = questionIds.Except(existingQuestionIds).ToArray();
            foreach (int errorId in notExistingQuestionIds)
                modelState.AddModelError(nameof(QuestionIds), $"{errorId} 不存在。");
        }

        private async Task ValidateUserIds(ApplicationDbContext db, ModelStateDictionary modelState)
        {
            int[] userIds = GetUserIds().ToArray();
            int[] existingUserIds = await db.Users.Where(x => userIds.Contains(x.Id)).Select(x => x.Id).ToArrayAsync();
            int[] notExistingUserIds = userIds.Except(existingUserIds).ToArray();
            foreach (int errorId in notExistingUserIds)
                modelState.AddModelError(nameof(UserIds), $"{errorId} 不存在。");
        }
    }

    public class NumberListAttribute : RegularExpressionAttribute
    {
        public NumberListAttribute() : base(@"^(\d+)(,\s*\d+)+\s*$")
        {
        }
    }

    public class OptionalNumberListAttribute : RegularExpressionAttribute
    {
        public OptionalNumberListAttribute() : base(@"^(\d+)?(,\s*\d+)+\s*$")
        {
        }
    }
}