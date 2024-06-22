namespace BossMod;

[ConfigDisplay(Parent = typeof(ActionTweaksConfig))]
class GNBConfig : ConfigNode
{
    [PropertyDisplay("Forbid 'Lightning Shot' too early in prepull")]
    public bool ForbidEarlyLightningShot = true;

    // TODO: things below should be strategy options rather than global options
    [PropertyDisplay("Use both Rough Divide charges in No Mercy")]
    public bool NoMercyRoughDivide = true;

    [PropertyDisplay("<= 2.47 sks rotation")]
    public bool Skscheck = true;

    [PropertyDisplay("Early No Mercy in Opener (NOTE: This will break Lv30-53 rotation)")]
    public bool EarlyNoMercy = true;

    [PropertyDisplay("Early Sonic Break in Opener (NOTE: This will break Lv30-53 rotation)")]
    public bool EarlySonicBreak = true;
}
