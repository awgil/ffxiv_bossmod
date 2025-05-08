namespace BossMod.Dawntrail.Dungeon.D02WorqorZormor.D021RyoqorTerteh;

public enum OID : uint
{
    Boss = 0x4159, // R5.280, x1
    Helper = 0x233C, // R0.500, x4, Helper type
    QorrlohTehHelper = 0x43A2, // R0.500, x4
    QorrlohTeh = 0x415A, // R3.000, x0 (spawn during fight) - circle aoe add
    RorrlohTeh = 0x415B, // R1.500, x0 (spawn during fight) - rect aoe add
    Snowball = 0x415C, // R2.500, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 872, // Boss/Snowball->player, no cast, single-target
    FrostingFracas = 36279, // Boss->self, 5.0s cast, single-target, visual (raidwide)
    FrostingFracasAOE = 36280, // Helper->self, 5.0s cast, range 60 circle, raidwide
    FluffleUp = 36265, // Boss->self, 4.0s cast, single-target, visual (spawn adds)
    ColdFeat = 36266, // Boss->self, 4.0s cast, single-target, visual (freeze adds)
    IceScream = 36270, // RorrlohTeh->self, 12.0s cast, range 20 width 20 rect
    FrozenSwirl = 36271, // QorrlohTeh->self, 12.0s cast, single-target, visual (circle aoe)
    FrozenSwirlAOE = 36272, // QorrlohTehHelper->self, 12.0s cast, range 15 circle
    Snowscoop = 36275, // Boss->self, 4.0s cast, single-target, visual (spawn snowballs)
    SnowBoulder = 36278, // Snowball->self, 4.0s cast, range 50 width 6 rect
    SparklingSprinkling = 36713, // Boss->self, 5.0s cast, single-target, visual (spread)
    SparklingSprinklingAOE = 36281, // Helper->player, 5.0s cast, range 5 circle spread
}

public enum IconID : uint
{
    SparklingSprinkling = 376, // player
}

public enum TetherID : uint
{
    Freeze = 272, // RorrlohTeh/QorrlohTeh->Boss
}

class FrostingFracas(BossModule module) : Components.RaidwideCast(module, AID.FrostingFracasAOE);

abstract class FreezableAOEs(BossModule module, Enum action, AOEShape shape) : Components.GenericAOEs(module, action)
{
    private readonly List<AOEInstance> _aoes = [];
    private int _numFrozen;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _numFrozen >= 2 ? _aoes.Take(2) : [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            _aoes.Add(new(shape, caster.Position, spell.Rotation, Module.CastFinishAt(spell)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction && _aoes.Count > 0)
        {
            _aoes.RemoveAt(0);
            if (_aoes.Count == 0)
                _numFrozen = 0;
        }
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.Freeze && _aoes.FindIndex(aoe => aoe.Origin.AlmostEqual(source.Position, 1)) is var index && index >= 0)
        {
            ++_numFrozen;
            var aoe = _aoes[index];
            aoe.Activation += TimeSpan.FromSeconds(8);
            _aoes.Add(aoe);
            _aoes.RemoveAt(index);
        }
    }
}
class IceScream(BossModule module) : FreezableAOEs(module, AID.IceScream, new AOEShapeRect(20, 10));
class FrozenSwirl(BossModule module) : FreezableAOEs(module, AID.FrozenSwirl, new AOEShapeCircle(15)); // note that helpers that cast visual cast are tethered

class SnowBoulder(BossModule module) : Components.StandardAOEs(module, AID.SnowBoulder, new AOEShapeRect(50, 3), 6);
class SparklingSprinkling(BossModule module) : Components.SpreadFromCastTargets(module, AID.SparklingSprinklingAOE, 5);

class D021RyoqorTertehStates : StateMachineBuilder
{
    public D021RyoqorTertehStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<FrostingFracas>()
            .ActivateOnEnter<IceScream>()
            .ActivateOnEnter<FrozenSwirl>()
            .ActivateOnEnter<SnowBoulder>()
            .ActivateOnEnter<SparklingSprinkling>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "xan", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 824, NameID = 12699)]
public class D021RyoqorTerteh(WorldState ws, Actor primary) : BossModule(ws, primary, new(-108, 119), new ArenaBoundsCircle(20));
