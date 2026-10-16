namespace BossMod;

[ConfigDisplay(Parent = typeof(ActionTweaksConfig), Group = "Job-specific")]
class DNCConfig : ConfigNode
{
    [PropertyDisplay("Align En Avant with camera direction")]
    public bool AlignDashToCamera = false;
}
