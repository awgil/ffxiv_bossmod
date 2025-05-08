namespace BossMod.Stormblood.Dungeon.D13TheBurn.D131Hedetet;
public enum OID : uint
{
    Boss = 0x2419, // R4.200, x1
    Helper = 0x233C, // R0.500, x4, Helper type
    DimCrystal = 0x241A, // R1.600, x4 (spawn during fight)
}
public enum AID : uint
{
    Attack = 870, // Boss->player, no cast, single-target
    CrystalNeedle = 12691, // Boss->player, 3.0s cast, single-target

    Hailfire = 12692, // Boss->self/players, 6.0s cast, range 40+R width 4 rect
    ResonantFrequency = 12696, // 241A->self, 3.0s cast, range 6 circle
    Shardfall = 12689, // Boss->self, 5.0s cast, range 40 circle
    Dissonance = 12690, // Boss->self, 5.0s cast, range -40 donut
    CrystallineFracture = 12695, // 241A->self, 3.0s cast, range 3 circle

    Shardstrike = 12693, // Boss->self, 5.0s cast, single-target
    ShardstrikeImpact = 12697, // 233C->player, no cast, range 5 circle

    A = 33212, // Boss->location, no cast, single-target
    B = 12694, // Boss->location, no cast, single-target
}
public enum IconID : uint
{
    Tankbuster = 381, // player
    Hailfire = 2, // player
    Shardstrike = 96, // player
}
class Hailfire(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    public DateTime NextExplosion;
    private readonly List<Actor> _casters = [];
    public Actor? ActiveCaster => _casters.MinBy(c => c.CastInfo!.RemainingTime);
    public float MaxRange = 40;
    public WPos? Origin;
    public List<(WPos Center, float Radius)> Blockers { get; private set; } = [];
    public List<(float Distance, Angle Dir, Angle HalfWidth)> Visibility { get; private set; } = [];
    public AOEShapeRect Shape = new(45, 2);
    public uint IID = (uint)IconID.Hailfire;
    public bool playerTarget;

    public virtual Actor? BaitSource(Actor target) => Module.PrimaryActor;
    public record struct Bait(Actor Source, Actor Target, AOEShape Shape, DateTime Activation = default)
    {
        public readonly Angle Rotation => Source != Target ? Angle.FromDirection(Target.Position - Source.Position) : Source.Rotation;
    }
    public bool AllowDeadTargets = true; // if false, baits with dead targets are ignored
    public bool EnableHints = true;
    public PlayerPriority BaiterPriority = PlayerPriority.Interesting;
    public List<Bait> CurrentBaits = [];

    public IEnumerable<Bait> ActiveBaits => AllowDeadTargets ? CurrentBaits : CurrentBaits.Where(b => !b.Target.IsDead);
    public IEnumerable<Bait> ActiveBaitsOn(Actor target) => ActiveBaits.Where(b => b.Target == target);
    public IEnumerable<Bait> ActiveBaitsNotOn(Actor target) => ActiveBaits.Where(b => b.Target != target);
    public WPos BaitOrigin(Bait bait) => bait.Source.Position;
    public bool IsClippedBy(Actor actor, Bait bait) => bait.Shape.Check(actor.Position, BaitOrigin(bait), bait.Rotation);
    public IEnumerable<Actor> PlayersClippedBy(Bait bait) => Raid.WithoutSlot().Exclude(bait.Target).InShape(bait.Shape, BaitOrigin(bait), bait.Rotation);

    public IEnumerable<Actor> BlockerActors() => Module.Enemies(OID.DimCrystal).Where(e => !e.IsDead);
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoes.Count > 0)
        {
            foreach (var i in _aoes)
                yield return i with { Color = ArenaColor.AOE };
        }
    }
    public void Modify(WPos? origin, IEnumerable<(WPos Center, float Radius)> blockers, DateTime nextExplosion = default)
    {
        NextExplosion = nextExplosion;
        Origin = origin;
        Blockers.Clear();
        Blockers.AddRange(blockers);
        Visibility.Clear();
        if (origin != null && playerTarget)
        {
            foreach (var b in Blockers)
            {
                var toBlock = b.Center - origin.Value;
                var dist = toBlock.Length();
                Visibility.Add((dist + b.Radius, Angle.FromDirection(toBlock), b.Radius < dist ? Angle.Asin(b.Radius / dist) : 90.Degrees()));
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Origin != null
            && actor.Position.InCircle(Origin.Value, MaxRange)
            && !Visibility.Any(v => !actor.Position.InCircle(Origin.Value, v.Distance) && actor.Position.InCone(Origin.Value, v.Dir, v.HalfWidth)) && playerTarget)
        {
            hints.Add("Hide behind obstacle!");
        }
        if (ActiveBaitsOn(actor).Any(b => PlayersClippedBy(b).Any()))
            hints.Add("Bait away from raid!");
        if (ActiveBaitsNotOn(actor).Any(b => IsClippedBy(actor, b)))
            hints.Add("GTFO from baited aoe!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Origin != null && playerTarget)
        {
            var normals = Visibility.Select(v => (v.Distance, (v.Dir + v.HalfWidth).ToDirection().OrthoL(), (v.Dir - v.HalfWidth).ToDirection().OrthoR())).ToArray();
            float invertedDistanceToSafe(WPos p)
            {
                var off = p - Origin.Value;
                var distOrigin = off.Length();
                var distanceToSafe = MaxRange - distOrigin;
                foreach (var (minRange, nl, nr) in normals)
                {
                    var distInnerInv = minRange - distOrigin;
                    var distLeft = off.Dot(nl);
                    var distRight = off.Dot(nr);
                    var distCone = Math.Max(distInnerInv, Math.Max(distLeft, distRight));
                    distanceToSafe = Math.Min(distanceToSafe, distCone);
                }
                return -distanceToSafe;
            }
            hints.AddForbiddenZone(p => invertedDistanceToSafe(p) <= 0);
        }
        foreach (var b in ActiveBaits)
        {
            if (b.Target != actor)
            {
                hints.AddForbiddenZone(b.Shape, BaitOrigin(b), b.Rotation, b.Activation);
            }
            else
            {
                foreach (var p in Raid.WithoutSlot().Exclude(actor))
                {
                    if (b.Source != b.Target)
                        hints.AddForbiddenZone(b.Shape, b.Source.Position, b.Source.AngleTo(p), b.Activation);
                }
            }
        }
    }
    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor) => ActiveBaitsOn(player).Any() ? BaiterPriority : PlayerPriority.Irrelevant;
    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (Origin != null && playerTarget)
        {
            foreach (var bait in ActiveBaitsOn(pc))
            {
                bait.Shape.Outline(Arena, BaitOrigin(bait), bait.Rotation);
            }
        }
    }
    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (Origin != null && playerTarget)
        {
            Arena.ZoneDonut(Origin.Value, MaxRange, 1000, ArenaColor.SafeFromAOE);
            foreach (var v in Visibility)
                Arena.ZoneCone(Origin.Value, v.Distance, 1000, v.Dir, v.HalfWidth, ArenaColor.SafeFromAOE);
        }
        foreach (var bait in ActiveBaitsNotOn(pc))
            bait.Shape.Draw(Arena, BaitOrigin(bait), bait.Rotation);
    }
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.Hailfire)
        {
            Origin = Module.PrimaryActor.Position;
            _casters.Add(caster);
            Refresh();
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.Hailfire)
        {
            Origin = null;
            _casters.Remove(caster);
            Refresh();
        }
    }
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == IID && BaitSource(actor) is var source && source != null)
        {
            CurrentBaits.Add(new(source, actor, Shape));
            if (actor == WorldState.Party.Player())
            {
                playerTarget = true;
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if ((AID)spell.Action.ID is AID.Hailfire)
        {
            CurrentBaits.Clear();
            playerTarget = false;
        }
    }
    private void Refresh()
    {
        var caster = ActiveCaster;
        Modify(Origin, BlockerActors().Select(b => (b.Position, b.HitboxRadius)), Module.CastFinishAt(caster?.CastInfo));
    }
}
class ResonantFrequency(BossModule module) : Components.StandardAOEs(module, AID.ResonantFrequency, new AOEShapeCircle(6));
class Shardfall(BossModule module) : Components.CastLineOfSightAOE(module, AID.Shardfall, 40, false)
{
    public override IEnumerable<Actor> BlockerActors() => Module.Enemies(OID.DimCrystal).Where(e => !e.IsDead);
}
class Dissonance(BossModule module) : Components.StandardAOEs(module, AID.Dissonance, new AOEShapeDonut(5, 40));
class CrystallineFracture(BossModule module) : Components.StandardAOEs(module, AID.CrystallineFracture, new AOEShapeCircle(3));

