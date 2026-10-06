namespace BossMod.Shadowbringers.Raid.E05Ramuh;

public enum OID : uint
{
    Boss = 0x2D02, // R4.180
    Helper = 0x233C, // R0.500
    Stormcloud = 0x2D03, // R1.000
    WillOfRamuh = 0x2D05, // R3.800
    WillOfIxion = 0x2D06, // R3.000
    SurgeOrb = 0x1EAF6B, // R0.500, EventObj
}

public enum AID : uint
{
    AutoAttack = 19362, // Boss->player, no cast, single-target
    CripplingBlow = 19363, // Boss->player, 5.0s cast, single-target
    StratospearSummons = 19341, // Boss->self, 4.0s cast, single-target
    Impact = 20026, // Helper->location, 2.5s cast, range 3 circle
    JudgmentJoltVisual = 19342, // Boss->self, 6.0s cast, single-target
    JudgmentJolt = 19343, // Helper->self, 7.0s cast, range 22 circle
    StormcloudSummons = 19355, // Boss->self, 3.0s cast, single-target
    JudgmentVoltsVisual = 19352, // Boss->self, 6.0s cast, single-target
    JudgmentVolts = 19353, // Helper->self, 6.5s cast, range 100 circle
    LightningBolt = 19356, // Stormcloud->self, no cast, range 8 circle
    FurysBolt = 19344, // Boss->self, 3.0s cast, single-target
    DivineJudgmentVoltsVisual = 20066, // Boss->self, 6.0s cast, single-target
    DivineJudgmentVolts = 19354, // Helper->self, 6.5s cast, range 100 circle
    TribunalSummons = 19345, // Boss->self, 4.0s cast, single-target
    DeadlyDischargeVisual = 19346, // WillOfRamuh->self, 4.0s cast, single-target
    DeadlyDischarge = 19347, // Helper->self, 4.5s cast, range 40 width 40 rect
    GallopVisual = 19350, // WillOfIxion->self, 4.0s cast, single-target
    Gallop = 19351, // Helper->self, 4.5s cast, range 50 width 5 rect
    Thunderstorm = 19360, // Boss->self, 5.0s cast, single-target
    ShockStrike = 19361, // Helper->location, 2.5s cast, range 3 circle
    VoltStrike = 19698, // Helper->location, 2.5s cast, range 5 circle
    _Weaponskill_ = 19403, // Boss->location, no cast, single-target
}

public enum IconID : uint
{
    Stormcloud = 110, // player
}

public enum SID : uint
{
    FurysBolt = 2231, // Boss->Boss, extra=0x1
    SurgeProtection = 2228, // none->player, extra=0x1/0x2/0x3
    StaticCondensation = 2229, // none->player, extra=0x0
}

class CripplingBlow(BossModule module) : Components.SingleTargetCast(module, AID.CripplingBlow);

class Impact(BossModule module) : Components.StandardAOEs(module, AID.Impact, 3);
class JudgmentJolt(BossModule module) : Components.StandardAOEs(module, AID.JudgmentJolt, 22);

class JudgmentVolts(BossModule module) : Components.RaidwideCast(module, AID.JudgmentVolts);
class DivineJudgmentVolts(BossModule module) : Components.RaidwideCast(module, AID.DivineJudgmentVolts, "Raidwide - get Surge Protection");

class SurgeOrbs(BossModule module) : BossComponent(module)
{
    private readonly List<Actor> _orbs = [];

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.SurgeOrb)
            _orbs.Add(actor);
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if (actor.OID == (uint)OID.SurgeOrb)
            _orbs.Remove(actor);
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (actor.OID == (uint)OID.SurgeOrb && state == 0x00040008)
            _orbs.Remove(actor);
    }

    private static bool NeedsOrb(Actor actor, Actor boss)
        => boss.FindStatus(SID.FurysBolt) != null && actor.FindStatus(SID.SurgeProtection) == null && actor.FindStatus(SID.StaticCondensation) == null;

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (!NeedsOrb(pc, Module.PrimaryActor))
            return;
        foreach (var orb in _orbs)
            Arena.AddCircle(orb.Position, 1, ArenaColor.Safe);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_orbs.Count == 0 || !NeedsOrb(actor, Module.PrimaryActor))
            return;

        foreach (var orb in _orbs)
            hints.GoalZones.Add(AIHints.GoalSingleTarget(orb.Position, 1));
    }
}

