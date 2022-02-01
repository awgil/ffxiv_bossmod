namespace BossMod
{
    class GeneralConfig : ConfigNode
    {
        public bool DumpWorldStateEvents = false;
        public bool DumpServerPackets = false;
        public bool DumpClientPackets = false;
        public bool AutorotationEnabled = false;
        public bool AutorotationShowUI = false;

        protected override void DrawContents()
        {
            DrawProperty(ref DumpWorldStateEvents, "Dump world state events");
            DrawProperty(ref DumpServerPackets, "Dump server packets");
            DrawProperty(ref DumpClientPackets, "Dump client packets");
            DrawProperty(ref AutorotationEnabled, "Enable autorotation (experimental!)");
            DrawProperty(ref AutorotationShowUI, "Show autorotation UI");
        }

        protected override string? NameOverride() => "General settings";
    }
}
