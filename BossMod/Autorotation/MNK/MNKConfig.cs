namespace BossMod
{
    [ConfigDisplay(Parent = typeof(AutorotationConfig))]
    class MNKConfig : ConfigNode
    {
        [PropertyDisplay(
            "Execute optimal rotations on Bootshine (ST) or Arm of the Destroyer (AOE)"
        )]
        public bool FullRotation = true;

        [PropertyDisplay("Execute form-specific aoe GCD on Four-point Fury")]
        public bool AOECombos = true;

        [PropertyDisplay("Use Form Shift out of combat")]
        public bool AutoFormShift = false;
    }
}
