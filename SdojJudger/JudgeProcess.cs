using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using log4net.Util;
using Microsoft.VisualBasic.Devices;
using SdojJudger.Compiler.Infrastructure;
using SdojJudger.Database;
using SdojJudger.Models;
using SdojJudger.Runner;
using SdojJudger.SandboxDll;

namespace SdojJudger
{
    public class JudgeProcess
    {
        public JudgeProcess(SolutionPushModel spush)
        {
            _spush = spush;
            _log = LogManager.GetLogger(typeof(JudgeProcess));
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
                if (info.AvailablePhysicalMemory < _spush.FullMemoryLimitMb * 1024 * 1024)
                {
                    _log.InfoExt(
                        () =>
                            string.Format("Skipped judging {0}, because system memory running low(Req {1}/ Need {2}).",
                                _spush.Id, info.AvailablePhysicalMemory, _spush.FullMemoryLimitMb * 1024 * 1024)
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
            var asm = compiler.Compile(_sfull.Source);

            if (asm.HasErrors)
            {
                await _client.Update(_spush.Id, SolutionState.CompileError, 0, 0, asm.Output);
                return;
            }
            else
            {
                await _client.UpdateInLock(_spush.Id, SolutionState.Judging);
            }

            IEnumerable<QuestionData> datas;
            using (var db = new JudgerDbContext())
            {
                var dataIds = _sfull.QuestionDatas
                    .Select(x => x.Id).ToArray();
                datas = await db.FindDatasByIds(dataIds);
            }

            // Judging
            using (compiler)
            {
                await Judge(datas, asm);
            }
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

                _log.DebugExt("NativeDll Juding...");
                //var result = NativeDll.Judge(info);
                var result = Sandbox.Judge(info);
                _log.DebugExt("NativeDll Judged...");

                runTimeMs += result.TimeMs;
                peakMemoryMb = Math.Max(peakMemoryMb, result.MemoryMb);

                if (result.ErrorCode != 0 || !result.Succeed)
                {
                    await _client.Update(_spush.Id, SolutionState.RuntimeError, runTimeMs, peakMemoryMb); // system error
                    return;
                }
                if (result.TimeMs >= data.TimeLimit)
                {
                    await _client.Update(_spush.Id, SolutionState.TimeLimitExceed, runTimeMs, peakMemoryMb);
                    return;
                }
                if (result.MemoryMb >= data.MemoryLimitMb)
                {
                    await _client.Update(_spush.Id, SolutionState.MemoryLimitExceed, runTimeMs, peakMemoryMb);
                    return;
                }
                if (result.ExitCode != 0)
                {
                    await _client.Update(_spush.Id, SolutionState.RuntimeError, runTimeMs, peakMemoryMb); // application error
                    return;
                }

                var trimed = result.Output.TrimEnd();
                if (trimed != data.Output)
                {
                    _log.DebugExt(() => string.Format("\r\nExpected: \r\n{0} \r\nActual: \r\n{1}", data.Output, result.Output));
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
                .Select(x => new QuestionDataSummary
                {
                    Id = x.Id,
                    UpdateTicks = x.UpdateTime.Ticks
                })
                .ToArray();
            var ids = _sfull.QuestionDatas.Select(x => x.Id);

            using (var db = new JudgerDbContext())
            {
                var dbItems = await db.FindDataSummarysByIdsInOrder(ids);
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
                        UpdateTicks = serverItems.First(x => x.Id == hubData.Id).UpdateTicks
                    }).ToArray();
                    await db.DeleteAndCreateData(dbDatas);

                    _log.InfoFormat("Updated {0} datas from server.", hubDatas.Length);
                    if (hubDatas.Length != except.Length)
                    {
                        _log.Warn("Server returned less data than excepted.");
                    }
                }
            }
        }

        private readonly SolutionPushModel _spush;

        private SolutionFullModel _sfull;

        private readonly ILog _log;

        private readonly HubClient _client;
    }
}
