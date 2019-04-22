using System;
using System.Linq;
using System.Threading.Tasks;
using SdojWeb.Models;
using SdojWeb.Models.ContestModels;
using SdojWeb.Models.DbModels;
using System.Data.Entity;

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
                    Rank = i + 1,
                }).ToList(),
                Users = model.GetUserIds()?.Select(x => new ContestUser
                {
                    UserId = x,
                }).ToList(),
            });
            await _db.SaveChangesAsync();
            return data.Id;
        }

        public Task<ContestDetailsModel> Get(int contestId, int rank)
        {
            return _db.Contests
                .Where(x => x.Id == contestId)
                .Select(x => new ContestDetailsModel
                {
                    Id = x.Id,
                    CompleteTime = x.CompleteTime,
                    Count = x.Questions.Count,
                    Questions = x.Questions
                        .Select(q => new QuestionBriefModel
                        {
                            Id = q.QuestionId,
                            Rank = q.Rank, 
                            Name = q.Question.Name
                        })
                        .ToList(),
                    Name = x.Name,
                    CreateTime = x.CreateTime,
                    Duration = x.Duration,
                    StartTime = x.StartTime,
                })
                .FirstOrDefaultAsync();
        }

        public async Task<bool> CheckAccess(int contestId, bool isManager, int currentUserId)
        {
            // 管理员有牛逼权限
            if (isManager) return true;

            var contest = await _db.Contests
                .Where(x => x.Id == contestId)
                .Select(x => new { x.CreateUserId, x.Public, UserIds = x.Users.Select(v => v.Id) })
                .FirstOrDefaultAsync();
            // 未找到该contest -> false
            if (contest == null) return false;
            // 自己是作者 -> true
            if (contest.CreateUserId == currentUserId) return true;
            // 公共contest -> true
            if (contest.Public) return true;
            // 最后：用户列表包含当前用户
            return contest.UserIds.Contains(currentUserId);
        }

        private readonly ApplicationDbContext _db;
    }
}