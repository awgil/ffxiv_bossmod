namespace BossMod.Dawntrail.Extreme.Ex3Sphene;

[ConfigDisplay(Order = 0x030, Parent = typeof(DawntrailConfig))]
class Ex3SpheneConfig() : ConfigNode()
{
    [PropertyDisplay("Absolute Authority: ignore flares, stack together")]
    public bool AbsoluteAuthorityIgnoreFlares = true;
}
