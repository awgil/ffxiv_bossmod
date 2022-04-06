namespace BossMod
{
    class AutorotationConfig : ConfigNode
    {
        public enum GroundTargetingMode { Manual, AtCursor, AtTarget }

        public bool Enabled = false;
        public bool Logging = false;
        public bool ShowUI = false;
        public bool EnableMovement = true;
        public bool SmartCooldownQueueing = true;
        public bool PreventMovingWhileCasting = false;
        public CommonRotation.Strategy.PotionUse PotionUse = CommonRotation.Strategy.PotionUse.Manual;
        public GroundTargetingMode GTMode = GroundTargetingMode.AtCursor;

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
            DrawProperty(ref EnableMovement, "Enable actions that affect position (e.g. Onslaught, Primal Rend)");
            DrawProperty(ref SmartCooldownQueueing, "Smart queue for cooldowns (when pressing a button, queue it into next ogcd slot without delaying GCDs)");
            DrawProperty(ref PreventMovingWhileCasting, "Prevent movement while casting");
            DrawProperty(ref PotionUse, "Potion use strategy");
            DrawProperty(ref GTMode, "Target selection for ground-targeted abilities (default, automatic at cursor location or at selected target)");
        }
    }
}
