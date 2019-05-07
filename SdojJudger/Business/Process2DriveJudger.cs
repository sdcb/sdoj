using log4net;
using log4net.Util;
using SdojJudger.Compiler.Infrastructure;
using SdojJudger.Database;
using SdojJudger.Models;
using System.Threading.Tasks;
using System;
using SdojJudger.SandboxDll;
using SdojJudger.Runner;
using System.Text;

namespace SdojJudger.Business
{
    public class Process2DriveJudger : JudgeDriver
    {
        public Process2DriveJudger(SolutionPushModel spush) : base()
        {
            _log = LogManager.GetLogger(GetType());
            _client = App.Starter.GetClient();
            _spush = spush;
        }

        public override async Task ExecuteAsync()
        {
            _lockM = await _client.LockProcess2(_spush.Id);
            if (_lockM == null)
            {
                _log.InfoExt($"Failed to lock {_spush.Id}, move next.");
                return;
            }
            await UpdateQuestionProcess2Code();

            // _fullM 里面包含 评测进程 的源代码，因此它是 compiler1；
            // _lockM 里面包含 待评测进程 的源代码，因此它是 compiler2。
            using (var judgerCompiler = CompilerProvider.GetCompiler(_fullM.Language))
            using (var judgedCompiler = CompilerProvider.GetCompiler(_spush.Language))
            {
                CompileResult res1 = judgerCompiler.Compile(_fullM.Code);
                if (res1.HasErrors)
                {
                    await _client.Update(ClientJudgeModel.CreateCompileError(_spush.Id, res1.Output)); // 评测代码 编译失败，属系统错误
                    return;
                }

                CompileResult res2 = judgedCompiler.Compile(_lockM.Source);
                if (res2.HasErrors)
                {
                    await _client.Update(ClientJudgeModel.CreateCompileError(_spush.Id, res2.Output)); // 待评测代码 编译失败，属用户错误
                    return;
                }

                await _client.UpdateInLock(_spush.Id, SolutionState.Judging);

                await Judge(res1, res2, judgedCompiler.GetEncoding());
            }
        }

        private async Task Judge(CompileResult res1, CompileResult res2, Encoding languageEncoding)
        {
            int peakRunTimeMs = 0;
            float peakMemoryMb = 0;

            var info = new Process2JudgeInfo
            {
                MemoryLimitMb = _fullM.MemoryLimitMb, 
                TimeLimitMs = _fullM.TimeLimitMs, 
                Path1 = res1.PathToAssembly, 
                Path2 = res2.PathToAssembly
            };

            for (short times = 0; times < _fullM.RunTimes; ++times)
            {
                _log.DebugExt($"NativeDll Juding {times + 1} of {_fullM.RunTimes}...");
                Process2JudgeResult result = Sandbox.Process2Judge(info, languageEncoding);
                _log.DebugExt("NativeDll Judged.");

                peakRunTimeMs = Math.Max(peakRunTimeMs, result.TimeMs);
                peakMemoryMb = Math.Max(peakMemoryMb, result.MemoryMb);

                if (!result.Process1Ok)
                {
                    await _client.Update(ClientJudgeModel.Create(_spush.Id, SolutionState.RuntimeError, peakRunTimeMs, peakMemoryMb));
                    return;
                }
                if (!result.Process2Ok)
                {
                    await _client.Update(ClientJudgeModel.Create(_spush.Id, SolutionState.RuntimeError, peakRunTimeMs, peakMemoryMb));
                    return;
                }

                if (!result.Accepted)
                {
                    await _client.Update(ClientJudgeModel.CreateProcess2WrongAnswer(_spush.Id, peakRunTimeMs, peakMemoryMb, result.P1Result.Output, result.P2Result.Output));
                    return;
                }
            }

            await _client.Update(ClientJudgeModel.Create(_spush.Id, SolutionState.Accepted, peakRunTimeMs, peakMemoryMb));
        }

        private async Task UpdateQuestionProcess2Code()
        {
            var questionId = _spush.QuestionId;

            using (var db = new JudgerDbContext())
            {
                var contains = await db.ContainsProcess2Code(questionId);

                bool needUpdate = true;
                if (contains)
                {
                    var item = (DbHashModel)_lockM.JudgeCode;
                    var dbItem = await db.FindProcess2HashById(questionId);

                    if (dbItem.UpdateTicks == item.UpdateTicks)
                    {
                        needUpdate = false;
                    }
                }

                if (needUpdate)
                {
                    // 数据过期，需要执行更新操作。
                    if (contains)
                        _log.Info("P2Code expired, updating from server.");
                    else
                        _log.Info("P2Code not exist, fetching from server.");

                    _fullM = await _client.GetProcess2Code(questionId);
                    var toDbItem = (QuestionP2Code)_fullM;
                    await db.DeleteAndCreateProcess2Code(toDbItem);
                }
                else
                {
                    var dbItem = await db.FindProcess2CodeById(questionId);
                    _fullM = (QuestionProcess2FullModel)dbItem;
                }
            }
        }

        private readonly ILog _log;
        private readonly HubClient _client;
        private readonly SolutionPushModel _spush;

        private Process2LockModel _lockM;
        private QuestionProcess2FullModel _fullM;
    }
}