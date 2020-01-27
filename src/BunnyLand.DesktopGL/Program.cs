using System;
using Microsoft.Extensions.Configuration;

namespace BunnyLand.DesktopGL
{
    public static class Program
    {
        [STAThread]
        private static void Main()
        {
            var config = new ConfigurationBuilder()
                .AddIniFile("config.ini", true, true)
                .Build();
            var gameSettings = config.GetSection("Game").Get<GameSettings>();
            using var game = new BunnyGame(gameSettings);
            game.Run();
        }
    }
}
