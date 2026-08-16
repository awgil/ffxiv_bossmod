namespace BossMod.Dawntrail.Foray.ForkedTower.FT14Index;

public enum OID : uint
{
    Boss = 0x4B5F, // R7.500, x1
    Helper = 0x233C, // R0.500, x15 (spawn during fight), Helper type
    HolyLance = 0x4B62, // R1.000, x3
    TranscribedIndex = 0x4B6F, // R7.500, x3
    Index = 0x4B72, // R1.000, x3
    SwirlingOrb = 0x4B64, // R1.500, x0 (spawn during fight)
    BallOfFire = 0x4B65, // R1.500, x0 (spawn during fight)
    BallOfLevin = 0x4B66, // R1.500, x0 (spawn during fight)
    SummonedBomb = 0x4B60, // R2.100, x0 (spawn during fight)
    ForetoldPhenomenon = 0x4B63, // R1.000, x0 (spawn during fight)

    FirePlatform = 0x1EC008,
    IcePlatform = 0x1EC009,
    LightningPlatform = 0x1EC00A,

    FireRing = 0x1EC00B,
    IceRing = 0x1EC00C,
    LightningRing = 0x1EC00D,
}

public enum AID : uint
{
    AutoAttack = 48421, // Boss->player, no cast, single-target
    FlareCast = 48415, // Boss->self, 5.0s cast, single-target
    FlareInstant = 48416, // Boss->self, no cast, single-target
    Flare = 48417, // Helper->self, no cast, range 60 ???
    SealedImplements1 = 48384, // Boss->self, 5.0+2.0s cast, single-target
    RomeosBallad = 48385, // Helper->self, 7.0s cast, range 15 circle
    SealedImplements2 = 48386, // Boss->self, 5.0+2.1s cast, single-target
    SealedImplements3 = 48904, // Boss->self, no cast, single-target
    Aim = 48387, // Helper->self, 7.1s cast, range 11 circle
    UnkBoss1 = 50665, // Boss->self, no cast, single-target
    OmniElementsCast = 48394, // Boss->self, 4.0+1.0s cast, single-target
    OmniElements = 48395, // Helper->self, no cast, range 60 ???
    ElementaryExpansion = 48399, // Boss->self, 3.0s cast, single-target
    ElementaryEvocation = 48400, // Boss->self, 3.0s cast, single-target
    FireIV = 48396, // Helper->self, no cast, range 30 ?-degree cone
    BlizzardIV = 48397, // Helper->self, no cast, range 30 ?-degree cone
    ThunderIV = 48398, // Helper->self, no cast, range 30 ?-degree cone
    ElementaryChemistryCast = 48401, // Boss->self, 3.9+1.1s cast, single-target
    ElementaryChemistry = 48402, // Helper->self, no cast, range 60 ???
    PlatformDisappear = 48905, // Helper->self, 6.0s cast, range 15 width 15 rect
    PropulsiveProphecy = 48403, // Boss->self, 3.0s cast, single-target
    Jump = 48404, // TranscribedIndex->self, no cast, single-target
    ShockwaveCast = 48405, // HolyLance->self, 5.0s cast, single-target
    ShockwaveKnockback = 48406, // Helper->self, 5.0s cast, range 15 ???
    Summon = 48408, // Boss->self, 3.0s cast, single-target
    DuologyOfImplements = 48388, // Boss->self, 5.0+1.0s cast, single-target
    Iainuki = 48389, // Helper->self, 6.0s cast, range 30 60-degree cone
    WindSlash = 48391, // Helper->self, 6.0s cast, range 30 60-degree cone
    AllKnowingFlames = 48418, // Boss->self, 5.0s cast, single-target
    AllConsumingFlames = 48420, // Helper->players, no cast, range 6 circle
    Predict = 48412, // Boss->self, 3.0s cast, single-target
    Starfall = 48413, // ForetoldPhenomenon->self, 0.5s cast, range 10 circle
    Cleansing = 48414, // ForetoldPhenomenon->self, 0.5s cast, range 4-15 donut
    Dualcast = 48407, // Boss->self, 3.0s cast, single-target
}

