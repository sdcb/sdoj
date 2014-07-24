using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace SdojWeb.SignalR
{
    [HubName("pcHub")]
    public class PerformanceCounterHub : Hub
    {
        public static int Started = 0;

        public override Task OnConnected()
        {
            if (Interlocked.CompareExchange(ref Started, 1, 0) == 0)
            {
                var ps = Process.GetCurrentProcess();
                var timer = new System.Timers.Timer(1000.0) {Enabled = true, AutoReset = true};

                timer.Elapsed += (o, e) => 
                    Clients.All.pc(
                        ps.TotalProcessorTime.ToString(@"hh\:mm\:ss"),
                        (DateTime.Now - ps.StartTime).ToString(@"hh\:mm\:ss"),
                        ps.Threads.Count,
                        GC.GetTotalMemory(false));
            }

            return base.OnConnected();
        }
    }
}