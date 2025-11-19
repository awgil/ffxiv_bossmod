namespace BossMod.Dawntrail.Trial.T01Valigarmanda;

public enum OID : uint
{
    Boss = 0x4115, // R20.000, x?
    Valigarmanda1 = 0x417A, // R0.000, x?, Part type
    Valigarmanda2 = 0x233C, // R0.500, x?, Helper type
    Valigarmanda3 = 0x4179, // R0.000, x?, Part type
    FeatherOfRuin = 0x4116, // R2.680, x?
    ArcaneSphere = 0x4181, // R1.000, x0 (spawn during fight)
    ArcaneSphere2 = 0x4493, // R1.000, x0 (spawn during fight)
    IceBoulder = 0x4117, // R1.260, x0 (spawn during fight)
    ThunderousBeacon = 0x438A, // R4.800, x0 (spawn during fight)
    GlacialBeacon = 0x438C, // R4.800, x0 (spawn during fight)
    FlameKissedBeacon = 0x438B, // R4.800, x0 (spawn during fight)
}

public enum AID : uint
{
    Attack = 36899, // 4115->player, no cast, single-target
    StranglingCoil = 36160, // 233C->self, 7.3s cast, range 8-30 donut
    SlitheringStrike = 36158, // 233C->self, 7.3s cast, range 24 180-degree cone
    SusurrantBreath = 36156, // 233C->self, 7.3s cast, range 50 ?-degree cone
    IceTalon = 36185, // 417A/4179->players, no cast, range 6 circle
    Skyruin1 = 36163, // 233C->self, 4.5s cast, range 80 circle
    Skyruin2 = 36162, // Valigarmanda2->self, 4.5s cast, range 80 circle
    ThunderousBreath = 36176, // 233C->self, 7.9s cast, range 50 135-degree cone
    HailOfFeathers = 36170, // 4115->self, 4.0+2.0s cast, single-target
    HailOfFeathers1 = 36361, // 233C->self, 6.0s cast, range 80 circle
    HailOfFeathers2 = 36171, // FeatherOfRuin->self, no cast, single-target
    BlightedBolt = 36172, // Boss->self, 7.0+0.8s cast, single-target
    BlightedBolt1 = 36174, // Valigarmanda2->FeatherOfRuin, 7.8s cast, range 7 circle
    BlightedBolt2 = 36173, // Valigarmanda2->player, no cast, range 3 circle
    ArcaneLightning = 39001, // 4181->self, 1.0s cast, range 50 width 5 rect
    DisasterZone1 = 36167, // Valigarmanda2->self, 3.8s cast, range 80 circle
    DisasterZone2 = 36165, // Valigarmanda2->self, 3.8s cast, range 80 circle
    Ruinfall = 36186, // Boss->self, 4.0+1.6s cast, single-target
    RuinfallTB = 36187, // Valigarmanda2->self, 5.6s cast, range 6 circle
    RuinfallKnockback = 36189, // Valigarmanda2->self, 8.0s cast, range 40 width 40 rect
    RuinfallAOEs = 39129, // Valigarmanda2->location, 9.7s cast, range 6 circle
    NorthernCrossNE = 36168, // Valigarmanda2->self, 3.0s cast, range 60 width 25 rect NE Avalanche
    NorthernCrossSW = 36169, // Valigarmanda2->self, 3.0s cast, range 60 width 25 rect SW Avalanche
    FreezingDust = 36177, // Boss->self, 5.0+0.8s cast, range 80 circle
    ChillingCataclysm = 39265, // Valigarmanda2->self, 1.5s cast, range 40 width 5 cross
    RuinForetold = 38545, // Boss->self, 5.0s cast, range 80 circle
    CalamitousEcho = 36195, // Valigarmanda2->self, 5.0s cast, range 40 20-degree cone
    CalamitousTargetLong = 34722, // Valigarmanda2->player, no cast, single-target
    CalamitousTargetShort = 26708, // Valigarmanda2->player, no cast, single-target
    CalamitousCryStack = 36194, // Valigarmanda2->self, no cast, range 80 width 6 rect
    Tulidisaster = 36197, // Boss->self, 7.0+3.0s cast, single-target
    Eruption = 36191, // Valigarmanda2->location, 3.0s cast, range 6 circle

}
public enum IconID : uint
{
    IceTalon = 344, // player
}
class StranglingCoil(BossModule module) : Components.StandardAOEs(module, AID.StranglingCoil, new AOEShapeDonut(8, 30));
class SlitheringStrike(BossModule module) : Components.StandardAOEs(module, AID.SlitheringStrike, new AOEShapeCone(24, 90.Degrees()));
class SusurrantBreath(BossModule module) : Components.StandardAOEs(module, AID.SusurrantBreath, new AOEShapeCone(50, 40.Degrees()));
class IceTalon(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCircle(6), (uint)IconID.IceTalon, AID.IceTalon, centerAtTarget: true, damageType: AIHints.PredictedDamageType.Tankbuster);
class Skyruin1(BossModule module) : Components.RaidwideCast(module, AID.Skyruin1);
class Skyruin2(BossModule module) : Components.RaidwideCast(module, AID.Skyruin2);
class Tulidisaster(BossModule module) : Components.RaidwideCast(module, AID.Tulidisaster);
class Eruption(BossModule module) : Components.StandardAOEs(module, AID.Eruption, new AOEShapeCircle(6));
class CalamitousEcho(BossModule module) : Components.StandardAOEs(module, AID.CalamitousEcho, new AOEShapeCone(40, 10.Degrees()));
class CalamitousCryLong(BossModule module) : Components.SimpleLineStack(module, 3, 80, AID.CalamitousTargetLong, AID.CalamitousCryStack, 6);
class CalamitousCryShort(BossModule module) : Components.SimpleLineStack(module, 3, 80, AID.CalamitousTargetShort, AID.CalamitousCryStack, 5)
{
    private new readonly ActionID TargetSelect = ActionID.MakeSpell(AID.CalamitousTargetShort);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == TargetSelect)
        {
            Source = caster;
            Activation = WorldState.FutureTime(5);
            foreach (var (i, p) in Raid.WithSlot(true))
                PlayerRoles[i] = p.InstanceID == spell.MainTargetID ? PlayerRole.Target : PlayerRole.Share;
        }
        else if ((AID)spell.Action.ID == AID.CalamitousCryStack || (AID)spell.Action.ID is AID.Tulidisaster)
        {
            ++NumCasts;
            Source = null;
            Array.Fill(PlayerRoles, PlayerRole.Ignore);
        }
    }
}
class FreezingDust(BossModule module) : Components.StayMove(module)
{
    private DateTime _time;

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.FreezingDust)
        {
            _time = WorldState.CurrentTime;
            Array.Fill(PlayerStates, new(Requirement.Move, Module.CastFinishAt(spell)));
        }
    }
    public override void Update()
    {
        if (WorldState.CurrentTime > _time.AddSeconds(3))
        {
            Array.Fill(PlayerStates, default);
        }
    }
}
class NorthernCross(BossModule module) : Components.GenericAOEs(module)
{
    public AOEInstance? AOE;

