using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Web.Mvc;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.SignalR;
using SdojWeb.Infrastructure.Extensions;
using SdojWeb.Infrastructure.Identity;
using SdojWeb.Models;
using System.Threading;
using SdojWeb.Models.JudgePush;
using Timer = System.Timers.Timer;

namespace SdojWeb.SignalR
{
    [Microsoft.AspNet.SignalR.Authorize(Roles = SystemRoles.Judger)]
    public class JudgeHub : Hub
    {
        // Hub API

        public async Task<bool> UpdateInLock(int solutionId,
            SolutionStatus statusId, int? runTimeMs, float? usingMemoryMb)
        {
            var db = DbContext;
            var slock = await db.SolutionLocks.FindAsync(solutionId);

            // 未被锁住，或者锁住的客户端不正确，或者锁已经过期，则不允许操作。
            if (slock == null || slock.LockClientId != Context.ConnectionId || slock.LockEndTime > DateTime.Now)
            {
                return false;
            }
            
            // 锁住，允许操作。
            var solution = await db.Solutions.FindAsync(solutionId);
            solution.Status = statusId;
            if (runTimeMs != null) solution.RunTime = runTimeMs.Value;
            if (usingMemoryMb != null) solution.UsingMemoryMb = usingMemoryMb.Value;

            // 保存数据。
            db.Entry(solution).State = EntityState.Modified;
            await db.SaveChangesAsync();

            return true;
        }

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

        public async Task<SolutionFullModel> Lock(int solutionId)
        {
            var db = DbContext;
            var userId = Context.User.Identity.GetIntUserId();

            var solution = await db.Solutions
                .Where(x =>
                    x.Question.CreateUserId == userId &&
                    solutionId == x.Id && 
                    (x.Lock == null || x.Lock.LockEndTime > DateTime.Now)) // 没有锁或者锁已过期
                .Select(x => new 
                {
                    Id = x.Id, 
                    Lock = x.Lock, 
                    Time = x.Question.Datas.Sum(d => d.TimeLimit)
                }).FirstOrDefaultAsync();

            if (solution == null) return null; // 找不到符合条件的解答，返回失败。

            // 锁定的时间，按题目的时间和，乘以一个倍数，再加上额外的传输时间为准。
            var lockMilliseconds = solution.Time*LockTimeFactor + LockAdditionalSeconds*1000;

            // 添加或者更新锁。
            var slock = solution.Lock ?? new SolutionLock {SolutionId = solution.Id};
            slock.LockClientId = Context.ConnectionId;
            slock.LockEndTime = DateTime.Now.AddMilliseconds(lockMilliseconds);

            db.Entry(slock).State = solution.Lock == null
                ? EntityState.Added
                : EntityState.Modified;

            await db.SaveChangesAsync();

            return await db.Solutions
                .Where(x => x.Id == solutionId)
                .Project().To<SolutionFullModel>()
                .FirstOrDefaultAsync();
        }

        public async Task<SolutionPushModel[]> GetAll()
        {
            var userId = Context.User.Identity.GetIntUserId();
            var db = DbContext;

            var models = await db.Solutions
                .Where(x => 
                    x.CreateUserId == userId &&
                    (x.Lock == null || x.Lock.LockEndTime > DateTime.Now))
                .Project().To<SolutionPushModel>()
                .Take(DispatchLimit)
                .ToArrayAsync();

            return models;
        }

        public async Task<QuestionDataFullModel[]> GetDatas(int[] dataId)
        {
            var db = DbContext;
            var userId = Context.User.Identity.GetIntUserId();

            var datas = await db.QuestionDatas
                .Where(x => 
                    x.Question.CreateUserId == userId &&
                    dataId.Contains(x.Id))
                .Project().To<QuestionDataFullModel>()
                .ToArrayAsync();

            return datas;
        }

        // Overrides

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

        // DBScan Jobs

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
                        .Where(x => 
                            x.Lock == null || 
                            x.Lock.LockEndTime > DateTime.Now)
                        .Project().To<SolutionPushModel>()
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

        // Field & Properties

        public static ApplicationDbContext DbContext
        {
            get { return DependencyResolver.Current.GetService<ApplicationDbContext>(); }
        }

        public static int DbScanTaskRunning = 0;

        public static int InScan = 0;

        public static int ConnectionCount;

        public const double LockAdditionalSeconds = 10.0;

        public const double LockTimeFactor = 1.5;

        public const int DbScanIntervalSeconds = 10;

        public const int DispatchLimit = 20;
    }
}