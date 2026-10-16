namespace BossMod;

[ConfigDisplay(Parent = typeof(ActionTweaksConfig), Group = "Job-specific")]
class WHMConfig : ConfigNode
{
    [PropertyDisplay("Align Aetherial Shift with camera direction")]
    public bool AlignDashToCamera = false;
}
