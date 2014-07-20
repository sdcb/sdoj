using System;
using System.Data.Entity;
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
        public async Task<bool> Update(int solutionId,
            SolutionStatus statusId, int? runTimeMs, float? usingMemoryMb)
        {
            var db = DbContext;
            var solutionLock = await db.SolutionLocks.FindAsync(solutionId);

            // 未被锁住，或者锁住的客户端不正确，或者锁已经过期，则不允许操作。
            if (solutionLock == null)
            {
                return false;
            }
            if (solutionLock.LockClientId != Context.ConnectionId || solutionLock.LockEndTime > DateTime.Now)
            {
                db.Entry(solutionLock).State = EntityState.Deleted;
                await db.SaveChangesAsync();
                return false;
            }

            // 锁住，允许操作，然后改变状态。
            var solution = await db.Solutions.FindAsync(solutionId);
            solution.Status = statusId;
            if (runTimeMs != null) solution.RunTime = runTimeMs.Value;
            if (usingMemoryMb != null) solution.UsingMemoryMb = usingMemoryMb.Value;
            solution.Lock = null;

            // 删除锁，保存数据。
            db.Entry(solution).State = EntityState.Modified;
            db.Entry(solutionLock).State = EntityState.Deleted;
            await db.SaveChangesAsync();

            return true;
        }

        public async Task<bool> Lock(int solutionId)
        {
            var db = DbContext;

            var solutionLock = await db.SolutionLocks.FindAsync(solutionId);

            // 已有锁并已被别人占用，直接失败。
            if (solutionLock != null && solutionLock.LockEndTime < DateTime.Now)
            {
                return false;
            }

            // 没有锁，或者锁已经过期，允许锁定。
            if (solutionLock == null)
            {
                solutionLock = new SolutionLock
                {
                    SolutionId = solutionId,
                    LockClientId = Context.ConnectionId,
                    LockEndTime = DateTime.Now.AddSeconds(LockTime)
                };
                db.SolutionLocks.Add(solutionLock);
            }
            else if (solutionLock.LockEndTime >= DateTime.Now)
            {
                solutionLock.LockClientId = Context.ConnectionId;
                solutionLock.LockEndTime = DateTime.Now.AddSeconds(LockTime);
            }

            await db.SaveChangesAsync();
            return true;
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

        public const int LockTime = 15;
    }
}