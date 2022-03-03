using MessagePack;

namespace BunnyLand.DesktopGL.Components;

[MessagePackObject]
public class GravityField
{
    [Key(0)]
    public float Angle { get; set; }
    [Key(1)]
    public float Gravity { get; set; }
}