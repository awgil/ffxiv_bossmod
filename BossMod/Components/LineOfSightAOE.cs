namespace BossMod.Components;

// generic component that shows line-of-sight cones for arbitrary origin and blocking shapes
public abstract class GenericLineOfSightAOE(BossModule module, ActionID aid, float maxRange, bool blockersImpassable) : CastCounter(module, aid)
{
    public DateTime NextExplosion;
    public bool BlockersImpassable = blockersImpassable;
    public float MaxRange { get; private set; } = maxRange;
    public WPos? Origin { get; private set; } // inactive if null
    public List<(WPos Center, float Radius)> Blockers { get; private set; } = [];
    public List<(float Distance, Angle Dir, Angle HalfWidth)> Visibility { get; private set; } = [];

    public void Modify(WPos? origin, IEnumerable<(WPos Center, float Radius)> blockers, DateTime nextExplosion = default)
    {
        NextExplosion = nextExplosion;
        Origin = origin;
        Blockers.Clear();
        Blockers.AddRange(blockers);
        Visibility.Clear();
        if (origin != null)
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
            && !Visibility.Any(v => !actor.Position.InCircle(Origin.Value, v.Distance) && actor.Position.InCone(Origin.Value, v.Dir, v.HalfWidth)))
        {
            hints.Add("Hide behind obstacle!");
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Origin != null)
        {
            // inverse of a union of inverted max-range circle and a bunch of infinite cones minus inner circles
            var normals = Visibility.Select(v => (v.Distance, (v.Dir + v.HalfWidth).ToDirection().OrthoL(), (v.Dir - v.HalfWidth).ToDirection().OrthoR())).ToArray();
            float invertedDistanceToSafe(WPos p)
            {
                var off = p - Origin.Value;
                var distOrigin = off.Length();
                var distanceToSafe = MaxRange - distOrigin; // this is positive if we're inside max-range
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
            hints.AddForbiddenZone(invertedDistanceToSafe, NextExplosion);
        }
        if (BlockersImpassable)
        {
            var blockers = Blockers.Select(b => ShapeDistance.Circle(b.Center, b.Radius)).ToArray();
            hints.AddForbiddenZone(p => blockers.Min(b => b(p)));
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        // TODO: reconsider, this looks like shit...
        if (Origin != null)
        {
            Arena.ZoneDonut(Origin.Value, MaxRange, 1000, ArenaColor.SafeFromAOE);
            foreach (var v in Visibility)
                Arena.ZoneCone(Origin.Value, v.Distance, 1000, v.Dir, v.HalfWidth, ArenaColor.SafeFromAOE);
        }
    }
}

// simple line-of-sight aoe that happens at the end of the cast
public abstract class CastLineOfSightAOE : GenericLineOfSightAOE
{
    private readonly List<Actor> _casters = [];
    public Actor? ActiveCaster => _casters.MinBy(c => c.CastInfo!.NPCFinishAt);

    protected CastLineOfSightAOE(BossModule module, ActionID aid, float maxRange, bool blockersImpassable) : base(module, aid, maxRange, blockersImpassable)
    {
        Refresh();
    }

    public abstract IEnumerable<Actor> BlockerActors();

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            _casters.Add(caster);
            Refresh();
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            _casters.Remove(caster);
            Refresh();
        }
    }

    private void Refresh()
    {
        var caster = ActiveCaster;
        WPos? position = caster != null ? (WorldState.Actors.Find(caster.CastInfo!.TargetID)?.Position ?? caster.CastInfo!.LocXZ) : null;
        Modify(position, BlockerActors().Select(b => (b.Position, b.HitboxRadius)), caster?.CastInfo?.NPCFinishAt ?? default);
    }
}

// generic component that shows line-of-sight safe spots for rect AOES, probably a bad solution but needed for Hermes in Ktis Hyperboreia
// TODO: rework to add support for arbitrary AOE shapes and blocker shapes (eg rectangles in shadowbringer alliance raid),
// add support for multiple AOE sources at the same time (I simplified Hermes from 4 AOEs into one)
// add support for blockers that spawn or get destroyed after cast already started (Hermes: again a cheat here by only using that meteor that exists for the whole mechanic)
public abstract class GenericLineOfSightRectAOE(BossModule module, ActionID aid) : GenericAOEs(module, aid, "Hide behind obstacle!")
{
    public List<AOEInstance> InvertedAOE = [];
    public List<Shape> UnionShapes = [];
    public List<Shape> DifferenceShapes = [];

    public abstract IEnumerable<Actor> BlockerActors();
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => InvertedAOE.Take(1);

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (ActiveAOEs(slot, actor).Any(c => c.Risky && !c.Check(actor.Position)))
            hints.Add(WarningText);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            foreach (var b in BlockerActors())
            {
                UnionShapes.Add(new RectangleSE(b.Position, b.Position + 1000 * caster.Rotation.ToDirection(), b.HitboxRadius));
                DifferenceShapes.Add(new Circle(b.Position, b.HitboxRadius));
            }
            InvertedAOE.Add(new(new AOEShapeCustom(CopyShapes(UnionShapes), CopyShapes(DifferenceShapes), true), Module.Arena.Center, default, spell.NPCFinishAt, ArenaColor.SafeFromAOE));
            UnionShapes.Clear();
            DifferenceShapes.Clear();
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            InvertedAOE.RemoveAt(0);
        }
    }

    private List<Shape> CopyShapes(List<Shape> shapes)
    {
        var copy = new List<Shape>();
        copy.AddRange(shapes);
        return copy;
    }
}
