namespace BossMod;

[ConfigDisplay(Parent = typeof(ActionTweaksConfig))]
class WARConfig : ConfigNode
{
    [PropertyDisplay("Prefer self-targeted holmgang; requires smart targeting to be enabled (and allows overriding target by mouseover)")]
    public bool HolmgangSelf = true;

    [PropertyDisplay("Forbid tomahawk too early in prepull")]
    public bool ForbidEarlyTomahawk = true;
}
