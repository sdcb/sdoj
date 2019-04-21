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
using System.Collections.Generic;
using SdojWeb.Manager;

namespace SdojWeb.SignalR
{
	[Authorize(Roles = SystemRoles.Judger)]
	public class JudgeHub : Hub
	{
		public JudgeHub()
		{
			_db = ApplicationDbContext.Create();
            _manager = new JudgeHubManager(_db);
		}

		// Hub API

		public async Task<bool> UpdateInLock(int solutionId, SolutionState stateId)
		{
			var slock = await _db.SolutionLocks.FindAsync(solutionId);

			// 未被锁住，或者锁住的客户端不正确，或者锁已经过期，则不允许操作。
			if (slock == null || slock.LockClientId != Guid.Parse(Context.ConnectionId) || slock.LockEndTime < DateTime.Now)
			{
				return false;
			}

			// 锁住，允许操作。
			var solution = await _db.Solutions.FindAsync(solutionId);
			solution.State = stateId;

			// 保存数据。
			_db.Entry(solution).State = EntityState.Modified;
			await _db.SaveChangesAsync();

			SolutionHub.PushChange(solution.Id, solution.State, 0, 0.0f);

			return true;
		}

        public async Task<bool> Update(ClientJudgeModel model)
        {
            SolutionLock solutionLock = await _db.SolutionLocks.FindAsync(model.SolutionId);

            // 未被锁住，或者锁住的客户端不正确，或者锁已经过期，则不允许操作。
            if (solutionLock == null)
            {
                return false;
            }
            if (solutionLock.LockClientId != Guid.Parse(Context.ConnectionId) || solutionLock.LockEndTime < DateTime.Now)
            {
                _db.Entry(solutionLock).State = EntityState.Deleted;
                await _db.SaveChangesAsync();
                return false;
            }

            // 锁住，允许操作，然后改变状态。
            Solution solution = await _db.Solutions.FindAsync(model.SolutionId);
            model.UpdateSolution(solution);

            // 删除锁，保存数据。
            _db.Entry(solution).State = EntityState.Modified;
            _db.Entry(solutionLock).State = EntityState.Deleted;
            await _db.SaveChangesAsync();

            SolutionHub.PushChange(solution.Id, solution.State, model.RunTimeMs, model.UsingMemoryMb);

            return true;
        }

        public async Task<SolutionDataModel> Lock(int solutionId)
		{
			if (!await _manager.LockInternal(solutionId, Context.ConnectionId))
            {
                return null;
            }

			var result = await _db.Solutions
				.Where(x => x.Id == solutionId)
                .ProjectTo<SolutionDataModel>()
				.FirstOrDefaultAsync();

			SolutionHub.PushChange(solutionId, SolutionState.Compiling, 0, 0.0f);
			return result;
		}

        public async Task<SolutionProcess2CodeModel> LockProcess2(int solutionId)
        {
            if (!await _manager.LockInternal(solutionId, Context.ConnectionId))
            {
                return null;
            }

            var result = await _db.Solutions
                .Where(x => x.Id == solutionId)
                .ProjectTo<SolutionProcess2CodeModel>()
                .FirstOrDefaultAsync();

            SolutionHub.PushChange(solutionId, SolutionState.Compiling, 0, 0.0f);
            return result;
        }

		public async Task<List<SolutionPushModel>> GetAll()
		{
			var models = await JudgeHubManager.GetPushModelFromDb(_db);

			return models;
		}

		public async Task<List<QuestionDataFullModel>> GetDatas(int[] dataId)
		{
			var datas = await _db.QuestionDatas
				.Where(x =>
					dataId.Contains(x.Id))
                .ProjectTo<QuestionDataFullModel>()
				.ToListAsync();

			return datas;
		}

        public async Task<QuestionProcess2FullModel> GetProcess2Code(int id)
        {
            var code = await _db.Process2JudgeCode
                .Where(x => x.QuestionId == id)
                .ProjectTo<QuestionProcess2FullModel>()
                .FirstOrDefaultAsync();

            return code;
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
			Interlocked.Increment(ref ConnectionCount);
			if (AppSettings.EnableSolutionDbScan)
			{
			    JudgeHubManager.EnsureDbScanTaskRunning();
			}
			return Task.FromResult(0);
		}

        public override Task OnDisconnected(bool stopCalled)
		{
			Interlocked.Decrement(ref ConnectionCount);
			return base.OnDisconnected(stopCalled);
		}

		protected override void Dispose(bool disposing)
		{
			_db.Dispose();
			base.Dispose(disposing);
		}

		private readonly ApplicationDbContext _db;

        private readonly JudgeHubManager _manager;

        private static int ConnectionCount;
    }
}