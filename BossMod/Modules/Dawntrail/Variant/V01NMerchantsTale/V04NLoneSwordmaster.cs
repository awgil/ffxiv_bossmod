namespace BossMod.Dawntrail.Variant.V01NMerchantsTale.V04NLoneSwordmaster;

public enum OID : uint
{
    Boss = 0x4B12, // R5.000, x?
    ForceOfWillBig = 0x4C3C, // R2.000, x?
    ForceOfWillJoint = 0x4B1E, // R1.000, x?
    ForceOfWillWall = 0x4B14, // R0.500, x?
    ForceOfWillBoulderSlash = 0x4B13,
    FallenRock = 0x4B15,
    MagneticRock = 0x4B16,
    Helper = 0x233C, // R0.500, x?, Helper type
}
public enum AID : uint
{
    Attack = 45128, // 4B12->player, no cast, single-target
    MaleficQuartering = 46608, // 4B12->self, 5.0s cast, single-target
    MaleficInfluence = 46610, // 233C->player/4B15, no cast, single-target
    MaleficInfluence1 = 46609, // 233C->player/4B15, no cast, single-target
    LashOfLight = 46614, // 233C->self, 4.0s cast, range 40 90-degree cone
    VanishingHorizon = 46613, // 4B12->self, 4.0s cast, single-target

    CrusherOfLionsCast = 47993, // 4B12->self, 5.5+0.5s cast, single-target
    CrusherOfLionsCone = 47994, // 233C->self, 6.0s cast, range 40 ?-degree cone

    EarthRendingEightCirc = 46619, // 233C->location, 5.0s cast, range 8 circle
    EarthRendingEightCast = 46618, // 4B12->self, 5.0s cast, single-target
    EarthRendingEightCross = 46620, // 233C->self, 3.0s cast, range 40 width 8 cross

    WaitingWounds = 46622, // 233C->location, 8.0s cast, range 10 circle
    WaitingWounds1 = 46621, // 4B12->self, 8.0s cast, single-target

    HeavensConfluenceCast = 47560, // 4B12->self, 5.0s cast, single-target
    HeavensConfluenceCast2 = 47561, // 4B12->self, 5.0s cast, single-target
    HeavensConfluenceInner = 46625, // 233C->self, 5.0s cast, range 8 circle
    HeavensConfluenceInner2 = 46627, // 233C->self, 7.0s cast, range 8 circle
    HeavensConfluenceDonut = 46628, // 233C->self, 7.0s cast, range 8-60 donut
    HeavensConfluenceDonut2 = 46626, // 233C->self, 5.0s cast, range 8-60 donut
    HeavensConfluenceBait = 46623, // 4B12->player, no cast, range 5 circle
    HeavensConfluenceBait2 = 46624, // 4B12->player, no cast, range 5 circle

    MaleficInfluence2 = 46611, // 233C->player, no cast, single-target
    MaleficInfluence3 = 46612, // 233C->player, no cast, single-target
    ShiftingHorizon = 46629, // 4B12->self, 4.0s cast, single-target

    UnyieldingWillWall = 48651, // 4B14->4B1E, 9.9s cast, width 4 rect charge
    UnyieldingWillJoint = 46630, // 4B14->location, no cast, width 4 rect charge
    UnyieldingWillGround = 46631, // 4B14->player, no cast, width 4 rect charge

    SteelsbreathReleaseRaidwide = 46638, // 4B12->self, 5.0s cast, range 60 circle
    SteelsbreathReleaseFloat = 46632, // 4B12->self, 5.0s cast, range 60 circle

    MawOfTheWolf = 46642, // 4B12->self, 3.4+1.6s cast, single-target
    MawOfTheWolf1 = 46643, // 233C->self, 5.0s cast, range 80 width 80 rect
    StingOfTheScorpion = 46645, // 4B12->player, 5.0s cast, single-target
    WillOfTheUnderworld = 47571, // 4C3C->self, 8.0s cast, range 40 width 20 rect
    WillOfTheUnderworldQuarters = 46615, // 4B13->self, 8.0s cast, range 40 width 10 rect

