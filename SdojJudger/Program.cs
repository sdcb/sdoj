namespace SdojJudger
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new SdojClient();
            client.Run().Wait();
        }
    }
}