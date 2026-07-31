namespace BossMod.Dawntrail.Variant.V02LoneSwordmaster;

public enum OID : uint
{
    Helper = 0x233C,
    Boss = 0x4B17,
    ForceOfWillGroundTether = 0x4B1E,
    ForceOfWillWallTether = 0x4B1C,
}

public enum AID : uint
{
    AutoAttack = 45128,
    FarFromHeavenSpread = 47563,
    FarFromHeavenStack = 47565,
    FarFromHeavenOne = 46661,
    FarFromHeavenTwo = 46659,
    NearToHeavenSpread = 47562,
    NearToHeavenStack = 47564,
    NearToHeavenOne = 46660,
    NearToHeavenTwo = 46658,
    HeavensConfluenceCircleFirst = 46662,
    HeavensConfluenceDonutFirst = 46663,
    HeavensConfluenceCircleLast = 46664,
    HeavensConfluenceDonutLast = 46665,
    WolfsCrossingCast = 46668,
    WolfsCrossing = 46669,
    EchoingHush = 46747,
    EchoingEightPuddle = 46670,
    EchoingEightCross = 46671,
    UnyieldingWill = 48652,
    UnyieldingWill1 = 46656,
    UnyieldingWill2 = 46657,
    WillOfTheUnderworldLarge = 47762,
    WillOfTheUnderworldSmall = 46683,
    SteelsbreathRelease = 48136,
    SteelsbreathReleaseLava = 46681,
    MaleficAlignment = 46673,
    MaleficAlignmentAssign = 46672,
    MaleficPortent = 46652,
    MaleficPortent1 = 46653,
    LashOfLight = 46655,
    StingOfTheScorpion = 46646,
    MawOfTheWolf = 46679,
    MawOfTheWolf1 = 46680,
    EchoingHush1 = 46667,
}

public enum SID : uint
{
    PhysicalVulnerabilityUp = 2940,
    Bind = 2518,
    MaleficE = 4773,
    MaleficW = 4774,
    MaleficEW = 4775,
    MaleficS = 4776,
    MaleficSE = 4777,
    MaleficSW = 4778,
    MaleficSEW = 4779,
    MaleficN = 4780,
    MaleficNE = 4781,
    MaleficNW = 4782,
    MaleficNEW = 4783,
    MaleficNS = 4784,
    MaleficNSE = 4785,
    MaleficNSW = 4786,
    MaleficNSEW = 4787,
}

public enum IconID : uint
{
    Ok = 136,
    Prey = 648,
    NotOk = 137,
}

public enum TetherID : uint
{
    TetherN = 357,
    TetherE = 358,
    TetherW = 359,
    TetherS = 360,
    UnyieldingWill = 371,
    Chains = 163,
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

class SteelsbreathRelease(BossModule module) : Components.RaidwideCast(module, AID.SteelsbreathRelease);
class StingOfTheScorpion(BossModule module) : Components.SingleTargetCast(module, AID.StingOfTheScorpion);
class LashOfLight(BossModule module) : Components.StandardAOEs(module, AID.LashOfLight, new AOEShapeCone(40, 45.Degrees()), maxCasts: 2);
class WolfsCrossing(BossModule module) : Components.StandardAOEs(module, AID.WolfsCrossing, new AOEShapeCross(40, 4));
class MawOfTheWolf(BossModule module) : Components.StandardAOEs(module, AID.MawOfTheWolf, new AOEShapeRect(80, 40));
class EchoingHush(BossModule module) : Components.StandardAOEs(module, AID.EchoingHush, 8);
class EchoingEightPuddle(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 8, AID.EchoingEightPuddle, m => m.Enemies(0x1EBF73).Where(e => e.EventState != 7), 2);
class EchoingEightCross(BossModule module) : Components.StandardAOEs(module, AID.EchoingEightCross, new AOEShapeCross(40, 4));
class MaleficPortentCounter(BossModule module) : Components.CastCounter(module, AID.MaleficPortent);
class SteelsbreathBonds(BossModule module) : Components.Chains(module, (uint)TetherID.Chains, chainLength: 30);
class WillOfTheUnderworldSmall(BossModule module) : WillOfTheUnderworld(module, AID.WillOfTheUnderworldSmall, 5);
class WillOfTheUnderworldLarge(BossModule module) : WillOfTheUnderworld(module, AID.WillOfTheUnderworldLarge, 10);

class MaleficAlignment(BossModule module) : Components.StandardAOEs(module, AID.MaleficAlignment, new AOEShapeCone(40f, 45.Degrees()))
{
    private readonly Malefic _malefic = module.FindComponent<Malefic>()!;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var my = _malefic[slot];

