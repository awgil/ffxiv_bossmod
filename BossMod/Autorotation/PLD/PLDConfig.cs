namespace BossMod;

[ConfigDisplay(Parent = typeof(AutorotationConfig))]
class PLDConfig : ConfigNode
{
    [PropertyDisplay("Execute optimal single-target rotation on Fast Blade and AOE rotation on Total Eclipse")]
    public bool FullRotation = true;

    [PropertyDisplay("Execute preceeding actions for single-target combos (Riot Blade, ...)")]
    public bool STCombos = true;

    [PropertyDisplay("Execute preceeding action for aoe combo (...)")]
    public bool AOECombos = true;

    [PropertyDisplay("Smart targeting for Shirk (target if friendly, otherwise mouseover if friendly, otherwise offtank if available)")]
    public bool SmartShirkTarget = true;

    [PropertyDisplay("Use provoke on mouseover, if available and hostile")]
    public bool ProvokeMouseover = true;
}
