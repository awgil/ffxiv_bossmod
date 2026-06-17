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

    [PropertyDisplay("Behavior for unknown donut AOEs", since: "0.3.0.0", tooltip: "This setting only applies when no module is active.")]
    public DonutFallbackBehavior DonutFallback = DonutFallbackBehavior.AssumeCircle;

    [PropertyDisplay("Guessed angle for unknown cone AOEs", since: "0.3.0.0", tooltip: "This setting only applies when no module is active.")]
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

    [PropertyDisplay("Behavior for actions without AOE indicators", since: "0.3.0.0", tooltip: "This setting only applies when no module is active.")]
    public OmenBehavior OmenSetting = OmenBehavior.Automatic;
}
