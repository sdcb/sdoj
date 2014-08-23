using System;
using System.Data.Entity;
using System.Linq;
using System.Security.Principal;
using AutoMapper;
using SdojWeb.Infrastructure.Identity;
using SdojWeb.Models;
using System.Threading.Tasks;

namespace SdojWeb.Manager
{
    public class QuestionManager
    {
        public QuestionManager(ApplicationDbContext dbContext, IPrincipal identity)
        {
            DbContext = dbContext;
            User = identity;
        }

        public async Task Create(QuestionCreateModel model)
        {
            var question = Mapper.Map<Question>(model);
            DbContext.Questions.Add(question);
            await DbContext.SaveChangesAsync();

            var data = Mapper.Map<QuestionData>(model);
            data.QuestionId = question.Id;
            DbContext.QuestionDatas.Add(data);
            await DbContext.SaveChangesAsync();

            question.SampleDataId = data.Id;
            await DbContext.SaveChangesAsync();
        }

        public async Task Update(QuestionNotMappedEditModel secretModel, QuestionEditModel model)
        {
            var question = new Question();
            Mapper.Map(model, question);
            Mapper.Map(secretModel, question);

            question.SampleData.Input = model.SampleInput;
            question.SampleData.Output = model.SampleOutput;
            question.UpdateTime = DateTime.Now;

            DbContext.Entry(question).State = EntityState.Modified;
            await DbContext.SaveChangesAsync();
        }

        public async Task<bool> IsUserOwnsQuestion(int questionId)
        {
            var userId = await DbContext.Questions
                .Where(x => x.Id == questionId)
                .Select(x => x.CreateUserId)
                .FirstOrDefaultAsync();

            return User.IsUserOrRole(userId, SystemRoles.QuestionAdmin);
        }

        public async Task<string> GetName(int questionId)
        {
            return await DbContext.Questions
                .Where(x => x.Id == questionId)
                .Select(x => x.Name)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> ExistName(string name)
        {
            return await DbContext.Questions
                .AnyAsync(x => x.Name == name);
        }

        public readonly ApplicationDbContext DbContext;

        public readonly IPrincipal User;
    }
}