using Microsoft.AspNet.Identity;
using SdojWeb.Models;
using SdojWeb.Models.DbModels;
using System.Linq;
using System.Web;
using System;
using System.Threading.Tasks;
using AutoMapper;

namespace SdojWeb.Manager
{
    public class QuestionGroupManager
    {
        private readonly ApplicationDbContext _db;

        private readonly HttpContextBase _httpContext;

        public QuestionGroupManager(ApplicationDbContext db, HttpContextBase httpContext)
        {
            _db = db;
            _httpContext = httpContext;
        }

        public IQueryable<QuestionGroup> List(int? id, bool? onlyMe, string name, string author)
        {
            var query = _db.QuestionGroups.AsQueryable();

            if (id != null)
            {
                query = query.Where(x => x.Id == id);
            }
            if (onlyMe != null)
            {
                var userId = _httpContext.User.Identity.GetUserId<int>();
                query = query.Where(x => x.CreateUserId == userId);
            }
            if (!string.IsNullOrWhiteSpace(name))
            {
                name = name.Trim();
                query = query.Where(x => x.Name.StartsWith(name));
            }
            if (!string.IsNullOrWhiteSpace(author))
            {
                author = author.Trim();
                query = query.Where(x => x.CreateUser.UserName == author);
            }

            return query;
        }

        public async Task Save(QuestionGroupEditModel toSave)
        {
            if (toSave.Id == 0)
            {
                await CreateQuestionGroup(toSave);
            }
            else
            {
                await ModifyQuestionGroup(toSave);
            }
        }

        private Task ModifyQuestionGroup(QuestionGroupEditModel toSave)
        {
            throw new NotImplementedException();
        }

        private async Task CreateQuestionGroup(QuestionGroupEditModel toSave)
        {
            var questionGroup = Mapper.Map<QuestionGroup>(toSave);
            questionGroup.CreateTime = DateTime.Now;

            var items = toSave.Questions.Select(x => Mapper.Map<QuestionGroupItem>(x)).ToList();

            _db.QuestionGroups.Add(questionGroup);
            await _db.SaveChangesAsync();
        }
    }
}