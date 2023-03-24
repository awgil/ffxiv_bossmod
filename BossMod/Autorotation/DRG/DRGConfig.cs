namespace BossMod
{
    [ConfigDisplay(Parent = typeof(AutorotationConfig))]
    class DRGConfig : ConfigNode
    {
        [PropertyDisplay("Execute optimal rotations on True Thrust (ST) or Doom Spike (AOE)")]
        public bool FullRotation = true;

        [PropertyDisplay("Smart targeting for Dragon Sight (target if friendly, otherwise mouseover if friendly, otherwise best player by class ranking)")]
        public bool SmartDragonSightTarget = true;
    }
}
