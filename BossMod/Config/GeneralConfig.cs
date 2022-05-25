namespace BossMod
{
    [ConfigDisplay(Name = "General settings", Order = 0)]
    public class GeneralConfig : ConfigNode
    {
        [PropertyDisplay("Dump world state events")]
        public bool DumpWorldStateEvents = false;

        [PropertyDisplay("Dump server packets")]
        public bool DumpServerPackets = false;

        [PropertyDisplay("Dump client packets")]
        public bool DumpClientPackets = false;
    }
}
