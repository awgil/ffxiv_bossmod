namespace BossMod
{
    [ConfigDisplay(Name = "Autorotation settings (experimental!)", Order = 5)]
    class AutorotationConfig : ConfigNode
    {
        [PropertyDisplay("Enable autorotation")]
        public bool Enabled = false;

        [PropertyDisplay("Log messages")]
        public bool Logging = false;

        [PropertyDisplay("Show in-game UI")]
        public bool ShowUI = false;

        [PropertyDisplay("Show positional hints in world")]
        public bool ShowPositionals = false;

        [PropertyDisplay("Enable actions that affect position (e.g. Onslaught, Primal Rend)")]
        public bool EnableMovement = true;

        [PropertyDisplay("Sticky auto actions")]
        public bool StickyAutoActions = false;
    }
}