class StormcloudBait(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCircle(8), (uint)IconID.Stormcloud, default, 5.1f, centerAtTarget: true)
{
    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.Stormcloud)
            CurrentBaits.RemoveAll(b => b.Target.Position.AlmostEqual(actor.Position, 3));
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        foreach (var b in ActiveBaitsOn(actor))
        {
            // bait to edge by making a forbidden zone that's -1.5y the arena size
            hints.AddForbiddenZone(ShapeDistance.Rect(Arena.Center, new WDir(0, 1), 13.5f, 13.5f, 18.5f), b.Activation);
        }
    }
}
class StormcloudVoidzone(BossModule module) : Components.Voidzone(module, 8, OID.Stormcloud);

class DeadlyDischarge(BossModule module) : Components.StandardAOEs(module, AID.DeadlyDischarge, new AOEShapeRect(40, 5));
class DeadlyDischargeKnockback(BossModule module) : Components.Knockback(module, AID.DeadlyDischarge)
{
    private WPos _from;
    private WPos _to;
    private DateTime _activation;

    private static WPos NearestOnSegment(WPos p, WPos a, WPos b)
    {
        var ab = b - a;
        var lenSq = ab.LengthSq();
        if (lenSq < 0.01f)
            return a;
        var t = Math.Clamp((p - a).Dot(ab) / lenSq, 0, 1);
        return a + t * ab;
    }

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        if (_activation == default)
            yield break;
        yield return new(NearestOnSegment(actor.Position, _from, _to), 12, _activation, Kind: Kind.AwayFromOrigin);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            var dir = spell.Rotation.ToDirection();
            _from = caster.Position;
            _to = caster.Position + 40 * dir;
            _activation = Module.CastFinishAt(spell);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            _activation = default;
            _from = default;
            _to = default;
            ++NumCasts;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var s in Sources(slot, actor))
        {
            if (IsImmune(slot, s.Activation))
                continue;
            var from = _from;
            var to = _to;
            var dist = s.Distance;
            hints.AddForbiddenZone(Sdf.Discrete(p =>
            {
                var origin = NearestOnSegment(p, from, to);
                return !Module.InBounds(AwayFromSource(p, origin, dist));
            }), s.Activation);
        }
    }
}

class Gallop(BossModule module) : Components.StandardAOEs(module, AID.Gallop, new AOEShapeRect(50, 2.5f));

class ShockStrike(BossModule module) : Components.StandardAOEs(module, AID.ShockStrike, 3);
class VoltStrike(BossModule module) : Components.StandardAOEs(module, AID.VoltStrike, 5);

class E05RamuhStates : StateMachineBuilder
{
    public E05RamuhStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<CripplingBlow>()
            .ActivateOnEnter<Impact>()
            .ActivateOnEnter<JudgmentJolt>()
            .ActivateOnEnter<JudgmentVolts>()
            .ActivateOnEnter<DivineJudgmentVolts>()
            .ActivateOnEnter<SurgeOrbs>()
            .ActivateOnEnter<StormcloudBait>()
            .ActivateOnEnter<StormcloudVoidzone>()
            .ActivateOnEnter<DeadlyDischarge>()
            .ActivateOnEnter<DeadlyDischargeKnockback>()
            .ActivateOnEnter<Gallop>()
            .ActivateOnEnter<ShockStrike>()
            .ActivateOnEnter<VoltStrike>();
    }
}

[ModuleInfo(Contributors = "croizat", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 715, NameID = 9281)]
public class E05Ramuh(ModuleInit init) : BossModule(init, new(100, 100), new ArenaBoundsRect(20, 15));