public enum SID : uint
{
    SealOfTheBell = 5532, // none->Boss, extra=0x403
    SealOfTheBlade = 5533, // none->Boss, extra=0x402
    SealOfTheBow = 5534, // none->Boss, extra=0x401
    SealOfTheHarp = 5535, // none->Boss, extra=0x404
    Unk2552 = 2552, // none->4B63, extra=0x44D/0x44C
    Dualcast = 5438, // Boss->Boss, extra=0x0
}

public enum IconID : uint
{
    Spread = 466, // player->self
}

public enum TetherID : uint
{
    Thunder = 363, // 4B66->4B66, lightning
    Ice = 364, // 4B64->4B64, ice
    Fire = 365, // 4B65->4B65, fire
    Foretold = 88, // 4B72->4B63, this is for the ball/donut tether thing
}

class Bounds(BossModule module) : BossComponent(module)
{
    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0)
        {
            if (state == 0x00020001)
                Arena.Bounds = FT14Index.MakeIndexBounds(true);
            if (state == 0x00080004)
                Arena.Bounds = FT14Index.MakeIndexBounds(false);
        }
    }
}

class Flare(BossModule module) : Components.RaidwideCastDelay(module, AID.FlareCast, AID.Flare, 0.8f);
class OmniElements(BossModule module) : Components.RaidwideCastDelay(module, AID.OmniElementsCast, AID.OmniElements, 1.1f);
class ElementaryChemistry(BossModule module) : Components.RaidwideCastDelay(module, AID.ElementaryChemistryCast, AID.ElementaryChemistry, 1.4f);
class RomeosBallad(BossModule module) : Components.StandardAOEs(module, AID.RomeosBallad, 15);
class Aim(BossModule module) : Components.StandardAOEs(module, AID.Aim, 11);

enum Element
{
    None,
    Fire,
    Ice,
    Lightning
}

class ElementaryEvocation(BossModule module) : Components.GenericAOEs(module)
{
    record struct Pair(Angle StartingAngle, Angle ClosestPlatform);

    private DateTime _nextActivation;

    private readonly List<Pair> _pairs = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var i = 0;
        foreach (var (_, c) in _pairs.Take(2))
        {
            var activation = _nextActivation.AddSeconds(2.5f * i);
            yield return new(new AOEShapeCone(60, 30.Degrees()), Arena.Center, c, activation, i == 0 ? ArenaColor.Danger : ArenaColor.AOE, i == 0);
            yield return new(new AOEShapeCone(60, 30.Degrees()), Arena.Center, c + 180.Degrees(), activation, i == 0 ? ArenaColor.Danger : ArenaColor.AOE, i == 0);
            i++;
        }
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        var element = (TetherID)tether.ID switch
        {
            TetherID.Fire => Element.Fire,
            TetherID.Ice => Element.Ice,
            TetherID.Thunder => Element.Lightning,
            _ => Element.None
        };
        if (element != default)
        {
            var startingAngle = (source.Position - Arena.Center).ToAngle();
            var closest = ((FT14Index)Module).GetPlatforms(element).MinBy(p => p.DistanceToAngle(startingAngle));

            _pairs.Add(new(startingAngle, closest));
            _pairs.SortBy(p =>
            {
                var distance = (p.ClosestPlatform - p.StartingAngle).Deg;
                if (distance > 0)
                    distance -= 180;
                return -distance;
            });
            _nextActivation = WorldState.FutureTime(7.7f);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.ThunderIV or AID.FireIV or AID.BlizzardIV)
        {
            NumCasts++;
            if (NumCasts % 2 == 0 && _pairs.Count > 0)
            {
                _nextActivation = WorldState.FutureTime(2.5f);
                _pairs.RemoveAt(0);
                if (_pairs.Count == 0)
                    _nextActivation = default;
            }
        }
    }
}

