using System;
using System.Linq;
using System.Threading.Tasks;
using SdojWeb.Models;
using SdojWeb.Models.ContestModels;

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

        public Task<int> Create(ContestCreateModel model)
        {
            throw new NotImplementedException();
        }

        private readonly ApplicationDbContext _db;
    }
}