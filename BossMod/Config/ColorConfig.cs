namespace BossMod;

[ConfigDisplay(Name = "Color Scheme", Order = -1)]
public sealed class ColorConfig : ConfigNode
{
    [PropertyDisplay("Arena: background")]
    public Color ArenaBackground = new(0xc00f0f0f);

    [PropertyDisplay("Arena: border")]
    public Color ArenaBorder = new(0xffffffff);

    [PropertyDisplay("Arena: typical danger zone (AOE)")]
    public Color ArenaAOE = new(0x80008080);

    [PropertyDisplay("Arena: typical safe zone")]
    public Color ArenaSafeFromAOE = new(0x80008000);

    // TODO: imminent aoes and dangerous players should use separate color
    [PropertyDisplay("Arena: typical danger foreground element (tether, etc)")]
    public Color ArenaDanger = new(0xff00ffff);

    [PropertyDisplay("Arena: typical safe foreground element (tether, etc)")]
    public Color ArenaSafe = new(0xff00ff00);

    [PropertyDisplay("Arena: enemy")]
    public Color ArenaEnemy = new(0xff0000ff);

    [PropertyDisplay("Arena: non-enemy important object (untargetable origin of a tether, interactible object, etc)")]
    public Color ArenaObject = new(0xff0080ff);

    [PropertyDisplay("Arena: player character")]
    public Color ArenaPC = new(0xff00ff00);

    [PropertyDisplay("Arena: interesting player, important for a mechanic")]
    public Color ArenaPlayerInteresting = new(0xffc0c0c0);

    [PropertyDisplay("Arena: vulnerable player, needs special attention")]
    public Color ArenaPlayerVulnerable = new(0xffff00ff);

    [PropertyDisplay("Arena: generic/irrelevant player (can be overridden by role-specific colors, depending on settings)")]
    public Color ArenaPlayerGeneric = new(0xff808080);

    [PropertyDisplay("Arena: generic/irrelevant tank")]
    public Color ArenaPlayerGenericTank = Color.FromComponents(30, 50, 110);

    [PropertyDisplay("Arena: generic/irrelevant healer")]
    public Color ArenaPlayerGenericHealer = Color.FromComponents(30, 110, 50);

    [PropertyDisplay("Arena: generic/irrelevant melee")]
    public Color ArenaPlayerGenericMelee = Color.FromComponents(110, 30, 30);

    [PropertyDisplay("Arena: generic/irrelevant caster")]
    public Color ArenaPlayerGenericCaster = Color.FromComponents(70, 30, 110);

    [PropertyDisplay("Arena: generic/irrelevant phys. ranged")]
    public Color ArenaPlayerGenericPhysRanged = Color.FromComponents(110, 90, 30);

    [PropertyDisplay("Arena: generic/irrelevant focus target")]
    public Color ArenaPlayerGenericFocus = Color.FromComponents(0, 255, 255);

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