    Plummet = 46639, // Helper->location, 3.0s cast, range 5 circle
    PlummetBig = 46640, // Helper->location, 4.0s cast, range 10 circle
    PlummetMeteor = 46641, // Helper->location, 10.0s cast, range 60 circle
    PlummetBoulder = 46633, // Helper->location, 3.0s cast, range 3 circle

    Concentrativity = 46644, // 4B12->self, 5.0s cast, range 60 circle
    ConcentrativityKB = 47233, // Boss->self, 6.5s cast, range 60 circle
    Magnetism = 46634, // 4B16->self, 8.0s cast, single-target
    Repel = 46635, // Helper->player, 8.0s cast, single-target
    Attract = 46636, // Helper->player, 8.0s cast, single-target
}
public enum SID : uint
{
    MaleficN = 4780, // none->player, extra=0x0
    MaleficE = 4773, // none->player, extra=0x0
    MaleficW = 4774, // none->player, extra=0x0
    MaleficS = 4776, // none->player, extra=0x0

    MaleficNE = 4781, // none->player, extra=0x0
    MaleficNW = 4782, // none->player, extra=0x0
    MaleficSE = 4777, // none->player, extra=0x0
    MaleficSW = 4778, // none->player, extra=0x0
    MaleficEW = 4775, // none->player, extra=0x0
    MaleficNS = 4784, // none->player, extra=0x0

    MaleficNEW = 4783,
    MaleficSEW = 4779,
    MaleficNSE = 4785,
    MaleficNSW = 4786,

    MaleficBoulderE = 4966,
    MaleficBoulderW = 4967,

    Bind = 2518, // none->player, extra=0x0

    PositiveCharge = 4821, // none->player, extra=0x0
    NegativeCharge = 4822, // none->player, extra=0x0
    MagneticLevitation = 4837, // none->player, extra=0x12C
    BoulderPositiveCharge = 4824,
    BoulderNegativeCharge = 4823,

}
public enum IconID : uint
{
    onethreesix = 136, // player->self
    HeavensConfluenceBaitIcon = 376, // player->self
    twooneeight = 218, // player->self
}
public enum TetherID : uint
{
    ForceOfWillWallTether = 371, // ForceOfWill2/ForceOfWill1->ForceOfWill1/player
    Magnetism = 38, // 4B16->player

}

