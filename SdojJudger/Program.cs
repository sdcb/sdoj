namespace SdojJudger
{
    class Program
    {
        static void Main(string[] args)
        {
            Client = new SdojClient();
            Client.Run().Wait();
        }

        public static SdojClient Client { get; set; }
    }
}