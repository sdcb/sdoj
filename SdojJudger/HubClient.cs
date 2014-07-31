using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;
using SdojJudger.Models;

namespace SdojJudger
{
    public class HubClient
    {
        public HubClient(IHubProxy server)
        {
            Server = server;
        }

        public async Task<ClientSolutionFullModel> Lock(int solutionId)
        {
            return await Server.Invoke<ClientSolutionFullModel>(
                AppSettings.HubLock, solutionId);
        }

        public async Task<bool> Update(int solutionId,
            SolutionStatus statusId, int? runTimeMs, float? usingMemoryMb)
        {
            return await Server.Invoke<bool>(AppSettings.HubUpdate,
                solutionId, statusId, runTimeMs, usingMemoryMb);
        }

        public async Task<bool> UpdateInLock(int solutionId, SolutionStatus statusId)
        {
            return await Server.Invoke<bool>(AppSettings.HubUpdateInLock,
                solutionId, statusId);
        }

        public async Task<ClientSolutionPushModel[]> GetAll()
        {
            return await Server.Invoke<ClientSolutionPushModel[]>(
                AppSettings.HubGetAll);
        }

        private IHubProxy Server { get; set; }
    }
}
