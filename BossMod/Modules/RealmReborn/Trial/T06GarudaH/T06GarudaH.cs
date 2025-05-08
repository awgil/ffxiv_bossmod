namespace BossMod.RealmReborn.Trial.T06GarudaH;

public enum OID : uint
{
    Boss = 0xF2, // x1
    Monolith = 0xF0, // x4
    Whirlwind = 0x623, // x7
    EyeOfTheStorm = 0x624, // x1
    RazorPlume = 0xF1, // spawn during fight
    SatinPlume = 0x5FF, // spawn during fight
    Chirada = 0x61E, // spawn during fight
    Suparna = 0x61F, // spawn during fight
    Monolith1 = 0x1E8706, // x1, EventObj type
    Monolith2 = 0x1E8707, // x1, EventObj type
    Monolith3 = 0x1E8708, // x1, EventObj type
    Monolith4 = 0x1E8709, // x1, EventObj type
}

public enum AID : uint
{
    AutoAttack = 870, // Boss/Chirada/Suparna->player, no cast, single-target
    Friction = 1379, // Boss/Chirada/Suparna->players, no cast, range 5 circle at random target
    Downburst = 1380, // Boss/Chirada/Suparna->self, no cast, range 10+R ?-degree cone cleave
    WickedWheel = 1381, // Boss/Chirada/Suparna->self, no cast, range 7+R circle cleave
    Slipstream = 1382, // Boss/Chirada/Suparna->self, 2.5s cast, range 10+R ?-degree cone aoe
    MistralShriek = 1384, // Boss/Chirada/Suparna->self, 3.0s cast, range 23+R LOSable raidwide (from center); adds start casting if not killed in ~44s
    MistralSong = 1390, // Boss->self, 3.0s cast, range 30+R circle LOSable raidwide (from side)
    AerialBlast = 1385, // Boss->self, 4.0s cast, raidwide
    GreatWhirlwind = 1386, // Whirlwind->location, 3.0s cast, range 8 circle aoe with knockback 15
    EyeOfTheStorm = 1387, // EyeOfTheStorm->self, 3.0s cast, range 12-25 donut
    Featherlance = 1388, // RazorPlume->self, no cast, range 8 circle, suicide attack if not killed in ~25s
    ThermalTumult = 1389, // SatinPlume->self, no cast, range 6 circle, suicide attack (applies sleep) if not killed in ~25s
}

public enum TetherID : uint
{
    Rehabilitation = 4, // Chirada/Suparna->Boss, green (heal)
    DamageUp = 11, // Chirada/Suparna->Boss, red (damage up)
}

// disallow clipping monoliths
class Friction(BossModule module) : BossComponent(module)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Module.PrimaryActor.CastInfo == null) // don't forbid standing near monoliths while boss is casting to allow avoiding aoes
            foreach (var m in ((T06GarudaH)Module).ActiveMonoliths)
                hints.AddForbiddenZone(ShapeContains.Circle(m.Position, 5));
    }
}

class Downburst(BossModule module) : Components.Cleave(module, AID.Downburst, new AOEShapeCone(11.7f, 60.Degrees()));
class Slipstream(BossModule module) : Components.StandardAOEs(module, AID.Slipstream, new AOEShapeCone(11.7f, 45.Degrees()));

class MistralShriek(BossModule module) : Components.CastLineOfSightAOE(module, AID.MistralShriek, 24.7f, true)
{
    public override IEnumerable<Actor> BlockerActors() => ((T06GarudaH)Module).ActiveMonoliths;
}

class MistralSong(BossModule module) : Components.CastLineOfSightAOE(module, AID.MistralSong, 31.7f, true)
{
    public override IEnumerable<Actor> BlockerActors() => ((T06GarudaH)Module).ActiveMonoliths;
}

class AerialBlast(BossModule module) : Components.RaidwideCast(module, AID.AerialBlast);
class GreatWhirlwind(BossModule module) : Components.StandardAOEs(module, AID.GreatWhirlwind, 8);
class EyeOfTheStorm(BossModule module) : Components.StandardAOEs(module, AID.EyeOfTheStorm, new AOEShapeDonut(12, 25));

class T06GarudaHStates : StateMachineBuilder
{
    public T06GarudaHStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Friction>()
            .ActivateOnEnter<Downburst>()
            .ActivateOnEnter<Slipstream>()
            .ActivateOnEnter<MistralShriek>()
            .ActivateOnEnter<MistralSong>()
            .ActivateOnEnter<AerialBlast>()
            .ActivateOnEnter<GreatWhirlwind>()
            .ActivateOnEnter<EyeOfTheStorm>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 61, NameID = 1644)]
public class T06GarudaH : BossModule
{
    private readonly IReadOnlyList<Actor> _monoliths;
    public IEnumerable<Actor> ActiveMonoliths => _monoliths;

    public T06GarudaH(WorldState ws, Actor primary) : base(ws, primary, new(0, 0), new ArenaBoundsCircle(22))
    {
        _monoliths = Enemies(OID.Monolith);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
        {
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.Suparna or OID.Chirada => e.Actor.Tether.ID == (uint)TetherID.Rehabilitation ? 3 : 2,
                OID.SatinPlume => 3,
                OID.RazorPlume => 2,
                OID.Boss => 1,
                _ => 0
            };
        }
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        foreach (var m in ActiveMonoliths)
            Arena.Actor(m, ArenaColor.Object, true);
        foreach (var e in Enemies(OID.Suparna))
            Arena.Actor(e, ArenaColor.Enemy);
        foreach (var e in Enemies(OID.Chirada))
            Arena.Actor(e, ArenaColor.Enemy);
    }
}