        foreach (var c in Casters)
        {
            var cast = c.CastInfo;
            if (cast == null)
                continue;

            var coneSide = SideHelpers.FromAngle(cast.Rotation + 180.Degrees());
            if (my != Side.None && my.Matches(coneSide))
                yield return new AOEInstance(
                    Shape,
                    cast.LocXZ,
                    cast.Rotation,
                    Module.CastFinishAt(cast)
                );
        }
    }
}

class FromHeaven(BossModule module) : Components.UniformStackSpread(module, 5, 5, maxStackSize: 2)
{
    Actor? _target;
    int _type; // 1 = stack, 2 = spread
    void Try()
    {
        if (_target == null || _type == 0 || Active)
            return;

        if (_type == 1)
            AddStack(_target);
        else
            AddSpread(_target);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.NearToHeavenStack:
            case AID.FarFromHeavenStack:
                _type = 1;
                Try();
                break;
            case AID.NearToHeavenSpread:
            case AID.FarFromHeavenSpread:
                _type = 2;
                Try();
                break;
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((IconID)iconID is IconID.Prey)
        {
            _target = actor;
            Try();
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.NearToHeavenOne or AID.NearToHeavenTwo or AID.FarFromHeavenOne or AID.FarFromHeavenTwo)
        {
            Stacks.Clear();
            Spreads.Clear();
            _target = null;
            _type = 0;
        }
    }
}

class HeavensConfluence(BossModule module) : Components.GenericAOEs(module)
{
    readonly List<AOEInstance> _first = [];
    readonly List<AOEInstance> _last = [];
    bool _showLast;

    private static readonly AOEShape Circle = new AOEShapeCircle(8);
    private static readonly AOEShape Donut = new AOEShapeDonut(8, 60);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var aoe in _first)
            yield return aoe;

        if (_showLast)
            foreach (var aoe in _last)
                yield return aoe;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var action = (AID)spell.Action.ID;
        var shape = action switch
        {
            AID.HeavensConfluenceCircleFirst or AID.HeavensConfluenceCircleLast => Circle,
            AID.HeavensConfluenceDonutFirst or AID.HeavensConfluenceDonutLast => Donut,
            _ => null
        };

        if (shape != null)
        {
            (action is AID.HeavensConfluenceCircleLast or AID.HeavensConfluenceDonutLast ? _last : _first).Add(new(shape, caster.Position, default, Module.CastFinishAt(spell)));
        }
    }
    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.HeavensConfluenceCircleFirst:
            case AID.HeavensConfluenceDonutFirst:
                _first.Clear();
                _showLast = true;
                break;

            case AID.HeavensConfluenceCircleLast:
            case AID.HeavensConfluenceDonutLast:
                _last.Clear();
                _showLast = false;
                break;
        }
    }
}

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
        {
            var targetSlot = Raid.FindSlot(tether.Target);
            if (targetSlot != default)
                _forceOfWillTargets.Set(targetSlot);
        }

        var side = (TetherID)tether.ID switch
        {
            TetherID.TetherE => Side.E,
            TetherID.TetherN => Side.N,
            TetherID.TetherS => Side.S,
            TetherID.TetherW => Side.W,
            _ => Side.None
        };

        if (side != Side.None && Raid.TryFindSlot(source, out var slot) && slot >= 0)
        {
            _sides[slot] = side;
            _targets.Set(slot);
        }
    }

    public override void OnUntethered(Actor source, ActorTetherInfo tether)
    {
        if (Raid.TryFindSlot(source, out var slot) && slot >= 0)
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

                Arena.AddLine(Module.PrimaryActor.Position, p.Position,
                    shouldTake ? ArenaColor.Safe : ArenaColor.Danger,
                    shouldTake && !_targets[pcSlot] ? 2 : 1);

                Malefic.DrawSides(Arena, p, _sides[i], ArenaColor.Danger);
            }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_targets.Any() && _forceOfWillTargets.Any() && !HasCorrectTether(slot))
            hints.Add(_targets[slot] ? "Pass tether!" : "Take tether!");
    }

    bool HasCorrectTether(int slot) => _targets[slot] != _forceOfWillTargets[slot] && !_malefic[slot].Matches(_sides[slot]);
    bool ShouldTake(int slot, int targetSlot) => !_targets[slot] && !_forceOfWillTargets[slot] && !HasCorrectTether(targetSlot) && !_malefic[slot].Matches(_sides[targetSlot]);
}

