namespace BossMod.Shadowbringers.Alliance.A21AegisUnit;

public enum OID : uint
{
    Boss = 0x2EA2,
    Helper = 0x233C,
    FlightUnit = 0x2ECB, // R2.800, x6
}

public enum AID : uint
{
    AutoAttack = 20924, // Helper->player, no cast, single-target
    FiringOrderAntiPersonnelLaser = 20621, // Boss->self, 3.0s cast, single-target
    AntiPersonnelLaser = 20624, // Helper->player, no cast, range 3 circle
    ManeuverBeamCannons = 20595, // Boss->self, 12.0s cast, single-target

    // the order for these might be wrong but we don't directly use them anyway
    BeamCannonsSmall = 20596, // Helper->self, no cast, range 40 30-degree cone
    BeamCannonsMedium = 20597, // Helper->self, no cast, range 40 60-degree cone
    BeamCannonsLarge = 20598, // Helper->self, no cast, range 40 90-degree cone

    ManeuverColliderCannons1 = 20604, // Boss->self, 7.5s cast, single-target
    ManeuverColliderCannons2 = 20605, // Boss->self, 8.0s cast, single-target
    ColliderCannons = 20606, // Helper->self, no cast, range 40 30-degree cone
    FiringOrderSurfaceLaser = 20622, // Boss->self, 3.0s cast, single-target
    AerialSupportSwoop = 20690, // Boss->self, 3.0s cast, single-target
    SurfaceLaser = 20625, // Helper->location, no cast, single-target
    SurfaceLaser1 = 20626, // Helper->location, no cast, range 4 circle
    FlightPath = 20620, // FlightUnit->self, 3.0s cast, range 60 width 10 rect
    ManeuverRefractionCannons = 20608, // Boss->self, 6.0s cast, single-target
    RefractionCannons = 20609, // Helper->self, no cast, range 40 36-degree cone
    ManeuverDiffusionCannon = 20633, // Boss->self, 6.0s cast, range 60 circle
    AerialSupportBombardment = 20691, // Boss->self, 3.0s cast, single-target
    FiringOrderHighPoweredLaser = 20623, // Boss->self, 3.0s cast, single-target
    HighPoweredLaser = 20627, // Helper->players, no cast, range 6 circle
    LifesLastSong = 21427, // Helper->self, 7.5s cast, range 30 100-degree cone
    ManeuverSaturationBombing = 20631, // FlightUnit->self, 25.0s cast, range 60 circle

    //_Ability_ = 20602, // Boss->self, no cast, single-target
    //_Ability_1 = 20601, // Boss->self, no cast, single-target
    //_Ability_2 = 21426, // Helper->self, no cast, single-target
    //_Ability_3 = 20599, // Boss->self, no cast, single-target
}

public enum IconID : uint
{
    Tankbuster = 198, // player->self
    SurfaceLaser = 23, // player->self
    Stack = 62, // player->self
}

class AntiPersonnelLaser(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCircle(3), (uint)IconID.Tankbuster, AID.AntiPersonnelLaser, 4.1f, centerAtTarget: true);

class BeamCannons(BossModule module) : Components.GenericAOEs(module, AID.ManeuverBeamCannons)
{
    private DateTime _activation;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_activation != default)
        {
            var src = Module.PrimaryActor;
            yield return new AOEInstance(new AOEShapeCone(30, 30.Degrees()), src.Position, src.Rotation, _activation);
            yield return new AOEInstance(new AOEShapeCone(30, 45.Degrees()), src.Position, src.Rotation - 72.Degrees(), _activation);
            yield return new AOEInstance(new AOEShapeCone(30, 15.Degrees()), src.Position, src.Rotation - 144.Degrees(), _activation);
            yield return new AOEInstance(new AOEShapeCone(30, 45.Degrees()), src.Position, src.Rotation + 144.Degrees(), _activation);
            yield return new AOEInstance(new AOEShapeCone(30, 30.Degrees()), src.Position, src.Rotation + 72.Degrees(), _activation);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            _activation = Module.CastFinishAt(spell, 0.3f);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.BeamCannonsMedium or AID.BeamCannonsLarge or AID.BeamCannonsSmall)
        {
            NumCasts++;
            _activation = default;
        }
    }
}

