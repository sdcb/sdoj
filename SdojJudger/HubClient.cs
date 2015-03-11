using System;
using System.Threading.Tasks;
using log4net;
using log4net.Util;
using Microsoft.AspNet.SignalR.Client;
using Newtonsoft.Json;
using SdojJudger.Models;

namespace SdojJudger
{
    public class HubClient
    {
        public HubClient(IHubProxy server)
        {
            _server = server;
            _log = LogManager.GetLogger(typeof (HubClient));
        }

        public async Task<SolutionFullModel> Lock(int solutionId)
        {
            try
            {
                var result = await _server.Invoke<SolutionFullModel>(
                    AppSettings.HubLock, solutionId);
                _log.DebugExt(() => JsonConvert.SerializeObject(result));
                return result;
            }
            catch (InvalidOperationException)
            {
                return null;
            }
            catch (JsonReaderException)
            {
                return null;
            }
        }

        public async Task<Process2LockModel> LockProcess2(int solutionId)
        {
            try
            {
                var result = await _server.Invoke<Process2LockModel>(
                    AppSettings.HubLockProcess2, solutionId);
                _log.DebugExt(() => JsonConvert.SerializeObject(result));
                return result;
            }
            catch (InvalidOperationException)
            {
                return null;
            }
            catch (JsonReaderException)
            {
                return null;
            }
        }

        public async Task<bool> Update(int solutionId, SolutionState statusId, 
            int runTimeMs, float usingMemoryMb, string compilerOutput = "")
        {
            var result = await _server.Invoke<bool>(AppSettings.HubUpdate,
                solutionId, statusId, runTimeMs, usingMemoryMb, compilerOutput);
            _log.DebugExt(() => JsonConvert.SerializeObject(result));
            _log.InfoExt(() => string.Format("Commit {0}-{1}, {2}ms,{3}MB", solutionId, statusId, runTimeMs, usingMemoryMb));
            return result;
        }

        public async Task<bool> UpdateInLock(int solutionId, SolutionState statusId)
        {
            var result = await _server.Invoke<bool>(AppSettings.HubUpdateInLock,
                solutionId, statusId);
            _log.DebugExt(() => JsonConvert.SerializeObject(result));
            return result;
        }

        public async Task<QuestionDataFullModel[]> GetDatas(int[] dataId)
        {
            var result = await _server.Invoke<QuestionDataFullModel[]>(AppSettings.HubGetDatas, 
                dataId);
            _log.DebugExt(() => JsonConvert.SerializeObject(result));
            return result;
        }

        public async Task<QuestionProcess2FullModel> GetProcess2Code(int questionId)
        {
            var result = await _server.Invoke<QuestionProcess2FullModel>(AppSettings.HubGetProcess2Code,
                questionId);
            _log.DebugExt(() => JsonConvert.SerializeObject(result));
            return result;
        }

        public async Task<SolutionPushModel[]> GetAll()
        {
            var result = await _server.Invoke<SolutionPushModel[]>(
                AppSettings.HubGetAll);
            _log.DebugExt(() => JsonConvert.SerializeObject(result));
            return result;
        }

        private readonly IHubProxy _server;

        private readonly ILog _log;
    }
}
