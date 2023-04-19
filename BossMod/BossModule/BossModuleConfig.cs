namespace BossMod
{
    [ConfigDisplay(Name = "Boss module window settings", Order = 1)]
    public class BossModuleConfig : ConfigNode
    {
        [PropertyDisplay("Arena scale factor")]
        [PropertySlider(0.1f, 10, Speed = 0.1f, Logarithmic = true)]
        public float ArenaScale = 1;

        [PropertyDisplay("Enable boss modules")]
        public bool Enable = true;

        [PropertyDisplay("Lock movement and mouse interaction")]
        public bool Lock = false;

        [PropertyDisplay("Rotate map to match camera orientation")]
        public bool RotateArena = true;

        [PropertyDisplay("Expand space reserved for map to allow arbitrary rotations")]
        public bool AddSlackForRotations = true;

        [PropertyDisplay("Show arena border")]
        public bool ShowBorder = true;

        [PropertyDisplay("Change arena border color if player is at risk")]
        public bool ShowBorderRisk = true;

        [PropertyDisplay("Show cardinal direction names")]
        public bool ShowCardinals = false;

        [PropertyDisplay("Show waymarks on radar")]
        public bool ShowWaymarks = false;

        [PropertyDisplay("Show mechanics sequence and timers")]
        public bool ShowMechanicTimers = true;

        [PropertyDisplay("Show raidwide hints")]
        public bool ShowGlobalHints = true;

        [PropertyDisplay("Show warnings and hints for player")]
        public bool ShowPlayerHints = true;

        [PropertyDisplay("Show text hints in separate window")]
        public bool HintsInSeparateWindow = false;

        [PropertyDisplay("Trisha mode: show radar without window")]
        public bool TrishaMode = false;

        [PropertyDisplay("Add opaque background to the arena")]
        public bool OpaqueArenaBackground = false;

        [PropertyDisplay("Show movement hints in world")]
        public bool ShowWorldArrows = false;

        [PropertyDisplay("Show boss module demo out of instances (useful for configuring windows)")]
        public bool ShowDemo = false;

        [PropertyDisplay("Show window with cooldown plan timers")]
        public bool EnableTimerWindow = false;

        [PropertyDisplay("Always show all alive party members")]
        public bool ShowIrrelevantPlayers = false;
    }
}
