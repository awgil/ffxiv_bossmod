namespace BossMod.Dawntrail.Criterion.C01MerchantsTale.C012LoneSwordmaster;

public enum OID : uint
{
    Boss = 0x4B1A, // R5.000, x1
    Helper = 0x233C, // R0.500, x26, Helper type
    ForceOfWillWallTether = 0x4B1C, // R0.500, x0 (spawn during fight)
    ForceOfWillGroundTether = 0x4B1E, // R1.000, x0 (spawn during fight)
    ForceOfWillSmall = 0x4B1B, // R1.250, x0 (spawn during fight)
    ForceOfWillLarge = 0x4C3E, // R2.000, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 45128, // Boss->player, no cast, single-target
    Jump = 46607, // Boss->location, no cast, single-target
    SteelsbreathReleaseCast = 46686, // Boss->self, 5.0s cast, range 60 circle
    SteelsbreathReleaseLavaCast = 46720, // Boss->self, 5.0s cast, range 60 circle
    NearToHeavenOneCast = 47566, // Boss->self, 5.0s cast, single-target
    FarFromHeavenOneCast = 47567, // Boss->self, 5.0s cast, single-target
    NearToHeavenTwoCast = 47568, // Boss->self, 5.0s cast, single-target
    FarFromHeavenTwoCast = 47569, // Boss->self, 5.0s cast, single-target
    NearToHeavenOne = 46687, // Boss->player, no cast, range 5 circle
    FarFromHeavenOne = 46688, // Boss->player, no cast, range 5 circle
    NearToHeavenTwo = 46689, // Boss->players, no cast, range 5 circle
    FarFromHeavenTwo = 46690, // Boss->players, no cast, range 5 circle
    HeavensConfluenceCircle = 46691, // Helper->players, no cast, range 8 circle
    HeavensConfluenceDonut = 46692, // Helper->player, no cast, range 8-60 donut
    MaleficQuartering = 46693, // Boss->self, 3.0s cast, single-target
    MaleficInfluence1 = 46694, // Helper->player, no cast, single-target
    MaleficInfluence2 = 46695, // Helper->player, no cast, single-target
    MaleficInfluence3 = 46696, // Helper->player, no cast, single-target
    MaleficInfluence4 = 46697, // Helper->player, no cast, single-target
    LashOfLight = 46701, // Helper->self, 4.0s cast, range 40 90-degree cone
    ShiftingHorizon = 46700, // Boss->self, 4.0s cast, single-target
    MaleficPortentCast = 46698, // Boss->self, 6.0s cast, single-target
    MaleficPortent = 46699, // Helper->player, no cast, single-target
    UnyieldingWillWall = 48653, // 4B1C->4B1E, 7.9s cast, width 4 rect charge
    UnyieldingWillVisual = 46702, // 4B1C->location, no cast, width 4 rect charge
    UnyieldingWillGround = 46703, // 4B1C->player, no cast, width 4 rect charge
    EchoingOrbitCast = 46704, // Boss->self, 4.0s cast, single-target
    EchoingHush = 46705, // Helper->location, 4.0s cast, range 8 circle
    EchoingOrbitAOE = 46706, // Helper->location, 3.0s cast, range 8-60 donut
    EchoingEightCast = 46707, // Boss->self, 4.0s cast, single-target
    EchoingEightAOE = 46708, // Helper->self, 3.0s cast, range 40 width 8 cross
    WaitingWoundsCast = 46713, // Boss->self, 5.0s cast, single-target
    WaitingWounds = 46714, // Helper->location, 5.0s cast, range 10 circle
    SilentEightCast = 46711, // Boss->self, 4.0s cast, single-target
    ResoundingSilenceSpread = 46712, // Helper->player, no cast, range 8 circle
    MawOfTheWolfCast = 46715, // Boss->self, 3.4+1.6s cast, single-target
    MawOfTheWolf = 46716, // Helper->self, 5.0s cast, range 80 width 80 rect
    FangsOfTheUnderworldCast = 46717, // Boss->self, 5.0s cast, single-target
    FangsOfTheUnderworldStack = 46719, // Helper->self, no cast, range 60 width 10 rect
    FangsOfTheUnderworldVisual = 46718, // Boss->self, no cast, single-target
    SteelHorizon = 46721, // Boss->self, 4.0s cast, single-target
    SilentHeat = 46725, // Boss->self, 4.0s cast, single-target
    WillOfTheUnderworldSmallSlow = 46722, // 4B1B->self, 8.0s cast, range 40 width 10 rect
    CardinalHorizons = 46733, // Boss->self, 4.0s cast, single-target
    WillOfTheUnderworldSmallFast = 46749, // 4B1B->self, 4.0s cast, range 40 width 10 rect
    MaleficAlignmentCast = 46723, // Boss->self, 3.0+1.0s cast, single-target
    MaleficAlignment = 46724, // Helper->self, 4.0s cast, range 40 90-degree cone
    PlummetTiny = 46727, // Helper->location, 3.0s cast, range 3 circle
    PlummetSmall = 46728, // Helper->location, 3.0s cast, range 5 circle
    PlummetLarge = 46729, // Helper->location, 4.0s cast, range 10 circle
    PlummetMeteor = 46730, // Helper->location, 10.0s cast, range 60 circle
    SteelsbreathBonds = 46731, // Boss->self, no cast, single-target
    WillOfTheUnderworldLarge = 47763, // 4C3E->self, 4.5s cast, range 40 width 20 rect
    SteelScream = 46732, // Helper->player, no cast, single-target
    MortalFateCast = 46734, // Boss->self, 12.0+1.0s cast, single-target
    MortalFate = 46735, // Helper->self, 13.0s cast, range 40 90-degree cone, enrage
}

