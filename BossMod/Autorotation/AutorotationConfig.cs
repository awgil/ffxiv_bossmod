namespace BossMod
{
    [ConfigDisplay(Name = "Autorotation settings (experimental!)", Order = 3)]
    class AutorotationConfig : ConfigNode
    {
        [PropertyDisplay("Enable autorotation")]
        public bool Enabled = false;

        [PropertyDisplay("Log messages")]
        public bool Logging = false;

        [PropertyDisplay("Show in-game UI")]
        public bool ShowUI = false;

        [PropertyDisplay("Enable actions that affect position (e.g. Onslaught, Primal Rend)")]
        public bool EnableMovement = true;

        [PropertyDisplay("Smart queue for cooldowns (when pressing a button, queue it into next ogcd slot without delaying GCDs)")]
        public bool SmartCooldownQueueing = true;

        [PropertyDisplay("Prevent movement while casting")]
        public bool PreventMovingWhileCasting = false;

        [PropertyDisplay("Potion use strategy")]
        public CommonRotation.Strategy.PotionUse PotionUse = CommonRotation.Strategy.PotionUse.Manual;

        public enum GroundTargetingMode { Manual, AtCursor, AtTarget }
        [PropertyDisplay("Target selection for ground-targeted abilities")]
        public GroundTargetingMode GTMode = GroundTargetingMode.AtCursor;
    }
}
