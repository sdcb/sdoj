using System;
using System.Threading;
using System.Web.Http;

namespace SdojWeb.Controllers
{
    public class JudgeController : ApiController
    {
        [HttpGet]
        public int Wait()
        {
            var before = Environment.TickCount;
            Event.WaitOne();
            var after = Environment.TickCount;
            return after - before;
        }

        [HttpGet]
        public string Set()
        {
            Event.Set();
            return "set ok";
        }

        public static AutoResetEvent Event = new AutoResetEvent(false);
    }
}