class MaleficQuartering(BossModule module) : Components.RaidwideCast(module, AID.MaleficQuartering);
class VanishingHorizon(BossModule module) : Components.StandardAOEs(module, AID.LashOfLight, new AOEShapeCone(40, 45f.Degrees()), 2);
class WillOfTheUnderworld(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private readonly AOEShapeRect rect = new(40f, 10f);
    private readonly AOEShapeCone cone = new(40, 45f.Degrees());

    public bool _northUnsafe;
    public bool _southUnsafe;
    public bool _eastUnsafe;
    public bool _westUnsafe;
    private static Angle SnapCardinal(Angle rot)
    {
        Angle[] cardinals =
        [
        0f.Degrees(),
        90f.Degrees(),
        180f.Degrees(),
        (-180f).Degrees(),
        (-90f).Degrees(),
    ];

        var snapped = cardinals.OrderBy(a => (rot - a).Abs()).First();

        if (snapped == (-180f).Degrees())
            snapped = 180f.Degrees();

        return snapped;
    }
    private static bool HasNorth(SID sid) => sid is SID.MaleficN or SID.MaleficNE or SID.MaleficNW or SID.MaleficNS or SID.MaleficNEW or SID.MaleficNSE or SID.MaleficNSW;
    private static bool HasEast(SID sid) => sid is SID.MaleficE or SID.MaleficEW or SID.MaleficNE or SID.MaleficSE or SID.MaleficNEW or SID.MaleficNSE or SID.MaleficSEW;
    private static bool HasSouth(SID sid) => sid is SID.MaleficS or SID.MaleficSE or SID.MaleficSW or SID.MaleficNS or SID.MaleficNSE or SID.MaleficNSW or SID.MaleficSEW;
    private static bool HasWest(SID sid) => sid is SID.MaleficW or SID.MaleficNW or SID.MaleficSW or SID.MaleficEW or SID.MaleficNEW or SID.MaleficNSW or SID.MaleficSEW;
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
        if ((OID)actor.OID is OID.ForceOfWillBig)
        {
            var rot = SnapCardinal(actor.Rotation);

            if (rot == 180f.Degrees() && _southUnsafe)
                _aoes.Add(new(rect, actor.Position, rot));
            if (rot == 0f.Degrees() && _northUnsafe)
                _aoes.Add(new(rect, actor.Position, rot));
            if (rot == 90f.Degrees() && _westUnsafe)
                _aoes.Add(new(rect, actor.Position, rot));
            if (rot == (-90f).Degrees() && _eastUnsafe)
                _aoes.Add(new(rect, actor.Position, rot));
        }
    }
    private void RefreshUnsafeDirections()
    {
        var player = Raid.Player();
        if (player == null)
            return;
        _northUnsafe = false;
        _eastUnsafe = false;
        _southUnsafe = false;
        _westUnsafe = false;
        foreach (var s in player.Statuses)
        {
            var sid = (SID)s.ID;
            if (HasNorth(sid))
                _northUnsafe = true;
            if (HasEast(sid))
                _eastUnsafe = true;
            if (HasSouth(sid))
                _southUnsafe = true;
            if (HasWest(sid))
                _westUnsafe = true;
        }
    }
    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (actor == Raid.Player())
            RefreshUnsafeDirections();
    }
    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (actor == Raid.Player())
            RefreshUnsafeDirections();
    }
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.WillOfTheUnderworld or AID.CrusherOfLionsCone)
        {
            _aoes.Clear();
        }
    }
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.CrusherOfLionsCast)
        {
            if (_southUnsafe)
                _aoes.Add(new(cone, Module.Center, 180f.Degrees()));
            if (_northUnsafe)
                _aoes.Add(new(cone, Module.Center, 0f.Degrees()));
            if (_westUnsafe)
                _aoes.Add(new(cone, Module.Center, 90f.Degrees()));
            if (_eastUnsafe)
                _aoes.Add(new(cone, Module.Center, -90f.Degrees()));
        }
    }
}
class EarthRendingEightCircle(BossModule module) : Components.StandardAOEs(module, AID.EarthRendingEightCirc, 8f);
class EarthRendingEightCross(BossModule module) : Components.StandardAOEs(module, AID.EarthRendingEightCross, new AOEShapeCross(40, 4f));
class WaitingWounds(BossModule module) : Components.StandardAOEs(module, AID.WaitingWounds, 10f, 6);
class HeavensConfluence(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _circle;
    private AOEInstance? _donut;

    private static readonly AOEShapeCircle circ = new(8f);
    private static readonly AOEShapeDonut donut = new(8f, 60f);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        AOEInstance? next = null;

        if (_circle != null)
            next = _circle;

        if (_donut != null && (next == null || _donut.Value.Activation < next.Value.Activation))
            next = _donut;

        if (next != null)
            yield return next.Value with { Color = ArenaColor.AOE };
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var activation = Module.CastFinishAt(spell);

        switch ((AID)spell.Action.ID)
        {
            case AID.HeavensConfluenceInner:
            case AID.HeavensConfluenceInner2:
                _circle = new(circ, Module.PrimaryActor.Position, default, activation);
                break;
            case AID.HeavensConfluenceDonut:
            case AID.HeavensConfluenceDonut2:
                _donut = new(donut, Module.PrimaryActor.Position, default, activation);
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.HeavensConfluenceInner:
            case AID.HeavensConfluenceInner2:
                _circle = null;
                break;
            case AID.HeavensConfluenceDonut:
            case AID.HeavensConfluenceDonut2:
                _donut = null;
                break;
        }
    }
}

