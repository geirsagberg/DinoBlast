namespace BunnyLand.DesktopGL
{
    public enum PlayerKey : byte
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
        public static bool IsDirectionInput(this PlayerKey key) => key switch {
            PlayerKey.Up => true,
            PlayerKey.Down => true,
            PlayerKey.Left => true,
            PlayerKey.Right => true,
            _ => false
        };
    }
}