class ElementaryExpansion(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<(Element element, Angle angle, DateTime deadline)> _predicted = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var i = 0;
        foreach (var (e, a, d) in _predicted.Take(2))
        {
            yield return new(new AOEShapeCone(60, 30.Degrees()), Arena.Center, a, d,
                i == 0 ? ArenaColor.Danger : ArenaColor.AOE, i == 0);
            yield return new(new AOEShapeCone(60, 30.Degrees()), Arena.Center, a + 180.Degrees(), d,
                i == 0 ? ArenaColor.Danger : ArenaColor.AOE, i == 0);
            i++;
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        var e = (OID)actor.OID switch
        {
            OID.FireRing => Element.Fire,
            OID.IceRing => Element.Ice,
            OID.LightningRing => Element.Lightning,
            _ => default
        };

        if (e != default)
        {
            foreach (var a in ((FT14Index)Module).GetPlatforms(e).Take(1))
                _predicted.Add((e, a, WorldState.FutureTime(6.9f)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.ThunderIV or AID.FireIV or AID.BlizzardIV)
        {
            NumCasts++;
            if (NumCasts % 2 == 0 && _predicted.Count > 0)
                _predicted.RemoveAt(0);
        }
    }
}

class SummonedBomb(BossModule module) : Components.Adds(module, (uint)OID.SummonedBomb, 1, true);

class PlatformDisappear(BossModule module) : Components.StandardAOEs(module, AID.PlatformDisappear, new AOEShapeRect(15, 7.5f));

class Shockwave(BossModule module) : Components.Knockback(module)
{
    private readonly List<(Actor, WPos, DateTime)> _casters = [];

    private bool _balladActive;
    private bool _aimActive;

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        foreach (var (_, p, d) in _casters)
            yield return new(p, 9, d, new AOEShapeCircle(15));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.ShockwaveKnockback)
            _casters.RemoveAll(c => c.Item1 == caster);

        if ((AID)spell.Action.ID == AID.RomeosBallad)
            _balladActive = false;
        if ((AID)spell.Action.ID == AID.Aim)
            _aimActive = false;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.RomeosBallad)
            _balladActive = true;
        if ((AID)spell.Action.ID == AID.Aim)
            _aimActive = true;

        if ((AID)spell.Action.ID == AID.ShockwaveKnockback)
        {
            var src = spell.LocXZ;
            if (_casters.Any(c => c.Item2.AlmostEqual(src, 1)))
                return;

            _casters.Add((caster, src, Module.CastFinishAt(spell)));
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_casters.Count == 0 || IsImmune(slot, _casters[0].Item3))
            return;

        var (_, closestSource, activation) = _casters.MinBy(c => (c.Item2 - actor.Position).LengthSq());

        var platformDir = (closestSource - Arena.Center).Normalized();

        var safeRect = ShapeDistance.InvertedRect(Arena.Center + platformDir * 5, platformDir, 23, 0, 7.5f);
        hints.AddForbiddenZone(p =>
        {
            var off = (p - closestSource).Normalized() * 9;
            return safeRect(p + off);
        }, activation);

        if (_balladActive)
            hints.AddForbiddenZone(ShapeDistance.HalfPlane(closestSource, platformDir), activation);

        if (_aimActive)
            hints.AddForbiddenZone(ShapeDistance.HalfPlane(closestSource, -platformDir), activation);
    }
}

class IainukiWindSlash(BossModule module) : Components.GroupedAOEs(module, [AID.Iainuki, AID.WindSlash], new AOEShapeCone(30, 30.Degrees()), 3);

class AllConsumingFlames(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.Spread, AID.AllConsumingFlames, 6, 5.1f)
{
    public override void Update()
    {
        Spreads.RemoveAll(s => s.Target.IsDead);
    }
}

class Predict(BossModule module) : Components.GenericAOEs(module)
{
    class AOE
    {
        public AOEShape? Shape;
        public Actor? Destination;
        public DateTime Activation;
    }
    private readonly Dictionary<ulong, AOE> _predicted = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _predicted.Values.Where(p => p.Shape != null && p.Destination != null).Select(p => new AOEInstance(p.Shape!, p.Destination!.Position, p.Destination!.Rotation, p.Activation));

