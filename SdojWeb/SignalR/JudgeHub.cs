using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNet.SignalR;
using SdojWeb.Infrastructure.Extensions;
using SdojWeb.Infrastructure.Identity;
using SdojWeb.Models;
using System.Threading;
using SdojWeb.Models.JudgePush;
using Timer = System.Timers.Timer;
using SdojWeb.Models.DbModels;
using Microsoft.AspNet.Identity;

namespace SdojWeb.SignalR
{
    [Microsoft.AspNet.SignalR.Authorize(Roles = SystemRoles.Judger)]
    public class JudgeHub : Hub
    {
        // Hub API

        public async Task<bool> UpdateInLock(int solutionId, SolutionState stateId)
        {
            var db = GetDbContext();
            var slock = await db.SolutionLocks.FindAsync(solutionId);

            // 未被锁住，或者锁住的客户端不正确，或者锁已经过期，则不允许操作。
            if (slock == null || slock.LockClientId != Guid.Parse(Context.ConnectionId) || slock.LockEndTime < DateTime.Now)
            {
                return false;
            }

            // 锁住，允许操作。
            var solution = await db.Solutions.FindAsync(solutionId);
            solution.State = stateId;

            // 保存数据。
            db.Entry(solution).State = EntityState.Modified;
            await db.SaveChangesAsync();
            Commit();

            SolutionHub.PushChange(solution.Id, solution.State, 0, 0.0f);

            return true;
        }

        public async Task<bool> Update(int solutionId,
            SolutionState stateId, int runTimeMs, float usingMemoryMb, 
            string compilerOutput)
        {
            var db = GetDbContext();
            var solutionLock = await db.SolutionLocks.FindAsync(solutionId);

            // 未被锁住，或者锁住的客户端不正确，或者锁已经过期，则不允许操作。
            if (solutionLock == null)
            {
                return false;
            }
            if (solutionLock.LockClientId != Guid.Parse(Context.ConnectionId) || solutionLock.LockEndTime < DateTime.Now)
            {
                db.Entry(solutionLock).State = EntityState.Deleted;
                await db.SaveChangesAsync();
                return false;
            }

            // 锁住，允许操作，然后改变状态。
            var solution = await db.Solutions.FindAsync(solutionId);
            solution.State = stateId;
            solution.RunTime = runTimeMs;
            solution.UsingMemoryMb = usingMemoryMb;
            if (solution.State == SolutionState.CompileError)
            {
                if (compilerOutput.Length > Solution.CompilerOutputLength)
                {
                    solution.CompilerOutput = compilerOutput.Substring(0, Solution.CompilerOutputLength);
                }
                else
                {
                    solution.CompilerOutput = compilerOutput;
                }
            }
            solution.Lock = null;

            // 删除锁，保存数据。
            db.Entry(solution).State = EntityState.Modified;
            db.Entry(solutionLock).State = EntityState.Deleted;
            await db.SaveChangesAsync();
            Commit();

            SolutionHub.PushChange(solution.Id, solution.State, runTimeMs, usingMemoryMb);

            return true;
        }

        public async Task<SolutionFullModel> Lock(int solutionId)
        {
            var db = GetDbContext();
            var userId = Context.User.Identity.GetUserId<int>();

            var detail = await db.Solutions
                .Where(x =>
                    //x.Question.CreateUserId == userId &&
                    solutionId == x.Id &&
                    x.State < SolutionState.Completed &&
                    (x.Lock == null || x.Lock.LockEndTime < DateTime.Now)) // 没有锁或者锁已过期，允许操作。
                .Select(x => new
                {
                    Lock = x.Lock,
                    Time = x.Question.Datas.Sum(d => d.TimeLimit), 
                    Solution = x
                }).FirstOrDefaultAsync();

            if (detail == null) return null; // 找不到符合条件的解答，返回失败。

            // 锁定的时间，按题目的时间和，乘以一个倍数，再加上额外的传输时间为准。
            var lockMilliseconds = detail.Time * LockTimeFactor + LockAdditionalSeconds * 1000;

            // 添加或者更新锁。
            var slock = detail.Lock ?? new SolutionLock { SolutionId = detail.Solution.Id };
            slock.LockClientId = Guid.Parse(Context.ConnectionId);
            slock.LockEndTime = DateTime.Now.AddMilliseconds(lockMilliseconds);

            detail.Solution.State = SolutionState.Compiling;
            db.Entry(detail.Solution).State = EntityState.Modified;

            db.Entry(slock).State = detail.Lock == null
                ? EntityState.Added
                : EntityState.Modified;

            await db.SaveChangesAsync();

            var result = await db.Solutions
                .Where(x => x.Id == solutionId)
                .Project().To<SolutionFullModel>()
                .FirstOrDefaultAsync();

            Commit();
            SolutionHub.PushChange(solutionId, SolutionState.Compiling, 0, 0.0f);
            return result;
        }

