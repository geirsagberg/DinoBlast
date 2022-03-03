using System.Collections.Generic;
using System.Linq;
using BunnyLand.DesktopGL.Enums;
using BunnyLand.DesktopGL.Extensions;
using BunnyLand.DesktopGL.Models;
using MessagePack;

namespace BunnyLand.DesktopGL.Components;

[MessagePackObject]
public class PlayerInput
{
    public const int InitialFrameBuffer = 3;

    [IgnoreMember] public int CurrentFrame { get; set; }

    [Key(0)]
    public Dictionary<int, Dictionary<PlayerKey, KeyState>> PlayerKeysByFrame { get; set; } =
        new Dictionary<int, Dictionary<PlayerKey, KeyState>>(InitialFrameBuffer);

    [Key(1)] public Dictionary<int, DirectionalInputs> DirectionalInputsByFrame { get; set; } = new Dictionary<int, DirectionalInputs>(InitialFrameBuffer);

    [IgnoreMember]
    public Dictionary<PlayerKey, KeyState> PlayerKeys => PlayerKeysByFrame.TryGetValue(CurrentFrame, out var keys) ? keys : DefaultPlayerKeys();

    [IgnoreMember]
    public DirectionalInputs DirectionalInputs => DirectionalInputsByFrame.TryGetValue(CurrentFrame, out var inputs)
        ? inputs
        : new DirectionalInputs();

    public static Dictionary<PlayerKey, KeyState> DefaultPlayerKeys()
    {
        return EnumHelper.GetValues<PlayerKey>().ToDictionary(k => k, _ => new KeyState());
    }

    public bool IsUpToDate() => PlayerKeysByFrame.Keys.DefaultIfEmpty(0).Max() >= CurrentFrame;
}