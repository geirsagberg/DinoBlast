using System;

namespace BunnyLand.DesktopGL.Models;

public class SharedContext
{
    public bool IsClient { get; set; }
    public bool IsPaused { get; set; } = true;
    public int FrameCounter { get; set; }
    public int FrameOffset { get; set; } = 2;
    public TimeSpan ResumeAtGameTime { get; set; }
    public bool IsSyncing { get; set; }
    public int? MyPeerId { get; set; }
    public bool ShowDebugInfo { get; set; }
}