        public async Task<SolutionPushModel[]> GetAll()
        {
            var userId = Context.User.Identity.GetUserId<int>();
            var db = GetDbContext();

            var models = await db.Solutions
                .Where(x =>
                    //x.Question.CreateUserId == userId &&
                    x.State < SolutionState.Completed &&
                    (x.Lock == null || x.Lock.LockEndTime < DateTime.Now)) // 没锁或者锁已经过期，允许操作。
                .Project().To<SolutionPushModel>()
                .Take(DispatchLimit)
                .ToArrayAsync();
            Commit();

            return models;
        }

        public async Task<QuestionDataFullModel[]> GetDatas(int[] dataId)
        {
            var db = GetDbContext();
            var userId = Context.User.Identity.GetUserId<int>();

            var datas = await db.QuestionDatas
                .Where(x =>
                    //x.Question.CreateUserId == userId &&
                    dataId.Contains(x.Id))
                .Project().To<QuestionDataFullModel>()
                .ToArrayAsync();
            Commit();

            return datas;
        }

        internal static void Judge(SolutionPushModel model)
        {
            var signalr = GlobalHost.ConnectionManager.GetHubContext<JudgeHub>();

            signalr.Clients
                //.Group(model.QuestionCreateUserId.ToStringInvariant())
                .All
                .Judge(model);
        }

        // Overrides

        public override Task OnConnected()
        {
            //await Groups.Add(Context.ConnectionId, Context.User.Identity.GetUserId());
            Interlocked.Increment(ref ConnectionCount);
            if (AppSettings.EnableSolutionDbScan)
            {
                EnsureDbScanTaskRunning();
            }
            return Task.FromResult(0);
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            Interlocked.Decrement(ref ConnectionCount);
            return base.OnDisconnected(stopCalled);
        }

        public void Commit()
        {
            if (DbLazy.IsValueCreated && !Commited)
            {
                Transaction.Commit();
                Commited = true;
            }
        }

        public void Rollback()
        {
            if (DbLazy.IsValueCreated && !Commited)
            {
                Transaction.Rollback();
                Commited = true;
            }
        }

        protected override void Dispose(bool disposing)
        {
            Rollback();
            base.Dispose(disposing);
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
                            x.State < SolutionState.Completed && 
                            (x.Lock == null || x.Lock.LockEndTime < DateTime.Now))
                        .Project().To<SolutionPushModel>()
                        .Take(DispatchLimit)
                        .ToArrayAsync();

                    foreach (var model in models)
                    {
                        hub.Clients
                            //.Group(model.QuestionCreateUserId.ToStringInvariant())
                            .All
                            .Judge(model);
                    }
                }
            }

            Interlocked.CompareExchange(ref InScan, 0, 1);
        }

        // Field & Properties

        public ApplicationDbContext GetDbContext()
        {
            var firstTimeRunning = !DbLazy.IsValueCreated;
            var db = DbLazy.Value;

            if (firstTimeRunning)
            {
                Transaction = db.Database.BeginTransaction(IsolationLevel.Serializable);
            }

            return db;
        }

        public Lazy<ApplicationDbContext> DbLazy = new Lazy<ApplicationDbContext>(() =>
        {
            var db = ApplicationDbContext.Create();
            return db;
        });

        public DbContextTransaction Transaction { get; set; }

        public bool Commited { get; set; }

        public static int DbScanTaskRunning = 0;

        public static int InScan = 0;

        public static int ConnectionCount;

        public const double LockAdditionalSeconds = 20.0;

        public const double LockTimeFactor = 4.0;

        public const int DbScanIntervalSeconds = 10;

        public const int DispatchLimit = 20;
    }
}