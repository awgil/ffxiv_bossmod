namespace BossMod;

[ConfigDisplay(Parent = typeof(ActionTweaksConfig))]
class SCHConfig : ConfigNode
{
    //[PropertyDisplay("Execute optimal rotations on Ruin/Broil (ST damage), Art of War (AOE damage), Physick (ST heal) and Succor (AOE heal)")]
    //public bool FullRotation = true;

    [PropertyDisplay("Use mouseover targeting for friendly spells")]
    public bool MouseoverFriendly = true;
}