class UnyieldingWill(BossModule module) : Components.GenericAOEs(module)
{
    readonly List<(Actor From, Actor To, int Order)> AllTethers = [];
    DateTime _appearedAt;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        ulong parentID = 0;

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
            case AID.UnyieldingWill:
                NumCasts++;
                AllTethers.RemoveAll(t => t.To.OID == 0);
                break;
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        switch ((IconID)iconID)
        {
            case IconID.Ok:
            case IconID.NotOk:
                NumCasts++;
                AllTethers.Clear();
                break;
        }
    }
}

class WillOfTheUnderworld(BossModule module, AID aid, float halfWidth) : Components.GenericAOEs(module, aid)
{
    private readonly Malefic _malefic = module.FindComponent<Malefic>()!;
    private readonly List<Actor> Casters = [];
    public bool Risky = true;

    private AOEShapeRect Shape => new(40, halfWidth);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var playerSides = _malefic[slot];

        foreach (var c in Casters)
        {
            var cast = c.CastInfo;
            if (cast == null)
                continue;

            var hitSide = SideHelpers.FromAngle(cast.Rotation + 180.Degrees());
            if (playerSides.Matches(hitSide))
                yield return new AOEInstance(
                    Shape,
                    cast.LocXZ,
                    cast.Rotation,
                    Module.CastFinishAt(cast),
                    Risky: Risky
                );
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
        if ((AID)spell.Action.ID == AID.SteelsbreathReleaseLava)
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

class V02LoneSwordmasterStates : StateMachineBuilder
{
    public V02LoneSwordmasterStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SteelsbreathRelease>()
            .ActivateOnEnter<StingOfTheScorpion>()
            .ActivateOnEnter<FromHeaven>()
            .ActivateOnEnter<HeavensConfluence>()
            .ActivateOnEnter<LashOfLight>()
            .ActivateOnEnter<MawOfTheWolf>()
            .ActivateOnEnter<EchoingHush>()
            .ActivateOnEnter<WolfsCrossing>()
            .ActivateOnEnter<EchoingEightPuddle>()
            .ActivateOnEnter<EchoingEightCross>()
            .ActivateOnEnter<Malefic>()
            .ActivateOnEnter<MaleficPortentCounter>()
            .ActivateOnEnter<MaleficPortent>()
            .ActivateOnEnter<MaleficAlignment>()
            .ActivateOnEnter<UnyieldingWill>()
            .ActivateOnEnter<WillOfTheUnderworldSmall>()
            .ActivateOnEnter<WillOfTheUnderworldLarge>()
            .ActivateOnEnter<SteelsbreathBonds>()
            .ActivateOnEnter<LavaRect>();
    }
}

[ModuleInfo(GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1084, NameID = 14323)]
public class V02LoneSwordmaster(WorldState ws, Actor primary) : BossModule(ws, primary, new(170, -815), new ArenaBoundsSquare(20));

