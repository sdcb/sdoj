using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.SignalR;
using SdojWeb.Infrastructure.Identity;
using SdojWeb.Models;

namespace SdojWeb.SignalR
{
    [Microsoft.AspNet.SignalR.Authorize(Roles = SystemRoles.Judger)]
    public class JudgeHub : Hub
    {
        public void Send(string text)
        {
            var user = Context.User;
            Clients.All.DoWork(text);
        }

        public void Update(int solutionId, 
            SolutionStatus statusId, long runTimeTicks, long usingMemory, 
            DateTime accessTime)
        {
            var db = DbContext;
            var solution = db.Solutions.Find(solutionId);
            if (solution != null)
            {
                
            }
        }

        public void Accept(int solutionId)
        {
            var db = DbContext;
            var solution = db.Solutions.Find(solutionId);

            
        }

        public override Task OnConnected()
        {
            Groups.Add(Context.ConnectionId, Context.User.Identity.GetUserId());
            return base.OnConnected();
        }

        public ApplicationDbContext DbContext
        {
            get { return DependencyResolver.Current.GetService<ApplicationDbContext>(); }
        }
    }
}