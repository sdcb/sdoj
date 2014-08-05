using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Microsoft.CSharp;
using Microsoft.VisualBasic;
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
            await ExecuteInternal();
            if (Interlocked.CompareExchange(ref _run, 1, 0) != 0)
                return;

            await ExecuteInternal();

            Interlocked.CompareExchange(ref _run, 0, 1);
        }

        private async Task ExecuteInternal()
        {
            // 获取并锁定解答的详情。
            _sfull = await _client.Lock(_spush.Id);
            if (_sfull == null) return;
            await UpdateQuestionData();

            if (_spush.Language == Languages.CSharp)
            {
                var csc = new CSharpCodeProvider();
                var options = new CompilerParameters { GenerateExecutable = true };
                var asm = csc.CompileAssemblyFromSource(options, _sfull.Source);

                if (asm.Errors.HasErrors)
                {
                    await _client.Update(_spush.Id, SolutionStatus.CompileError, 0, 0);
                    return;
                }

                var db = JudgerDbContext.Create();
                var dataIds = _sfull.QuestionDatas
                    .Select(x => x.Id).ToArray();
                var datas = await db.Datas
                    .Where(x => dataIds.Contains(x.Id))
                    .ToArrayAsync();

                foreach (var data in datas)
                {
                    var ps = Process.Start(new ProcessStartInfo
                    {
                        FileName = asm.PathToAssembly,
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true,
                    });
                    await ps.StandardInput.WriteAsync(data.Input);
                    var result = await ps.StandardOutput.ReadToEndAsync();

                    if (result != data.Output)
                    {
                        await _client.Update(_spush.Id, SolutionStatus.WrongAnswer, 0, 0);
                        return;
                    }
                }
                await _client.Update(_spush.Id, SolutionStatus.Accepted, 768, 34.5f);
            }
            else if (_spush.Language == Languages.Vb)
            {
                var vbc = new VBCodeProvider();
                var options = new CompilerParameters();
                var asm = vbc.CompileAssemblyFromSource(options, _sfull.Source);
            }
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
                datas.AddRange(hubDatas.Select(hubData => new QuestionData
                {
                    Id = hubData.Id,
                    Input = hubData.Input,
                    Output = hubData.Output,
                    MemoryLimitMb = hubData.MemoryLimitMb,
                    TimeLimit = hubData.TimeLimit,
                    UpdateTicks = serverItems.First(x => x.Id == hubData.Id).Ticks
                }));
                db.Datas.AddRange(datas);
                await db.SaveChangesAsync();

                _log.InfoFormat("Updated {0} datas from server.", hubDatas.Length);
                if (datas.Count != datas.Capacity)
                {
                    _log.Warn("Server returned less data than excepted.");
                }
            }
        }

        private readonly SolutionPushModel _spush;

        private SolutionFullModel _sfull;

        private readonly ILog _log;

        private readonly HubClient _client;

        private static int _run;
    }
}
