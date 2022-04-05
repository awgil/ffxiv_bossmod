namespace BossMod
{
    class AutorotationConfig : ConfigNode
    {
        public bool Enabled = false;
        public bool Logging = false;
        public bool ShowUI = false;
        public bool PreventMovingWhileCasting = false;

        public AutorotationConfig()
        {
            DisplayName = "Autorotation settings (experimental!)";
            DisplayOrder = 3;
        }

        protected override void DrawContents()
        {
            DrawProperty(ref Enabled, "Enable autorotation");
            DrawProperty(ref Logging, "Log messages");
            DrawProperty(ref ShowUI, "Show in-game UI");
            DrawProperty(ref PreventMovingWhileCasting, "Prevent movement while casting");
        }
    }
}
