using System;

namespace SdojJudger
{
    class App
    {
        static void Main(string[] args)
        {
            Runner = new Runner();
            var task = Runner.Run();

            Console.ReadKey();
        }

        public static Runner Runner { get; set; }
    }
}