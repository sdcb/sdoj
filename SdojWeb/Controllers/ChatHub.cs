using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;

namespace SdojWeb.Controllers
{
    public class ChatHub : Hub
    {
        public static bool Started = false;

        public ChatHub()
        {
            if (Started) return;
            Started = true;

            Task.Run(async() =>
            {
                GC.SuppressFinalize(this);
                var ps = Process.GetCurrentProcess();
                while (true)
                {
                    Clients.All.pc(
                        ps.TotalProcessorTime.ToString("g"), 
                        (DateTime.Now - ps.StartTime).ToString("g"), 
                        ps.Threads.Count, 
                        GC.GetTotalMemory(false));
                    await Task.Delay(1000);
                }
            });
        }

        public void Send(string name, string message)
        {
            Clients.All.broadcastMessage(name, message);
        }
    }
}