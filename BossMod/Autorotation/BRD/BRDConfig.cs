namespace BossMod
{
    [ConfigDisplay(Parent = typeof(AutorotationConfig))]
    class BRDConfig : ConfigNode
    {
        [PropertyDisplay("Execute optimal rotations on Heavy Shot (ST) or Quick Nock (AOE)")]
        public bool FullRotation = true;

        [PropertyDisplay("Smart targeting for Warden's Paean (target if friendly, otherwise mouseover if friendly, otherwise random party member with esunable debuff if available, otherwise self)")]
        public bool SmartWardensPaeanTarget = true;
    }
}
