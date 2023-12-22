namespace BossMod
{
    [ConfigDisplay(Parent = typeof(AutorotationConfig))]
    class RPRConfig : ConfigNode
    {
        [PropertyDisplay("Execute optimal rotations on Slice (ST) or Spinning Scythe (AOE) XD")]
        public bool FullRotation = true;
    }
}
