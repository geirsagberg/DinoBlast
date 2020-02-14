using LanguageExt;
using Microsoft.Xna.Framework.Input;

namespace BunnyLand.DesktopGL
{
    public class ButtonMap : IButtonMap
    {
        public Option<PlayerKey> GetKey(Buttons buttons) => buttons switch {
            Buttons.DPadUp => PlayerKey.Up,
            Buttons.DPadDown => PlayerKey.Down,
            Buttons.DPadLeft => PlayerKey.Left,
            Buttons.DPadRight => PlayerKey.Right,
            Buttons.A => PlayerKey.Jump,
            Buttons.B => PlayerKey.ToggleBrake,
            Buttons.X => PlayerKey.Fire,
            _ => Option<PlayerKey>.None
        };
    }
}
