namespace BossMod.Dawntrail.Dungeon.D09YuweyawataFieldStation.D092OverseerKanilokka;

public enum OID : uint
{
    Boss = 0x464A, // R9.0
    RawElectrope = 0x4642, // R1.0
    PreservedSoul = 0x464B, // R2.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 40659, // Boss->player, no cast, single-target

    DarkSouls = 40658, // Boss->player, 5.0s cast, single-target

    FreeSpiritsVisual = 40639, // Boss->self, 4.0+1,0s cast, single-target
    FreeSpirits = 40640, // Helper->self, 5.0s cast, range 20 circle

    Soulweave1 = 40642, // PreservedSoul->self, 2.5s cast, range 28-32 donut
    Soulweave2 = 40641, // PreservedSoul->self, 2.5s cast, range 28-32 donut

    PhantomFloodVisual = 40643, // Boss->self, 3.7+1,3s cast, single-target
    PhantomFlood = 40644, // Helper->self, 5.0s cast, range 5-20 donut

    DarkIIVisual1 = 40654, // Boss->self, 4.5+0,5s cast, single-target
    DarkIIVisual2 = 40655, // Boss->self, no cast, single-target
    DarkII1 = 40656, // Helper->self, 5.0s cast, range 35 30-degree cone
    DarkII2 = 40657, // Helper->self, 7.5s cast, range 35 30-degree cone

    TelltaleTears = 40649, // Helper->players, 5.0s cast, range 5 circle, spread
    LostHope = 40645, // Boss->self, 3.0s cast, range 20 circle
    Necrohazard = 40646, // Boss->self, 15.0s cast, range 20 circle, damage fall off AOE, very extreme damage if not almost at the border
    Bloodburst = 40647, // Boss->self, 5.0s cast, range 45 circle
    SoulDouse = 40651, // Helper->players, 5.0s cast, range 6 circle
}

