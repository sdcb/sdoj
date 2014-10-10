using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using log4net.Util;
using Microsoft.VisualBasic.Devices;
using SdojJudger.Compiler;
using SdojJudger.Database;
using SdojJudger.Models;
using SdojJudger.Runner;

namespace SdojJudger
{
    public class JudgeProcess
    {
        public JudgeProcess(SolutionPushModel spush)
        {
            _spush = spush;
            _log = LogManager.GetLogger(typeof (JudgeProcess));
            _client = App.Starter.GetClient();
        }

        public async Task ExecuteAsync()
        {
            // 获取并锁定解答的详情。
            if (!CompilerProvider.IsLanguageAvailable(_spush))
            {
                _log.InfoExt(() => string.Format("Skipped compiling {0}, Because {1} compiler is not availabel.", 
                        _spush.Id, _spush.Language));
                return;
            }
            {
                var info = new ComputerInfo();
                if (info.AvailablePhysicalMemory < _spush.FullMemoryLimitMb*1024*1024)
                {
                    _log.InfoExt(
                        () =>
                            string.Format("Skipped judging {0}, because system memory running low(Req {1}/ Need {2}).",
                                _spush.Id, info.AvailablePhysicalMemory, _spush.FullMemoryLimitMb*1024*1024)
                        );
                    return;
                }
            }

            _sfull = await _client.Lock(_spush.Id);
            if (_sfull == null)
            {
                _log.InfoExt(() => "Failed to lock " + _spush.Id + ", move next.");
                return;
            }
            await UpdateQuestionData();

            var compiler = CompilerProvider.GetCompiler(_spush);
            if (compiler == null)
            {
                await _client.Update(_spush.Id, SolutionState.CompileError, 0, 0.0f);
                return;
            }
            var asm = compiler.Compile(_sfull.Source);

            if (asm.HasErrors)
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

            // Judging
            await Judge(datas, asm);
        }

        private async Task Judge(IEnumerable<QuestionData> datas, CompileResult asm)
        {
            int runTimeMs = 0;
            float peakMemoryMb = 0;

            foreach (var data in datas)
            {
                var info = new JudgeInfo
                {
                    Input = data.Input,
                    MemoryLimitMb = data.MemoryLimitMb,
                    Path = asm.PathToAssembly,
                    TimeLimitMs = data.TimeLimit,
                };

                var result = NativeDll.Judge(info);

                runTimeMs += result.TimeMs;
                peakMemoryMb = Math.Max(peakMemoryMb, result.MemoryMb);

                if (result.ErrorCode != 0 || !result.Succeed)
                {
                    await _client.Update(_spush.Id, SolutionState.RuntimeError, runTimeMs, peakMemoryMb); // system error
                }
                else if (result.ExitCode != 0)
                {
                    await _client.Update(_spush.Id, SolutionState.RuntimeError, runTimeMs, peakMemoryMb); // application error
                }
                else if (result.TimeMs > data.TimeLimit)
                {
                    await _client.Update(_spush.Id, SolutionState.TimeLimitExceed, runTimeMs, peakMemoryMb);
                }
                else if (result.MemoryMb > data.MemoryLimitMb)
                {
                    await _client.Update(_spush.Id, SolutionState.MemoryLimitExceed, runTimeMs, peakMemoryMb);
                }

                var trimed = result.Output.TrimEnd();
                if (trimed != data.Output)
                {
                    await _client.Update(_spush.Id, SolutionState.WrongAnswer, runTimeMs, peakMemoryMb);
                    return;
                }
            }
            await _client.Update(_spush.Id, SolutionState.Accepted, runTimeMs, peakMemoryMb);
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
                if (hubDatas.Length != except.Length)
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
