using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SdojWeb.Models;
using SdojWeb.Models.ContestModels;
using SdojWeb.Models.DbModels;

namespace SdojWeb.Manager
{
    public class ContestManager
    {
        public ContestManager(ApplicationDbContext db)
        {
            _db = db;
        }

        public IQueryable<ContestListModel> List(int currentUserId, bool isManager)
        {
            return _db.Contests
                .Where(x =>
                    isManager ||
                    x.Public ||
                    x.CreateUserId == currentUserId ||
                    x.Users.Select(v => v.UserId).Contains(currentUserId))
                .Select(c => new ContestListModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    Count = c.Questions.Count,
                    Duration = c.Duration, 
                    CreateTime = c.CreateTime,
                    StartTime = c.StartTime,
                    CompleteTime = c.CompleteTime, 
                })
                .OrderByDescending(x => x.Id);
        }

        public async Task<int> Create(ContestCreateModel model, int currentUserId)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            Contest data = _db.Contests.Add(new Contest
            {
                Name = model.Name,
                Public = model.Public,
                Duration = model.Duration,
                CreateTime = DateTime.Now,
                CreateUserId = currentUserId,
                Questions = model.GetQuestionIds().Select((x, i) => new ContestQuestion
                {
                    QuestionId = x,
                    Rank = i,
                }).ToList(),
                Users = model.GetUserIds()?.Select(x => new ContestUser
                {
                    UserId = x,
                }).ToList(),
            });
            await _db.SaveChangesAsync();
            return data.Id;
        }

        private readonly ApplicationDbContext _db;
    }
}