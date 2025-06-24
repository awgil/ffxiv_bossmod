namespace BossMod;

[ConfigDisplay(Name = "Generic solver settings", Order = 7)]
public sealed class AIHintsConfig : ConfigNode
{
    public enum DonutFallbackBehavior
    {
        [PropertyDisplay("Assume the AOE is circular, with no inner safe spot")]
        AssumeCircle,
        [PropertyDisplay("Ignore entirely")]
        Ignore
    }

    [PropertyDisplay("Behavior for unknown donut AOEs", tooltip: "This setting only applies when no module is active.")]
    public DonutFallbackBehavior DonutFallback = DonutFallbackBehavior.AssumeCircle;

    [PropertyDisplay("Guessed angle for unknown cone AOEs", tooltip: "This setting only applies when no module is active.")]
    [PropertySlider(1, 180, Speed = 5)]
    public float ConeFallbackAngle = 180;

    public enum OmenBehavior
    {
        [PropertyDisplay("Best-effort guess; ignore large untelegraphed circles, which are usually raidwides")]
        Automatic,
        [PropertyDisplay("Best-effort guess; hint all actions")]
        AutomaticConservative,
        [PropertyDisplay("Ignore entirely")]
        OmenOnly
    }

    [PropertyDisplay("Behavior for actions without AOE indicators", tooltip: "This setting only applies when no module is active.")]
    public OmenBehavior OmenSetting = OmenBehavior.Automatic;

    [PropertyDisplay("Run generic solver for 'Helper'-type actors", tooltip: "This setting only applies when no module is active.\n\nHelpers are invisible enemies that are responsible for casting a majority of the mechanics in boss fights. Enabling this option may cause unwanted results. For example, vbm may treat gaze attacks, knockbacks, raidwides, or other mechanics as regular AOEs and try to run out of them. Enable this setting at your own discretion.")]
    public bool EnableHelperHints = false;
}
