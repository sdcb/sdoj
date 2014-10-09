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

            try
            {
                Starter = new Starter();
                var task = Starter.Run();
                task.Wait();

                while (true)
                {
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
            }
            catch (Exception e)
            {
                _log.Fatal("", e);
            }
            finally
            {
                _log.InfoFormat("App exited at {0}", DateTime.Now);
            }
        }

        public static Starter Starter { get; set; }

        private static ILog _log;
    }
}