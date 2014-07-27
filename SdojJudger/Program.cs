using System;

namespace SdojJudger
{
    class Program
    {
        static void Main(string[] args)
        {
            Client = new SdojClient();
            var task = Client.Run();

            Console.ReadKey();
        }

        public static SdojClient Client { get; set; }
    }
}