using AutoMapper.QueryableExtensions;
using Microsoft.AspNet.SignalR;
using SdojWeb.Models;
using SdojWeb.Models.DbModels;
using SdojWeb.Models.JudgePush;
using SdojWeb.SignalR;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SdojWeb.Manager
{
    public class JudgeHubManager
    {
        public JudgeHubManager(ApplicationDbContext db)
        {
            _db = db;
        }

        public static async Task<List<SolutionPushModel>> GetPushModelFromDb(ApplicationDbContext db)
        {
            var data = await db.Solutions
                .Where(x => x.State < SolutionState.Completed &&
                            (x.Lock == null || x.Lock.LockEndTime < DateTime.Now))
                .ProjectTo<SolutionPushModel>()
                .Take(DispatchLimit)
                .ToListAsync();
            return data;
        }

        public async Task<bool> LockInternal(int solutionId, string connectionId)
        {
            var detail = await _db.Solutions
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

            if (detail == null) return false; // 找不到符合条件的解答，返回失败。

            // 锁定的时间，按题目的时间和，乘以一个倍数，再加上额外的传输时间为准。
            var lockMilliseconds = detail.Time * LockTimeFactor + LockAdditionalSeconds * 1000;

            // 添加或者更新锁。
            var slock = detail.Lock ?? new SolutionLock { SolutionId = detail.Solution.Id };
            slock.LockClientId = Guid.Parse(connectionId);
            slock.LockEndTime = DateTime.Now.AddMilliseconds(lockMilliseconds);

            detail.Solution.State = SolutionState.Compiling;
            _db.Entry(detail.Solution).State = EntityState.Modified;

            _db.Entry(slock).State = detail.Lock == null
                ? EntityState.Added
                : EntityState.Modified;

            await _db.SaveChangesAsync();
            return true;
        }

        // DBScan Jobs

        public static void EnsureDbScanTaskRunning()
        {
            if (Interlocked.CompareExchange(ref DbScanTaskRunning, 1, 0) == 0)
            {
                var timer = new System.Timers.Timer
                {
                    AutoReset = true,
                    Enabled = true,
                    Interval = DbScanIntervalSeconds * 1000,
                };
                timer.Elapsed += DbScan;
            }
        }

        private static async void DbScan(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (Interlocked.CompareExchange(ref InScan, 1, 0) == 0)
            {
                var hub = GlobalHost.ConnectionManager.GetHubContext<JudgeHub>();
                using (var db = ApplicationDbContext.Create())
                {
                    List<SolutionPushModel> models = await GetPushModelFromDb(db);

                    foreach (var model in models)
                    {
                        hub.Clients.All
                            .Judge(model);
                    }
                }
            }

            Interlocked.CompareExchange(ref InScan, 0, 1);
        }

        public static int DbScanTaskRunning = 0;

        public static int InScan = 0;

        public const int DbScanIntervalSeconds = 10;

        public const int DispatchLimit = 20;

        public const double LockAdditionalSeconds = 20.0;

        public const double LockTimeFactor = 4.0;

        private readonly ApplicationDbContext _db;
    }
}