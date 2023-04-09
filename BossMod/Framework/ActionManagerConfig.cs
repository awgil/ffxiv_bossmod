namespace BossMod
{
    [ConfigDisplay(Name = "Action tweaks settings", Order = 4)]
    class ActionManagerConfig : ConfigNode
    {
        // TODO: consider exposing max-delay to config; 0 would mean 'remove all delay', max-value would mean 'disable'
        [PropertyDisplay("Remove extra lag-induced animation lock delay from instant casts (a-la xivalex)")]
        public bool RemoveAnimationLockDelay = false;

        [PropertyDisplay("Remove extra framerate-induced cooldown delay")]
        public bool RemoveCooldownDelay = false;

        [PropertyDisplay("Prevent movement while casting")]
        public bool PreventMovingWhileCasting = false;

        [PropertyDisplay("Restore rotation after action use")]
        public bool RestoreRotation = false;

        public enum GroundTargetingMode
        {
            [PropertyDisplay("Manually select position by extra click (normal game behaviour)")]
            Manual,

            [PropertyDisplay("Cast at current mouse position")]
            AtCursor,

            [PropertyDisplay("Cast at selected target's position")]
            AtTarget
        }
        [PropertyDisplay("Target selection for ground-targeted abilities")]
        public GroundTargetingMode GTMode = GroundTargetingMode.Manual;
    }
}
