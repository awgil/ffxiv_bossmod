namespace BossMod;

[ConfigDisplay(Parent = typeof(ActionTweaksConfig))]
class RPRConfig : ConfigNode
{
    [PropertyDisplay("Forbid Harpe too early in prepull")]
    public bool ForbidEarlyHarpe = true;

    [PropertyDisplay("Align Hell's Egress/Ingress with camera direction")]
    public bool AlignDashToCamera = false;
}