class Shardstrike(BossModule module) : Components.IconStackSpread(module, default, (uint)IconID.Shardstrike, default, AID.ShardstrikeImpact, 0, 5, 0, alwaysShowSpreads: true);
class ShardstrikeAvoid(BossModule module) : Components.StandardAOEs(module, AID.ShardstrikeImpact, new AOEShapeCircle(6));
class CrystalObject(BossModule module) : BossComponent(module)
{
    private IEnumerable<Actor> Crystals => Module.Enemies(OID.DimCrystal).Where(e => !e.IsDead);

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actors(Crystals, ArenaColor.Object, true);
    }
}
class D131HedetetStates : StateMachineBuilder
{
    public D131HedetetStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Hailfire>()
            .ActivateOnEnter<ResonantFrequency>()
            .ActivateOnEnter<Shardfall>()
            .ActivateOnEnter<Dissonance>()
            .ActivateOnEnter<CrystallineFracture>()
            .ActivateOnEnter<Shardstrike>()
            .ActivateOnEnter<ShardstrikeAvoid>()
            .ActivateOnEnter<CrystalObject>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "VeraNala", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 585, NameID = 7667)]
public class D131Hedetet(WorldState ws, Actor primary) : BossModule(ws, primary, new(174f, 178f), new ArenaBoundsCircle(20));