class ColliderCannons(BossModule module) : Components.GenericAOEs(module)
{
    private DateTime _activation;
    private Angle _rotation;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_activation != default)
        {
            var src = Module.PrimaryActor;
            for (var i = 0; i < 5; i++)
                yield return new AOEInstance(new AOEShapeCone(30, 15.Degrees()), src.Position, src.Rotation + _rotation + (72 * i).Degrees(), _activation);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.ManeuverColliderCannons1:
                _activation = Module.CastFinishAt(spell, 0.5f);
                _rotation = default;
                break;
            case AID.ManeuverColliderCannons2:
                _activation = Module.CastFinishAt(spell, 0.5f);
                _rotation = -90.Degrees();
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.ColliderCannons)
        {
            NumCasts++;
            _activation = default;
        }
    }
}

class SurfaceLaserBait(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.SurfaceLaser, AID.SurfaceLaser, 4, 5.1f)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == SpreadAction)
            Spreads.Clear();
    }
}

class SurfaceLaserRepeat(BossModule module) : Components.GenericAOEs(module, AID.SurfaceLaser1)
{
    private readonly List<(WPos position, int casts)> _lasers = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _lasers.Select(l => new AOEInstance(new AOEShapeCircle(4), l.position));

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.SurfaceLaser)
            _lasers.Add((spell.TargetXZ, 1));

        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            var laser = _lasers.FindIndex(l => l.position.AlmostEqual(spell.TargetXZ, 0.1f));
            if (laser >= 0)
            {
                // it's actually 10 casts, but the "target select" cast and the aoe happen on the same frame, so we might get the order backwards
                if (++_lasers.Ref(laser).casts >= 9)
                    _lasers.RemoveAt(laser);
            }
        }
    }
}

class RefractionCannons(BossModule module) : Components.GenericAOEs(module, AID.ManeuverRefractionCannons)
{
    private DateTime _activation;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_activation != default)
        {
            var src = Module.PrimaryActor;
            for (var i = 0; i < 5; i++)
                yield return new AOEInstance(new AOEShapeCone(30, 18.Degrees()), src.Position, src.Rotation + (72 * i + 18).Degrees(), _activation);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            _activation = Module.CastFinishAt(spell, 0.3f);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.RefractionCannons)
        {
            NumCasts++;
            _activation = default;
        }
    }
}

class FlightPath(BossModule module) : Components.StandardAOEs(module, AID.FlightPath, new AOEShapeRect(60, 5));
class DiffusionCannon(BossModule module) : Components.RaidwideCast(module, AID.ManeuverDiffusionCannon);
class HighPoweredLaser(BossModule module) : Components.StackWithIcon(module, (uint)IconID.Stack, AID.HighPoweredLaser, 6, 5);
class FlightUnit(BossModule module) : Components.Adds(module, (uint)OID.FlightUnit, 1);
class LifesLastSong(BossModule module) : Components.StandardAOEs(module, AID.LifesLastSong, new AOEShapeCone(30, 50.Degrees()));
class SaturationBombing(BossModule module) : Components.CastHint(module, AID.ManeuverSaturationBombing, "Kill adds!", true);

class A21AegisUnitStates : StateMachineBuilder
{
    public A21AegisUnitStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AntiPersonnelLaser>()
            .ActivateOnEnter<BeamCannons>()
            .ActivateOnEnter<ColliderCannons>()
            .ActivateOnEnter<SurfaceLaserBait>()
            .ActivateOnEnter<SurfaceLaserRepeat>()
            .ActivateOnEnter<FlightPath>()
            .ActivateOnEnter<RefractionCannons>()
            .ActivateOnEnter<DiffusionCannon>()
            .ActivateOnEnter<HighPoweredLaser>()
            .ActivateOnEnter<FlightUnit>()
            .ActivateOnEnter<LifesLastSong>()
            .ActivateOnEnter<SaturationBombing>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 736, NameID = 9642)]
public class A21AegisUnit(WorldState ws, Actor primary) : BossModule(ws, primary, new(-230, 192), MakeBounds())
{
    private static ArenaBoundsCustom MakeBounds()
    {
        var clipper = new PolygonClipper();
        var arenaBase = new RelSimplifiedComplexPolygon(CurveApprox.Circle(25, 0.02f));
        var platform = CurveApprox.Circle(12.15f, 0.02f);

        var p1 = clipper.Union(new(arenaBase), new(platform.Select(p => p + new WDir(-15.155f, -8.75f))));
        var p2 = clipper.Union(new(p1), new(platform.Select(p => p + new WDir(15.155f, -8.75f))));
        var p3 = clipper.Union(new(p2), new(platform.Select(p => p + new WDir(0, 17.5f))));
        var complete = clipper.Difference(new(p3), new(CurveApprox.Circle(10.5f, 0.02f)));
        return new(30, complete);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc) => Arena.ActorInsideBounds(PrimaryActor.Position, PrimaryActor.Rotation, ArenaColor.Enemy);
}

