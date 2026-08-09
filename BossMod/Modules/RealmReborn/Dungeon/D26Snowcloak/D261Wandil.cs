namespace BossMod.RealmReborn.Dungeon.D26Snowcloak.D261Wandil;

public enum OID : uint
{
    Boss = 0xD07, // R3.23
    FrostBomb = 0xD09, // R0.9
    Voidzone = 0x1E9661, // R2.0
    Helper = 0xD2E
}

public enum AID : uint
{
    AutoAttack = 872, // Boss/D09->player, no cast, single-target

    SnowDriftVisual = 3080, // Boss->self, 5.0s cast, single-target
    SnowDrift = 3079, // Helper->self, no cast, range 80+R circle
    IceGuillotine = 3084, // Boss->player, no cast, range 8+R ?-degree cone
    ColdWaveVisual = 3083, // Boss->self, 3.0s cast, ???
    ColdWave = 3111, // Helper->location, 4.0s cast, range 8 circle
    Tundra = 3082, // Boss->self, 3.0s cast, single-target
    HypothermalCombustion = 3085 // FrostBomb->self, 3.0s cast, range 80+R circle
}

sealed class TundraArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];
    public static Polygon[] GetSmallPolygon() => [new Polygon(new(new(56.41631f, -88.51856f)), 12f, 20)];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (state == 0x00040008u && actor.OID == (uint)OID.Voidzone)
        {
            var arena = new ArenaBoundsCustom(GetSmallPolygon());
            Arena.Bounds = arena;
            Arena.Center = arena.Center;
            _aoe = [];
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Tundra)
        {
            var center = Arena.Center;
            var shape = new AOEShapeCustom(center, [new Square(new(56.41631f, -88.51856f), 20f)], GetSmallPolygon());
            _aoe = [new(shape, center, default, Module.CastFinishAt(spell, 2d), shapeDistance: shape.Distance(center, default))];
        }
    }
}

sealed class IceGuillotine(BossModule module) : Components.Cleave(module, (uint)AID.IceGuillotine, new AOEShapeCone(11.23f, 60f.Degrees()), activeWhileCasting: false);
sealed class SnowDriftRaidwide(BossModule module) : Components.RaidwideCastDelay(module, (uint)AID.SnowDriftVisual, (uint)AID.SnowDrift, 2d);
sealed class SnowDriftMove(BossModule module) : Components.StayMove(module, 3d)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.SnowDriftVisual)
        {
            Array.Fill(PlayerStates, new(Requirement.Move, Module.CastFinishAt(spell, 2d)));
        }
    }
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.SnowDrift)
        {
            Array.Clear(PlayerStates);
        }
    }
}

sealed class ColdWave(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ColdWave, 8f);

sealed class D261WandilStates : StateMachineBuilder
{
    public D261WandilStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SnowDriftRaidwide>()
            .ActivateOnEnter<SnowDriftMove>()
            .ActivateOnEnter<ColdWave>()
            .ActivateOnEnter<IceGuillotine>()
            .ActivateOnEnter<TundraArenaChange>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 27u, NameID = 3038u, SortOrder = 1)]
public sealed class D261Wandil : BossModule
{
    public D261Wandil(WorldState ws, Actor primary) : this(ws, primary, BuildArena()) { }

    private D261Wandil(WorldState ws, Actor primary, (WPos center, ArenaBoundsCustom arena) a) : base(ws, primary, a.center, a.arena) { }

    private static (WPos center, ArenaBoundsCustom arena) BuildArena()
    {
        var arena = new ArenaBoundsCustom([new PolygonCustom([new(55.95f, -106.95f), new(65.84f, -103.79f), new(66.36f, -103.43f),
        new(70.19f, -99.6f), new(71.49f, -97.2f),
        new(71.59f, -96.53f), new(71.53f, -95.91f), new(71.88f, -95.48f), new(72.77f, -94.6f), new(73.28f, -94.2f),
        new(73.49f, -93.58f), new(74.29f, -88.55f), new(74.26f, -87.89f), new(73.45f, -82.74f), new(71.07f, -78.03f),
        new(70.70f, -77.48f), new(67.01f, -73.79f), new(66.44f, -73.42f), new(61.82f, -71.07f), new(56.1f, -70.18f),
        new(50.75f, -70.93f), new(42.64f, -75.96f), new(42.17f, -76.42f), new(42.01f, -77.06f), new(38.99f, -82.65f),
        new(38.09f, -88.44f), new(38.92f, -93.64f), new(39.14f, -94.25f), new(41.39f, -98.67f), new(41.77f, -99.2f),
        new(45.45f, -102.88f), new(50.44f, -105.47f), new(55.27f, -106.93f), new(55.95f, -106.95f)])]);
        return (arena.Center, arena);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.FrostBomb => 1,
                _ => 0
            };
        }
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.FrostBomb));
    }
}
