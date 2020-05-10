using System.Collections.Generic;
using System.Linq;
using BunnyLand.DesktopGL.Enums;
using BunnyLand.DesktopGL.Extensions;
using BunnyLand.DesktopGL.Models;
using MessagePack;

namespace BunnyLand.DesktopGL.Components
{
    [MessagePackObject]
    public class PlayerInput
    {
        [Key(0)]
        public Dictionary<PlayerKey, KeyState> PlayerKeys { get; set; } =
            EnumHelper.GetValues<PlayerKey>().ToDictionary(k => k, _ => new KeyState());

        [Key(1)] public DirectionalInputs DirectionalInputs { get; set; } = new DirectionalInputs();
    }
}
