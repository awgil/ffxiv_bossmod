namespace BossMod;

[ConfigDisplay(Parent = typeof(ActionTweaksConfig))]
class SMNConfig : ConfigNode
{
    [PropertyDisplay("Execute optimal rotations on Ruin (ST) or Outburst (AOE)")]
    public bool FullRotation = true;

    [PropertyDisplay("Use mouseover targeting for friendly spells")]
    public bool MouseoverFriendly = true;
}
