namespace BossMod
{
    [ConfigDisplay(Parent = typeof(AutorotationConfig))]
    class BRDConfig : ConfigNode
    {
        [PropertyDisplay("Execute optimal rotations on Heavy Shot (ST) or Quick Nock (AOE)")]
        public bool FullRotation = true;
    }
}
