using MessagePack;

namespace BunnyLand.DesktopGL.Components
{
    [MessagePackObject]
    public class Serializable
    {
        [Key(0)]
        public int Id { get; }

        public Serializable(int id)
        {
            Id = id;
        }
    }
}