public enum SID : uint
{
    PhysicalVulnerabilityUp = 2940, // Boss/Helper->player, extra=0x0
    MaleficE = 4773, // none->player, extra=0x0
    MaleficW = 4774, // none->player, extra=0x0
    MaleficEW = 4775, // none->player, extra=0x0
    MaleficS = 4776, // none->player, extra=0x0
    MaleficSE = 4777, // none->player, extra=0x0
    MaleficSW = 4778, // none->player, extra=0x0
    MaleficSEW = 4779, // none->player, extra=0x0
    MaleficN = 4780, // none->player, extra=0x0
    MaleficNE = 4781, // none->player, extra=0x0
    MaleficNW = 4782, // none->player, extra=0x0
    MaleficNEW = 4783, // none->player, extra=0x0
    MaleficNS = 4784, // none->player, extra=0x0
    MaleficNSE = 4785, // none->player, extra=0x0
    MaleficNSW = 4786, // none->player, extra=0x0
    MaleficNSEW = 4787, // none->player, extra=0x0
    Bind = 2518, // none->player, extra=0x0
    Incurable = 2398, // none->4B1D, extra=0x0
}

public enum IconID : uint
{
    FromHeaven1 = 332, // player->self
    FromHeaven2 = 333, // player->self
    Prey = 652, // player->self
    Ok = 136, // player->self
    Spread = 499, // player->self
    Fangs = 572, // Boss->player
    Chains = 653, // player->self
}

public enum TetherID : uint
{
    TetherN = 357, // player->Boss
    TetherE = 358, // player->Boss
    TetherW = 359, // player->Boss
    TetherS = 360, // player->Boss
    UnyieldingWill = 371, // 4B1C/4B1E->4B1E/player
    Chains = 163, // player->player
}

[Flags]
enum Side
{
    None = 0,
    E = 1,
    W = 2,
    S = 4,
    N = 8,
    All = N | E | S | W
}

static class SideHelpers
{
    extension(Side a)
    {
        public bool Matches(Side b) => (a & b) != Side.None;

        public Side RotateR()
        {
            var s = Side.None;
            if (a.HasFlag(Side.E))
                s |= Side.S;
            if (a.HasFlag(Side.S))
                s |= Side.W;
            if (a.HasFlag(Side.W))
                s |= Side.N;
            if (a.HasFlag(Side.N))
                s |= Side.E;
            return s;
        }

        public Side RotateL()
        {
            var s = Side.None;
            if (a.HasFlag(Side.E))
                s |= Side.N;
            if (a.HasFlag(Side.N))
                s |= Side.W;
            if (a.HasFlag(Side.W))
                s |= Side.S;
            if (a.HasFlag(Side.S))
                s |= Side.E;
            return s;
        }
    }

    public static Side FromDirection(WDir d)
    {
        var abs = d.Abs();
        if (abs.X < abs.Z)
            d.X = 0;
        else
            d.Z = 0;
        return d.Sign() switch
        {
            (0, 1) => Side.S,
            (0, -1) => Side.N,
            (1, 0) => Side.E,
            (-1, 0) => Side.W,
            _ => throw new InvalidOperationException("unreachable")
        };
    }
    public static Side FromAngle(Angle a) => FromDirection(a.ToDirection());
}

class SteelsbreathRelease(BossModule module) : Components.RaidwideCast(module, AID.SteelsbreathReleaseCast);
class SteelsbreathRelease2(BossModule module) : Components.RaidwideCast(module, AID.SteelsbreathReleaseLavaCast);

class FromHeaven(BossModule module) : Components.UniformStackSpread(module, 5, 5, maxStackSize: 2)
{
    Actor? _target;
    int _count;
    BitMask _forbidden;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.NearToHeavenOneCast:
            case AID.FarFromHeavenOneCast:
                _count = 1;
                Init();
                break;
            case AID.NearToHeavenTwoCast:
            case AID.FarFromHeavenTwoCast:
                _count = 2;
                Init();
                break;
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        switch ((IconID)iconID)
        {
            case IconID.Prey:
                _target = actor;
                Init();
                break;
            case IconID.FromHeaven2:
            case IconID.FromHeaven1:
                _forbidden.Set(Raid.FindSlot(actor.InstanceID));
                Init();
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.NearToHeavenOne or AID.NearToHeavenTwo or AID.FarFromHeavenOne or AID.FarFromHeavenTwo)
        {
            Stacks.Clear();
            Spreads.Clear();
        }
    }

    void Init()
    {
        if (Active || _count == 0 || _target == null || _forbidden.NumSetBits() < 2)
            return;

        if (_count == 1)
            AddSpread(_target, WorldState.FutureTime(5.2f));

        if (_count == 2)
            AddStack(_target, WorldState.FutureTime(5.2f), _forbidden);
    }
}

class HeavensConfluence(BossModule module) : Components.GenericBaitAway(module, centerAtTarget: true)
{
    enum Order
    {
        None,
        NearFirst,
        FarFirst
    }

    bool _initialized;
    Order _nextOrder = Order.None;
    readonly int[] _playerOrder = Utils.MakeArray(4, -1);

    public static readonly AOEShape Donut = new AOEShapeDonut(8, 60);
    public static readonly AOEShape Circle = new AOEShapeCircle(8);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.FarFromHeavenOneCast:
            case AID.FarFromHeavenTwoCast:
                _nextOrder = Order.FarFirst;
                Init();
                break;
            case AID.NearToHeavenOneCast:
            case AID.NearToHeavenTwoCast:
                _nextOrder = Order.NearFirst;
                Init();
                break;
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        switch ((IconID)iconID)
        {
            case IconID.FromHeaven2:
                _playerOrder[Raid.FindSlot(actor.InstanceID)] = 1;
                Init();
                break;
            case IconID.FromHeaven1:
                _playerOrder[Raid.FindSlot(actor.InstanceID)] = 0;
                Init();
                break;
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        base.DrawArenaBackground(pcSlot, pc);

        // it's impossible to tell circle and donut baits apart just from the outline
        if (CurrentBaits is [{ Target: var t, Shape: var s }, ..] && t == pc && s is AOEShapeDonut)
            s.Draw(Arena, t, ArenaColor.AOE);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!EnableHints || CurrentBaits.Count == 0)
            return;