sealed class ArenaChanges(BossModule module) : Components.GenericAOEs(module)
{
    private readonly AOEShapeDonut donutSmall = new(5f, 15f), donutBig = new(15f, 20f);
    public readonly Polygon[] tinyPolygon = [new Polygon(new(116f, -66f), 5f, 64)];
    private readonly WPos[] vertices02000100West = [new(111.116f, -65.833f), new(110.654f, -65.493f), new(107.256f, -65.506f), new(105.557f, -65.231f), new(104.614f, -64.362f),
    new(103.623f, -62.397f), new(103.125f, -60.408f), new(103.742f, -59.448f), new(103.925f, -57.932f), new(102.756f, -57.151f), new(100.074f, -58.462f), new(98.007f, -57.349f),
    new(98.367f, -56.588f), new(99.369f, -54.89f), new(99.472f, -54.752f), new(100.749f, -55.538f), new(103.599f, -53.968f), new(104.63f, -54.63f), new(106.026f, -55.591f),
    new(106.706f, -56.606f), new(106.861f, -58.172f), new(106.885f, -58.519f), new(107.005f, -59.99f), new(107.231f, -61.292f), new(108.879f, -62.314f), new(110.425f, -62.275f),
    new(111.901f, -61.901f), new(113.179f, -61.779f), new(113.27f, -61.814f)];
    private readonly WPos[] vertices02000100North = [new(118.832f, -69.987f), new(118.838f, -70.248f), new(118.475f, -72.109f), new(117.978f, -72.52f), new(114.363f, -74.935f),
    new(114.086f, -75.623f), new(114.363f, -76.492f), new(114.925f, -76.961f), new(116f, -77.189f), new(117.126f, -77.333f), new(118.369f, -77.708f), new(119.344f, -78.285f),
    new(120.082f, -78.855f), new(120.657f, -80.633f), new(120.255f, -83.257f), new(119.559f, -83.894f), new(117.427f, -85.064f), new(117.477f, -85.922f), new(116f, -86f),
    new(114.535f, -85.923f), new(114.873f, -84.137f), new(116f, -83.292f), new(117.605f, -82.243f), new(117.465f, -80.887f), new(117.088f, -80.113f), new(116f, -79.851f),
    new(114.658f, -79.729f), new(113.371f, -79.419f), new(112.084f, -78.91f), new(111.137f, -77.537f), new(110.837f, -76.159f), new(110.837f, -74.844f), new(113.357f, -72.382f),
    new(113.314f, -71.026f), new(113.162f, -70.247f), new(112.787f, -69.83f)];
    private readonly WPos[] vertices02000100East = [new(118.833f, -61.921f), new(119.469f, -61.185f), new(119.413f, -60.144f), new(119.473f, -58.867f), new(119.791f, -57.88f),
    new(120.141f, -57.679f), new(122.108f, -56.658f), new(123.843f, -55.884f), new(124.737f, -55.841f), new(125.679f, -56.271f), new(126.875f, -56.885f), new(127.948f, -57.751f),
    new(128.788f, -59.071f), new(131.158f, -58.659f), new(131.481f, -57.725f), new(131.425f, -55.926f), new(131.531f, -55.377f), new(132.491f, -54.745f), new(132.629f, -54.889f),
    new(133.638f, -56.572f), new(133.955f, -57.21f), new(133.725f, -57.423f), new(133.559f, -60.674f), new(131.641f, -62.617f), new(129.075f, -62.622f), new(126.341f, -60.472f),
    new(124.5f, -59.225f), new(122.207f, -60.495f), new(122.479f, -61.989f), new(122.001f, -63.285f), new(121.616f, -64.139f), new(121.34f, -64.649f), new(120.889f, -65.637f)];
    private readonly WPos[] vertices00800040North = [new(119.75f, -69.197f), new(119.8f, -72.259f), new(117.464f, -76.648f), new(116.16f, -78.381f), new(117.654f, -79.69f),
    new(121.004f, -81.108f), new(123.091f, -84.666f), new(121.806f, -85.139f), new(119.902f, -85.616f), new(119.865f, -85.432f), new(117.019f, -83.869f), new(113.634f, -80.926f),
    new(112.872f, -77.57f), new(115.792f, -72.885f), new(113.913f, -70.537f)];
    private readonly WPos[] vertices00800040East = [new(119.843f, -62.908f), new(122.912f, -63.585f), new(126.417f, -65.231f), new(127.875f, -67.423f), new(129.415f, -67.738f),
    new(131.682f, -64.324f), new(135.734f, -62.999f), new(135.904f, -64.04f), new(136f, -66f), new(133.316f, -66.632f), new(131.885f, -70.919f), new(127.118f, -71.718f),
    new(124.3f, -68.396f), new(122.692f, -67.635f), new(120.364f, -68.278f)];
    private readonly WPos[] vertices00800040South = [new(112.599f, -62.343f), new(112.144f, -60.134f), new(107.746f, -57.919f), new(106.949f, -53.585f), new(111.361f, -49.956f),
    new(110.741f, -46.737f), new(112.098f, -46.384f), new(113.836f, -46.127f), new(114.178f, -47.5f), new(115.156f, -51.21f), new(111.583f, -53.679f), new(111.439f, -55.538f),
    new(116.423f, -58.691f), new(117.812f, -61.417f)];
    private readonly WPos[] vertices00800040West = [new(112.885f, -69.813f), new(110.681f, -70.201f), new(108.074f, -73.1f), new(103.282f, -73.098f), new(100.933f, -70.326f),
    new(98.686f, -70.669f), new(98.201f, -71.399f), new(97.212f, -72.791f), new(96.861f, -71.806f), new(96.384f, -69.902f), new(98.13f, -67.76f), new(98.394f, -67.416f),
    new(101.8f, -66.852f), new(104.669f, -69.078f), new(106.579f, -69.753f), new(108.694f, -66.477f), new(111.106f, -65.727f)];

    private readonly DonutV[] difference = [new DonutV(new(116f, -66f), 19.5f, 22f, 64)];

    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var id = spell.Action.ID;
        if (id == (uint)AID.FreeSpirits)
        {
            AddAOE(donutBig);
        }
        else if (id == (uint)AID.PhantomFlood)
        {
            AddAOE(donutSmall);
        }
        void AddAOE(AOEShape shape) => _aoe = [new(shape, Arena.Center.Quantized(), default, Module.CastFinishAt(spell))];
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index != 0x07)
        {
            return;
        }
        switch (state)
        {
            case 0x00020001u:
                SetArena(new([new Polygon(new(116f, -66f), 15f, 64)]));
                break;
            case 0x00200010u:
                SetArena(new(tinyPolygon, MapResolution: 0.1f));
                break;
            case 0x00800040u:
                SetArena(new([new PolygonCustom(vertices00800040North), new PolygonCustom(vertices00800040East),
                new PolygonCustom(vertices00800040South), new PolygonCustom(vertices00800040West), ..tinyPolygon], difference));
                break;
            case 0x02000100u:
                SetArena(new([new PolygonCustom(vertices02000100East), new PolygonCustom(vertices02000100North),
                new PolygonCustom(vertices02000100West), ..tinyPolygon], difference));
                break;
            case 0x00080004u:
                SetArena(D092OverseerKanilokka.BuildArena().arena);
                break;
        }

        void SetArena(ArenaBoundsCustom bounds)
        {
            Arena.Bounds = bounds;
            Arena.Center = bounds.Center;
            _aoe = [];
        }
    }
}

