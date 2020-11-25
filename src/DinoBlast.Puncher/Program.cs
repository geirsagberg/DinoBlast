
using System.Linq;

namespace DinoBlast.Puncher
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Contains("--client")) {
                // Starts 1x test client
                var testClient = new HolePunchServerTestClient();
                //testClient.Run("34.107.86.191");
                testClient.Run("localhost");
            } else {
                var server = new HolePunchServer();
                server.Run();
            }

        }
    }
}
