namespace BossMod;

[ConfigDisplay(Parent = typeof(ActionTweaksConfig), Group = "Job-specific")]
class RPRConfig : ConfigNode
{
    [PropertyDisplay("Align Hell's Egress/Ingress with camera direction")]
    public bool AlignDashToCamera = false;
}
