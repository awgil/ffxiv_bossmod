namespace BossMod.Endwalker.Trial.T02Hydaelyn;

public enum OID : uint
{
    Boss = 0x34EA, //R=5.04
    Helper = 0x233C,
    Parhelion = 0x34F1, //R=3.0
    EchoOfHydaelyn = 0x34EB, //R=5.04
    CrystalOfLight = 0x34EC, //R=2.5
    MysticRefulgence = 0x3503, //R=1.0
    MysticRefulgence2 = 0x34C9, //R=2.0
    RefulgenceTriangle = 0x1EB24D,
    RefulgenceHexagon = 0x1EB24C,
    LightwaveWaveTarget = 0x1EB24B,
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    AutoAttackStaff = 27732, // Boss->player, no cast, single-target
    AutoAttackChakram = 27733, // Boss->player, no cast, single-target
    HerossRadiance = 26071, // Boss->self, 5.0s cast, range 40 circle
    Teleport = 26025, // Boss->location, no cast, single-target, boss teleports mid
    Teleport2 = 28282, // Boss->location, no cast, single-target
    Teleport3 = 26030, // 3503->location, no cast, single-target, Mystic Refulgence teleports
    DawnMantle = 27660, // Boss->self, 4.9s cast, single-target
    Anthelion = 26056, // Boss->self, no cast, range 5-40 donut (not sure if 5.04 or 5, impossible to tell with naked eye)
    MousasScorn = 26070, // Boss->players, 5.0s cast, range 4 circle, shared tankbuster
    HighestHoly = 26055, // Boss->self, no cast, range 10 circle
    MagossRadiance = 26072, // Boss->self, 5.0s cast, range 40 circle
    Equinox = 26051, // Boss->self, no cast, single-target
    Equinox2 = 26255, // Helper->self, no cast, range 40 width 10 cross
    HerossSundering = 26069, // Boss->self/player, 5.0s cast, range 40 90-degree cone, tankbuster
    StartsCrystalPhase = 26067, // Boss->self, no cast, single-target
    HydaelynsRay = 26060, // EchoOfHydaelyn->self, 6.0s cast, range 45 width 40 rect, damage fall off rectangle, approx 15 optimal width
    CrystallizeTriggerEarth = 26015, // EchoOfHydaelyn/Boss->self, no cast, single-target
    Crystallize = 27729, // EchoOfHydaelyn->self, no cast, single-target
    CrystallineStoneIII = 27855, // EchoOfHydaelyn->self, 5.0s cast, single-target
    CrystallineStoneIII2 = 27737, // Helper->players, 5.0s cast, range 6 circle
    CrystallizeTriggerIce = 26016, // Boss->self, no cast, single-target
    Crystallize2 = 27853, // EchoOfHydaelyn->self, no cast, single-target
    CrystallineBlizzardIII = 27854, // EchoOfHydaelyn->self, 5.0s cast, single-target
    CrystallineBlizzardIII2 = 27738, // Helper->players, 5.0s cast, range 5 circle
    Exodus = 26043, // Boss->self, no cast, single-target
    Exodus2 = 26066, // Helper->self, no cast, range 40 circle
    CrystallizeStaffIce = 26012, // Boss->self, 4.0s cast, single-target
    ParhelicCircle = 26028, // Boss->self, 6.0s cast, single-target
    IncandescenceTrigger = 26029, // 34C9->self, no cast, single-target
    Incandescence = 26061, // 3503->self, 1.5s cast, range 6 circle
    CrystallizeChakramEarth = 26014, // Boss->self, 4.0s cast, single-target
    CrystallizeTriggerWater = 26017, // Boss->self, no cast, single-target
    Parhelion = 26032, // Boss->self, 5.0s cast, single-target
    Parhelion2 = 26033, // Boss->self, no cast, single-target
    Beacon = 26062, // Parhelion->location, 5.2s cast, width 6 rect charge
    Beacon2 = 26063, // Parhelion->self, 5.0s cast, range 45 width 6 rect
    Subparhelion = 27734, // Boss->self, 5.0s cast, single-target
    RadiantHalo = 26064, // Boss->self, 5.0s cast, range 40 circle
    EchoesA = 26037, // Boss->self, 5.0s cast, single-target
    EchoesB = 26038, // Boss->self, 5.0s cast, single-target
    EchoesC = 26039, // Boss->self, 5.0s cast, single-target
    Echoes = 26065, // Helper->players, no cast, range 6 circle
    RayOfLight = 26058, // Helper->self, no cast, range 15 width 16 rect
    Lightwave = 26260, // Boss->self, 4.0s cast, single-target
    Lightwave2 = 26261, // Boss->self, 4.0s cast, single-target
    Lightwave3 = 26259, // Boss->self, 4.0s cast, single-target
}

