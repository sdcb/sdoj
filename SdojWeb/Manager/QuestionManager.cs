using System;
using System.Data.Entity;
using System.Linq;
using System.Security.Principal;
using AutoMapper;
using EntityFramework.Extensions;
using SdojWeb.Infrastructure.Identity;
using SdojWeb.Models;
using System.Threading.Tasks;
using SdojWeb.Models.DbModels;
using Microsoft.AspNet.Identity;
using AutoMapper.QueryableExtensions;
using System.Web;
using SdojWeb.Infrastructure;

namespace SdojWeb.Manager
{
    public class QuestionManager
    {
        public QuestionManager(ApplicationDbContext db, IPrincipal identity)
        {
            _db = db;
            _user = identity;
        }

        public async Task Create(QuestionCreateModel model)
        {
            var question = Mapper.Map<Question>(model);
            _db.Entry(question).State = EntityState.Added;

            if (question.QuestionType == QuestionTypes.Process2Drive)
            {
                question.Process2JudgeCode = new Process2JudgeCode
                {
                    Code = model.Source,
                    Language = model.Language,
                    MemoryLimitMb = model.MemoryLimitMb,
                    TimeLimitMs = model.TimeLimit,
                    RunTimes = model.RunTimes,
                    UpdateTime = DateTime.Now,
                };
            }
            await _db.SaveChangesAsync();
        }

        public IQueryable<QuestionSummaryViewModel> List(string name, string creator, QuestionTypes? type, bool? onlyMe)
        {
            var uid = _user.Identity.GetUserId<int>();
            IQueryable<Question> dbModels = _db.Questions;
            var userId = HttpContext.Current.User.Identity.GetUserId<int>();

            if (onlyMe != null)
            {
                dbModels = dbModels.Where(x => x.CreateUserId == userId);
            }

            var query = dbModels
                .ProjectTo<QuestionSummaryViewModel>(new { currentUserId = userId });

            if (!string.IsNullOrWhiteSpace(name))
            {
                name = name.Trim();
                query = query.Where(x => x.Name.StartsWith(name.Trim()));
            }
            if (!string.IsNullOrWhiteSpace(creator))
            {
                query = query.Where(x => x.Creator == creator.Trim());
            }
            if (type != null)
            {
                query = query.Where(x => x.QuestionType == type.Value);
            }

            return query;
        }

        public async Task Update(QuestionNotMappedEditModel secretModel, QuestionEditModel model)
        {
            var question = new Question();
            Mapper.Map(model, question);
            Mapper.Map(secretModel, question);

            question.UpdateTime = DateTime.Now;

            if (question.QuestionType == QuestionTypes.Process2Drive && question.Process2JudgeCode == null)
            {
                question.Process2JudgeCode = new Process2JudgeCode
                {
                    Code = "请尽快填写评测代码",
                    Language = Languages.CSharp,
                    TimeLimitMs = model.TimeLimit,
                    MemoryLimitMb = model.MemoryLimitMb,
                    QuestionId = model.Id,
                    RunTimes = 0,
                    UpdateTime = DateTime.Now
                };
                _db.Entry(question.Process2JudgeCode).State = EntityState.Added;
            }

            _db.Entry(question).State = EntityState.Modified;
            if (question.QuestionType == QuestionTypes.DataDrive && question.Process2JudgeCode != null)
            {
                _db.Entry(question.Process2JudgeCode).State = EntityState.Deleted;
            }
            
            await _db.SaveChangesAsync();
        }

        public async Task<bool> IsUserOwnsQuestion(int questionId)
        {
            var userId = await _db.Questions
                .Where(x => x.Id == questionId)
                .Select(x => x.CreateUserId)
                .FirstOrDefaultAsync();

            return _user.IsUserOrRole(userId, SystemRoles.QuestionAdmin);
        }

        public async Task<string> GetName(int questionId)
        {
            return await _db.Questions
                .Where(x => x.Id == questionId)
                .Select(x => x.Name)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> CheckName(string name, int? id = null)
        {
            var exist = await _db.Questions
                .AnyAsync(x => x.Name == name && x.Id != id);
            return !exist;
        }

        public async Task DeleteData(int id)
        {
            await _db.QuestionDatas
                .Where(x => x.Id == id)
                .DeleteAsync();
        }

        public async Task SaveData(int questionId, int? id, string input, string output, int time, float memory, bool isSample)
        {
            var state = id == null ? EntityState.Added : EntityState.Modified;

            QuestionData data;
            if (id == null)
            {
                data = new QuestionData { QuestionId = questionId };
            }
            else
            {
                data = await _db.QuestionDatas
                    .Where(x => x.QuestionId == questionId && x.Id == id)
                    .FirstAsync();
            }

            data.Input = input;
            data.Output = output.TrimEnd();
            data.TimeLimit = time;
            data.MemoryLimitMb = memory;
            data.UpdateTime = DateTime.Now;
            data.IsSample = isSample;

            _db.Entry(data).State = state;
            await _db.SaveChangesAsync();
        }

        private readonly ApplicationDbContext _db;

        private readonly IPrincipal _user;
    }
}