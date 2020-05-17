using MessagePack;

namespace BunnyLand.DesktopGL.Enums
{
    [MessagePackObject]
    public readonly struct KeyState
    {
        public KeyState(bool pressed, bool changed)
        {
            Pressed = pressed;
            Changed = changed;
        }

        [Key(0)] public bool Pressed { get; }
        [Key(1)] public bool Changed { get; }

        [IgnoreMember] public bool JustPressed => Pressed & Changed;
        [IgnoreMember] public bool JustReleased => Changed & !Pressed;
    }
}
