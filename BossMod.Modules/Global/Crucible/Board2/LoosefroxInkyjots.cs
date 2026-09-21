#pragma warning disable CA1707 // Identifiers should not contain underscores
namespace BossMod.Global.Crucible.LoosefroxInkyjots;

public enum OID : uint
{
    Boss = 0x4C65, // R1.400, x1
    Helper = 0x233C, // R0.500, x32 (spawn during fight), Helper type
    ChewchumPopoto = 0x4C66, // R6.105, x1
    _Gen_GobbieBomb1 = 0x4C67, // R1.200, x0 (spawn during fight)
    _Gen_GobbieBomb = 0x4C69, // R0.600, x0 (spawn during fight)

    Quicksand = 0x1EC026
}

public enum AID : uint
{
    _AutoAttack_ = 50784, // Boss/4C66->player, no cast, single-target
    _Weaponskill_Burrow = 48224, // 4C66->self, no cast, single-target
    _Weaponskill_SandPillar = 48225, // Helper->location, no cast, range 4 circle
    _Weaponskill_ = 50451, // Helper->location, no cast, single-target
    _Spell_SandBreath = 48226, // 4C66->self, 7.8+1.2s cast, single-target
    _Spell_SandBreath1 = 48227, // Helper->self, 9.0s cast, range 60 90-degree cone
    _Weaponskill_GobspinHeadlops = 48230, // Boss->self, 5.3+0.7s cast, single-target
    _Weaponskill_GobspinHeadlops1 = 48231, // Helper->self, 6.0s cast, range 4-40 donut
    _Weaponskill_Kinborrow = 48242, // Boss->self, 4.0s cast, single-target
    _Weaponskill_GobbieboomBarrage = 50446, // Boss->self, 3.0s cast, single-target
    _Weaponskill_1 = 50452, // Helper->location, no cast, range 6 circle
    _Weaponskill_Explosion = 48235, // 4C69->self, 2.0s cast, range 12 circle
    _Weaponskill_BombToss = 48518, // Boss->location, 3.0+1.0s cast, single-target
    _Weaponskill_BombToss1 = 48519, // Helper->location, 4.0s cast, range 6 circle
    _Weaponskill_GobbieboomBarrage1 = 48234, // Boss->self, 3.0s cast, single-target
    _Weaponskill_GobspinHeadlops2 = 48228, // Boss->self, 5.3+0.7s cast, single-target
    _Weaponskill_GobspinHeadlops3 = 48229, // Helper->self, 6.0s cast, range 8 circle
    _Weaponskill_Earthquake = 48239, // ChewchumPopoto->self, 5.0s cast, range 60 circle
    _Weaponskill_Earthquake1 = 48240, // ChewchumPopoto->self, no cast, range 60 circle
    _Weaponskill_Explosion2 = 48238, // Helper->self, no cast, single-target
    _Weaponskill_2 = 48520, // ChewchumPopoto->self, no cast, single-target
    _Weaponskill_3 = 50444, // ChewchumPopoto->self, no cast, single-target
    _Weaponskill_Explosion1 = 48236, // _Gen_GobbieBomb1->self, 2.0s cast, range 40 width 8 cross
}

public enum SID : uint
{
    _Gen_PopotoSkin = 5423, // Boss->Boss, extra=0x0
    _Gen_SixFulmsUnder = 567, // none->player, extra=0x522
}

public enum IconID : uint
{
    _Gen_Icon_exclamation_8s_x = 569, // Helper->self
}

class Quicksand(BossModule module) : BossComponent(module)
{
    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var q in Module.Enemies(OID.Quicksand))
            Arena.ZoneCircle(q.Position, 6, ArenaColor.AOE);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var drown = actor.FindStatus(SID._Gen_SixFulmsUnder)?.ExpireAt ?? DateTime.MaxValue;
        foreach (var q in Module.Enemies(OID.Quicksand))
            hints.AddForbiddenZone(ShapeDistance.Circle(q.Position, 6), drown);
    }
}

class SandBreath(BossModule module) : Components.StandardAOEs(module, AID._Spell_SandBreath1, new AOEShapeCone(60, 45.Degrees()));
class GobspinHeadlops1(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_GobspinHeadlops3, 8);
class GobspinHeadlops2(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_GobspinHeadlops1, new AOEShapeDonut(4, 40));
class ExplosionCircle(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_Explosion, 12);
class ExplosionCross(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_Explosion1, new AOEShapeCross(40, 4));
class BombToss(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_BombToss1, 6);
class Earthquake(BossModule module) : Components.RaidwideCast(module, AID._Weaponskill_Earthquake);

class LoosefroxInkyjotsStates : StateMachineBuilder
{
    public LoosefroxInkyjotsStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Quicksand>()
            .ActivateOnEnter<SandBreath>()
            .ActivateOnEnter<GobspinHeadlops1>()
            .ActivateOnEnter<GobspinHeadlops2>()
            .ActivateOnEnter<ExplosionCircle>()
            .ActivateOnEnter<ExplosionCross>()
            .ActivateOnEnter<BombToss>()
            .ActivateOnEnter<Earthquake>()
            .Raw.Update = () => module.PrimaryActor.IsDeadOrDestroyed && ((LoosefroxInkyjots)module).Worm is { IsDeadOrDestroyed: true };
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1089, NameID = 14561)]
public class LoosefroxInkyjots(WorldState ws, Actor primary) : BossModule(ws, primary, new(520, -420), CustomBounds)
{
    public static readonly ArenaBoundsCustom CustomBounds = new(27, Utils.LoadResource<RelSimplifiedComplexPolygon>("BossMod.Global.Crucible.Board2.FinalBoss.json"));

    public Actor? Worm { get; private set; }

    protected override void UpdateModule()
    {
        Worm ??= Enemies(OID.ChewchumPopoto).FirstOrDefault();
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        base.DrawEnemies(pcSlot, pc);

        Arena.Actor(Worm, ArenaColor.Enemy);
    }
}

