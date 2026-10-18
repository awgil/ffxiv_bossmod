namespace BossMod;

[ConfigDisplay(Parent = typeof(ActionTweaksConfig), Group = "Job-specific")]
public class WHMConfig : ConfigNode
{
    [PropertyDisplay("Align Aetherial Shift with camera direction")]
    public bool AlignDashToCamera = false;
}
