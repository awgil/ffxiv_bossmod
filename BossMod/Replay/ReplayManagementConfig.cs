namespace BossMod;

[ConfigDisplay(Name = "Replays", Order = 0)]
public class ReplayManagementConfig : ConfigNode
{
    [PropertyDisplay("Show replay management UI")]
    public bool ShowUI = false;

    [PropertyDisplay("Auto record replays on duty start/end")]
    public bool AutoRecord = true;

    [PropertyDisplay("Max replays to keep before removal")]
    [PropertySlider(0, 1000)]
    public int MaxReplays = 20;

    [PropertyDisplay("Record and store server packets in the replay")]
    public bool RecordServerPackets = false;

    [PropertyDisplay("Dump server packets into dalamud.log")]
    public bool DumpServerPackets = false;

    //[PropertyDisplay("Store client packets in the replay")]
    //public bool DumpClientPackets = false;

    [PropertyDisplay("Format for recorded logs")]
    public ReplayLogFormat WorldLogFormat = ReplayLogFormat.BinaryCompressed;
}
