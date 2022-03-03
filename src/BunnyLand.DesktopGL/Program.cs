using System;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Configuration;
[assembly: InternalsVisibleTo("BunnyLand.Tests")]

namespace BunnyLand.DesktopGL;

public static class Program
{
    [STAThread]
    private static void Main()
    {
        var config = new ConfigurationBuilder()
            .AddIniFile("defaultConfig.ini", false)
            .AddIniFile("config.ini", true, true)
            .Build();
        var gameSettings = config.GetSection("Game").Get<GameSettings>();

        using var game = new BunnyGame(gameSettings);

        game.Run();
    }
}