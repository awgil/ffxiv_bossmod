namespace BossMod.Shadowbringers.Raid.E04Titan;

public enum OID : uint
{
    Boss = 0x298C, // R4.999
    Helper = 0x233C, // R0.500
    MassiveBoulder = 0x298E, // R2.210
    BombBoulder = 0x298D, // R1.300-3.900
}

public enum AID : uint
{
    AutoAttack = 16701, // Boss->player, no cast, single-target
    VoiceOfTheLand = 16631, // Boss->self, 4.0s cast, range 100 circle
    EvilEarth = 16622, // Boss->self, 4.1s cast, single-target
    EvilEarthAOE = 16623, // Helper->self, 5.0s cast, range 10 width 10 rect
    Aftershock = 16820, // Helper->self, 5.0s cast, range 10 width 10 rect
    WeightOfTheLand = 16618, // Boss->self, 2.1s cast, single-target
    WeightOfTheLandAOE = 16619, // Helper->self, 3.0s cast, range 10 width 10 rect
    Stonecrusher = 16633, // Boss->player, 5.0s cast, range 5 circle
    CrumblingDown = 16625, // Boss->self, 3.0s cast, single-target
    CrumblingDownAOE = 16626, // MassiveBoulder->self, 5.0s cast, range 60 circle
    SeismicWave = 16627, // Boss->self, 4.0s cast, range 100 circle
    Geocrush = 16630, // Boss->location, 4.0s cast, range 100 circle
    EarthenGauntlets = 16614, // Boss->self, no cast, single-target
    MassiveLandslideAOE1 = 16636, // Helper->self, 3.0s cast, range 42 width 22 rect
    MassiveLandslideAOE2 = 16635, // Helper->self, 3.0s cast, range 42 width 22 rect
    MassiveLandslide = 16634, // Boss->self, 3.0s cast, single-target
    AftershockLandslide = 16624, // Helper->self, 3.0s cast, range 10 width 10 rect
    EarthenArmor = 16615, // Boss->self, no cast, single-target
    BombBoulders = 16620, // Boss->self, 3.0s cast, single-target
    Bury = 16705, // BombBoulder->self, no cast, range 3+R circle
    CobaltBomb = 16628, // Boss->self, 3.0s cast, single-target
    Explosion = 16621, // BombBoulder->self, 6.0s cast, range 10 circle
    CobaltExplosion = 16629, // BombBoulder->self, 6.0s cast, range 22 circle
    EarthenWheels = 16616, // Boss->self, no cast, single-target
    FaultZoneAOE = 16643, // Helper->self, 3.0s cast, range 42 width 52 rect
    FaultZone = 16642, // Boss->location, 3.0s cast, ???
    EarthenArmorEnd = 16617, // Boss->self, no cast, single-target
    EarthenFury = 16632, // Boss->self, 6.0s cast, single-target
    EarthenFuryAOE = 17382, // Helper->self, 7.5s cast, range 100 circle
    FaultLine = 16641, // Boss->location, no cast, width 10 rect charge
    Magnitude50 = 16644, // Boss->self, 6.0s cast, range 5-40 donut
    Landslide = 16637, // Boss->location, 4.4s cast, visual (jump)
    LandslideAOE = 16638, // Helper->self, 5.0s cast, range 32 width 20 cross
    LeftwardLandslide = 16639, // Boss->self, 3.0s cast, range 21 width 77 rect
    RightwardLandslide = 16640, // Boss->self, 3.0s cast, range 21 width 77 rect
}

class VoiceOfTheLand(BossModule module) : Components.RaidwideCast(module, AID.VoiceOfTheLand);
class EarthenFury(BossModule module) : Components.RaidwideCast(module, AID.EarthenFuryAOE);

class EvilEarth(BossModule module) : Components.GroupedAOEs(module, [AID.EvilEarthAOE, AID.Aftershock], new AOEShapeRect(10, 5), highlightImminent: true);

class WeightOfTheLand(BossModule module) : Components.StandardAOEs(module, AID.WeightOfTheLandAOE, new AOEShapeRect(10, 5));

class Stonecrusher(BossModule module) : Components.BaitAwayCast(module, AID.Stonecrusher, new AOEShapeCircle(5), centerAtTarget: true);

class CrumblingDown(BossModule module) : Components.ProximityAOEs(module, AID.CrumblingDownAOE, 20);

class SeismicWave(BossModule module) : Components.GenericLineOfSightAOE(module, AID.SeismicWave, 100, false)
{
    private DateTime _activation;

    private IEnumerable<Actor> IntactBoulders() => Module.Enemies(OID.MassiveBoulder).Where(b => !b.IsDeadOrDestroyed && b.ModelState.AnimState2 == 0);

    private void Refresh()
    {
        var blockers = IntactBoulders().Select(b => (b.Position, b.HitboxRadius)).ToList();
        if (blockers.Count == 0)
        {
            Modify(null, []);
            return;
        }
        var activation = Module.PrimaryActor.CastInfo?.IsSpell(AID.SeismicWave) == true ? Module.CastFinishAt(Module.PrimaryActor.CastInfo) : _activation;
        Modify(Module.PrimaryActor.Position, blockers, activation);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.CrumblingDownAOE)
        {
            _activation = WorldState.FutureTime(8);
            Refresh();
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            Refresh();
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            _activation = default;
            Modify(null, []);
        }
    }

    public override void OnActorModelStateChange(Actor actor, byte modelState, byte animState1, byte animState2)
    {
        if (actor.OID == (uint)OID.MassiveBoulder)
            Refresh();
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if (actor.OID == (uint)OID.MassiveBoulder)
            Refresh();
    }
}

