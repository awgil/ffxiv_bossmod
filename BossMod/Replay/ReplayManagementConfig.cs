namespace BossMod;

[ConfigDisplay(Name = "Replay settings", Order = 0)]
public class ReplayManagementConfig : ConfigNode
{
    [PropertyDisplay("Show replay management UI")]
    public bool ShowUI = false;

    [PropertyDisplay("Store server packets in the replay")]
    public bool DumpServerPackets = false;

    [PropertyDisplay("Store client packets in the replay")]
    public bool DumpClientPackets = false;

    [PropertyDisplay("Format for recorded logs")]
    public ReplayLogFormat WorldLogFormat = ReplayLogFormat.BinaryCompressed;
}
