using MessagePack;

namespace BunnyLand.DesktopGL.Models;

[MessagePackObject]
[Union(0, typeof(Circle))]
[Union(1, typeof(Rectangle))]
public abstract class SomeShape
{
}

[MessagePackObject]
public class Circle : SomeShape
{
    [Key(0)] public float Radius { get; }

    public Circle(float radius)
    {
        Radius = radius;
    }
}

[MessagePackObject]
public class Rectangle : SomeShape
{
    [Key(0)] public float Width { get; }

    [Key(1)] public float Height { get; }

    public Rectangle(float width, float height)
    {
        Width = width;
        Height = height;
    }
}