class HeavensConfluenceBait(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCircle(5f), (uint)IconID.HeavensConfluenceBaitIcon, AID.HeavensConfluenceBait, centerAtTarget: true)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if ((AID)spell.Action.ID is AID.HeavensConfluenceBait or AID.HeavensConfluenceBait2)
            CurrentBaits.Clear();
    }
}
class UnyieldingWillBaited(BossModule module) : Components.GenericAOEs(module)
{
    bool _bound;
    readonly WillOfTheUnderworld _will = module.FindComponent<WillOfTheUnderworld>()!;
    readonly List<(Actor From, Actor To, int Order)> AllTethers = [];
    DateTime _appearedAt;

    Actor? GetRealSource(Actor target)
    {
        Actor? parent = null;
        while (AllTethers.FirstOrNull(t => t.To == target) is { } tether)
            parent = target = tether.From;
        return parent;
    }

    private static Angle SnapCardinal(Angle rot)
    {
        Angle[] cardinals =
        [
            0f.Degrees(),
            90f.Degrees(),
            180f.Degrees(),
            (-90f).Degrees(),
        ];

        var snapped = cardinals.OrderBy(a => (rot - a).Abs()).First();

        if (snapped == (-180f).Degrees())
            snapped = 180f.Degrees();

        return snapped;
    }
    private bool IsUnsafeCardinal(Angle rot)
    {
        var snapped = SnapCardinal(rot);

        if (snapped == 180f.Degrees() || snapped == (-180f).Degrees())
            return _will._southUnsafe;
        if (snapped == 0f.Degrees())
            return _will._northUnsafe;
        if (snapped == (-90f).Degrees())
            return _will._eastUnsafe;
        if (snapped == 90f.Degrees())
            return _will._westUnsafe;

        return false;
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        ulong parentID = 0;

        if (GetRealSource(actor) is { } force)
        {
            parentID = force.InstanceID;

            if (!_bound)
            {
                var bindAt = _appearedAt.AddSeconds(6.8f);

                if (IsUnsafeCardinal(force.Rotation + 180f.Degrees()))
                    yield return new(new AOEShapeRect(40, 2), force.Position, force.Rotation, bindAt);
                if (IsUnsafeCardinal(force.Rotation + 90f.Degrees()))
                    yield return new(new AOEShapeRect(40, 40, -2), force.Position, force.Rotation + 90f.Degrees(), bindAt);
                if (IsUnsafeCardinal(force.Rotation - 90f.Degrees()))
                    yield return new(new AOEShapeRect(40, 40, -2), force.Position, force.Rotation - 90f.Degrees(), bindAt);
            }
        }

        foreach (var (from, to, i) in AllTethers)
        {
            // skip any charges not targeting us
            if (to == actor || from.InstanceID == parentID)
                continue;

            var dir = to.Position - from.Position;
            var activation = _appearedAt.AddSeconds(8 + 0.5 * i);
            yield return new(new AOEShapeRect(dir.Length(), 2), from.Position, dir.ToAngle(), activation);
        }
    }
    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        // TODO: add hints for baiting this away from other players?
        if (AllTethers.FirstOrNull(t => t.To == pc) is { From: var src } && (OID)src.OID != OID.ForceOfWillWall)
        {
            var dir = pc.Position - src.Position;
            Arena.AddRect(src.Position, dir.Normalized(), dir.Length(), 0, 2, ArenaColor.Danger);
        }
    }
    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == TetherID.ForceOfWillWallTether)
        {
            if (_appearedAt == default)
                _appearedAt = WorldState.CurrentTime;

            var target = WorldState.Actors.Find(tether.Target)!;

            AllTethers.RemoveAll(t => t.From == source);
            AllTethers.Add((source, target, (OID)source.OID == OID.ForceOfWillWall ? 0 : 1));

            // if the target player is already in the rect when tethers appear, second tether only spawns after they move
            if ((OID)target.OID == OID.ForceOfWillJoint)
            {
                var targetDist = (target.Position - source.Position).Length();
                var playerTarget = Raid.WithoutSlot().InShape(new AOEShapeRect(60, 2), source.Position, source.Rotation).FirstOrDefault(p =>
                {
                    var playerDist = (p.Position - source.Position).Dot(source.Rotation.ToDirection());
                    return MathF.Abs(playerDist - targetDist) < 0.1f;
                });
                if (playerTarget != null)
                    AllTethers.Add((target, playerTarget, 1));
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.UnyieldingWillWall:
                NumCasts++;
                AllTethers.RemoveAll(t => (OID)t.From.OID == OID.ForceOfWillWall);
                break;
            case AID.UnyieldingWillJoint:
                NumCasts++;
                AllTethers.RemoveAll(t => t.To.OID == 0);
                break;
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Bind)
            _bound = true;
    }
    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Bind)
            _bound = false;
    }
}