    public override void OnStatusGain(Actor actor, in ActorStatus status)
    {
        if ((OID)actor.OID == OID.ForetoldPhenomenon && (SID)status.ID == SID.Unk2552)
        {
            AOEShape? shape = status.Extra == 0x44D ? new AOEShapeCircle(10) : status.Extra == 0x44C ? new AOEShapeDonut(4, 15) : null;
            if (shape == null)
                return;

            if (_predicted.TryGetValue(actor.InstanceID, out var existing))
                existing.Shape = shape;
            else
                _predicted.Add(actor.InstanceID, new() { Shape = shape, Activation = WorldState.FutureTime(10) });
        }
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == TetherID.Foretold)
        {
            if (_predicted.TryGetValue(tether.Target, out var existing))
                existing.Destination = source;
            else
                _predicted.Add(tether.Target, new() { Destination = source, Activation = WorldState.FutureTime(10) });
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.Starfall or AID.Cleansing && _predicted.Count > 0)
            _predicted.Clear();
    }
}

class FT14IndexStates : StateMachineBuilder
{
    public FT14IndexStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Bounds>()
            .ActivateOnEnter<Flare>()
            .ActivateOnEnter<OmniElements>()
            .ActivateOnEnter<ElementaryChemistry>()
            .ActivateOnEnter<RomeosBallad>()
            .ActivateOnEnter<Aim>()
            .ActivateOnEnter<ElementaryEvocation>()
            .ActivateOnEnter<ElementaryExpansion>()
            .ActivateOnEnter<PlatformDisappear>()
            .ActivateOnEnter<SummonedBomb>()
            .ActivateOnEnter<Shockwave>()
            .ActivateOnEnter<AllConsumingFlames>()
            .ActivateOnEnter<IainukiWindSlash>()
            .ActivateOnEnter<Predict>();
    }
}

[ModuleInfo(GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1093, NameID = 14717, BitmapType = BossModuleInfo.BitmapType.Disabled)]
public class FT14Index(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, -628), MakeIndexBounds(false))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        if (PrimaryActor.IsTargetable)
            Arena.ActorInsideBounds(PrimaryActor.Position, PrimaryActor.Rotation, ArenaColor.Enemy);
    }

    public static ArenaBoundsCustom MakeIndexBounds(bool allPlatforms)
    {
        WDir[] platformSlice = [new(7.5f, 0), new(7.5f, 28), new(-7.5f, 28), new(-7.5f, 0)];
        // widened so the connection between slices is clean
        WDir[] noPlatform = [new(8, 0), new(8, 13), new(-8, 13), new(-8, 0)];

        var poly = new RelSimplifiedComplexPolygon(platformSlice);

        for (var i = 1; i < 6; i++)
        {
            var isPlat = i % 2 == 0 || allPlatforms;
            var shape = (isPlat ? platformSlice : noPlatform).Select(r => r.Rotate((i * 60).Degrees()));
            poly = new PolygonClipper().Union(new(poly), new(shape));
        }

        var holePoint = new WDir(2.886742f, 5); // ~tan(60deg) * 5
        var holePoly = Enumerable.Range(0, 6).Select(i => holePoint.Rotate((i * 60).Degrees()));

        poly = new PolygonClipper().Difference(new(poly), new(holePoly));

        return new(new WDir(7.5f, 28).Length(), poly);
    }

    internal IEnumerable<Angle> GetPlatforms(Element el)
    {
        var oid = el switch
        {
            Element.Fire => OID.FirePlatform,
            Element.Ice => OID.IcePlatform,
            Element.Lightning => OID.LightningPlatform,
            _ => default
        };

        if (oid == default)
            yield break;

        foreach (var a in Enemies(oid))
        {
            yield return a.Rotation;
            yield return a.Rotation + 180.Degrees();
        }
    }
}
