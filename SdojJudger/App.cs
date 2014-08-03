using System;
using log4net.Config;

namespace SdojJudger
{
    class App
    {
        static void Main(string[] args)
        {
            XmlConfigurator.Configure();

            Runner = new Runner();
            var task = Runner.Run();

            Console.ReadKey();
        }

        public static Runner Runner { get; set; }
    }
}