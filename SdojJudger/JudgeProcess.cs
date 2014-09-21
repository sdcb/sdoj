using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using JobManagement;
using log4net;
using SdojJudger.Compiler;
using SdojJudger.Database;
using SdojJudger.Models;

namespace SdojJudger
{
    public class JudgeProcess
    {
        public JudgeProcess(SolutionPushModel spush)
        {
            _spush = spush;
            _log = LogManager.GetLogger(typeof (JudgeProcess));
            _client = App.Runner.GetClient();
        }

        public async Task ExecuteAsync()
        {
            // 获取并锁定解答的详情。
            _sfull = await _client.Lock(_spush.Id);
            if (_sfull == null) return;
            await UpdateQuestionData();

            var compiler = CompilerProvider.GetCompiler(_spush);
            if (compiler == null)
            {
                await _client.Update(_spush.Id, SolutionState.CompileError, 0, 0.0f);
                return;
            }
            var asm = compiler.Compile(_sfull.Source);

            if (asm.Errors.HasErrors)
            {
                await _client.Update(_spush.Id, SolutionState.CompileError, 0, 0);
                return;
            }

            var db = JudgerDbContext.Create();
            var dataIds = _sfull.QuestionDatas
                .Select(x => x.Id).ToArray();
            var datas = await db.Datas
                .Where(x => dataIds.Contains(x.Id))
                .ToArrayAsync();

            var runTime = new TimeSpan();
            float peakMemory = 0;

            var job = new JobObject();
            job.Limits.ActiveProcessLimit = 2;
            job.Limits.PerProcessUserTimeLimit = TimeSpan.FromMilliseconds(datas.Sum(x => x.TimeLimit));
            job.Limits.ProcessMemoryLimit = (IntPtr)((double)_spush.FullMemoryLimitMb*1024*1024);

            foreach (var data in datas)
            {
                var pi = new ProcessStartInfo(asm.PathToAssembly, "")
                {
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                };
                var ps = job.CreateProcessSecured(pi);

                ps.StandardInput.Write(data.Input);
                var result = ps.StandardOutput.ReadToEnd();

                // 时间与内存。
                runTime += ps.TotalProcessorTime;
                var memory = (long)job.PeakProcessMemoryUsed/1024f/1024f;
                var runTimeMs = (int)runTime.TotalMilliseconds;
                peakMemory = Math.Max(peakMemory, memory);

                if (memory > data.MemoryLimitMb)
                {
                    job.TerminateAllProcesses(0);
                    await _client.Update(_spush.Id, SolutionState.MemoryLimitExceed, runTimeMs, memory);
                    return;
                }
                if (ps.TotalProcessorTime.TotalMilliseconds > data.TimeLimit)
                {
                    job.TerminateAllProcesses(0);
                    await _client.Update(_spush.Id, SolutionState.TimeLimitExceed, runTimeMs, memory);
                    return;
                }

                // 正确性。
                if (result != data.Output)
                {
                    job.TerminateAllProcesses(0);
                    await _client.Update(_spush.Id, SolutionState.WrongAnswer, 0, 0);
                    return;
                }
            }
            job.TerminateAllProcesses(0);
            await _client.Update(_spush.Id, SolutionState.Accepted, (int)runTime.TotalMilliseconds, peakMemory);
        }

        private async Task UpdateQuestionData()
        {
            // 与本地数据库对比时间戳。
            var serverItems = _sfull.QuestionDatas
                .OrderBy(x => x.Id)
                .Select(x => new { x.Id, x.UpdateTime.Ticks })
                .ToArray();
            var ids = _sfull.QuestionDatas.Select(x => x.Id);

            var db = JudgerDbContext.Create();
            var dbItems = await db.Datas
                .Where(x => ids.Contains(x.Id))
                .Select(x => new { x.Id, Ticks = x.UpdateTicks })
                .OrderBy(x => x.Id)
                .ToArrayAsync();
            var except = serverItems.Except(dbItems).ToArray();

            var datas = new List<QuestionData>(except.Length);

            // 将旧数据或者没有的数据更新。
            if (except.Length > 0)
            {
                _log.InfoFormat("Found dirty datas, try get {0} data from server.", except.Length);
                var hubDatas = await _client.GetDatas(except.Select(x => x.Id).ToArray());
                var dbDatas = hubDatas.Select(hubData => new QuestionData
                {
                    Id = hubData.Id,
                    Input = hubData.Input,
                    Output = hubData.Output,
                    MemoryLimitMb = hubData.MemoryLimitMb,
                    TimeLimit = hubData.TimeLimit,
                    UpdateTicks = serverItems.First(x => x.Id == hubData.Id).Ticks
                }).ToArray();
                db.Datas.AddOrUpdate(dbDatas);
                await db.SaveChangesAsync();

                _log.InfoFormat("Updated {0} datas from server.", hubDatas.Length);
                if (datas.Count != except.Length)
                {
                    _log.Warn("Server returned less data than excepted.");
                }
            }
        }

        private readonly SolutionPushModel _spush;

        private SolutionFullModel _sfull;

        private readonly ILog _log;

        private readonly HubClient _client;
    }
}
