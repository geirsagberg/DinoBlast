using BunnyLand.DesktopGL.Components;
using BunnyLand.DesktopGL.Serialization;
using MessagePack;

namespace BunnyLand.DesktopGL
{
    [Union(0, typeof(PlayerInput))]
    [Union(1, typeof(Serializable))]
    [Union(2, typeof(SpriteInfo))]
    [Union(3, typeof(Movable))]
    [Union(4, typeof(Level))]
    [Union(5, typeof(CollisionBody))]
    [Union(6, typeof(Damaging))]
    [Union(7, typeof(GravityField))]
    [Union(8, typeof(GravityPoint))]
    [Union(9, typeof(Health))]
    [Union(10, typeof(SerializableTransform))]
    public interface ISerializableComponent
    {
    }
}