class SteelsbreathRelease(BossModule module) : Components.RaidwideCast(module, AID.SteelsbreathReleaseRaidwide);
class PlummetSmall(BossModule module) : Components.StandardAOEs(module, AID.Plummet, 5f);
class PlummetBig(BossModule module) : Components.StandardAOEs(module, AID.PlummetBig, 10f);
class PlummetMeteor(BossModule module) : Components.StandardAOEs(module, AID.PlummetMeteor, 26f);
class PlummetBoulders(BossModule module) : Components.StandardAOEs(module, AID.PlummetBoulder, 3f);
class WillOfTheUnderworldLOS(BossModule module) : Components.GenericAOEs(module, AID.WillOfTheUnderworldQuarters)
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeRect FrontRect = new(40f, 5f, 0f);
    private static readonly AOEShapeRect RearRect = new(40f, 1.65f, 0f);
    private readonly Dictionary<ulong, (bool EastUnsafe, bool WestUnsafe)> _rockUnsafe = [];
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoes.Count > 0)
        {
            foreach (var z in _aoes)
                yield return z with { Color = ArenaColor.AOE };
        }
    }

    private static bool SameRow(WPos a, WPos b) => MathF.Abs(a.Z - b.Z) < 1f;

    private void SetRockUnsafe(Actor rock, bool? east = null, bool? west = null)
    {
        _rockUnsafe.TryGetValue(rock.InstanceID, out var flags);
        if (east != null)
            flags.EastUnsafe = east.Value;
        if (west != null)
            flags.WestUnsafe = west.Value;
        _rockUnsafe[rock.InstanceID] = flags;
    }

    private void Refresh()
    {
        _aoes.Clear();

        var attacks = Module.Enemies(OID.ForceOfWillBoulderSlash).Where(a => a.CastInfo?.Action.ID == (uint)AID.WillOfTheUnderworldQuarters).ToList();
        var rocks = Module.Enemies(OID.FallenRock).ToList();

        foreach (var attack in attacks)
        {
            var rock = rocks.FirstOrDefault(r => SameRow(r.Position, attack.Position));
            if (rock == null)
                continue;

            var dir = attack.Rotation.ToDirection();

            _aoes.Add(new AOEInstance(FrontRect, rock.Position, attack.Rotation - 180f.Degrees()));
            _aoes.Add(new AOEInstance(RearRect, new WPos(rock.Position.X, rock.Position.Z + 3.3f), attack.Rotation));
            _aoes.Add(new AOEInstance(RearRect, new WPos(rock.Position.X, rock.Position.Z - 3.3f), attack.Rotation));

            _rockUnsafe.TryGetValue(rock.InstanceID, out var flags);

            bool fillCenter = dir.X > 0 ? flags.WestUnsafe : dir.X < 0 && flags.EastUnsafe;

            if (fillCenter)
                _aoes.Add(new AOEInstance(RearRect, rock.Position, attack.Rotation));
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((OID)caster.OID == OID.ForceOfWillBoulderSlash && (AID)spell.Action.ID == AID.WillOfTheUnderworldQuarters)
            Refresh();
    }
    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((OID)caster.OID == OID.ForceOfWillBoulderSlash && (AID)spell.Action.ID == AID.WillOfTheUnderworldQuarters)
            Refresh();
    }
    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID is OID.FallenRock or OID.ForceOfWillBoulderSlash)
            Refresh();
    }
    public override void OnActorDestroyed(Actor actor)
    {
        if ((OID)actor.OID is OID.FallenRock or OID.ForceOfWillBoulderSlash)
        {
            _rockUnsafe.Remove(actor.InstanceID);
            Refresh();
        }
    }
    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((OID)actor.OID is OID.FallenRock)
        {
            if ((SID)status.ID == SID.MaleficBoulderE)
                SetRockUnsafe(actor, east: true);
            if ((SID)status.ID == SID.MaleficBoulderW)
                SetRockUnsafe(actor, west: true);
            Refresh();
        }
    }
    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((OID)actor.OID is OID.FallenRock)
        {
            if ((SID)status.ID == SID.MaleficBoulderE)
                SetRockUnsafe(actor, east: false);
            if ((SID)status.ID == SID.MaleficBoulderW)
                SetRockUnsafe(actor, west: false);
            Refresh();
        }
    }
}
class MawOfTheWolf(BossModule module) : Components.StandardAOEs(module, AID.MawOfTheWolf, new AOEShapeRect(80f, 40f));
class StingOfTheScorpion(BossModule module) : Components.SingleTargetCast(module, AID.StingOfTheScorpion);
class MagneticQuadrants(BossModule module) : Components.GenericAOEs(module, AID.Concentrativity)
{
    private static readonly AOEShapeRect Quad = new(15f, 15f, 15f);

