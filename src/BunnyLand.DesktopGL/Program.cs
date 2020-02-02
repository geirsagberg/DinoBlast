using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Nez.Console;

namespace BunnyLand.DesktopGL
{
    public static class Program
    {
        [STAThread]
        private static void Main()
        {
            DebugConsole.ConsoleKey = Keys.F1;
            DebugConsole.RenderScale = 2;

            var config = new ConfigurationBuilder()
                .AddIniFile("config.ini", false, true)
                .Build();
            var gameSettings = config.GetSection("Game").Get<GameSettings>();

            using var game = new BunnyGame(gameSettings);

            game.Run();
        }
    }
}
