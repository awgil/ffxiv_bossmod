namespace BossMod
{
    [ConfigDisplay(Parent = typeof(AutorotationConfig))]
    class RPRConfig : ConfigNode
    {
        [PropertyDisplay("Execute optimal rotations on Slice (ST) or Spinning Scythe (AOE)")]
        public bool FullRotation = true;

        [PropertyDisplay("Forbid Harpe too early in prepull")]
        public bool ForbidEarlyHarpe = true;
    }
}