    private bool _positive;
    private bool _negative;
    private DateTime _activation;

    private static readonly WDir NE = new(+15f, -15f);
    private static readonly WDir NW = new(-15f, -15f);
    private static readonly WDir SE = new(+15f, +15f);
    private static readonly WDir SW = new(-15f, +15f);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_activation != default)
        {
            if (_positive)
            {
                yield return new AOEInstance(Quad, Module.Center + NW, default, _activation) with { Color = ArenaColor.AOE };
                yield return new AOEInstance(Quad, Module.Center + SE, default, _activation) with { Color = ArenaColor.AOE };
            }
            if (_negative)
            {
                yield return new AOEInstance(Quad, Module.Center + NE, default, _activation) with { Color = ArenaColor.AOE };
                yield return new AOEInstance(Quad, Module.Center + SW, default, _activation) with { Color = ArenaColor.AOE };
            }
        }
    }
    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (actor == Raid.Player())
        {
            switch ((SID)status.ID)
            {
                case SID.PositiveCharge:
                    _positive = true;
                    _negative = false;
                    break;
                case SID.NegativeCharge:
                    _negative = true;
                    _positive = false;
                    break;
            }
        }
    }
    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (actor == Raid.Player())
        {
            switch ((SID)status.ID)
            {
                case SID.PositiveCharge:
                    _positive = false;
                    break;
                case SID.NegativeCharge:
                    _negative = false;
                    break;
            }
        }
    }
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Concentrativity)
            _activation = Module.CastFinishAt(spell);
    }
    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Concentrativity)
            _activation = default;
    }
}
class MagneticBoulders(BossModule module) : Components.GenericAOEs(module, AID.Magnetism)
{
    private readonly List<Actor> _rocks = [];
    private readonly Dictionary<Actor, bool> _rockPositive = []; // true = positive, false = negative
    private Actor? _pairedRock;
    private bool? _playerPositive;
    private DateTime _activation;

    private static readonly AOEShapeDonut donut = new(2f, 70f);

    private bool Active => _activation != default;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (!Active || actor != Raid.Player())
            yield break;

        if (_pairedRock == null || _playerPositive == null)
            yield break;

        if (!_rockPositive.TryGetValue(_pairedRock, out var rockPositive))
            yield break;

