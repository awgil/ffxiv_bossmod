namespace BossMod
{
    [ConfigDisplay(Name = "General settings", Order = 0)]
    public class GeneralConfig : ConfigNode
    {
        public bool DumpWorldStateEvents = false;
        public bool DumpServerPackets = false;
        public bool DumpClientPackets = false;

        public override void DrawContents(Tree tree)
        {
            DrawProperty(ref DumpWorldStateEvents, "Dump world state events");
            DrawProperty(ref DumpServerPackets, "Dump server packets");
            DrawProperty(ref DumpClientPackets, "Dump client packets");
        }
    }
}
