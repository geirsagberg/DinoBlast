using System;

namespace BunnyLand
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (BunnyGame game = new BunnyGame())
            {
                game.Run();
            }
        }
    }
}