public enum IconID : uint
{
    Echoes = 305, // player, stack 5 times
}

public enum SID : uint
{
    HerossMantle = 2876, // none->EchoOfHydaelyn/Boss, extra=0x0, sword stance
    HydaelynsWeapon = 2273, // Boss->Boss, extra=0x1B5/0x1B4 (n/a for sword, 1B4 for staff, 1B5 for chakram)
    MousasMantle = 2878, // none->Boss, extra=0x0, chakram stance
    MagossMantle = 2877, // none->Boss, extra=0x0, staff stance
    CrystallizeElement = 2056, // EchoOfHydaelyn/Boss->EchoOfHydaelyn/Boss, extra=0x152/0x153
}

class Echoes(BossModule module) : Components.UniformStackSpread(module, 6, 0, 8, stackHits: 5)
{
    public int NumCasts { get; private set; }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Echoes)
        {
            ++NumCasts;
            foreach (ref var stack in Stacks.AsSpan())
                stack.Hits = 5 - NumCasts;
            if (NumCasts == 5)
            {
                Stacks.Clear();
                NumCasts = 0;
            }
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.Echoes)
            AddStack(actor);
    }
}

class Exodus(BossModule module) : BossComponent(module)
{
    private int _numCrystalsDestroyed;
    private DateTime _activation;

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (_activation != default)
            hints.Add("Raidwide");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_activation != default)
            hints.AddPredictedDamage(Raid.WithSlot().Mask(), _activation);
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if ((OID)actor.OID == OID.CrystalOfLight)
        {
            ++_numCrystalsDestroyed;
            if (_numCrystalsDestroyed == 6)
                _activation = WorldState.FutureTime(7.2f);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Exodus2)
            _activation = default;
    }
}

class Lightwave(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<Actor> Waves = [];
    private static readonly AOEShapeRect WaveAOE = new(16, 8, 12);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (Waves.Count > 0)
            foreach (var w in Waves)
                yield return new(WaveAOE, w.Position, w.Rotation);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if ((AID)spell.Action.ID == AID.RayOfLight && !Waves.Contains(caster))
            Waves.Add(caster);
    }
}

class ParhelicCircle(BossModule module) : Components.GenericAOEs(module)
{
    private const float _triRadius = 8;
    private const float _hexRadius = 17;
    private static readonly AOEShapeCircle _circle = new(6);
    private DateTime _activation;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_activation != default)
        {
            // there are 10 orbs: 1 in center, 3 in vertices of a triangle with radius=8, 6 in vertices of a hexagon with radius=17
            // note: i'm not sure how exactly orientation is determined, it seems to be related to eventobj rotations...
            var hex = Module.Enemies(OID.RefulgenceHexagon).FirstOrDefault();
            var tri = Module.Enemies(OID.RefulgenceTriangle).FirstOrDefault();
            if (hex != null && tri != null)
            {
                var c = Module.Center;
                yield return new(_circle, c, Activation: _activation);
                yield return new(_circle, c + _triRadius * (tri.Rotation + 60.Degrees()).ToDirection(), Activation: _activation);
                yield return new(_circle, c + _triRadius * (tri.Rotation + 180.Degrees()).ToDirection(), Activation: _activation);
                yield return new(_circle, c + _triRadius * (tri.Rotation - 60.Degrees()).ToDirection(), Activation: _activation);
                yield return new(_circle, c + _hexRadius * hex.Rotation.ToDirection(), Activation: _activation);
                yield return new(_circle, c + _hexRadius * (hex.Rotation + 60.Degrees()).ToDirection(), Activation: _activation);
                yield return new(_circle, c + _hexRadius * (hex.Rotation + 120.Degrees()).ToDirection(), Activation: _activation);
                yield return new(_circle, c + _hexRadius * (hex.Rotation + 180.Degrees()).ToDirection(), Activation: _activation);
                yield return new(_circle, c + _hexRadius * (hex.Rotation - 120.Degrees()).ToDirection(), Activation: _activation);
                yield return new(_circle, c + _hexRadius * (hex.Rotation - 60.Degrees()).ToDirection(), Activation: _activation);
            }
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID is OID.RefulgenceHexagon)
            _activation = WorldState.FutureTime(8.9f);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Incandescence)
            ++NumCasts;
        if (NumCasts == 10)
        {
            NumCasts = 0;
            _activation = default;
        }
    }
}

