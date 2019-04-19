using System;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using SdojWeb.Models;

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
                .Select(c => new ContestListModel(
                    c.Id,
                    c.Name,
                    c.Questions.Count,
                    c.CreateTime,
                    c.StartTime,
                    c.CompleteTime
                ))
                .OrderByDescending(x => x.Id);
        }

        public Task<int> Create(ContestCreateModel model)
        {
            throw new NotImplementedException();
        }

        private readonly ApplicationDbContext _db;

        private readonly IPrincipal _user;
    }
}