namespace BossMod;

[ConfigDisplay(Parent = typeof(ActionTweaksConfig))]
class WHMConfig : ConfigNode
{
    [PropertyDisplay("Align Aetherial Shift with camera direction")]
    public bool AlignDashToCamera = false;
}
