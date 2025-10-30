namespace BossMod.Dawntrail.Dungeon.D09Yuweyawata.D091LindblumZaghnal;

public enum OID : uint
{
    Boss = 0x4641, // R9.000, x1
    RawElectrope = 0x4642, // R1.000, x0 (spawn during fight)
    Helper = 0x233C, // R0.500, x32, Helper type
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    Teleport = 40622, // Boss->location, no cast, single-target
    ElectricalOverload = 40635, // Boss->self, 5.0s cast, range 40 circle, raidwide
    GoreCaberToss = 41266, // Boss->self, 3.0s cast, single-target, visual (mechanic start)
    CaberToss = 40624, // Boss->self, 19.0s cast, single-target, visual (line sequence)
    LineVoltageNarrowLong = 40625, // Helper->self, 4.0s cast, range 50 width 5 rect
    LineVoltageWideShort = 41121, // Helper->self, 3.3s cast, range 50 width 10 rect
    LineVoltageWideLong = 40627, // Helper->self, 3.5s cast, range 50 width 10 rect
    LineVoltageNarrowShort = 41122, // Helper->self, 3.0s cast, range 50 width 5 rect
    CellShock = 40626, // Helper->self, 2.0s cast, range 26 circle
    LightningStorm = 40636, // Boss->self, 4.5s cast, single-target, visual (spread)
    LightningStormAOE = 40637, // Helper->player, 5.0s cast, range 5 circle spread
    GoreSparkingFissure = 40630, // Boss->self, 3.0s cast, single-target, visual (mechanic start)
    SparkingFissureStart = 41267, // Helper->self, 5.2s cast, range 40 circle, raidwide
    SparkingFissureRepeat = 40631, // Helper->self, no cast, range 40 circle, raidwide
    SparkingFissureResolve = 40632, // Boss->self, 13.0s cast, single-target, visual (mechanic end)
    SparkingFissureResolveAOE = 41258, // Helper->self, 13.7s cast, range 40 circle, raidwide
    LightningBolt = 40638, // Helper->location, 5.0s cast, range 6 circle puddle
    Electrify = 40634, // RawElectrope->self, 16.0s cast, range 40 circle, ???
}

public enum IconID : uint
{
    LightningStorm = 315, // player->self
}

class ElectricalOverload(BossModule module) : Components.RaidwideCast(module, AID.ElectricalOverload);
class LineVoltageNarrowLong(BossModule module) : Components.StandardAOEs(module, AID.LineVoltageNarrowLong, new AOEShapeRect(50, 2.5f), 9);
class LineVoltageWideShort(BossModule module) : Components.StandardAOEs(module, AID.LineVoltageWideShort, new AOEShapeRect(50, 5));
class LineVoltageWideLong(BossModule module) : Components.StandardAOEs(module, AID.LineVoltageWideLong, new AOEShapeRect(50, 5));
class LineVoltageNarrowShort(BossModule module) : Components.StandardAOEs(module, AID.LineVoltageNarrowShort, new AOEShapeRect(50, 2.5f), 6);

class CellShock(BossModule module) : Components.GenericAOEs(module, AID.CellShock)
{
    private AOEInstance? _aoe;

    private static readonly AOEShapeCircle _shape = new(26);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnMapEffect(byte index, uint state)
    {
        // potential origins are at intercardinals at distance 11.5 from center; there are 4 'tether sources', each can target one of 2 neighbouring intercardinals
        var dir = index switch
        {
            13 => -135.Degrees(),
            14 => 135.Degrees(),
            15 => -45.Degrees(),
            16 => 45.Degrees(),
            _ => default
        };
        if (dir != default && state is 0x00020001 or 0x00200010)
        {
            dir += state == 0x00020001 ? -90.Degrees() : 90.Degrees();
            _aoe = new(_shape, Module.Center + 11.5f * dir.ToDirection(), default, WorldState.FutureTime(8.1f));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            ++NumCasts;
            if (_aoe == null || !_aoe.Value.Origin.AlmostEqual(caster.Position, 1))
                ReportError($"Unexpected resolve at {caster.Position}, expected {_aoe?.Origin}");
            _aoe = null;
        }
    }
}

class LightningStorm(BossModule module) : Components.SpreadFromCastTargets(module, AID.LightningStormAOE, 5);
class SparkingFissureStart(BossModule module) : Components.RaidwideCast(module, AID.SparkingFissureStart);
class SparkingFissureResolve(BossModule module) : Components.RaidwideCast(module, AID.SparkingFissureResolveAOE);
class RawElectrope(BossModule module) : Components.Adds(module, (uint)OID.RawElectrope, 1);
class LightningBolt(BossModule module) : Components.StandardAOEs(module, AID.LightningBolt, 6);

class D091LindblumZaghnalStates : StateMachineBuilder
{
    public D091LindblumZaghnalStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ElectricalOverload>()
            .ActivateOnEnter<LineVoltageNarrowLong>()
            .ActivateOnEnter<LineVoltageWideShort>()
            .ActivateOnEnter<LineVoltageWideLong>()
            .ActivateOnEnter<LineVoltageNarrowShort>()
            .ActivateOnEnter<CellShock>()
            .ActivateOnEnter<LightningStorm>()
            .ActivateOnEnter<SparkingFissureStart>()
            .ActivateOnEnter<SparkingFissureResolve>()
            .ActivateOnEnter<RawElectrope>()
            .ActivateOnEnter<LightningBolt>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1008, NameID = 13623)]
public class D091LindblumZaghnal(WorldState ws, Actor primary) : BossModule(ws, primary, new(73, 277), new ArenaBoundsCircle(20));
