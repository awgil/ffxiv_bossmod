namespace BossMod;

[ConfigDisplay(Parent = typeof(ActionTweaksConfig), Group = "Job-specific")]
public class PCTConfig : ConfigNode
{
    [PropertyDisplay("Align Smudge with camera direction")]
    public bool AlignDashToCamera = false;
}
