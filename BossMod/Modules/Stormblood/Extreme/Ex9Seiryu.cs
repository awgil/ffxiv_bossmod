namespace BossMod.Stormblood.Extreme.Ex9Seiryu;

public enum OID : uint
{
    Boss = 0x25EC, // Seiryu
    Helper = 0x233C, // invisible helper - casts the Kuji-kiri sigils, Great Typhoon, etc.
    AkaNoShiki = 0x25EE, // casts Red Rush
    AoNoShiki = 0x25EF, // casts Blue Bolt
    IwaNoShiki = 0x25F0, // casts 100-tonze Swing / Kanabo
    NumaNoShiki = 0x25F1, // casts Explosion
    DoroNoShiki = 0x25F2, // casts Explosion
    TenNoShiki = 0x2777, // casts Yama-kagura
}

public enum AID : uint
{
    FifthElement = 14275, // Boss->self, room-wide raidwide (circle R100)
    StrengthOfSpirit = 14281, // Boss->self, raidwide (circle R80)
    DragonsWakeEffect = 14282, // Boss->self, instant landing effect (14283 is the telegraphed cast)
    DragonsWake = 14283, // Boss->self, 2.4s cast, transitions the arena to phase 2 (island surrounded by water)
    SummonShiki1 = 14286, // Boss->self, summons the -no-Shiki adds
    SummonShiki2 = 14288, // Boss->self, summons the -no-Shiki adds
    SerpentAscending = 14300, // Boss->self, no-shape
    SerpentDescending = 14301, // Helper->player, circle R5 (targeted)
    KujiKiri = 14305, // Boss->self, no-shape parent cast; spawns the Fortune-blade Sigil grid
    FortuneBladeSigil = 14306, // Helper->self, range 50 width 4 line AoE forming the Kuji-kiri grid
    HundredTonzeSwing = 14317, // IwaNoShiki->self, circle R16 AoE
    Kanabo = 14318, // IwaNoShiki->self, range 40 60-degree cone AoE
    BlueBolt = 14320, // AoNoShiki->self, range 80 width 5 line AoE
    RedRush = 14321, // AkaNoShiki->self, range 80 width 5 line AoE
    ExplosionNuma = 14322, // NumaNoShiki->self, raidwide-sized explosion (circle R80) if the add is not killed
    ExplosionDoro = 14323, // DoroNoShiki->self, raidwide-sized explosion (circle R80) if the add is not killed
    GreatTyphoonCone = 14328, // Helper->self, cone (part of the Great Typhoon rings+cone combo)
    GreatTyphoonDonut2 = 14330, // Helper->self, donut (part of the Great Typhoon rings)
    GreatTyphoonDonut3 = 14331, // Helper->self, donut (part of the Great Typhoon rings)
    YamaKagura = 15393, // TenNoShiki->self, range 60 width 6 line AoE
}

// after Dragon's Wake the arena expands from the phase-1 island to a larger circle, with only the central island being safe;
// the surrounding water is not instantly lethal but increases damage taken and prevents casting, so it must be avoided
// TODO: confirm EX arena geometry matches normal (phase-1 R20 island, phase-2 R38 arena with R20 safe island)
class Phase2Water(BossModule module) : BossComponent(module)
{
    public bool Active { get; private set; }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.DragonsWake)
        {
            Active = true;
            Arena.Bounds = new ArenaBoundsCircle(Ex9Seiryu.ArenaRadius);
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (Active)
            Arena.ZoneDonut(Arena.Center, Ex9Seiryu.IslandRadius, Ex9Seiryu.ArenaRadius, ArenaColor.AOE);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Active && !actor.Position.InCircle(Arena.Center, Ex9Seiryu.IslandRadius))
            hints.Add("Return to the island!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Active)
            hints.AddForbiddenZone(ShapeContains.Donut(Arena.Center, Ex9Seiryu.IslandRadius, Ex9Seiryu.ArenaRadius));
    }
}

class FifthElement(BossModule module) : Components.RaidwideCast(module, AID.FifthElement);
class StrengthOfSpirit(BossModule module) : Components.RaidwideCast(module, AID.StrengthOfSpirit);
class HundredTonzeSwing(BossModule module) : Components.StandardAOEs(module, AID.HundredTonzeSwing, 16);
class Kanabo(BossModule module) : Components.StandardAOEs(module, AID.Kanabo, new AOEShapeCone(40, 30.Degrees()));
class YamaKagura(BossModule module) : Components.StandardAOEs(module, AID.YamaKagura, new AOEShapeRect(60, 3));
class KujiKiri(BossModule module) : Components.StandardAOEs(module, AID.FortuneBladeSigil, new AOEShapeRect(50, 2));
class BlueBolt(BossModule module) : Components.StandardAOEs(module, AID.BlueBolt, new AOEShapeRect(80, 2.5f));
class RedRush(BossModule module) : Components.StandardAOEs(module, AID.RedRush, new AOEShapeRect(80, 2.5f));
class ExplosionNuma(BossModule module) : Components.RaidwideCast(module, AID.ExplosionNuma);
class ExplosionDoro(BossModule module) : Components.RaidwideCast(module, AID.ExplosionDoro);
// Serpent Ascending (14300) is the instant boss wind-up; the spreads land as Serpent Descending (14301, R5 targeted circles)
class SerpentDescending(BossModule module) : Components.SpreadFromCastTargets(module, AID.SerpentDescending, 5);

// TODO: Great Typhoon (14328 cone + 14330/14331 donuts) needs replay confirmation of the donut inner radii and the resolution order before it can be drawn accurately.
// TODO: no "Coursing River" / Blue Orochi push action appeared in the EX action list - confirm whether the knockback exists in EX.

class Ex9SeiryuStates : StateMachineBuilder
{
    public Ex9SeiryuStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Phase2Water>()
            .ActivateOnEnter<FifthElement>()
            .ActivateOnEnter<StrengthOfSpirit>()
            .ActivateOnEnter<HundredTonzeSwing>()
            .ActivateOnEnter<Kanabo>()
            .ActivateOnEnter<YamaKagura>()
            .ActivateOnEnter<KujiKiri>()
            .ActivateOnEnter<BlueBolt>()
            .ActivateOnEnter<RedRush>()
            .ActivateOnEnter<ExplosionNuma>()
            .ActivateOnEnter<ExplosionDoro>()
            .ActivateOnEnter<SerpentDescending>();
    }
}

[ModuleInfo(Contributors = "skmagiik", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 638, NameID = 7922)]
public class Ex9Seiryu(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(IslandRadius))
{
    public const float IslandRadius = 20; // phase-1 arena and phase-2 safe island
    public const float ArenaRadius = 38; // phase-2 full arena (island + surrounding water)
}