        var nextBait = CurrentBaits[0];
        if (nextBait.Target == actor && PlayersClippedBy(nextBait).Any())
            hints.Add("Bait away from raid!");

        if (nextBait.Target != actor && IsClippedBy(actor, nextBait))
            hints.Add("GTFO from baited aoe!");
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.HeavensConfluenceDonut or AID.HeavensConfluenceCircle)
        {
            NumCasts++;
            if (CurrentBaits.Count > 0)
                CurrentBaits.RemoveAt(0);
        }
    }

    void Init()
    {
        if (_initialized)
            return;

        if (_nextOrder == Order.None)
            return;

        if (Raid[_playerOrder.IndexOf(0)] is not { } first)
            return;
        if (Raid[_playerOrder.IndexOf(1)] is not { } second)
            return;

        var (shape1, shape2) = _nextOrder == Order.NearFirst ? (Circle, Donut) : (Donut, Circle);
        CurrentBaits.Add(new(Module.PrimaryActor, first, shape1, WorldState.FutureTime(5.2f)));
        CurrentBaits.Add(new(Module.PrimaryActor, second, shape2, WorldState.FutureTime(7.3f)));
        _initialized = true;
    }
}

class LashOfLight(BossModule module) : Components.StandardAOEs(module, AID.LashOfLight, new AOEShapeCone(40, 45.Degrees()), maxCasts: 2);

class Malefic(BossModule module) : BossComponent(module)
{
    readonly Side[] _playerStates = new Side[4];

    public Side this[int index]
    {
        get => _playerStates.BoundSafeAt(index);
        set
        {
            if (index is >= 0 and < 4)
                _playerStates[index] = value;
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID is >= 4773 and <= 4787 && Raid.TryFindSlot(actor, out var slot))
            this[slot] = (Side)(status.ID - 4772);
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (status.ID is >= 4773 and <= 4787 && Raid.TryFindSlot(actor, out var slot))
        {
            var side = (Side)(status.ID - 4772);
            if (this[slot] == side)
                this[slot] = Side.None;
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var (i, p) in Raid.WithSlot())
            DrawSides(p, this[i]);
    }

    public static void DrawSides(MiniArena arena, Actor actor, Side state, uint color = 0)
    {
        if (state.HasFlag(Side.E))
            DrawSide(arena, actor, 90, color);
        if (state.HasFlag(Side.W))
            DrawSide(arena, actor, -90, color);
        if (state.HasFlag(Side.N))
            DrawSide(arena, actor, 180, color);
        if (state.HasFlag(Side.S))
            DrawSide(arena, actor, 0, color);
    }

    private void DrawSides(Actor actor, Side state, uint color = 0)
    {
        if (state.HasFlag(Side.E))
            DrawSide(actor, 90, color);
        if (state.HasFlag(Side.W))
            DrawSide(actor, -90, color);
        if (state.HasFlag(Side.N))
            DrawSide(actor, 180, color);
        if (state.HasFlag(Side.S))
            DrawSide(actor, 0, color);
    }

    private void DrawSide(Actor actor, float deg, uint color = 0) => DrawSide(Arena, actor, deg, color);

    private static void DrawSide(MiniArena arena, Actor actor, float deg, uint color = 0)
    {
        arena.PathArcTo(actor.Position, 1.5f, (deg - 35).Degrees().Rad, (deg + 35).Degrees().Rad);
        arena.PathStroke(false, color == 0 ? ArenaColor.Enemy : color);
    }
}

class MaleficPortentCounter(BossModule module) : Components.CastCounter(module, AID.MaleficPortent);
class MaleficPortent(BossModule module) : Components.CastCounter(module, AID.MaleficPortent)
{
    readonly Side[] _sides = new Side[4];
    BitMask _targets;
    BitMask _forceOfWillTargets;
    readonly Malefic _malefic = module.FindComponent<Malefic>()!;

    public bool Active => _targets.Any();

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == TetherID.UnyieldingWill)
            _forceOfWillTargets.Set(Raid.FindSlot(tether.Target));

        var side = (TetherID)tether.ID switch
        {
            TetherID.TetherE => Side.E,
            TetherID.TetherN => Side.N,
            TetherID.TetherS => Side.S,
            TetherID.TetherW => Side.W,
            _ => Side.None
        };

        if (side != Side.None && Raid.TryFindSlot(source, out var slot))
        {
            _sides[slot] = side;
            _targets.Set(slot);
        }
    }

    public override void OnUntethered(Actor source, ActorTetherInfo tether)
    {
        if (Raid.TryFindSlot(source, out var slot))
        {
            _sides[slot] = Side.None;
            _targets.Clear(slot);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var (i, p) in Raid.WithSlot())
            if (_targets[i])
            {
                var shouldTake = ShouldTake(pcSlot, i);

                Arena.AddLine(Module.PrimaryActor.Position, p.Position, shouldTake ? ArenaColor.Safe : ArenaColor.Danger, shouldTake && !_targets[pcSlot] ? 2 : 1);
                Malefic.DrawSides(Arena, p, _sides[i], ArenaColor.Danger);
            }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_targets.Any() && _forceOfWillTargets.Any() && !HasCorrectTether(slot))
            hints.Add(_targets[slot] ? "Pass tether!" : "Take tether!");
    }

    bool HasCorrectTether(int slot)
    {
        return _targets[slot] != _forceOfWillTargets[slot] && !_malefic[slot].Matches(_sides[slot]);
    }

    bool ShouldTake(int slot, int targetSlot)
    {
        return !_targets[slot] && !_forceOfWillTargets[slot] && !HasCorrectTether(targetSlot) && !_malefic[slot].Matches(_sides[targetSlot]);
    }
}

class ForceOfWill(BossModule module) : Components.GenericAOEs(module)
{
    bool _bound;
    readonly Malefic _malefic = module.FindComponent<Malefic>()!;
    readonly List<(Actor From, Actor To, int Order)> AllTethers = [];
    DateTime _appearedAt;

