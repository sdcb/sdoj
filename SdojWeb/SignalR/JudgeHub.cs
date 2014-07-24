using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Web.Mvc;
using AutoMapper.QueryableExtensions;
using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.SignalR;
using SdojWeb.Infrastructure.Extensions;
using SdojWeb.Infrastructure.Identity;
using SdojWeb.Models;
using System.Threading;
using Timer = System.Timers.Timer;

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
            EnsureDbScanTaskRunning();
        }

        public override Task OnDisconnected()
        {
            Interlocked.Decrement(ref ConnectionCount);
            return base.OnDisconnected();
        }

        public static void EnsureDbScanTaskRunning()
        {
            if (Interlocked.CompareExchange(ref DbScanTaskRunning, 1, 0) == 0)
            {
                var timer = new Timer
                {
                    AutoReset = true,
                    Enabled = true,
                    Interval = DbScanIntervalSeconds * 1000,
                };
                timer.Elapsed += DbScan;
            }
        }

        private static async void DbScan(object sender, ElapsedEventArgs e)
        {
            if (Interlocked.CompareExchange(ref InScan, 1, 0) == 0)
            {
                var hub = GlobalHost.ConnectionManager.GetHubContext<JudgeHub>();
                using (var db = ApplicationDbContext.Create())
                {
                    var models = await db.Solutions
                    .Where(x => x.Lock == null || x.Lock.LockEndTime > DateTime.Now)
                    .Project().To<JudgeModel>()
                    .Take(DispatchLimit)
                    .ToArrayAsync();

                    foreach (var model in models)
                    {
                        hub.Clients
                            .Group(model.QuestionCreateUserId.ToStringInvariant())
                            .Judge(model);
                    }
                }
            }

            Interlocked.CompareExchange(ref InScan, 0, 1);
        }

        public static ApplicationDbContext DbContext
        {
            get { return DependencyResolver.Current.GetService<ApplicationDbContext>(); }
        }

        public static int DbScanTaskRunning = 0;

        public static int InScan = 0;

        public static int ConnectionCount;

        public const int SolutionLockingSeconds = 15;

        public const int DbScanIntervalSeconds = 5;

        public const int DispatchLimit = 20;
    }
}