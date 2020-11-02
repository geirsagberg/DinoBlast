
namespace DinoBlast.Puncher
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new HolePunchServer();
            server.Run();

            /*
            // Starts 1x test client
            var testClient = new HolePunchServerTestClient();
            testClient.Run("34.107.125.252");
            */
        }
    }
}
