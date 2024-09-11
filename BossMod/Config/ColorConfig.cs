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

    [PropertyDisplay("Player colors: tank")]
    public Color PlayerColorsTank = new(Color.FromComponents(30, 50, 110).ABGR);

    [PropertyDisplay("Player colors: healer")]
    public Color PlayerColorsHealer = new(Color.FromComponents(30, 110, 50).ABGR);

    [PropertyDisplay("Player colors: melee")]
    public Color PlayerColorsMelee = new(Color.FromComponents(110, 30, 30).ABGR);

    [PropertyDisplay("Player colors: caster")]
    public Color PlayerColorsCaster = new(Color.FromComponents(70, 30, 110).ABGR);

    [PropertyDisplay("Player colors: phys. ranged")]
    public Color PlayerColorsPhysRanged = new(Color.FromComponents(110, 90, 30).ABGR);

    [PropertyDisplay("Player colors: focus")]
    public Color PlayerColorsFocus = new(Color.FromComponents(0, 255, 255).ABGR);
}