    private static readonly AOEShapeRect _shape = new(25, 30);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(AOE);

    public override void OnMapEffect(byte index, uint state)
    {
        if (index != 2)
            return;
        var offset = state switch
        {
            0x00200010 => -90.Degrees(),
            0x00020001 => 90.Degrees(),
            _ => default
        };
        if (offset != default)
            AOE = new(_shape, Module.Center, -127.Degrees() + offset, WorldState.FutureTime(9.2f));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.NorthernCrossNE or AID.NorthernCrossSW)
        {
            ++NumCasts;
            AOE = null;
        }
    }
}
class HailOfFeathers(BossModule module) : Components.RaidwideCast(module, AID.HailOfFeathers1);
class RuinfallKnockback(BossModule module) : Components.Knockback(module)
{
    private Actor? _source;
    private DateTime _activation;
    private readonly List<Actor> _casters = [];
    private bool tbSoaked;

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        if (_casters != null)
        {
            if (_source != null && _source != actor)
                yield return new(_source.Position, 21, _activation, Kind: Kind.DirForward);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.RuinfallKnockback)
        {
            _source = caster;
            _activation = Module.CastFinishAt(spell);
            _casters.Add(caster);
        }
    }
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.RuinfallKnockback)
        {
            _casters.Clear();
            _source = null;
        }
    }
    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.RuinfallTB)
        {
            tbSoaked = true;
        }
        if ((AID)spell.Action.ID is AID.RuinfallKnockback)
        {
            _casters.Clear();
        }
    }
    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        if (Module.FindComponent<RuinfallAOEs>() is var aoe && aoe != null && aoe.ActiveAOEs(slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation)))
            return true;
        if (!Module.InBounds(pos))
            return true;
        else
            return false;
    }
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_source != null)
        {
            if (actor.Role == Role.Tank && !tbSoaked)
            {
                hints.AddForbiddenZone(new AOEShapeRect(30, 20f, 3), Module.Center);
            }
            else if (actor.Role == Role.Tank && tbSoaked || actor.Role != Role.Tank)
            {
                hints.AddForbiddenZone(new AOEShapeRect(30, 20f, 10), Module.Center);
            }
        }
    }
}

