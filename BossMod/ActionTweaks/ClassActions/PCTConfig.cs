namespace BossMod;

[ConfigDisplay(Parent = typeof(ActionTweaksConfig))]
class PCTConfig : ConfigNode
{
    [PropertyDisplay("Align Smudge with camera direction")]
    public bool AlignDashToCamera = false;
}
