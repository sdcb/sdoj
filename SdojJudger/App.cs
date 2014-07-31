using System;

namespace SdojJudger
{
    class App
    {
        static void Main(string[] args)
        {
            Client = new Runner();
            var task = Client.Run();

            Console.ReadKey();
        }

        public static Runner Client { get; set; }

        public static HubClient GetHubClient()
        {
            return new HubClient(Client.Server);
        }
    }
}