namespace BossMod.Dawntrail.Dungeon.D02WorqorZormor.D021RyoqorTerteh;

public enum OID : uint
{
    Boss = 0x4159, // R5.28
    RorrlohTeh = 0x415B, // R1.5
    QorrlohTeh1 = 0x415A, // R3.0
    QorrlohTeh2 = 0x43A2, // R0.5
    Snowball = 0x415C, // R2.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target

    FrostingFracasVisual = 36279, // Boss->self, 5.0s cast, single-target
    FrostingFracas = 36280, // Helper->self, 5.0s cast, range 60 circle

    FluffleUp = 36265, // Boss->self, 4.0s cast, single-target
    ColdFeat = 36266, // Boss->self, 4.0s cast, single-target
    IceScream = 36270, // RorrlohTeh->self, 12.0s cast, range 20 width 20 rect

    FrozenSwirlVisual = 36271, // QorrlohTeh1->self, 12.0s cast, single-target
    FrozenSwirl = 36272, // QorrlohTeh2->self, 12.0s cast, range 15 circle

    Snowscoop = 36275, // Boss->self, 4.0s cast, single-target
    SnowBoulder = 36278, // Snowball->self, 4.0s cast, range 50 width 6 rect

    SparklingSprinklingVisual = 36713, // Boss->self, 5.0s cast, single-target
    SparklingSprinkling = 36281, // Helper->player, 5.0s cast, range 5 circle
}

public enum TetherID : uint
{
    Freeze = 272, // RorrlohTeh/QorrlohTeh1->Boss
}

class FrostingFracasVoidzone(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeDonut donut = new(20, 22.5f);
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.FrostingFracas && Module.Arena.Bounds == D021RyoqorTerteh.StartingBounds)
            _aoe = new(donut, Module.Center, default, spell.NPCFinishAt.AddSeconds(0.6f));
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (state == 0x00020001 && index == 0x17)
        {
            Module.Arena.Bounds = D021RyoqorTerteh.DefaultBounds;
            _aoe = null;
        }
    }
}

class IceScreamFrozenSwirl(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect rect = new(20, 10);
    private static readonly AOEShapeCircle circle = new(15);
    private readonly List<AOEInstance> _aoesCircle = [];
    private readonly List<AOEInstance> _aoesRect = [];
    private readonly HashSet<Actor> circleAOE = [];
    private readonly HashSet<Actor> rectAOE = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoesCircle.Take(2).Concat(_aoesRect.Take(2));

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.QorrlohTeh1)
            circleAOE.Add(actor);
        else if ((OID)actor.OID == OID.RorrlohTeh)
            rectAOE.Add(actor);
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.Freeze)
        {
            var activation1 = Module.WorldState.FutureTime(9.9f);
            var activation2 = Module.WorldState.FutureTime(14.9f);
            if (circleAOE.Contains(source))
            {
                _aoesCircle.Add(new(circle, source.Position, default, activation2));
                circleAOE.Remove(source);
                if (_aoesCircle.Count == 2)
                {
                    foreach (var e in circleAOE)
                        _aoesCircle.Add(new(circle, e.Position, default, activation1));
                    circleAOE.Clear();
                    _aoesCircle.SortBy(e => e.Activation);
                }
            }
            else if (rectAOE.Contains(source))
            {
                _aoesRect.Add(new(rect, source.Position, source.Rotation, activation2));
                rectAOE.Remove(source);
                if (_aoesRect.Count == 2)
                {
                    foreach (var e in rectAOE)
                        _aoesRect.Add(new(rect, e.Position, e.Rotation, activation1));
                    rectAOE.Clear();
                    _aoesRect.SortBy(e => e.Activation);
                }
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoesRect.Count > 0 && (AID)spell.Action.ID == AID.IceScream)
            _aoesRect.RemoveAt(0);
        else if (_aoesCircle.Count > 0 && (AID)spell.Action.ID == AID.FrozenSwirl)
            _aoesCircle.RemoveAt(0);
    }
}

class FrostingFracas(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.FrostingFracas));
class SnowBoulder(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.SnowBoulder), new AOEShapeRect(50, 3), 6);
class SparklingSprinkling(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.SparklingSprinkling), 5);

class D021RyoqorTertehStates : StateMachineBuilder
{
    public D021RyoqorTertehStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<FrostingFracasVoidzone>()
            .ActivateOnEnter<FrostingFracas>()
            .ActivateOnEnter<IceScreamFrozenSwirl>()
            .ActivateOnEnter<SnowBoulder>()
            .ActivateOnEnter<SparklingSprinkling>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 824, NameID = 12699)]
public class D021RyoqorTerteh(WorldState ws, Actor primary) : BossModule(ws, primary, new(-108, 119), StartingBounds)
{
    public static readonly ArenaBounds StartingBounds = new ArenaBoundsCircle(22.5f);
    public static readonly ArenaBounds DefaultBounds = new ArenaBoundsCircle(20);
}
