using System;
using AutoMapper;
using SdojWeb.Models;
using System.Threading.Tasks;

namespace SdojWeb.Manager
{
    public class QuestionManager
    {
        public QuestionManager(ApplicationDbContext dbContext)
        {
            DbContext = dbContext;
        }

        public async Task Create(QuestionCreateModel model)
        {
            var question = Mapper.Map<Question>(model);
            DbContext.Questions.Add(question);
            await DbContext.SaveChangesAsync();
            
            var sampleData = new QuestionData
            {
                Input = model.SampleInput,
                Output = model.SampleOutput,
                QuestionId = question.Id,
                UpdateTime = DateTime.Now,
            };
            await DbContext.SaveChangesAsync();

            question.SampleDataId = sampleData.Id;
            await DbContext.SaveChangesAsync();
        }

        public readonly ApplicationDbContext DbContext;
    }
}