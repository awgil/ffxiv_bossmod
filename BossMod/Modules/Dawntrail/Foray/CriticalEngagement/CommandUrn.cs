namespace BossMod.Dawntrail.Foray.CriticalEngagement.CommandUrn;

public enum OID : uint
{
    Boss = 0x46E1, // R4.920, x1
    Helper = 0x233C, // R0.500, x27, Helper type
    CommandUrn = 0x4739, // R1.000, x2
    VassalVessel = 0x46E2, // R2.210, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 37821, // Boss->player, no cast, single-target
    Summon = 41416, // Boss->self, 3.0s cast, single-target
    Assail = 41417, // Boss->self, 3.0s cast, single-target
    StoneSwell1 = 41420, // VassalVessel->self, 1.0s cast, range 16 circle
    StoneSwell2 = 39470, // VassalVessel->self, 1.0s cast, range 16 circle
    Rockslide1 = 39471, // VassalVessel->self, 1.0s cast, range 40 width 10 cross
    Rockslide2 = 41421, // VassalVessel->self, 1.0s cast, range 40 width 10 cross
    AethericBurst = 41425, // Helper->self, 5.0s cast, ???
    AethericBurstCast = 41424, // Boss->self, 5.0s cast, single-target
    AutoAttackVessel = 43116, // VassalVessel->player, no cast, single-target
    Destruct = 41422, // Boss->self, 3.0s cast, single-target
    SelfDestructCast = 41423, // VassalVessel->self, no cast, single-target
    SelfDestruct = 39094, // Helper->self, no cast, ???
}

public enum TetherID : uint
{
    StoneSwell = 303, // 4739/46E2->46E2
    Rockslide = 304, // 4739/46E2->46E2
    Net = 302, // 46E2->4739
    StoneSwellReverse = 306, // 46E2->4739
    SelfDestruct = 305, // 46E2->4739
}

class AethericBurst(BossModule module) : Components.RaidwideCast(module, AID.AethericBurstCast);

abstract class TetherAOEs(BossModule module, AOEShape shape, TetherID tetherID, AID[] actions, bool reverse = false) : Components.GenericAOEs(module)
{
    private readonly List<(Actor Caster, DateTime Activation)> _casters = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _casters.Select(c => new AOEInstance(shape, c.Caster.Position, Activation: c.Activation));

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == tetherID && WorldState.Actors.Find(tether.Target) is { } tar)
            _casters.Add((reverse ? source : tar, WorldState.FutureTime(6)));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (actions.Contains((AID)spell.Action.ID))
            _casters.RemoveAll(c => c.Caster == caster);
    }
}

class StoneSwellTether(BossModule module) : TetherAOEs(module, new AOEShapeCircle(16), TetherID.StoneSwell, [AID.StoneSwell1, AID.StoneSwell2]);
class RockslideTether(BossModule module) : TetherAOEs(module, new AOEShapeCross(40, 5), TetherID.Rockslide, [AID.Rockslide2, AID.Rockslide1]);
class StoneSwell(BossModule module) : Components.GroupedAOEs(module, [AID.StoneSwell1, AID.StoneSwell2], new AOEShapeCircle(16));
class Rockslide(BossModule module) : Components.GroupedAOEs(module, [AID.Rockslide2, AID.Rockslide1], new AOEShapeCross(40, 5));
class StoneSwellClockwise(BossModule module) : TetherAOEs(module, new AOEShapeCircle(16), TetherID.StoneSwellReverse, [AID.StoneSwell1, AID.StoneSwell2], true);
class VassalVessel(BossModule module) : Components.Adds(module, (uint)OID.VassalVessel);

class SelfDestruct(BossModule module) : BossComponent(module)
{
    private readonly List<(Actor, DateTime)> _targets = [];

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var nextExplosion = DateTime.MinValue;

        foreach (var t in _targets)
        {
            if (nextExplosion == default)
                nextExplosion = t.Item2.AddSeconds(1);
            hints.SetPriority(t.Item1, t.Item2 < nextExplosion ? 2 : 1);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.SelfDestructCast)
            _targets.RemoveAll(t => t.Item1 == caster);
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (_targets.Count > 0)
        {
            var left = Math.Max(0, (_targets[0].Item2 - WorldState.CurrentTime).TotalSeconds);
            hints.Add($"Adds enrage in {left:f1}s!");
        }
    }

    public override void Update()
    {
        for (var i = _targets.Count - 1; i >= 0; --i)
        {
            var t = _targets[i].Item1;
            if (t.PendingDead || t.IsDeadOrDestroyed)
                _targets.RemoveAt(i);
        }
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == TetherID.SelfDestruct)
            _targets.Add((source, WorldState.FutureTime(23)));
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var nextExplosion = DateTime.MinValue;

        foreach (var t in _targets)
        {
            if (nextExplosion == default)
                nextExplosion = t.Item2.AddSeconds(1);
            if (t.Item2 < nextExplosion)
                Arena.AddCircle(t.Item1.Position, 1.5f, ArenaColor.Danger);
        }
    }
}

class CommandUrnStates : StateMachineBuilder
{
    public CommandUrnStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AethericBurst>()
            .ActivateOnEnter<StoneSwellTether>()
            .ActivateOnEnter<StoneSwell>()
            .ActivateOnEnter<StoneSwellClockwise>()
            .ActivateOnEnter<RockslideTether>()
            .ActivateOnEnter<Rockslide>()
            .ActivateOnEnter<VassalVessel>()
            .ActivateOnEnter<SelfDestruct>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1018, NameID = 13814)]
public class CommandUrn(WorldState ws, Actor primary) : BossModule(ws, primary, new(-352, -608), new ArenaBoundsCircle(19.5f))
{
    public override bool DrawAllPlayers => true;
}
