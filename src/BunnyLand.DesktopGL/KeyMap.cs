using LanguageExt;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace BunnyLand.DesktopGL
{
    public interface IKeyMap
    {
        Option<(PlayerIndex index, PlayerKey key)> GetKey(Keys keys);
    }

    public class KeyMap : IKeyMap
    {
        public Option<(PlayerIndex index, PlayerKey key)> GetKey(Keys keys) => keys switch {
            Keys.A => (PlayerIndex.One, PlayerKey.Left),
            Keys.D => (PlayerIndex.One, PlayerKey.Right),
            Keys.W => (PlayerIndex.One, PlayerKey.Up),
            Keys.S => (PlayerIndex.One, PlayerKey.Down),
            Keys.Space => (PlayerIndex.One, PlayerKey.Jump),
            Keys.LeftControl => (PlayerIndex.One, PlayerKey.Fire),
            _ => Option<(PlayerIndex, PlayerKey)>.None
        };
    }
}
