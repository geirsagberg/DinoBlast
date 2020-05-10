using MessagePack;
using Microsoft.Xna.Framework;

namespace BunnyLand.DesktopGL.Models
{
    [MessagePackObject]
    public class DirectionalInputs
    {
        [Key(0)]
        public Vector2 AccelerationDirection { get; set; }
        [Key(1)]
        public Vector2 AimDirection { get; set; }
    }
}
