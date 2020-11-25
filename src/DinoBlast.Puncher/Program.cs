using System.Linq;

namespace DinoBlast.Puncher
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (args.Contains("--client")) {
                // Starts 1x test client
                var testClient = new HolePunchServerTestClient();
                testClient.Run("35.198.111.159");
                // testClient.Run("localhost");
            } else {
                var server = new HolePunchServer();
                server.Run();
            }
        }
    }
}