sealed class Soulweave(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeDonut donut = new(28f, 32f);
    private readonly List<AOEInstance> _aoes = [with(10)];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
        {
            return [];
        }
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        ref var aoe0 = ref aoes[0];
        ref var aoeL = ref aoes[^1];
        var deadline = aoe0.Activation.AddSeconds(1.3d);
        var isNotLastSet = aoeL.Activation > deadline;
        var color = Colors.Danger;
        for (var i = 0; i < count; ++i)
        {
            ref var aoe = ref aoes[i];
            if (aoe.Activation < deadline)
            {
                if (isNotLastSet)
                    aoe.Color = color;
                aoe.Risky = true;
            }
        }
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.Soulweave1 or (uint)AID.Soulweave2)
        {
            _aoes.Add(new(donut, spell.LocXZ, default, Module.CastFinishAt(spell), risky: false));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID is (uint)AID.Soulweave1 or (uint)AID.Soulweave2)
        {
            _aoes.RemoveAt(0);
        }
    }
}

sealed class FreeSpirits(BossModule module) : Components.RaidwideCast(module, (uint)AID.FreeSpirits);
sealed class Bloodburst(BossModule module) : Components.RaidwideCast(module, (uint)AID.Bloodburst);
sealed class DarkSouls(BossModule module) : Components.SingleTargetCast(module, (uint)AID.DarkSouls);
sealed class TelltaleTears(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.TelltaleTears, 5f);
sealed class SoulDouse(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.SoulDouse, 6f, 4, 4);

sealed class LostHope(BossModule module) : Components.TemporaryMisdirection(module, (uint)AID.LostHope)
{
    private bool prepare;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.LostHope)
        {
            prepare = true;
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index != 0x07)
        {
            return;
        }

        prepare = false;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (prepare)
        {
            hints.GoalZones.Add(AIHints.GoalSingleTarget(Arena.Center, 1f, 9f));
        }
    }
}

sealed class Necrohazard(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Necrohazard, 18f);
sealed class DarkII(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.DarkII1, (uint)AID.DarkII2], new AOEShapeCone(35f, 15f.Degrees()), 6, 12);

sealed class D092OverseerKanilokkaStates : StateMachineBuilder
{
    public D092OverseerKanilokkaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChanges>()
            .ActivateOnEnter<FreeSpirits>()
            .ActivateOnEnter<Soulweave>()
            .ActivateOnEnter<DarkSouls>()
            .ActivateOnEnter<DarkII>()
            .ActivateOnEnter<TelltaleTears>()
            .ActivateOnEnter<SoulDouse>()
            .ActivateOnEnter<Bloodburst>()
            .ActivateOnEnter<LostHope>()
            .ActivateOnEnter<Necrohazard>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.AISupport, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1008u, NameID = 13634u, SortOrder = 6)]
public sealed class D092OverseerKanilokka : BossModule
{
    public D092OverseerKanilokka(WorldState ws, Actor primary) : this(ws, primary, BuildArena()) { }

    private D092OverseerKanilokka(WorldState ws, Actor primary, (WPos center, ArenaBoundsCustom arena) a) : base(ws, primary, a.center, a.arena) { }

    public static (WPos center, ArenaBoundsCustom arena) BuildArena()
    {
        var arena = new ArenaBoundsCustom([new Polygon(new(116f, -66f), 19.5f, 64)], [new Rectangle(new(116f, -46f), 20f, 1.25f), new Rectangle(new(116f, -86f), 20f, 1.25f)]);
        return (arena.Center, arena);
    }
}
