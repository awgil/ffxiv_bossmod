namespace BossMod;

[ConfigDisplay(Name = "Color Scheme", Order = -1)]
public sealed class ColorConfig : ConfigNode
{
    [PropertyDisplay("Planner: background")]
    public Color PlannerBackground = new(0x80362b00); // solarized base03; old: 0x40404040

    [PropertyDisplay("Planner: background highlight")]
    public Color PlannerBackgroundHighlight = new(0x80423607); // solarized base02; old: n/a

    [PropertyDisplay("Planner: cooldown")]
    public Color PlannerCooldown = new(0x80756e58); // solarized base01; old: 0x80808080

    [PropertyDisplay("Planner: fallback color for options")]
    public Color PlannerFallback = new(0x80969483); // solarized base0; old: ???

    [PropertyDisplay("Planner: effect")]
    public Color PlannerEffect = new(0x8000ff00); // TODO: solarized base1 (0x80a1a193) ???

    [PropertyDisplay("Planner: window")]
    public Color[] PlannerWindow = [new(0x800089b5), new(0x80164bcb), new(0x802f32dc), new(0x808236d3), new(0x80c4716c), new(0x80d28b26), new(0x8098a12a), new(0x80009985)]; // solarized accents
}