    Actor? GetRealSource(Actor target)
    {
        Actor? parent = null;
        while (AllTethers.FirstOrNull(t => t.To == target) is { } tether)
            parent = target = tether.From;
        return parent;
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

                var sides = _malefic[slot];
                var effCenter = SideHelpers.FromAngle(force.Rotation + 180.Degrees());
                if (sides.HasFlag(effCenter))
                    yield return new(new AOEShapeRect(40, 2), force.Position, force.Rotation, bindAt);
                if (sides.HasFlag(effCenter.RotateL()))
                    yield return new(new AOEShapeRect(40, 40, -2), force.Position, force.Rotation + 90.Degrees(), bindAt);
                if (sides.HasFlag(effCenter.RotateR()))
                    yield return new(new AOEShapeRect(40, 40, -2), force.Position, force.Rotation - 90.Degrees(), bindAt);
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
        if (AllTethers.FirstOrNull(t => t.To == pc) is { From: var src } && (OID)src.OID != OID.ForceOfWillWallTether)
        {
            var dir = pc.Position - src.Position;
            Arena.AddRect(src.Position, dir.Normalized(), dir.Length(), 0, 2, ArenaColor.Danger);
        }
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == TetherID.UnyieldingWill)
        {
            if (_appearedAt == default)
                _appearedAt = WorldState.CurrentTime;

            var target = WorldState.Actors.Find(tether.Target)!;

            AllTethers.RemoveAll(t => t.From == source);
            AllTethers.Add((source, target, (OID)source.OID == OID.ForceOfWillWallTether ? 0 : 1));

            // if the target player is already in the rect when tethers appear, second tether only spawns after they move
            if ((OID)target.OID == OID.ForceOfWillGroundTether)
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
                AllTethers.RemoveAll(t => (OID)t.From.OID == OID.ForceOfWillWallTether);
                break;
            case AID.UnyieldingWillGround:
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
}

class LavaRect(BossModule module) : Components.GenericAOEs(module)
{
    public static readonly AOEShape Rect = new AOEShapeRect(10, 10);

    DateTime _activation;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_activation == default)
            yield break;

        yield return new(Rect, Arena.Center + new WDir(10, -10), 180.Degrees(), _activation);
        yield return new(Rect, Arena.Center + new WDir(10, 10), 90.Degrees(), _activation);
        yield return new(Rect, Arena.Center + new WDir(-10, 10), default, _activation);
        yield return new(Rect, Arena.Center + new WDir(-10, -10), -90.Degrees(), _activation);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.SteelsbreathReleaseLavaCast)
            _activation = Module.CastFinishAt(spell);
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (actor.OID == 0x1EBF72)
        {
            if (state == 0x00010002)
            {
                _activation = default;
                var rect = CurveApprox.Rect(new WDir(10, -5), new WDir(10, 0), new WDir(0, 5));
                var clipper = Arena.Bounds.Clipper;
                Arena.Bounds = new ArenaBoundsCustom(20, clipper.UnionAll(new(rect), new PolygonClipper.Operand(rect.Select(r => r.OrthoR())), new PolygonClipper.Operand(rect.Select(r => r.OrthoL())), new PolygonClipper.Operand(rect.Select(r => -r))));
            }

            if (state == 0x00040008)
            {
                Arena.Bounds = new ArenaBoundsSquare(20);
                _activation = default;
            }
        }
    }
}

class EchoesBait(BossModule module) : BossComponent(module)
{
    DateTime _activation;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.EchoingEightCast or AID.EchoingOrbitCast)
            _activation = Module.CastFinishAt(spell);

        if ((AID)spell.Action.ID is AID.EchoingHush)
            _activation = default;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_activation != default)
            hints.AddForbiddenZone(ShapeContains.InvertedCircle(Arena.Center, 1), _activation);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_activation != default)
            Arena.AddCircle(pc.Position, 8, ArenaColor.Danger);
    }
}

class EchoingHush(BossModule module) : Components.StandardAOEs(module, AID.EchoingHush, 8);
class EchoingEight(BossModule module) : Components.StandardAOEs(module, AID.EchoingEightAOE, new AOEShapeCross(40, 4));
class EchoingOrbit(BossModule module) : Components.StandardAOEs(module, AID.EchoingOrbitAOE, new AOEShapeDonut(8, 60));
class EchoPredict(BossModule module) : Components.GenericAOEs(module)
{
    public static readonly AOEShape Donut = new AOEShapeDonut(8, 60);
    public static readonly AOEShape Cross = new AOEShapeCross(40, 4);

    AOEShape? _next;
    readonly List<AOEInstance> _predicted = [];
    bool _draw;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _draw ? _predicted : [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.EchoingEightCast:
                _next = Cross;
                break;
            case AID.EchoingOrbitCast:
                _next = Donut;
                break;
            case AID.EchoingEightAOE:
            case AID.EchoingOrbitAOE:
                _predicted.Clear();
                _next = null;
                break;
            case AID.EchoingHush:
                _predicted.Add(new(_next!, spell.LocXZ, default, Module.CastFinishAt(spell, 3)));
                if (_next is AOEShapeCross)
                    _predicted.Add(new(_next, spell.LocXZ, 45.Degrees(), Module.CastFinishAt(spell, 3)));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.EchoingHush:
                _draw = true;
                break;
            case AID.EchoingEightAOE:
            case AID.EchoingOrbitAOE:
                NumCasts++;
                break;
        }
    }
}

class WaitingWounds(BossModule module) : Components.StandardAOEs(module, AID.WaitingWounds, 10, 6, highlightImminent: true);

class ResoundingSilence(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.Spread, AID.ResoundingSilenceSpread, 8, 5)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (IsSpreadTarget(actor))
            hints.AddForbiddenZone(_ => true, DateTime.MaxValue);
    }
}
class ResoundingSilencePuddle(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 8, AID.ResoundingSilenceSpread, m => m.Enemies(0x1EBF73).Where(e => e.EventState != 7), 2);
class MawOfTheWolf(BossModule module) : Components.StandardAOEs(module, AID.MawOfTheWolf, new AOEShapeRect(80, 40));

