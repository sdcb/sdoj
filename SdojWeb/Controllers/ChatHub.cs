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
                var cpu = new PerformanceCounter("Processor", "% Processor Time", "_Total", true);
                var mem = new PerformanceCounter("Memory", "Available KBytes", true);
                var ps = Process.GetCurrentProcess();
                while (true)
                {
                    Clients.All.pc(cpu.NextValue(), mem.NextValue(), ps.Threads.Count, GC.GetTotalMemory(false));
                    await Task.Delay(800);
                }
            });
        }

        public void Send(string name, string message)
        {
            Clients.All.broadcastMessage(name, message);
        }
    }
}