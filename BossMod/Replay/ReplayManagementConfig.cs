namespace BossMod;

[ConfigDisplay(Name = "Replays", Order = 0)]
public class ReplayManagementConfig : ConfigNode
{
    [PropertyDisplay("Show replay management UI")]
    public bool ShowUI = false;

    [PropertyDisplay("Auto record replays on duty start/end or outdoor module start/end")]
    public bool AutoRecord = true;

    [PropertyDisplay("Auto record in Duty Recorder replays", depends: nameof(AutoRecord))]
    public bool AutoARR = true;

    [PropertyDisplay("Anonymize replays", tooltip: "If this option is disabled, replays will contain personally identifying information for your character and any other player you see during the recording - specifically, names and content IDs.", since: "7.5.5.4")]
    public bool Anonymize = true;

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
}
