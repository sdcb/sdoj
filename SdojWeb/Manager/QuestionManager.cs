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
            _db.Questions.Add(question);
            await _db.SaveChangesAsync();

            var data = Mapper.Map<QuestionData>(model);
            data.QuestionId = question.Id;
            _db.QuestionDatas.Add(data);
            await _db.SaveChangesAsync();

            question.SampleDataId = data.Id;
            await _db.SaveChangesAsync();
        }

        public IQueryable<QuestionSummaryViewModel> List(string name, string creator)
        {
            var uid = _user.Identity.GetUserId<int>();

            var query = _db.Questions.Select(x => new QuestionSummaryViewModel
            {
                Id = x.Id,
                Creator = x.CreateUser.UserName,
                DataCount = x.Datas.Count,
                Name = x.Name,
                UpdateTime = x.UpdateTime,

                MemoryLimitMb = x.Datas.Max(v => v.MemoryLimitMb),
                TimeLimit = x.Datas.Sum(v => v.TimeLimit),

                SolutionCount = x.Solutions.Count,
                AcceptedCount = x.Solutions.Count(v => v.State == SolutionState.Accepted),

                Complished = x.Solutions.Any(v => v.CreateUserId == uid && v.State == SolutionState.Accepted),
                Started = x.Solutions.Any(v => v.CreateUserId == uid)
            });

            if (!string.IsNullOrWhiteSpace(name))
            {
                name = name.Trim();
                query = query.Where(x => x.Name.StartsWith(name.Trim()));
            }
            if (!string.IsNullOrWhiteSpace(creator))
            {
                query = query.Where(x => x.Creator == creator.Trim());
            }

            return query;
        }

        public IQueryable<QuestionSummaryBasicViewModel> BasicList(string name, string creator)
        {
            var query = _db.Questions
                .Project().To<QuestionSummaryBasicViewModel>();

            if (!string.IsNullOrWhiteSpace(name))
            {
                name = name.Trim();
                query = query.Where(x => x.Name.StartsWith(name.Trim()));
            }
            if (!string.IsNullOrWhiteSpace(creator))
            {
                query = query.Where(x => x.Creator == creator.Trim());
            }

            return query;
        }

        public async Task Update(QuestionNotMappedEditModel secretModel, QuestionEditModel model)
        {
            var question = new Question();
            Mapper.Map(model, question);
            Mapper.Map(secretModel, question);

            if (question.SampleData.Input != model.SampleInput ||
                question.SampleData.Output != model.SampleOutput)
            {
                question.SampleData.UpdateTime = DateTime.Now;
            }
            question.SampleData.Input = model.SampleInput;
            question.SampleData.Output = model.SampleOutput;
            
            question.UpdateTime = DateTime.Now;

            _db.Entry(question).State = EntityState.Modified;
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

        public async Task SaveData(int questionId, int? id, string input, string output, int time, float memory)
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

            _db.Entry(data).State = state;
            await _db.SaveChangesAsync();
        }

        private readonly ApplicationDbContext _db;

        private readonly IPrincipal _user;
    }
}