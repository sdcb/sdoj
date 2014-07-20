using System;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using AutoMapper.QueryableExtensions;
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

            // 已有锁并已被占用，直接失败。
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
                    LockEndTime = DateTime.Now.AddSeconds(SolutionLockingSeconds)
                };
                db.SolutionLocks.Add(solutionLock);
            }
            else if (solutionLock.LockEndTime >= DateTime.Now)
            {
                solutionLock.LockClientId = Context.ConnectionId;
                solutionLock.LockEndTime = DateTime.Now.AddSeconds(SolutionLockingSeconds);
            }

            await db.SaveChangesAsync();
            return true;
        }

        public async Task<JudgeModel[]> GetAll()
        {
            var userId = Context.User.Identity.GetIntUserId();
            var db = DbContext;

            var models = await db.Solutions
                .Where(x => 
                    x.CreateUserId == userId &&
                    (x.Lock == null || x.Lock.LockEndTime > DateTime.Now))
                .Project().To<JudgeModel>()
                .Take(DispatchLimit)
                .ToArrayAsync();

            return models;
        }

        public override async Task OnConnected()
        {
            await Groups.Add(Context.ConnectionId, Context.User.Identity.GetUserId());
            Interlocked.Increment(ref ConnectionCount);
        }

        public override Task OnDisconnected()
        {
            Interlocked.Decrement(ref ConnectionCount);
            return base.OnDisconnected();
        }

        public ApplicationDbContext DbContext
        {
            get { return DependencyResolver.Current.GetService<ApplicationDbContext>(); }
        }

        public static int ConnectionCount;

        public const int SolutionLockingSeconds = 15;

        public const int DbScanIntervalSeconds = 30;

        public const int DispatchLimit = 20;
    }
}