class WeaponTracker(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnStatusGain(Actor actor, in ActorStatus status)
    {
        if ((SID)status.ID == SID.HydaelynsWeapon && status.Extra == 0x1B4)
            _aoe = new(new AOEShapeCircle(10), Module.PrimaryActor.Position, default, WorldState.FutureTime(6));
        if ((SID)status.ID == SID.HydaelynsWeapon && status.Extra == 0x1B5)
            _aoe = new(new AOEShapeDonut(5, 40), Module.PrimaryActor.Position, default, WorldState.FutureTime(6));
    }

    public override void OnStatusLose(Actor actor, in ActorStatus status)
    {
        if ((SID)status.ID == SID.HydaelynsWeapon)
            _aoe = new(new AOEShapeCross(40, 5), Module.PrimaryActor.Position, default, WorldState.FutureTime(6.9f));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.Equinox2 or AID.HighestHoly or AID.Anthelion)
            _aoe = null;
    }
}

class MousasScorn(BossModule module) : Components.CastSharedTankbuster(module, AID.MousasScorn, 4);
class MousasScornHint(BossModule module) : Components.SingleTargetCast(module, AID.MousasScorn, "Shared Tankbuster");
class HerossSundering(BossModule module) : Components.BaitAwayCast(module, AID.HerossSundering, new AOEShapeCone(40, 45.Degrees()));
class HerossSunderingHint(BossModule module) : Components.SingleTargetCast(module, AID.HerossSundering, "Tankbuster cleave");
class HerossRadiance(BossModule module) : Components.RaidwideCast(module, AID.HerossRadiance);
class MagossRadiance(BossModule module) : Components.RaidwideCast(module, AID.MagossRadiance);
class RadiantHalo(BossModule module) : Components.RaidwideCast(module, AID.RadiantHalo);
class CrystallineStoneIII(BossModule module) : Components.StackWithCastTargets(module, AID.CrystallineStoneIII2, 6, 5);
class CrystallineBlizzardIII(BossModule module) : Components.SpreadFromCastTargets(module, AID.CrystallineBlizzardIII2, 5);
class Beacon(BossModule module) : Components.ChargeAOEs(module, AID.Beacon, 3);
class Beacon2(BossModule module) : Components.StandardAOEs(module, AID.Beacon2, new AOEShapeRect(45, 3), 10);
class HydaelynsRay(BossModule module) : Components.StandardAOEs(module, AID.HydaelynsRay, new AOEShapeRect(45, 15));

class T02HydaelynStates : StateMachineBuilder
{
    public T02HydaelynStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ParhelicCircle>()
            .ActivateOnEnter<MousasScorn>()
            .ActivateOnEnter<Echoes>()
            .ActivateOnEnter<Beacon>()
            .ActivateOnEnter<Beacon2>()
            .ActivateOnEnter<CrystallineStoneIII>()
            .ActivateOnEnter<CrystallineBlizzardIII>()
            .ActivateOnEnter<MousasScornHint>()
            .ActivateOnEnter<HerossSundering>()
            .ActivateOnEnter<HerossSunderingHint>()
            .ActivateOnEnter<HerossRadiance>()
            .ActivateOnEnter<MagossRadiance>()
            .ActivateOnEnter<HydaelynsRay>()
            .ActivateOnEnter<RadiantHalo>()
            .ActivateOnEnter<Lightwave>()
            .ActivateOnEnter<WeaponTracker>()
            .ActivateOnEnter<Exodus>();
    }
}

[ModuleInfo(Contributors = "Malediktus, Kagekazu", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 790, NameID = 10453)]
public class T02Hydaelyn(ModuleInit init) : BossModule(init, new(100, 100), new ArenaBoundsCircle(20))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.CrystalOfLight))
            Arena.Actor(s, ArenaColor.Object);
    }
}
