namespace BossMod.Stormblood.Raid.O9NChaos;

public enum OID : uint
{
    Boss = 0x2435, // R4.500
    Helper = 0x233C, // R0.500
    Chaosphere = 0x2437, // R1.000
    ChaosphereTether = 0x2438, // R1.500
    DarkCrystal = 0x2436, // R1.800
}

public enum AID : uint
{
    AutoAttack = 12622, // Boss->player, no cast, single-target
    ChaoticDispersion = 12623, // Boss->player, 5.0s cast, single-target
    DamningEdict = 12624, // Boss->self, 5.0s cast, range 60+R width 60 rect
    LongitudinalImplosion = 12625, // Boss->self, visual for the north/south cones
    LatitudinalImplosion = 12626, // Boss->self, 4.0s cast, visual for the east/west cones
    Implosion = 12627, // Helper->self, 4.7s cast, range 60+R 90-degree cone
    ChaosphereSpawn = 12629, // Boss->self, no cast, single-target
    ChaosphereSpawnTether = 12630, // Boss->self, no cast, single-target
    KnockDown = 12631, // Boss->self, 5.0s cast, stack visual
    KnockDownProximity = 12632, // Boss->self, 5.0s cast, flare visual
    BigBang = 12633, // Boss->self, 3.0s cast, visual
    BigBangProximity = 12634, // Boss->self, 5.0s cast, visual
    FiendishOrbs = 12635, // Boss->self, 2.3s cast, line orbs
    FiendishOrbsTether = 12636, // Boss->self, 5.0s cast, orb tethers
    KnockDownStack = 12637, // Helper->player, no cast, range 6 circle
    KnockDownFlare = 12638, // Helper->player, no cast, proximity
    BigBangStack = 12639, // Helper->self, 3.7s cast, range 6 circle
    BigBangFlare = 12640, // Helper->self, 5.7s cast, range 15 circle
    FiendishOrbCharge = 12641, // Helper->location, 3.0s cast, width 3 rect charge
    Orbshadow = 12642, // Chaosphere->location, no cast, width 3 rect charge
    Orbshadow1 = 12643, // Chaosphere->location, no cast, width 3 rect charge
    OrbshadowTether = 12644, // ChaosphereTether->player, no cast, single-target
    Inferno = 12645, // Boss->self, 5.0s cast, single-target
    Tsunami = 12646, // Boss->self, 5.0s cast, single-target
    Cyclone = 12647, // Boss->self, 5.0s cast, single-target
    Earthquake = 12648, // Boss->self, 5.0s cast, single-target
    BowelsOfAgony = 12649, // Boss->self, 5.0s cast, range 60 circle
    SoulOfChaos = 12650, // Boss->self, no cast, range 100 circle
    StrayFlames = 12651, // Helper->player, no cast, range 5 circle
    StraySpray = 12652, // Helper->player, no cast, range ?-10 donut
    CycloneKnockback = 12653, // Helper->self, 5.0s cast, range 30 circle, knockback 10
    EarthquakeSouth = 12654, // Helper->self, 5.0s cast, range 40+R width 40 rect
    InfernoField = 12698, // Helper->self, 5.0s cast, range 10-30 donut
    TsunamiField = 12699, // Helper->self, 5.0s cast, range 15 width 40 rect
    CycloneField = 12700, // Helper->self, 5.0s cast, range 10 circle
    EarthquakeField = 12701, // Helper->self, 5.0s cast, range 20 width 40 rect
    InfernoRaidwide = 12802, // Helper->self, no cast, range 60 circle
    TsunamiRaidwide = 12803, // Helper->self, no cast, range 60 circle
    CycloneRaidwide = 12804, // Helper->self, no cast, range 60 circle
    EarthquakeRaidwide = 12805, // Helper->self, no cast, range 60 circle
    CrystalUntargetable = 12821, // Boss->self, no cast, single-target
    EarthquakeKnockback = 13336, // Helper->self, 5.0s cast, range 40+R width 40 rect, knockback 15 south
}

public enum IconID : uint
{
    Stack = 62, // player
    Flare = 87, // player
    Entropy = 96, // player
    DynamicFluid = 153, // player
}

class ChaoticDispersion(BossModule module) : Components.SingleTargetCast(module, AID.ChaoticDispersion);
class DamningEdict(BossModule module) : Components.StandardAOEs(module, AID.DamningEdict, new AOEShapeRect(64.5f, 30));
class Implosion(BossModule module) : Components.GenericAOEs(module, AID.Implosion)
{
    private static readonly AOEShapeCone Shape = new(60.5f, 45.Degrees());
    private readonly List<AOEInstance> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoes.Count == 0)
            yield break;

        var first = _aoes[0].Activation;
        foreach (var aoe in _aoes)
        {
            if ((aoe.Activation - first).TotalSeconds > 0.5)
                yield break;
            yield return aoe;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID != AID.Implosion)
            return;
        _aoes.Add(new(Shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
        _aoes.Sort((a, b) => a.Activation.CompareTo(b.Activation));
    }

    public override void Update()
    {
        var now = WorldState.CurrentTime;
        _aoes.RemoveAll(aoe => aoe.Activation <= now);
    }
}
class FiendishOrbCharge(BossModule module) : Components.ChargeAOEs(module, AID.FiendishOrbCharge, 1.5f);
class FiendishOrbsTether(BossModule module) : Components.CastHint(module, AID.FiendishOrbsTether, "Orb tether");
class BigBangStack(BossModule module) : Components.StandardAOEs(module, AID.BigBangStack, 6);
class BigBangFlare(BossModule module) : Components.StandardAOEs(module, AID.BigBangFlare, 15);
class Entropy(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.Entropy, AID.StrayFlames, 5, 5.5f);
class KnockDownStack(BossModule module) : Components.StackWithIcon(module, (uint)IconID.Stack, AID.KnockDownStack, 6, 6.1f);
class KnockDownFlare(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCircle(15), (uint)IconID.Flare, AID.KnockDownFlare, 6.1f, true)
{
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (ActiveBaitsOn(actor).Any() && (actor.Position - Arena.Center).LengthSq() < 17.5f * 17.5f)
            hints.Add("Bait to the edge!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        foreach (var b in ActiveBaitsOn(actor))
            hints.AddForbiddenZone(ShapeDistance.Circle(Arena.Center, 17.5f), b.Activation);
    }
}
class StraySpray(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeDonut(5, 10), (uint)IconID.DynamicFluid, AID.StraySpray, 5.5f, true);
class CycloneKnockback(BossModule module) : Components.KnockbackFromCastTarget(module, AID.CycloneKnockback, 10, shape: new AOEShapeCircle(30));
class Inferno(BossModule module) : Components.RaidwideCast(module, AID.Inferno);
class Cyclone(BossModule module) : Components.RaidwideCast(module, AID.Cyclone);
class Tsunami(BossModule module) : Components.RaidwideCast(module, AID.Tsunami);
class Earthquake(BossModule module) : Components.RaidwideCast(module, AID.Earthquake);
class BowelsOfAgony(BossModule module) : Components.RaidwideCast(module, AID.BowelsOfAgony);