        if (_playerPositive.Value == rockPositive)
        {
            var toCenter = (Module.Center - _pairedRock.Position).Normalized();
            var hintPos = _pairedRock.Position + 6f * toCenter;
            yield return new AOEInstance(donut, hintPos, default, _activation) with { Color = ArenaColor.AOE };
            yield break;
        }
        var opposite = GetOppositeRock(_pairedRock);
        if (opposite != null)
        {
            var toCenter = (Module.Center - opposite.Position).Normalized();
            var hintPos = opposite.Position + 6f * toCenter;
            yield return new AOEInstance(donut, hintPos, default, _activation) with { Color = ArenaColor.AOE };
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.MagneticRock && !_rocks.Contains(actor))
            _rocks.Add(actor);
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if ((OID)actor.OID == OID.MagneticRock)
        {
            _rocks.Remove(actor);
            _rockPositive.Remove(actor);

            if (_pairedRock == actor)
                _pairedRock = null;
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.PositiveCharge:
                if (actor == Raid.Player())
                    _playerPositive = true;
                break;

            case SID.NegativeCharge:
                if (actor == Raid.Player())
                    _playerPositive = false;
                break;

            case SID.BoulderPositiveCharge:
                if ((OID)actor.OID == OID.MagneticRock)
                    _rockPositive[actor] = true;
                break;

            case SID.BoulderNegativeCharge:
                if ((OID)actor.OID == OID.MagneticRock)
                    _rockPositive[actor] = false;
                break;
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.PositiveCharge:
                if (actor == Raid.Player() && _playerPositive == true)
                    _playerPositive = null;
                break;

            case SID.NegativeCharge:
                if (actor == Raid.Player() && _playerPositive == false)
                    _playerPositive = null;
                break;
        }
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID != TetherID.Magnetism)
            return;

        if ((OID)source.OID != OID.MagneticRock)
            return;

        var target = WorldState.Actors.Find(tether.Target);
        if (target == Raid.Player())
            _pairedRock = source;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Magnetism)
            _activation = Module.CastFinishAt(spell);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Magnetism)
            _activation = default;
    }

    private Actor? GetOppositeRock(Actor rock)
    {
        Actor? best = null;
        float bestDist = float.MinValue;

        foreach (var other in _rocks)
        {
            if (other == rock)
                continue;

            var dist = (other.Position - rock.Position).LengthSq();
            if (dist > bestDist)
            {
                bestDist = dist;
                best = other;
            }
        }

        return best;
    }
}
class V04NLoneSwordmasterStates : StateMachineBuilder
{
    public V04NLoneSwordmasterStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<MaleficQuartering>()
            .ActivateOnEnter<VanishingHorizon>()
            .ActivateOnEnter<WillOfTheUnderworld>()
            .ActivateOnEnter<WillOfTheUnderworldLOS>()
            .ActivateOnEnter<EarthRendingEightCircle>()
            .ActivateOnEnter<EarthRendingEightCross>()
            .ActivateOnEnter<WaitingWounds>()
            .ActivateOnEnter<HeavensConfluence>()
            .ActivateOnEnter<HeavensConfluenceBait>()
            .ActivateOnEnter<UnyieldingWillBaited>()
            .ActivateOnEnter<SteelsbreathRelease>()
            .ActivateOnEnter<PlummetSmall>()
            .ActivateOnEnter<PlummetBig>()
            .ActivateOnEnter<PlummetMeteor>()
            .ActivateOnEnter<PlummetBoulders>()
            .ActivateOnEnter<MawOfTheWolf>()
            .ActivateOnEnter<StingOfTheScorpion>()
            .ActivateOnEnter<MagneticQuadrants>()
            .ActivateOnEnter<MagneticBoulders>();
    }
}

[ModuleInfo(Contributors = "VeraNala", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1066, NameID = 14323)]
public class V04NLoneSwordmaster(WorldState ws, Actor primary) : BossModule(ws, primary, new(169.97f, -815.03f), new ArenaBoundsSquare(19.5f));
