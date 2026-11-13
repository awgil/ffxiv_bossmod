namespace BossMod.Endwalker.Trial.T03Endsinger;

public enum OID : uint
{
    Boss = 0x34F3, // The Endsinger, Teamcraft index
    Endsinger2 = 0x34F5, // Epigonoi heads, source of Misery
    Endsinger3 = 0x34F6, // The Endsinger, Teamcraft index
    Kakodaimon = 0x34F7, // Teamcraft index
    DoomedStars = 0x34F8, // Teamcraft index
    FeatherOfDespair = 0x34FA, // Teamcraft index
    Helper = 0x233C, // Helper type
}

public enum AID : uint
{
    Elegeia = 26156, // Boss->self, 5.0s cast, raidwide
    ElegeiaAOE = 26618, // Helper->self, no cast, range 40 circle
    ElegeiaUnforgotten = 26242, // Boss->self, 5.0s cast, raidwide
    StellarCollision = 26158, // DoomedStars->self, 7.0s cast, range 30 circle
    StellarCollisionRect = 26164, // DoomedStars->self, 3.0s cast, range 80 width 22 rect
    FatalismStellarCollision = 26171, // ?->self, 2.0s cast, range 30 circle
    Fatalism1 = 26162, // Boss->self, 3.0s cast, single-target
    Galaxias = 27753, // Boss->self, 5.0s cast, single-target
    GalaxiasAOE = 27754, // Helper->location, 5.0s cast, range 40 circle, knockback from center
    ElenchosCenter = 26180, // Boss->self, 6.0s cast, range 40 width 14 rect
    ElenchosSides = 26178, // Boss->self, 6.0s cast, single-target
    ElenchosSidesAOE = 26179, // Helper->self, 6.0s cast, range 40 width 13 rect
    DeathsEmbrace = 26185, // Boss->self, 6.0s cast, single-target
    DeathsEmbraceAOE = 26187, // Boss->self, no cast, range 45 cone
    Misery = 26184, // Helper->self, no cast, circle growing every ~1.07s for 14 pulses, last ~12
    Pharmakon = 26188, // FeatherOfDespair->self, 2.0s cast, range 6 circle
    Aporrhoia = 26173, // Boss->self, 3.0s cast, single-target
    AporrhoiaLaser = 26174, // ?->self, 5.0s cast, range 40 width 5 rect (sheet name Elenchos)
    FatalismLaser = 26175, // ?->self, 4.0s cast, range 40 width 5 rect
    Hubris = 26194, // Boss->self, 5.0s cast, single-target
    HubrisAOE = 26195, // Helper->player, 5.0s cast, range 6 circle
    Ekstasis = 26181, // Boss->self, 3.0s cast, single-target
    Epigonoi = 26182, // ?->location, 3.0s cast, range 6 circle
    Interstellar = 26189, // Boss->self, 3.0s cast, single-target
    InterstellarAOE = 26619, // Helper->self, 5.0s cast, range 40 width 28 rect
    Nemesis1 = 26190, // Helper->player, 5.0s cast, range 6 circle
    Nemesis2 = 26196, // Boss->self, 5.0s cast, single-target
    Nemesis2AOE = 26197, // Helper->player, 5.0s cast, range 6 circle
    Planetes = 27480, // Boss->self, 4.5s cast, single-target
    MeteorRadiant = 26198, // Kakodaimon->self, 3.0s cast, single-target
    Crash = 26199, // ?->location, 9.0s cast, range 15 circle
    MeteorOutburst = 26204, // Kakodaimon->self, 5.0s cast, single-target
    DeadStarSpread = 26205, // ?->player, 5.0s cast, range 6 circle
    Katastrophe = 26203, // Kakodaimon->self, 5.0s cast, range 40 circle
    UltimateFate = 27481, // Boss->self, 5.0s cast, range 40 circle
    Fatalism2 = 27483, // Boss->self, 3.0s cast, single-target
    Telos = 26206, // Boss->self, 5.0s cast, range 40 circle
    Telomania = 26207, // Boss->self, 3.0s cast, single-target
    TelomaniaSmall = 26208, // ?->self, no cast, range 40 circle
    TelomaniaBig = 26209, // ?->self, no cast, range 40 circle
    DeadStar = 24142, // ?->location, 3.0s cast, range 6 circle
}

public enum TetherID : uint
{
    Fatalism = 166, // Boss->DoomedStars
}

class Elegeia(BossModule module) : Components.RaidwideCast(module, AID.Elegeia);
class ElegeiaUnforgotten(BossModule module) : Components.RaidwideCast(module, AID.ElegeiaUnforgotten);
class StellarCollision(BossModule module) : Components.StandardAOEs(module, AID.StellarCollision, 30);
class StellarCollisionRect(BossModule module) : Components.StandardAOEs(module, AID.StellarCollisionRect, new AOEShapeRect(80, 11));
class FatalismStellarCollision(BossModule module) : Components.StandardAOEs(module, AID.FatalismStellarCollision, 30);

class FatalismStars(BossModule module) : Components.GenericAOEs(module, AID.FatalismStellarCollision)
{
    private static readonly AOEShapeCircle _shape = new(30);
    private readonly Dictionary<ulong, WPos> _recorded = [];
    private readonly List<AOEInstance> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Take(1);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.StellarCollision)
            _recorded[caster.InstanceID] = caster.Position;
        else if (spell.Action == WatchedAction && _aoes.Count > 0)
            _aoes.RemoveAt(0);
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.Fatalism && WorldState.Actors.Find(tether.Target) is { } star)
            _aoes.Add(new(_shape, _recorded.GetValueOrDefault(star.InstanceID, star.Position), default, WorldState.FutureTime(19.6f)));
    }
}

