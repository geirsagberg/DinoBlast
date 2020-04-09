namespace BunnyLand.DesktopGL
{
    public class GameSettings
    {
        public bool DebugMode { get; set; }
        public int Width { get; set; } = 1280;
        public int Height { get; set; } = 720;
        public bool FullScreen { get; set; }
        public bool BrakeWhileJetpacking { get; set; }
        public bool VSyncEnabled { get; set; } = false;
        public bool FixedTimeStep { get; set; } = true;
        public int Fps { get; set; } = 60;
        public int Port { get; set; } = 9050;
    }
}
