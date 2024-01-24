namespace BossMod
{
    [ConfigDisplay(Parent = typeof(AutorotationConfig))]
    class SAMConfig : ConfigNode
    {
        [PropertyDisplay("Execute optimal rotations on Hakaze (ST) or Fuko/Fuga (AOE)")]
        public bool FullRotation = true;
    }
}
