namespace BossMod
{
    public class GeneralConfig : ConfigNode
    {
        public bool DumpWorldStateEvents = false;
        public bool DumpServerPackets = false;
        public bool DumpClientPackets = false;

        public GeneralConfig()
        {
            DisplayName = "General settings";
            DisplayOrder = 0;
        }

        protected override void DrawContents()
        {
            DrawProperty(ref DumpWorldStateEvents, "Dump world state events");
            DrawProperty(ref DumpServerPackets, "Dump server packets");
            DrawProperty(ref DumpClientPackets, "Dump client packets");
        }
    }
}
