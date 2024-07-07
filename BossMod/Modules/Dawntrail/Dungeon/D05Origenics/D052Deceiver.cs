using BossMod.Dawntrail.Dungeon.D05Origenics.D051Herpekaris;
using BossMod.Endwalker.Alliance.A32Llymlaen;

namespace BossMod.Dawntrail.Dungeon.D05Origenics.D052Deceiver;

public enum OID : uint
{
    Boss = 0x4170, // R5.000, x1
    Helper = 0x233C, // R0.500, x10, 523 type

    Cahciua = 0x418F, // R0.960, x1
    OrigenicsSentryG91 = 0x4172, // R0.900, x0 (spawn during fight)
    OrigenicsSentryG92 = 0x4171, // R0.900, x0 (spawn during fight)
    Actor1e8f2f = 0x1E8F2F, // R0.500, x1, EventObj type
    Actor1e8fb8 = 0x1E8FB8, // R2.000, x2, EventObj type
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    AutoAttack2 = 873, // OrigenicsSentryG92->player, no cast, single-target

    Electrowave = 36371, // Boss->self, 5.0s cast, range 72 circle // Raidwide
    UnknownAbility = 36362, // Boss->location, no cast, single-target

    BionicThrash1 = 36369, // Boss->self, 7.0s cast, single-target
    BionicThrash2 = 36370, // Helper->self, 8.0s cast, range 30 90.000-degree cone // Conal AOE
    BionicThrash3 = 36368, // Boss->self, 7.0s cast, single-target

    InitializeAndroids = 36363, // Boss->self, 4.0s cast, single-target // Spawns OrigenicsSentryG91 and OrigenicsSentryG92

    Synchroshot1 = 36373, // OrigenicsSentryG91->self, 5.0s cast, range 40 width 4 rect
    Synchroshot2 = 36372, // OrigenicsSentryG92->self, 5.0s cast, range 40 width 4 rect

    InitializeTurrets1 = 36364, // Boss->self, 4.0s cast, single-target
    InitializeTurrets2 = 36365, // Helper->self, 4.7s cast, range 4 width 10 rect

    UnknownWeaponskill1 = 36426, // Helper->self, 4.7s cast, range 4 width 10 rect
    UnknownWeaponskill2 = 38807, // Helper->self, 5.0s cast, range 40 width 10 rect

    LaserLash = 36366, // Helper->self, 5.0s cast, range 40 width 10 rect

    Surge1 = 36367, // Boss->location, 8.0s cast, range 40 width 40 rect
    Surge2 = 39736, // Helper->self, 8.5s cast, range 40 width 40 rect

    Electray = 38320, // Helper->player, 8.0s cast, range 5 circle
}

public enum SID : uint
{
    UnknownStatus = 2056, // none->OrigenicsSentryG91, extra=0x2AB
    VulnerabilityUp = 1789, // OrigenicsSentryG92/Helper->player, extra=0x1/0x2
    Electrocution1 = 3073, // none->player, extra=0x0
    Electrocution2 = 3074, // none->player, extra=0x0
}

public enum IconID : uint
{
    Icon345 = 345, // player
}

class Electrowave(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Electrowave));
class BionicThrash2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.BionicThrash2), new AOEShapeCone(30, 45.Degrees()));
class Synchroshot1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Synchroshot1), new AOEShapeRect(40, 2));
class Synchroshot2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Synchroshot2), new AOEShapeRect(40, 2));
class InitializeTurrets2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.InitializeTurrets2), new AOEShapeRect(4, 5));
class UnknownWeaponskill1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.UnknownWeaponskill1), new AOEShapeRect(4, 5));
class UnknownWeaponskill2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.UnknownWeaponskill2), new AOEShapeRect(40, 5));
class LaserLash(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.LaserLash), new AOEShapeRect(40, 5));
class Electray(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.Electray), 5);

class Surge1(BossModule module) : Components.Knockback(module)
{
    private readonly List<Source> _sources = [];

    private static readonly AOEShapeCone _shape = new(30, 90.Degrees());

    public override IEnumerable<Source> Sources(int slot, Actor actor) => _sources;
    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => !Module.InBounds(pos);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Surge1)
        {
            _sources.Clear();
            _sources.Add(new(caster.Position, 30, spell.NPCFinishAt, _shape, spell.Rotation + 90.Degrees(), Kind.DirForward));
            _sources.Add(new(caster.Position, 30, spell.NPCFinishAt, _shape, spell.Rotation - 90.Degrees(), Kind.DirForward));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Surge1)
        {
            _sources.Clear();
            ++NumCasts;
        }
    }
}

class D052DeceiverStates : StateMachineBuilder
{
    public D052DeceiverStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Electrowave>()
            .ActivateOnEnter<BionicThrash2>()
            //.ActivateOnEnter<Synchroshot1>()
            .ActivateOnEnter<Synchroshot2>()
            .ActivateOnEnter<InitializeTurrets2>()
            .ActivateOnEnter<UnknownWeaponskill1>()
            //.ActivateOnEnter<UnknownWeaponskill2>()
            .ActivateOnEnter<LaserLash>()
            .ActivateOnEnter<Electray>()
            .ActivateOnEnter<Surge1>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 825, NameID = 12693)]
public class D052Deceiver(WorldState ws, Actor primary) : BossModule(ws, primary, new(-172, -142), new ArenaBoundsSquare(20));

