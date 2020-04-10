using BunnyLand.DesktopGL.Enums;
using BunnyLand.DesktopGL.Services;

namespace BunnyLand.DesktopGL.Messages
{
    internal class SetGlobalVariableMessage : INotification
    {
        public GlobalVariable Name { get; }
        public float Value { get; }

        public SetGlobalVariableMessage(GlobalVariable name, float value)
        {
            Name = name;
            Value = value;
        }
    }
}