class RuinfallTankbuster(BossModule module) : Components.CastSharedTankbuster(module, AID.RuinfallTB, 6);
class RuinfallAOEs(BossModule module) : Components.StandardAOEs(module, AID.RuinfallAOEs, new AOEShapeCircle(6));
class ThunderPlatform(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _tiles = [];
    private readonly List<AOEInstance> _aoes = [];
    private readonly List<Actor> _feathers = [];
    private static readonly AOEShapeRect tile = new(5, 5, 5);
    private static readonly AOEShapeCircle featherBolt = new(7);
    private bool _needFloat;
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_tiles.Count > 0)
        {
            var floatTiles = new List<int>() { 0, 3, 5, 6, 8, 11 }; //Float Tiles
            var tiles = 12;
            if (_needFloat)
            {
                for (var i = 0; i < tiles; i++)
                {
                    if (!floatTiles.Contains(i))
                    {
                        yield return _tiles[i] with { Color = ArenaColor.AOE };
                    }
                }
            }
            else if (!_needFloat)
            {
                for (var i = 0; i < tiles; i++)
                {
                    if (floatTiles.Contains(i))
                    {
                        yield return _tiles[i] with { Color = ArenaColor.AOE };
                    }
                }
                foreach (var e in _aoes)
                {
                    yield return e with { Color = ArenaColor.AOE };
                }
            }
        }
    }
    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID is OID.FeatherOfRuin)
        {
            _feathers.Add(actor);
        }
    }
    public override void OnActorDestroyed(Actor actor)
    {
        if ((OID)actor.OID is OID.FeatherOfRuin)
        {
            _feathers.Remove(actor);
        }
    }
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.ThunderousBreath)
        {
            _needFloat = true;
            int[] xOffsets = [85, 95, 105, 115];
            int[] zOffsets = [90, 100, 110];

            foreach (var zOffset in zOffsets)
            {
                foreach (var xOffset in xOffsets)
                {
                    _tiles.Add(new(tile, new(xOffset, zOffset)));
                }
            }
        }
        if ((AID)spell.Action.ID is AID.BlightedBolt)
        {
            _needFloat = false;
            int[] xOffsets = [85, 95, 105, 115];
            int[] zOffsets = [90, 100, 110];

            foreach (var zOffset in zOffsets)
            {
                foreach (var xOffset in xOffsets)
                {
                    _tiles.Add(new(tile, new(xOffset, zOffset)));
                }
            }
        }
        if ((AID)spell.Action.ID is AID.BlightedBolt1)
        {
            foreach (var e in _feathers)
            {
                _aoes.Add(new(featherBolt, new(e.Position.X, e.Position.Z)));
            }
        }
    }
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_tiles.Count > 0 && (AID)spell.Action.ID == AID.ThunderousBreath)
        {
            _tiles.Clear();
        }
        if (_tiles.Count > 0 && (AID)spell.Action.ID == AID.BlightedBolt)
        {
            _feathers.Clear();
            _aoes.Clear();
            _tiles.Clear();
        }
    }
}
class ArcaneSpheres(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private readonly List<Actor> _adds = [];
    private static readonly AOEShapeRect rect = new(45, 2.5f, 45);
    private static readonly AOEShapeCross cross = new(30, 2.5f);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoes.Count > 0)
        {
            foreach (var e in _aoes)
            {
                yield return e with { Color = ArenaColor.AOE };
            }
        }
    }
    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID is OID.ArcaneSphere)
        {
            _adds.Add(actor);
            _aoes.Add(new(rect, new(actor.Position.X, actor.Position.Z), 90.Degrees()));
        }
        if ((OID)actor.OID is OID.ArcaneSphere2)
        {
            _adds.Add(actor);
            _aoes.Add(new(cross, new(actor.Position.X, actor.Position.Z)));
            _aoes.Add(new(cross, new(actor.Position.X, actor.Position.Z), 45.Degrees()));
        }
    }
    public override void OnActorDestroyed(Actor actor)
    {
        if ((OID)actor.OID is OID.ArcaneSphere)
        {
            _adds.Remove(actor);
            _aoes.Clear();
        }
        if ((OID)actor.OID is OID.ArcaneSphere2)
        {
            _adds.Remove(actor);
            _aoes.Clear();
        }
    }
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.ArcaneLightning)
        {
            _aoes.Clear();
        }
        if ((AID)spell.Action.ID is AID.ChillingCataclysm)
        {
            _aoes.Clear();
        }
    }
}
class AddsMulti(BossModule module) : Components.AddsMulti(module, [(uint)OID.IceBoulder, (uint)OID.ThunderousBeacon, (uint)OID.GlacialBeacon, (uint)OID.FlameKissedBeacon])
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.IceBoulder => 5,
                OID.FlameKissedBeacon => 4,
                OID.ThunderousBeacon => 3,
                OID.GlacialBeacon => 2,
                OID.Boss => 1,
                _ => 0
            };
    }
}
class T01ValigarmandaStates : StateMachineBuilder
{
    public T01ValigarmandaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<StranglingCoil>()
            .ActivateOnEnter<SlitheringStrike>()
            .ActivateOnEnter<SusurrantBreath>()
            .ActivateOnEnter<IceTalon>()
            .ActivateOnEnter<Skyruin1>()
            .ActivateOnEnter<Skyruin2>()
            .ActivateOnEnter<FreezingDust>()
            .ActivateOnEnter<NorthernCross>()
            .ActivateOnEnter<RuinfallKnockback>()
            .ActivateOnEnter<RuinfallTankbuster>()
            .ActivateOnEnter<RuinfallAOEs>()
            .ActivateOnEnter<HailOfFeathers>()
            .ActivateOnEnter<ThunderPlatform>()
            .ActivateOnEnter<Tulidisaster>()
            .ActivateOnEnter<Eruption>()
            .ActivateOnEnter<CalamitousEcho>()
            .ActivateOnEnter<CalamitousCryLong>()
            .ActivateOnEnter<CalamitousCryShort>()
            .ActivateOnEnter<ArcaneSpheres>()
            .ActivateOnEnter<AddsMulti>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "VeraNala", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 832, NameID = 12854)]
public class T01Valigarmanda(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsRect(20, 15));
