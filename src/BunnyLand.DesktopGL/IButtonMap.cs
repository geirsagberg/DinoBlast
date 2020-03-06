using LanguageExt;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Input;

namespace BunnyLand.DesktopGL
{
    public interface IButtonMap
    {
        Option<PlayerKey> GetKey(Buttons buttons);
        Option<PlayerKey> GetKey(Keys keys);
        Option<PlayerKey> GetKey(MouseButton buttons);
    }
}
