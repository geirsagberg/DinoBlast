using LanguageExt;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace BunnyLand.DesktopGL
{
    public interface IKeyMap
    {
        Option<(PlayerIndex index, PlayerKey key)> GetKey(Keys keys);
    }
}