class Galaxias(BossModule module) : Components.KnockbackFromCastTarget(module, AID.GalaxiasAOE, 13);

class ElenchosCenter(BossModule module) : Components.StandardAOEs(module, AID.ElenchosCenter, new AOEShapeRect(40, 7));
class ElenchosSides(BossModule module) : Components.StandardAOEs(module, AID.ElenchosSidesAOE, new AOEShapeRect(40, 6.5f, 40));

class DeathsEmbrace(BossModule module) : Components.GenericBaitAway(module, AID.DeathsEmbraceAOE)
{
    private static readonly AOEShapeCone _shape = new(45, 5.Degrees());

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.DeathsEmbrace)
            foreach (var p in Raid.WithoutSlot())
                CurrentBaits.Add(new(caster, p, _shape, Module.CastFinishAt(spell, 1.2f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            ++NumCasts;
            CurrentBaits.Clear();
        }
    }
}

class Pharmakon(BossModule module) : Components.GenericAOEs(module, AID.Pharmakon)
{
    private static readonly AOEShapeCircle _shape = new(6);
    private readonly List<(Actor feather, DateTime activation)> _feathers = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _feathers.Select(f => new AOEInstance(_shape, f.feather.Position, default, f.activation));

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.FeatherOfDespair)
            _feathers.Add((actor, WorldState.FutureTime(6.7f)));
    }

    public override void OnActorDestroyed(Actor actor) => _feathers.RemoveAll(f => f.feather == actor);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            ++NumCasts;
            _feathers.RemoveAll(f => f.feather == caster);
        }
    }
}

class Misery(BossModule module) : Components.GenericAOEs(module, AID.Misery)
{
    private static readonly AOEShapeCircle _shape = new(12.5f);
    private DateTime _activation;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Module.Enemies(OID.Endsinger2).Select(h => new AOEInstance(_shape, h.Position, default, _activation));

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.Endsinger2 && _activation < WorldState.CurrentTime)
            _activation = WorldState.FutureTime(6.3f);
    }
}

class AporrhoiaLaser(BossModule module) : Components.StandardAOEs(module, AID.AporrhoiaLaser, new AOEShapeRect(40, 2.5f));
class FatalismLaser(BossModule module) : Components.StandardAOEs(module, AID.FatalismLaser, new AOEShapeRect(40, 2.5f));
class Hubris(BossModule module) : Components.BaitAwayCast(module, AID.HubrisAOE, new AOEShapeCircle(6), true);

class Epigonoi(BossModule module) : Components.StandardAOEs(module, AID.Epigonoi, 6);
class Interstellar(BossModule module) : Components.StandardAOEs(module, AID.InterstellarAOE, new AOEShapeRect(40, 14));
class Nemesis1(BossModule module) : Components.SpreadFromCastTargets(module, AID.Nemesis1, 6);
class Nemesis2(BossModule module) : Components.SpreadFromCastTargets(module, AID.Nemesis2AOE, 6);

class Crash(BossModule module) : Components.StandardAOEs(module, AID.Crash, 15);
class DeadStarSpread(BossModule module) : Components.SpreadFromCastTargets(module, AID.DeadStarSpread, 6);
class Katastrophe(BossModule module) : Components.RaidwideCast(module, AID.Katastrophe);
class UltimateFate(BossModule module) : Components.RaidwideCast(module, AID.UltimateFate, "Raidwide, tank LB3!");

class Telos(BossModule module) : Components.RaidwideCast(module, AID.Telos);
class Telomania(BossModule module) : Components.RaidwideCast(module, AID.Telomania, "Raidwide x5");
class DeadStar(BossModule module) : Components.StandardAOEs(module, AID.DeadStar, 6);

class T03EndsingerStates : StateMachineBuilder
{
    public T03EndsingerStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Elegeia>()
            .ActivateOnEnter<ElegeiaUnforgotten>()
            .ActivateOnEnter<StellarCollision>()
            .ActivateOnEnter<StellarCollisionRect>()
            .ActivateOnEnter<FatalismStellarCollision>()
            .ActivateOnEnter<FatalismStars>()
            .ActivateOnEnter<Galaxias>()
            .ActivateOnEnter<ElenchosCenter>()
            .ActivateOnEnter<ElenchosSides>()
            .ActivateOnEnter<DeathsEmbrace>()
            .ActivateOnEnter<Pharmakon>()
            .ActivateOnEnter<Misery>()
            .ActivateOnEnter<AporrhoiaLaser>()
            .ActivateOnEnter<FatalismLaser>()
            .ActivateOnEnter<Hubris>()
            .ActivateOnEnter<Epigonoi>()
            .ActivateOnEnter<Interstellar>()
            .ActivateOnEnter<Nemesis1>()
            .ActivateOnEnter<Nemesis2>()
            .ActivateOnEnter<Crash>()
            .ActivateOnEnter<DeadStarSpread>()
            .ActivateOnEnter<Katastrophe>()
            .ActivateOnEnter<UltimateFate>()
            .ActivateOnEnter<Telos>()
            .ActivateOnEnter<Telomania>()
            .ActivateOnEnter<DeadStar>();
    }
}

[ModuleInfo(Contributors = "Kagekazu", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 796, NameID = 10448)]
public class T03Endsinger(ModuleInit init) : BossModule(init, new(100, 100), new ArenaBoundsCircle(20))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.Endsinger2), ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.Endsinger3), ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.Kakodaimon), ArenaColor.Enemy);
    }
}
