using System;
using System.Net.Http;
using System.Security.Authentication;
using System.Threading;
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

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;

            Starter = new Starter();
            var task = Starter.Run();

            while (true)
            {
                try
                {
                    task.Wait();

                    var line = Console.ReadLine();
                    if (line == "exit")
                    {
                        break;
                    }
                    if (line == "restart")
                    {
                        task = Starter.Restart();
                    }
                }
                catch (AggregateException e)
                {
                    if (e.InnerException is HttpRequestException)
                    {
                        _log.Fatal("", e);
                        _log.Info("Retry in 3 seconds...");
                        Thread.Sleep(3000);
                        task = Starter.Restart();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }

        static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            _log.InfoFormat("App exited at {0}", DateTime.Now);
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is AuthenticationException)
            {
                // do nothing.
            }
            else
            {
                _log.Fatal("Fatal ERROR: \n", e.ExceptionObject as Exception);
            }
        }

        public static Starter Starter { get; private set; }

        private static ILog _log;
    }
}