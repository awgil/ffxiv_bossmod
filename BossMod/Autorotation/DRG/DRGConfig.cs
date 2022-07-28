namespace BossMod
{
    [ConfigDisplay(Parent = typeof(AutorotationConfig))]
    class DRGConfig : ConfigNode
    {
        [PropertyDisplay("Execute optimal rotations on True Thrust (ST) or Doom Spike (AOE)")]
        public bool FullRotation = true;
    }
}
