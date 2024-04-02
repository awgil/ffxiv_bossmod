namespace BossMod;

[ConfigDisplay(Parent = typeof(AutorotationConfig))]
class WHMConfig : ConfigNode
{
    [PropertyDisplay("Execute optimal rotations on Glare (ST damage), Holy (AOE damage), Cure1 (ST heal) and Medica1 (AOE heal)")]
    public bool FullRotation = true;

    [PropertyDisplay("When trying to cast raise, apply swiftcast and thin air automatically, if possible")]
    public bool SwiftFreeRaise = true;

    [PropertyDisplay("Use mouseover targeting for friendly spells")]
    public bool MouseoverFriendly = true;

    [PropertyDisplay("Smart targeting for Cure 3 (target/mouseover if friendly, otherwise party member that has most nearby damaged players)")]
    public bool SmartCure3Target = true;

    [PropertyDisplay("Never overcap blood lilies: cast misery instead of solace/rapture if needed")]
    public bool NeverOvercapBloodLilies = false;
}