class FangsOfTheUnderworld(BossModule module) : Components.IconLineStack(module, 5, 60, (uint)IconID.Fangs, AID.FangsOfTheUnderworldStack, 5.2f)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            if (NumCasts >= 3)
            {
                Source = null;
                Array.Fill(PlayerRoles, PlayerRole.Ignore);
            }
        }
    }
}

// initial cast of Malefic 2. 8 seconds means standard hint should be sufficient
class WillOfTheUnderworldSmallSlow(BossModule module) : Components.GenericAOEs(module, AID.WillOfTheUnderworldSmallSlow)
{
    readonly Malefic _malefic = module.FindComponent<Malefic>()!;
    readonly List<Actor> Casters = [];
    public bool Risky = true;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var playerSides = _malefic[slot];

        foreach (var c in Casters)
        {
            var hitSide = SideHelpers.FromAngle(c.CastInfo!.Rotation + 180.Degrees());
            if (playerSides.Matches(hitSide))
                yield return new(new AOEShapeRect(40, 5), c.CastInfo!.LocXZ, c.CastInfo!.Rotation, Module.CastFinishAt(c.CastInfo), Risky: Risky);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            Casters.Add(caster);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            Casters.Remove(caster);
        }
    }
}

// initial spawns are staggered (2 -> 2), cast starts 2.8s after second set spawns, 4s cast (6.8s)
// subsequent spawns are in groups of 4, cast starts 3s after spawns
class WillOfTheUnderworldSmallFast(BossModule module) : Components.GenericAOEs(module, AID.WillOfTheUnderworldSmallFast)
{
    readonly Malefic _malefic = module.FindComponent<Malefic>()!;
    public bool Risky = true;
    int _numSpawned;

    readonly Side[] _predictedSide = new Side[4];
    DateTime _tethersAt = DateTime.MaxValue;

    readonly List<(Actor Caster, DateTime Predicted)> _casters = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var sides = _malefic[slot];

        foreach (var c in _casters.Take(4))
        {
            if (_tethersAt < c.Predicted)
                sides |= _predictedSide[slot];

            var hitSide = SideHelpers.FromAngle(c.Caster.Rotation + 180.Degrees());

            if (sides.Matches(hitSide))
                yield return new AOEInstance(new AOEShapeRect(40, 5), c.Caster.Position, c.Caster.Rotation, c.Predicted, Risky: Risky);
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.ForceOfWillSmall)
        {
            var startAt = _numSpawned switch
            {
                < 2 => WorldState.FutureTime(8.7f),
                < 4 => WorldState.FutureTime(6.8f),
                _ => WorldState.FutureTime(7)
            };
            _casters.Add((actor, startAt));
            _numSpawned++;
        }
    }

    // 189.14 -> 196.18
    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        var s = (TetherID)tether.ID switch
        {
            TetherID.TetherE => Side.E,
            TetherID.TetherW => Side.W,
            TetherID.TetherS => Side.S,
            TetherID.TetherN => Side.N,
            _ => default
        };
        if (s != default && Raid.TryFindSlot(source, out var slot))
        {
            _tethersAt = WorldState.FutureTime(7);
            _predictedSide[slot] = s;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            var ix = _casters.FindIndex(c => c.Caster == caster);
            if (ix >= 0)
                _casters.Ref(ix).Predicted = Module.CastFinishAt(spell);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            _casters.RemoveAll(c => c.Caster == caster);
        }
    }
}

class MaleficAlignment(BossModule module) : Components.StandardAOEs(module, AID.MaleficAlignment, new AOEShapeCone(40, 45.Degrees()))
{
    readonly Malefic _malefic = module.FindComponent<Malefic>()!;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var sides = _malefic[slot];
        foreach (var c in Casters)
        {
            var hitSide = SideHelpers.FromAngle(c.CastInfo!.Rotation + 180.Degrees());
            if (sides.Matches(hitSide))
                yield return new(Shape, c.CastInfo!.LocXZ, c.CastInfo!.Rotation, Module.CastFinishAt(c.CastInfo));
        }
    }
}

class Plummet(BossModule module) : Components.GenericAOEs(module)
{
    readonly List<Actor> Casters = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var c in Casters)
        {
            var radius = (AID)c.CastInfo!.Action.ID switch
            {
                AID.PlummetTiny => 3,
                AID.PlummetSmall => 5,
                AID.PlummetLarge => 10,
                _ => 0
            };
            if (radius > 0)
                yield return new(new AOEShapeCircle(radius), c.CastInfo!.LocXZ, default, Module.CastFinishAt(c.CastInfo));
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.PlummetSmall:
            case AID.PlummetLarge:
            case AID.PlummetTiny:
                Casters.Add(caster);
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.PlummetSmall:
            case AID.PlummetLarge:
            case AID.PlummetTiny:
                NumCasts++;
                Casters.Remove(caster);
                break;
        }
    }
}

// no idea how big this is
class PlummetProximity(BossModule module) : Components.ProximityAOEs(module, AID.PlummetMeteor, 30);

// rock puddle is 2.5 radius
class SilentEightRock(BossModule module) : BossComponent(module)
{
    DateTime _activation;

