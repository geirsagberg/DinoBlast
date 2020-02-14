using LanguageExt;
using Microsoft.Xna.Framework.Input;

namespace BunnyLand.DesktopGL
{
    public interface IButtonMap
    {
        Option<PlayerKey> GetKey(Buttons buttons);
    }
}
