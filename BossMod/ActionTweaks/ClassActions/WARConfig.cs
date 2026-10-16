namespace BossMod;

[ConfigDisplay(Parent = typeof(ActionTweaksConfig), Group = "Job-specific")]
class WARConfig : ConfigNode
{
    [PropertyDisplay("Prefer self-targeted holmgang; requires smart targeting to be enabled (and allows overriding target by mouseover)")]
    public bool HolmgangSelf = true;
}
