namespace BossMod;

[ConfigDisplay(Parent = typeof(ActionTweaksConfig))]
class MNKConfig : ConfigNode
{
    [PropertyDisplay("Delay Thunderclap if already in melee range of target")]
    public bool PreventCloseDash = true;
}