    public const float RockRadius = 1.8f;
    public static readonly AOEShape Cross = new AOEShapeCross(40, 4);
    public static readonly AOEShape CrossExpanded = new AOEShapeCross(40, 4 + RockRadius);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.EchoingEightCast)
            _activation = Module.CastFinishAt(spell);
        if ((AID)spell.Action.ID == AID.EchoingHush)
            _activation = default;
    }

    IEnumerable<Actor> Rocks => Raid.WithoutSlot().Where(r => r.OID == 0x4B1D);
    public int NumRocks => Rocks.Count();

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_activation != default && pc.OID == 0)
        {
            Cross.Outline(Arena, pc.Position, default, ArenaColor.Danger);
            Cross.Outline(Arena, pc.Position, 45.Degrees(), ArenaColor.Danger);

            foreach (var actor in Rocks)
                Arena.AddCircle(actor.Position, RockRadius, ArenaColor.Object);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_activation != default && actor.OID == 0)
        {
            foreach (var rock in Rocks)
            {
                hints.AddForbiddenZone(CrossExpanded, rock.Position, default, _activation);
                hints.AddForbiddenZone(CrossExpanded, rock.Position, 45.Degrees(), _activation);
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_activation != default && actor.OID == 0)
        {
            if (Rocks.Any(r => CrossExpanded.Check(actor.Position, r.Position, default) || CrossExpanded.Check(actor.Position, r.Position, 45.Degrees())))
                hints.Add("Bait away from rocks!");
        }
    }
}
class RockPuddle(BossModule module) : Components.PersistentVoidzone(module, 2.5f, m => m.Enemies(0x1EBFC8).Where(e => e.EventState != 7));

class SteelsbreathBonds(BossModule module) : Components.Chains(module, (uint)TetherID.Chains, chainLength: 30);

class WillOfTheUnderworldLarge(BossModule module) : Components.GenericAOEs(module, AID.WillOfTheUnderworldLarge)
{
    readonly Malefic _malefic = module.FindComponent<Malefic>()!;
    readonly List<(Actor Caster, DateTime Predicted)> _casters = [];
    readonly List<Actor> _rocks = [];
    int _numSpawned;
    public bool Risky;

    const float rockR = SilentEightRock.RockRadius;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var sides = _malefic[slot];

        foreach (var (c, p) in _casters.Take(4))
        {
            var hitSide = SideHelpers.FromAngle(c.Rotation + 180.Degrees());
            if (!sides.Matches(hitSide))
                continue;

            float lenFront = 40;
            var cDir = c.Rotation.ToDirection();
            if (_rocks.FirstOrDefault(r => !r.IsDead && r.Position.InRect(c.Position, c.Rotation, 40, 0, 10)) is { } hitRock)
            {
                var rockDir = hitRock.Position - c.Position;
                lenFront = cDir.Dot(rockDir);
                var xy = cDir.Cross(rockDir);
                var rectRWidth = 10 - xy - rockR;
                //yield return new(new AOEShapeRect(40 - lenFront, 1.8f), hitRock.Position, c.Rotation, p);
                yield return new(new AOEShapeRect(40 - lenFront, rectRWidth * 0.5f), hitRock.Position + cDir.OrthoR() * (rectRWidth * 0.5f + rockR), c.Rotation, p, Risky: Risky);

                var rectLWidth = 10 + xy - rockR;
                yield return new(new AOEShapeRect(40 - lenFront, rectLWidth * 0.5f), hitRock.Position + cDir.OrthoL() * (rectLWidth * 0.5f + rockR), c.Rotation, p, Risky: Risky);
            }

            yield return new(new AOEShapeRect(lenFront, 10), c.Position, c.Rotation, p, Risky: Risky);
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.ForceOfWillLarge)
        {
            if (_rocks.Count == 0)
                _rocks.AddRange(Module.Enemies(0x4B1D));

            var startAt = _numSpawned switch
            {
                < 2 => WorldState.FutureTime(8.7f),
                < 4 => WorldState.FutureTime(6.8f),
                _ => WorldState.FutureTime(7)
            };
            _casters.Add((actor, startAt));
            _numSpawned++;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            var ix = _casters.FindIndex(c => c.Caster == caster);
            if (ix >= 0)
                _casters.Ref(ix).Predicted = Module.CastFinishAt(spell);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            _casters.RemoveAll(c => c.Caster == caster);
        }
    }
}

class C012LoneSwordmasterStates : StateMachineBuilder
{
    public C012LoneSwordmasterStates(BossModule module) : base(module)
    {
        DeathPhase(0, SinglePhase);
    }

    void SinglePhase(uint id)
    {
        Malefic1(id, 9.2f);
        Echoes(id + 0x10000, 5.5f);
        Malefic2(id + 0x20000, 5.2f);
        Plummet(id + 0x30000, 11);
        Malefic3(id + 0x40000, 10);

        Cast(id + 0x50000, AID.MaleficQuartering, 8.8f, 3);
        Cast(id + 0x50010, AID.MortalFateCast, 5.2f, 12);
        Timeout(id + 0x50020, 1, "Enrage");
    }

    void Malefic1(uint id, float delay)
    {
        SteelsbreathRelease(id, delay);
        FromHeaven(id + 0x100, 7.2f);

        Cast(id + 0x200, AID.MaleficQuartering, 2, 3)
            .ActivateOnEnter<Malefic>();

        Cast(id + 0x210, AID.ShiftingHorizon, 3.5f, 4, "Diagonals")
            .ActivateOnEnter<LashOfLight>()
            .ActivateOnEnter<ForceOfWill>()
            .ActivateOnEnter<MaleficPortent>();

        ComponentCondition<MaleficPortent>(id + 0x212, 3, p => p.Active, "Tethers appear")
            .DeactivateOnExit<LashOfLight>();
        Cast(id + 0x220, AID.MaleficPortentCast, 0.1f, 6);
        ComponentCondition<MaleficPortent>(id + 0x222, 1, m => m.NumCasts > 0, "Tethers resolve")
            .DeactivateOnExit<MaleficPortent>();
        ComponentCondition<ForceOfWill>(id + 0x230, 1.3f, w => w.NumCasts > 0, "Charges 1");
        ComponentCondition<ForceOfWill>(id + 0x231, 0.5f, w => w.NumCasts > 2, "Charges 2")
            .DeactivateOnExit<ForceOfWill>()
            .DeactivateOnExit<Malefic>();
    }

