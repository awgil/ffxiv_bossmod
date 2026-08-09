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
    SparklingSprinkling = 36281 // Helper->player, 5.0s cast, range 5 circle
}

public enum TetherID : uint
{
    Freeze = 272 // RorrlohTeh/QorrlohTeh1->Boss
}

sealed class ArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private readonly AOEShapeDonut donut = new(20f, 23f);
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.FrostingFracas && Arena.Bounds.Radius != 20f)
        {
            var center = Arena.Center;
            _aoe = [new(donut, center, default, Module.CastFinishAt(spell, 0.6d), shapeDistance: donut.Distance(center, default))];
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x17 && state == 0x00020001u)
        {
            var arena = new ArenaBoundsCustom([new Polygon(new(-108f, 119f), 20f, 52)]);
            Arena.Bounds = arena;
            Arena.Center = arena.Center;
            _aoe = [];
        }
    }
}

sealed class IceScreamFrozenSwirl(BossModule module) : Components.GenericAOEs(module)
{
    private readonly AOEShapeRect rect = new(20f, 10f);
    private readonly AOEShapeCircle circle = new(15f);
    private readonly List<AOEInstance> _aoes = [with(8)];
    private int tetherCount;
    private byte tutorial;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (tetherCount == default)
            return [];

        var aoes = CollectionsMarshal.AsSpan(_aoes);
        var isTutorial = tutorial < 2u;
        var len = aoes.Length;
        var max = len > 1 && isTutorial ? 2 : len > 3 && !isTutorial ? 4 : len;
        return aoes[..max];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        AOEShape? shape = spell.Action.ID switch
        {
            (uint)AID.IceScream => rect,
            (uint)AID.FrozenSwirlVisual => circle,
            _ => null
        };
        if (shape != null)
            _aoes.Add(new(shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell), actorID: caster.InstanceID));
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.Freeze)
        {
            var count = _aoes.Count;
            var id = source.InstanceID;
            var aoes = CollectionsMarshal.AsSpan(_aoes);
            for (var i = 0; i < count; ++i)
            {
                ref var aoe = ref aoes[i];
                if (aoe.ActorID == id)
                {
                    aoe.Activation = WorldState.FutureTime(14.9d);
                    ++tetherCount;
                    var isTutorial = tutorial < 2u;
                    if (isTutorial && tetherCount == 2 || !isTutorial && tetherCount == 4)
                    {
                        aoes.Sort((x, y) => x.Activation.CompareTo(y.Activation));
                    }
                    return;
                }
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.IceScream or (uint)AID.FrozenSwirlVisual)
        {
            var count = _aoes.Count;
            var id = caster.InstanceID;
            var aoes = CollectionsMarshal.AsSpan(_aoes);
            for (var i = 0; i < count; ++i)
            {
                ref var aoe = ref aoes[i];
                if (aoe.ActorID == id)
                {
                    _aoes.RemoveAt(i);
                    if (_aoes.Count == 0)
                    {
                        ++tutorial;
                        tetherCount = default;
                    }
                    return;
                }
            }
        }
    }
}

sealed class FrostingFracas(BossModule module) : Components.RaidwideCast(module, (uint)AID.FrostingFracas);
sealed class SnowBoulder(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SnowBoulder, new AOEShapeRect(50f, 3f), 6);
sealed class SparklingSprinkling(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.SparklingSprinkling, 5f);

sealed class D021RyoqorTertehStates : StateMachineBuilder
{
    public D021RyoqorTertehStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChange>()
            .ActivateOnEnter<FrostingFracas>()
            .ActivateOnEnter<IceScreamFrozenSwirl>()
            .ActivateOnEnter<SnowBoulder>()
            .ActivateOnEnter<SparklingSprinkling>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.AISupport, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 824u, NameID = 12699u)]
public sealed class D021RyoqorTerteh : BossModule
{
    public D021RyoqorTerteh(WorldState ws, Actor primary) : this(ws, primary, BuildArena()) { }

    private D021RyoqorTerteh(WorldState ws, Actor primary, (WPos center, ArenaBoundsCustom arena) a) : base(ws, primary, a.center, a.arena) { }

    private static (WPos center, ArenaBoundsCustom arena) BuildArena()
    {
        var arena = new ArenaBoundsCustom([new Polygon(new(-108f, 119f), 22.5f, 52)], [new Rectangle(new(-108f, 141.95f), 20f, 1.25f), new Rectangle(new(-108f, 96.25f), 20f, 1.25f)]);
        return (arena.Center, arena);
    }
}
