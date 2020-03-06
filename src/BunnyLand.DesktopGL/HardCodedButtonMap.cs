using LanguageExt;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Input;

namespace BunnyLand.DesktopGL
{
    public class HardCodedButtonMap : IButtonMap
    {
        public Option<PlayerKey> GetKey(Buttons buttons) => buttons switch {
            Buttons.DPadUp => PlayerKey.Up,
            Buttons.DPadDown => PlayerKey.Down,
            Buttons.DPadLeft => PlayerKey.Left,
            Buttons.DPadRight => PlayerKey.Right,
            Buttons.A => PlayerKey.Jump,
            Buttons.B => PlayerKey.ToggleBrake,
            Buttons.RightTrigger => PlayerKey.Fire,
            _ => Option<PlayerKey>.None
        };

        public Option<PlayerKey> GetKey(Keys keys) => keys switch {
            Keys.A => PlayerKey.Left,
            Keys.D => PlayerKey.Right,
            Keys.W => PlayerKey.Up,
            Keys.S => PlayerKey.Down,
            Keys.Space => PlayerKey.Jump,
            Keys.LeftControl => PlayerKey.Fire,
            Keys.LeftShift => PlayerKey.ToggleBrake,
            _ => Option<PlayerKey>.None
        };

        public Option<PlayerKey> GetKey(MouseButton buttons) => buttons switch {
            MouseButton.Left => PlayerKey.Fire,
            _ => Option<PlayerKey>.None
        };
    }
}
