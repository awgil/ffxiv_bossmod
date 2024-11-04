namespace BossMod;

[ConfigDisplay(Parent = typeof(ActionTweaksConfig))]
class SCHConfig : ConfigNode
{
    public enum SoilOverride
    {
        [PropertyDisplay("Do not override")]
        Manual,
        [PropertyDisplay("Place on self")]
        Self,
        [PropertyDisplay("Place on fairy (or self)")]
        Fairy
    }

    [PropertyDisplay("Position override for manually pressed Sacred Soil")]
    public SoilOverride Position = SoilOverride.Manual;
}
