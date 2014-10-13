using System;
using log4net;
using log4net.Config;

namespace SdojJudger
{
    class App
    {
        static void Main(string[] args)
        {
            XmlConfigurator.Configure();
            _log = LogManager.GetLogger(typeof(App));
            _log.InfoFormat("App started at {0}", DateTime.Now);

            while (true)
            {
                Starter = new Starter();
                try
                {
                    var task = Starter.Run();
                    task.Wait();

                    var line = Console.ReadLine();
                    if (line == "exit")
                    {
                        break;
                    }
                    if (line == "restart")
                    {
                        task = Starter.Restart();
                        task.Wait();
                    }
                }
                catch (Exception e)
                {
                    _log.Fatal("", e);
                    var task = Starter.Restart();
                    task.Wait();
                }
            }

            _log.InfoFormat("App exited at {0}", DateTime.Now);
        }

        public static Starter Starter { get; set; }

        private static ILog _log;
    }
}