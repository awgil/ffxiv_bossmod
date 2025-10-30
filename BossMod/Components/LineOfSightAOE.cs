namespace BossMod.Components;

// generic component that shows line-of-sight cones for arbitrary origin and blocking shapes
public abstract class GenericLineOfSightAOE(BossModule module, Enum? aid, float maxRange, bool blockersImpassable) : CastCounter(module, aid)
{
    public DateTime NextExplosion;
    public bool BlockersImpassable = blockersImpassable;
    public float MaxRange { get; private set; } = maxRange;
    public BitMask IgnoredPlayers;
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
                Visibility.Add((dist, Angle.FromDirection(toBlock), b.Radius < dist ? Angle.Asin(b.Radius / dist) : 90.Degrees()));
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Origin != null
            && !IgnoredPlayers[slot]
            && actor.Position.InCircle(Origin.Value, MaxRange)
            && !Visibility.Any(v => !actor.Position.InCircle(Origin.Value, v.Distance) && actor.Position.InCone(Origin.Value, v.Dir, v.HalfWidth)))
        {
            hints.Add("Hide behind obstacle!");
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Origin != null && !IgnoredPlayers[slot])
        {
            // inverse of a union of inverted max-range circle and a bunch of infinite cones minus inner cirles
            var normals = Visibility.Select(v => (v.Distance, (v.Dir + v.HalfWidth).ToDirection().OrthoL(), (v.Dir - v.HalfWidth).ToDirection().OrthoR())).ToArray();
            bool invertedDistanceToSafe(WPos p)
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
                return distanceToSafe > 0;
            }
            hints.AddForbiddenZone(invertedDistanceToSafe, NextExplosion);
        }
        if (BlockersImpassable)
        {
            var blockers = Blockers.Select(b => ShapeContains.Circle(b.Center, b.Radius)).ToArray();
            if (blockers.Length > 0)
                hints.TemporaryObstacles.Add(p => blockers.Any(b => b(p)));
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        // TODO: reconsider, this looks like shit...
        if (Origin != null && !IgnoredPlayers[pcSlot])
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
    public readonly float RadiusOverride;
    public Actor? ActiveCaster => _casters.MinBy(c => c.CastInfo!.RemainingTime);

    protected CastLineOfSightAOE(BossModule module, Enum aid, float maxRange, bool blockersImpassable, float radiusOverride = 0) : base(module, aid, maxRange, blockersImpassable)
    {
        RadiusOverride = radiusOverride;
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

    protected void Refresh()
    {
        var caster = ActiveCaster;
        WPos? position = caster != null ? (WorldState.Actors.Find(caster.CastInfo!.TargetID)?.Position ?? caster.CastInfo!.LocXZ) : null;
        Modify(position, BlockerActors().Select(b => (b.Position, RadiusOverride > 0 ? RadiusOverride : b.HitboxRadius)), Module.CastFinishAt(caster?.CastInfo));
    }
}