    void Echoes(uint id, float delay)
    {
        CastMulti(id, [AID.EchoingEightCast, AID.EchoingOrbitCast], delay, 4)
            .ActivateOnEnter<EchoesBait>()
            .ActivateOnEnter<EchoingHush>()
            .ActivateOnEnter<EchoPredict>()
            .ActivateOnEnter<EchoingEight>()
            .ActivateOnEnter<EchoingOrbit>();

        ComponentCondition<EchoingHush>(id + 0x10, 0.1f, h => h.Casters.Count > 0, "Puddle bait")
            .DeactivateOnExit<EchoesBait>();
        ComponentCondition<EchoingHush>(id + 0x11, 4, h => h.NumCasts > 0, "Puddles")
            .DeactivateOnExit<EchoingHush>();
        ComponentCondition<EchoPredict>(id + 0x12, 3, p => p.NumCasts > 0, "Stored AOEs")
            .DeactivateOnExit<EchoPredict>()
            .DeactivateOnExit<EchoingEight>()
            .DeactivateOnExit<EchoingOrbit>();

        ComponentCondition<WaitingWounds>(id + 0x100, 4.4f, w => w.NumCasts > 0, "Puddles 1")
            .ActivateOnEnter<WaitingWounds>();
        ComponentCondition<WaitingWounds>(id + 0x101, 1, w => w.NumCasts > 3, "Puddles 2");

        FromHeaven(id + 0x200, 1, id => ComponentCondition<WaitingWounds>(id, 0, w => w.NumCasts > 6, "Puddles 3").DeactivateOnExit<WaitingWounds>());

        Cast(id + 0x300, AID.SilentEightCast, 1.1f, 4)
            .ActivateOnEnter<ResoundingSilence>()
            .ActivateOnEnter<EchoingEight>()
            .ActivateOnEnter<MawOfTheWolf>();
        ComponentCondition<ResoundingSilence>(id + 0x310, 5, s => s.NumFinishedSpreads > 0, "Spreads")
            .DeactivateOnExit<ResoundingSilence>();
        ComponentCondition<EchoingEight>(id + 0x311, 3.2f, e => e.NumCasts > 0, "Crosses")
            .DeactivateOnExit<EchoingEight>();
        ComponentCondition<MawOfTheWolf>(id + 0x312, 3.2f, m => m.NumCasts > 0, "Half-room")
            .DeactivateOnExit<MawOfTheWolf>();

        Fangs(id + 0x400, 3.2f);
    }

    void Malefic2(uint id, float delay)
    {
        SteelsbreathRelease(id, delay, "Raidwide + arena change")
            .ActivateOnEnter<LavaRect>();

        Cast(id + 0x10, AID.MaleficQuartering, 3.3f, 3)
            .ActivateOnEnter<Malefic>()
            .ActivateOnEnter<LashOfLight>()
            .ActivateOnEnter<ForceOfWill>()
            .ActivateOnEnter<WillOfTheUnderworldSmallSlow>()
            .ActivateOnEnter<ResoundingSilence>()
            .ActivateOnEnter<ResoundingSilencePuddle>();

        ComponentCondition<LashOfLight>(id + 0x20, 8.4f, l => l.NumCasts > 0, "Diagonals 1");
        ComponentCondition<LashOfLight>(id + 0x21, 2.1f, l => l.NumCasts > 2, "Diagonals 2")
            .DeactivateOnExit<LashOfLight>();

        ComponentCondition<WillOfTheUnderworldSmallSlow>(id + 0x30, 11.4f, w => w.NumCasts > 0, "Direction AOEs")
            .DeactivateOnExit<WillOfTheUnderworldSmallSlow>();
        ComponentCondition<ResoundingSilence>(id + 0x31, 0.7f, s => s.NumFinishedSpreads > 0, "Spreads")
            .DeactivateOnExit<ResoundingSilence>()
            .DeactivateOnExit<ForceOfWill>();

        FromHeaven(id + 0x100, 2.1f);

        Cast(id + 0x200, AID.MaleficQuartering, 3, 3);
        Cast(id + 0x210, AID.CardinalHorizons, 3.6f, 4)
            .ActivateOnEnter<LashOfLight>();
        ComponentCondition<LashOfLight>(id + 0x220, 0, l => l.NumCasts > 0, "Diagonals 1");
        ComponentCondition<LashOfLight>(id + 0x221, 2.1f, l => l.NumCasts > 2, "Diagonals 2")
            .DeactivateOnExit<LashOfLight>();

        Cast(id + 0x230, AID.MawOfTheWolfCast, 0.2f, 3.4f)
            .ActivateOnEnter<MawOfTheWolf>()
            .ActivateOnEnter<WillOfTheUnderworldSmallFast>()
            .ActivateOnEnter<MaleficPortent>()
            .ActivateOnEnter<MaleficPortentCounter>()
            .ExecOnEnter<WillOfTheUnderworldSmallFast>(n => n.Risky = false);
        ComponentCondition<MawOfTheWolf>(id + 0x232, 1.5f, m => m.NumCasts > 0, "Half-room")
            .DeactivateOnExit<MawOfTheWolf>()
            .ExecOnExit<WillOfTheUnderworldSmallFast>(n => n.Risky = true);

        ComponentCondition<WillOfTheUnderworldSmallFast>(id + 0x240, 2.3f, w => w.NumCasts > 0, "Directions 1");
        ComponentCondition<WillOfTheUnderworldSmallFast>(id + 0x241, 5.6f, w => w.NumCasts > 4, "Directions 2");
        ComponentCondition<WillOfTheUnderworldSmallFast>(id + 0x242, 5.6f, w => w.NumCasts > 8, "Directions 3");
        ComponentCondition<MaleficPortentCounter>(id + 0x243, 3.2f, m => m.NumCasts > 0, "Tethers")
            .DeactivateOnExit<MaleficPortent>()
            .DeactivateOnExit<MaleficPortentCounter>();
        ComponentCondition<WillOfTheUnderworldSmallFast>(id + 0x244, 2.4f, w => w.NumCasts > 12, "Directions 4")
            .DeactivateOnExit<ResoundingSilencePuddle>()
            .DeactivateOnExit<WillOfTheUnderworldSmallFast>();

        Cast(id + 0x300, AID.MaleficAlignmentCast, 2.3f, 3)
            .ActivateOnEnter<MaleficAlignment>();
        ComponentCondition<MaleficAlignment>(id + 0x302, 1, m => m.NumCasts > 0, "Directions 5")
            .DeactivateOnExit<MaleficAlignment>();

        Fangs(id + 0x400, 2.2f);

        SteelsbreathRelease(id + 0x500, 5.1f, "Raidwide + restore arena")
            .DeactivateOnExit<Malefic>();
    }

