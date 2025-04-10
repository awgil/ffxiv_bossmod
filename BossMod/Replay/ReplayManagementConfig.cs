namespace BossMod;

public record struct ReplayMemory(string Path, bool IsOpen, DateTime PlaybackPosition);

[ConfigDisplay(Name = "Replays", Order = 0)]
public class ReplayManagementConfig : ConfigNode
{
    [PropertyDisplay("Show replay management UI")]
    public bool ShowUI = false;

    [PropertyDisplay("Auto record replays on duty start/end or outdoor module start/end")]
    public bool AutoRecord = true;

    [PropertyDisplay("Auto record in Duty Recorder replays", tooltip: "Requires auto-record to be turned on")]
    public bool AutoARR = true;

    [PropertyDisplay("Max replays to keep before removal")]
    [PropertySlider(0, 1000)]
    public int MaxReplays = 20;

    [PropertyDisplay("Record and store server packets in the replay")]
    public bool RecordServerPackets = false;

    [PropertyDisplay("Dump server packets into dalamud.log")]
    public bool DumpServerPackets = false;

    [PropertyDisplay("Ignore packets for other players when dumping to dalamud.log")]
    public bool DumpServerPacketsPlayerOnly = false;

    [PropertyDisplay("Dump client packets into dalamud.log")]
    public bool DumpClientPackets = false;

    [PropertyDisplay("Format for recorded logs")]
    public ReplayLogFormat WorldLogFormat = ReplayLogFormat.BinaryCompressed;

    [PropertyDisplay("Open previously open replays on plugin reload")]
    public bool RememberReplays;

    [PropertyDisplay("Remember playback position for previously opened replays")]
    public bool RememberReplayTimes;

    // TODO: this should not be part of the actual config! figure out where to store transient user preferences...
    public List<ReplayMemory> ReplayHistory = [];
}
