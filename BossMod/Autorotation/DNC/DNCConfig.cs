namespace BossMod
{
    [ConfigDisplay(Parent = typeof(AutorotationConfig))]
    class DNCConfig : ConfigNode
    {
        [PropertyDisplay("Execute optimal rotations on Cascade (ST) or Windmill (AOE)")]
        public bool FullRotation = true;

        [PropertyDisplay("Pause autorotation while Improvisation is active")]
        public bool PauseDuringImprov = false;

        [PropertyDisplay("Automatically choose dance partner")]
        public bool AutoPartner = true;
    }
}
