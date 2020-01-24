using System;

namespace BunnyLand.DesktopGL
{
    public static class Program
    {
        [STAThread]
        private static void Main()
        {
            using var game = new BunnyGame();
            game.Run();
        }
    }
}
