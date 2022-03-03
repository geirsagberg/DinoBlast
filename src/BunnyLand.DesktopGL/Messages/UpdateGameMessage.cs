using BunnyLand.DesktopGL.Serialization;

namespace BunnyLand.DesktopGL.Messages;

internal class UpdateGameMessage : INotification
{
    public SerializableComponents Components { get; }

    public UpdateGameMessage(SerializableComponents components)
    {
        Components = components;
    }
}