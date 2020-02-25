using System;

namespace BunnyLand.DesktopGL.Enums
{
    [Flags]
    public enum KeyState
    {
        None = 0,
        Pressed = 1,
        Changed = 0b10,
        JustPressed = Pressed | Changed,
        JustReleased = None | Changed
    }
}