class EarthquakeKnockback(BossModule module) : Components.KnockbackFromCastTarget(module, AID.EarthquakeKnockback, 15, shape: new AOEShapeRect(40.5f, 20), kind: Kind.DirForward)
{
    private static readonly AOEShapeRect PushedSouth = new(35, 20);
    private static readonly WPos PushedSouthOrigin = new(100, 85);
    private static readonly WPos NorthEdge = new(100, 82);

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (Casters.Count > 0)
            PushedSouth.Draw(Arena, PushedSouthOrigin, default);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (Casters.Count == 0)
            return;

        hints.AddForbiddenZone(PushedSouth, PushedSouthOrigin, default, WorldState.CurrentTime);
        hints.GoalZones.Add(p => p.InCircle(NorthEdge, 2) ? 10 : 0);
    }
}

class ArenaHazard(BossModule module) : Components.GenericAOEs(module)
{
    private enum Hazard { None, Inferno, Cyclone, Tsunami, Earthquake }

    private static readonly AOEShapeDonut InfernoShape = new(10, 30);
    private static readonly AOEShapeCircle CycloneShape = new(10);
    private static readonly AOEShapeRect TsunamiShape = new(15, 20);
    private static readonly AOEShapeRect EarthquakeShape = new(20, 20);

    private readonly List<AOEInstance> _aoes = [];
    private Hazard _hazard;
    private DateTime _expire = DateTime.MaxValue;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void Update()
    {
        if (WorldState.CurrentTime < _expire)
            return;
        _aoes.Clear();
        _hazard = Hazard.None;
        _expire = DateTime.MaxValue;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.BowelsOfAgony:
                _aoes.Clear();
                _hazard = Hazard.None;
                _expire = DateTime.MaxValue;
                return;
            case AID.FiendishOrbs:
                if (_hazard != Hazard.None)
                    _expire = WorldState.FutureTime(9.4f);
                return;
            case AID.FiendishOrbsTether:
                if (_hazard != Hazard.None)
                    _expire = WorldState.FutureTime(12);
                return;
            case AID.InfernoField:
                Set(Hazard.Inferno);
                _aoes.Add(new(InfernoShape, Arena.Center, default, WorldState.CurrentTime));
                break;
            case AID.CycloneField:
                Set(Hazard.Cyclone);
                _aoes.Add(new(CycloneShape, Arena.Center, default, WorldState.CurrentTime));
                break;
            case AID.TsunamiField:
                Set(Hazard.Tsunami);
                _aoes.Add(new(TsunamiShape, spell.LocXZ, spell.Rotation, WorldState.CurrentTime));
                break;
            case AID.EarthquakeField:
                Set(Hazard.Earthquake);
                _aoes.Add(new(EarthquakeShape, spell.LocXZ, spell.Rotation, WorldState.CurrentTime));
                break;
        }
    }

    private void Set(Hazard next)
    {
        if (_hazard == next)
            return;
        _aoes.Clear();
        _hazard = next;
        _expire = DateTime.MaxValue;
    }
}

class ChaosStates : StateMachineBuilder
{
    public ChaosStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ChaoticDispersion>()
            .ActivateOnEnter<DamningEdict>()
            .ActivateOnEnter<Implosion>()
            .ActivateOnEnter<FiendishOrbCharge>()
            .ActivateOnEnter<FiendishOrbsTether>()
            .ActivateOnEnter<BigBangStack>()
            .ActivateOnEnter<BigBangFlare>()
            .ActivateOnEnter<Entropy>()
            .ActivateOnEnter<KnockDownStack>()
            .ActivateOnEnter<KnockDownFlare>()
            .ActivateOnEnter<StraySpray>()
            .ActivateOnEnter<CycloneKnockback>()
            .ActivateOnEnter<EarthquakeKnockback>()
            .ActivateOnEnter<ArenaHazard>()
            .ActivateOnEnter<Inferno>()
            .ActivateOnEnter<Cyclone>()
            .ActivateOnEnter<Tsunami>()
            .ActivateOnEnter<Earthquake>()
            .ActivateOnEnter<BowelsOfAgony>();
    }
}

[ModuleInfo(Contributors = "croizat", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 587, NameID = 7691)]
public class Chaos(ModuleInit init) : BossModule(init, new(100, 100), new ArenaBoundsSquare(20));