    void Plummet(uint id, float delay)
    {
        ComponentCondition<PlummetProximity>(id, delay, p => p.NumCasts > 0, "Proximity")
            .ActivateOnEnter<Plummet>()
            .ActivateOnEnter<PlummetProximity>()
            .DeactivateOnExit<PlummetProximity>()
            .DeactivateOnExit<LavaRect>();

        ComponentCondition<SilentEightRock>(id + 0x10, 2.2f, r => r.NumRocks > 0, "Rocks appear")
            .ActivateOnEnter<SilentEightRock>()
            .ActivateOnEnter<RockPuddle>()
            .ActivateOnEnter<EchoingHush>()
            .ActivateOnEnter<EchoingEight>()
            .ActivateOnEnter<SteelsbreathBonds>();

        ComponentCondition<SteelsbreathBonds>(id + 0x20, 3.5f, s => s.TethersAssigned, "Chains appear");
        ComponentCondition<EchoingHush>(id + 0x21, 5.7f, e => e.Casters.Count > 0, "Puddles baited")
            .DeactivateOnExit<SilentEightRock>();
        ComponentCondition<EchoingHush>(id + 0x22, 4, e => e.NumCasts > 0, "Puddles")
            .DeactivateOnExit<EchoingHush>()
            .DeactivateOnExit<SteelsbreathBonds>()
            .DeactivateOnExit<Plummet>();

        ComponentCondition<EchoingEight>(id + 0x30, 3, e => e.NumCasts > 0, "Crosses")
            .DeactivateOnExit<EchoingEight>();
    }

    void Malefic3(uint id, float delay)
    {
        Cast(id, AID.MaleficQuartering, delay, 3)
            .ActivateOnEnter<Malefic>()
            .ActivateOnEnter<WillOfTheUnderworldLarge>();

        Cast(id + 0x10, AID.CardinalHorizons, 5.6f, 4)
            .ActivateOnEnter<LashOfLight>();
        ComponentCondition<LashOfLight>(id + 0x20, 0, l => l.NumCasts > 0, "Diagonals 1");
        ComponentCondition<LashOfLight>(id + 0x21, 2.1f, l => l.NumCasts > 2, "Diagonals 2")
            .DeactivateOnExit<LashOfLight>()
            .ExecOnExit<WillOfTheUnderworldLarge>(w => w.Risky = true);

        ComponentCondition<WillOfTheUnderworldLarge>(id + 0x100, 8, w => w.NumCasts > 0, "Rocks 1");
        ComponentCondition<WillOfTheUnderworldLarge>(id + 0x101, 5.6f, w => w.NumCasts > 4, "Rocks 2");
        ComponentCondition<WillOfTheUnderworldLarge>(id + 0x102, 5.6f, w => w.NumCasts > 8, "Rocks 3");
        ComponentCondition<WillOfTheUnderworldLarge>(id + 0x103, 5.6f, w => w.NumCasts > 12, "Rocks 4")
            .DeactivateOnExit<WillOfTheUnderworldLarge>();

        SteelsbreathRelease(id + 0x200, 0.6f)
            .DeactivateOnExit<RockPuddle>()
            .DeactivateOnExit<Malefic>();

        Fangs(id + 0x300, 3.2f);
    }

    State SteelsbreathRelease(uint id, float delay, string hint = "Raidwide")
    {
        return CastMulti(id, [AID.SteelsbreathReleaseCast, AID.SteelsbreathReleaseLavaCast], delay, 5, hint)
           .ActivateOnEnter<SteelsbreathRelease>()
           .ActivateOnEnter<SteelsbreathRelease2>()
           .DeactivateOnExit<SteelsbreathRelease>()
           .DeactivateOnExit<SteelsbreathRelease2>();
    }

    void FromHeaven(uint id, float delay, Action<uint>? extra = null)
    {
        CastStartMulti(id, [AID.NearToHeavenOneCast, AID.FarFromHeavenOneCast, AID.NearToHeavenTwoCast, AID.FarFromHeavenTwoCast], delay)
            .ActivateOnEnter<HeavensConfluence>()
            .ActivateOnEnter<FromHeaven>();
        extra?.Invoke(id + 1);
        CastEnd(id + 2, 5);

        ComponentCondition<HeavensConfluence>(id + 0x10, 0.3f, c => c.NumCasts > 0, "In/out 1");
        ComponentCondition<HeavensConfluence>(id + 0x11, 2.1f, c => c.NumCasts > 1, "In/out 2")
            .DeactivateOnExit<HeavensConfluence>()
            .DeactivateOnExit<FromHeaven>();
    }

    void Fangs(uint id, float delay)
    {
        CastStart(id, AID.FangsOfTheUnderworldCast, delay)
            .ActivateOnEnter<FangsOfTheUnderworld>();
        ComponentCondition<FangsOfTheUnderworld>(id + 1, 5.2f, f => f.NumCasts > 0, "Stack 1");
        ComponentCondition<FangsOfTheUnderworld>(id + 2, 2, f => f.NumCasts > 2, "Stack 3")
            .DeactivateOnExit<FangsOfTheUnderworld>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1079, NameID = 14323, PlanLevel = 100)]
public class C012LoneSwordmaster(WorldState ws, Actor primary) : BossModule(ws, primary, new(170, -815), new ArenaBoundsSquare(20));

