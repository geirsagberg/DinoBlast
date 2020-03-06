namespace BunnyLand.DesktopGL
{
    public enum PlayerKey
    {
        Left,
        Right,
        Up,
        Down,
        Fire,
        Jump,
        ToggleBrake
    }

    public static class PlayerKeyExtensions
    {
        public static bool IsDirectionInput(this PlayerKey key)
        {
            switch (key) {
                case PlayerKey.Up: return true;
                case PlayerKey.Down: return true;
                case PlayerKey.Left: return true;
                case PlayerKey.Right: return true;
                default: return false;
            }
        }
    }
}
