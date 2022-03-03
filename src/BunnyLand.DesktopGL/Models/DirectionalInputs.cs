using MessagePack;
using Microsoft.Xna.Framework;

namespace BunnyLand.DesktopGL.Models;

[MessagePackObject]
public readonly struct DirectionalInputs
{
    public DirectionalInputs(Vector2 accelerationDirection, Vector2 aimDirection)
    {
        AccelerationDirection = accelerationDirection;
        AimDirection = aimDirection;
    }

    [Key(0)] public Vector2 AccelerationDirection { get; }
    [Key(1)] public Vector2 AimDirection { get; }
}