class Geocrush(BossModule module) : Components.KnockbackFromCastTarget(module, AID.Geocrush, 20)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var s in Sources(slot, actor))
        {
            if (IsImmune(slot, s.Activation))
                continue;
            var origin = s.Origin;
            var dist = s.Distance;
            var center = Arena.Center;
            var toCenter = (center - origin).LengthSq() > 0.01f ? (center - origin).Normalized() : new WDir(0, -1);
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(origin + 2.5f * toCenter, 4), s.Activation);
            hints.AddForbiddenZone(Sdf.Discrete(p =>
            {
                var dest = AwayFromSource(p, origin, dist);
                return dest.X is < 80 or > 120 || dest.Z is < 80 or > 120;
            }), s.Activation);
        }
    }
}

class MassiveLandslide(BossModule module) : Components.GroupedAOEs(module, [AID.MassiveLandslideAOE1, AID.MassiveLandslideAOE2], new AOEShapeRect(42, 11));
class AftershockLandslide(BossModule module) : Components.StandardAOEs(module, AID.AftershockLandslide, new AOEShapeRect(10, 5), highlightImminent: true);

class Landslide(BossModule module) : Components.StandardAOEs(module, AID.LandslideAOE, new AOEShapeCross(32, 10));
class SideLandslide(BossModule module) : Components.GroupedAOEs(module, [AID.LeftwardLandslide, AID.RightwardLandslide], new AOEShapeRect(21, 38.5f));

class Explosion(BossModule module) : Components.StandardAOEs(module, AID.Explosion, 10, highlightImminent: true);
class CobaltExplosion(BossModule module) : Components.StandardAOEs(module, AID.CobaltExplosion, 22);

class FaultZoneCharge(BossModule module) : Components.ChargeAOEs(module, AID.FaultZone, 5)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            var dir = spell.LocXZ - caster.Position;
            Casters.Add((caster, new AOEShapeRect(dir.Length(), HalfWidth, caster.HitboxRadius), Angle.FromDirection(dir)));
        }
    }
}
class FaultZoneKnockback(BossModule module) : Components.Knockback(module, AID.FaultZoneAOE)
{
    private WPos _from;
    private WPos _to;
    private DateTime _activation;

    private static WPos NearestOnSegment(WPos p, WPos a, WPos b)
    {
        var ab = b - a;
        var lenSq = ab.LengthSq();
        if (lenSq < 0.01f)
            return a;
        var t = Math.Clamp((p - a).Dot(ab) / lenSq, 0, 1);
        return a + t * ab;
    }

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        if (_activation == default)
            yield break;
        yield return new(NearestOnSegment(actor.Position, _from, _to), 15, _activation, Kind: Kind.AwayFromOrigin);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.FaultZone:
                _from = caster.Position;
                _to = spell.LocXZ;
                break;
            case AID.FaultZoneAOE:
                _activation = Module.CastFinishAt(spell);
                if (_from == default)
                {
                    _from = Module.PrimaryActor.Position;
                    _to = Module.PrimaryActor.Position + 40 * spell.Rotation.ToDirection();
                }
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.FaultZoneAOE)
        {
            _activation = default;
            _from = default;
            _to = default;
            ++NumCasts;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var s in Sources(slot, actor))
        {
            if (IsImmune(slot, s.Activation))
                continue;
            var from = _from;
            var to = _to;
            var dist = s.Distance;
            hints.AddForbiddenZone(Sdf.Discrete(p =>
            {
                var origin = NearestOnSegment(p, from, to);
                return !Module.InBounds(AwayFromSource(p, origin, dist));
            }), s.Activation);
        }
    }
}

class Magnitude50(BossModule module) : Components.StandardAOEs(module, AID.Magnitude50, new AOEShapeDonut(5, 40));

class E04TitanStates : StateMachineBuilder
{
    public E04TitanStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<VoiceOfTheLand>()
            .ActivateOnEnter<EarthenFury>()
            .ActivateOnEnter<EvilEarth>()
            .ActivateOnEnter<WeightOfTheLand>()
            .ActivateOnEnter<Stonecrusher>()
            .ActivateOnEnter<CrumblingDown>()
            .ActivateOnEnter<SeismicWave>()
            .ActivateOnEnter<Geocrush>()
            .ActivateOnEnter<MassiveLandslide>()
            .ActivateOnEnter<AftershockLandslide>()
            .ActivateOnEnter<Landslide>()
            .ActivateOnEnter<SideLandslide>()
            .ActivateOnEnter<Explosion>()
            .ActivateOnEnter<CobaltExplosion>()
            .ActivateOnEnter<FaultZoneCharge>()
            .ActivateOnEnter<FaultZoneKnockback>()
            .ActivateOnEnter<Magnitude50>();
    }
}

[ModuleInfo(Contributors = "croizat", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 689, NameID = 8350)]
public class E04Titan(ModuleInit init) : BossModule(init, new(100, 100), new ArenaBoundsRect(20, 20));
