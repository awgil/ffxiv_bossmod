namespace BossMod
{
    [ConfigDisplay(Parent = typeof(AutorotationConfig))]
    class MNKConfig : ConfigNode
    {
        [PropertyDisplay("Execute optimal rotations on Bootshine (ST) or Arm of the Destroyer (AOE)")]
        public bool FullRotation = true;
    }
}
