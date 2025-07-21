namespace BossMod;

// information relevant for AI decision making process for a specific player
public sealed class AIHints
{
    public class Enemy(Actor actor, int priority, bool shouldBeTanked)
    {
        public const int PriorityPointless = -1; // attacking enemy won't improve your parse, but will give gauge and advance combo (e.g. boss locked to 1 HP, useless add in raid, etc)
        public const int PriorityInvincible = -2; // attacking enemy will have no effect at all besides breaking your combo, but hitting it with AOEs is fine
        public const int PriorityUndesirable = -3; // enemy can be attacked if targeted manually by a player, but should be considered forbidden for AOE actions (i.e. mobs that are not in combat, or are in combat with someone else's party)
        public const int PriorityForbidden = -4; // attacking this enemy will probably lead to a wipe; autoattacks and actions that target it will be forcibly prevented (if custom queueing is enabled)

        public Actor Actor = actor;
        public int Priority = priority;
        //public float TimeToKill;
        public float AttackStrength = 0.05f; // target's predicted HP percent is decreased by this amount (0.05 by default)
        public WPos DesiredPosition = actor.Position; // tank AI will try to move enemy to this position
        public Angle DesiredRotation = actor.Rotation; // tank AI will try to rotate enemy to this angle
        public float TankDistance = 2; // enemy will start moving if distance between hitboxes is bigger than this
        public bool ShouldBeTanked = shouldBeTanked; // tank AI will try to tank this enemy
        public bool PreferProvoking; // tank AI will provoke enemy if not targeted
        public bool ForbidDOTs; // if true, dots on target are forbidden
        public bool ShouldBeInterrupted; // if set and enemy is casting interruptible spell, some ranged/tank will try to interrupt
        public bool ShouldBeStunned; // if set, AI will stun if possible
        public bool ShouldBeDispelled; // if set, AI will try to cast a dispel action; only relevant for foray content
        public bool StayAtLongRange; // if set, players with ranged attacks don't bother coming closer than max range (TODO: reconsider)
        public bool Spikes; // if set, autoattacks will be prevented

        // easier to read
        public bool AllowDOTs { get => !ForbidDOTs; set => ForbidDOTs = !value; }
    }

    public enum SpecialMode
    {
        Normal,
        Pyretic, // pyretic/acceleration bomb type of effects - no movement, no actions, no casting allowed at activation time
        Freezing, // should be moving at activation time
        Misdirection, // temporary misdirection - if current time is greater than activation, use special pathfinding codepath
    }

    public enum PredictedDamageType
    {
        None,
        Tankbuster, // cast is expected to do a decent amount of damage, tank AI should use mitigation
        Raidwide, // cast is expected to hit everyone and deal minor damage; also used for spread components
        Shared, // cast is expected to hit multiple players; modules might have special behavior when intentionally taking this damage solo
    }

    public record struct DamagePrediction(BitMask Players, DateTime Activation, PredictedDamageType Type = PredictedDamageType.None)
    {
        public readonly BitMask Players = Players;
    }

    public static readonly ArenaBounds DefaultBounds = new ArenaBoundsSquare(30);

    // information needed to build base pathfinding map (onto which forbidden/goal zones are later rasterized), if needed (lazy, since it's somewhat expensive and not always needed)
    public WPos PathfindMapCenter;
    public ArenaBounds PathfindMapBounds = DefaultBounds;
    public Bitmap.Region PathfindMapObstacles;

    // list of potential targets
    public readonly Enemy?[] Enemies = new Enemy?[100];
    public Enemy? FindEnemy(Actor? actor) => Enemies.BoundSafeAt(actor?.CharacterSpawnIndex ?? -1);

    // enemies in priority order
    public List<Enemy> PotentialTargets = [];
    public int HighestPotentialTargetPriority;

    // forced target
    // this should be set only if either explicitly planned by user or by ai, otherwise it will be annoying to user
    public Actor? ForcedTarget;

    // low-level forced movement - if set, character will move in specified direction (ignoring casts, uptime, forbidden zones, etc), or stay in place if set to default
    public Vector3? ForcedMovement;

    // which direction should we point during the Spinning status in Alzadaal's Legacy? (yes, this is a bespoke movement gimmick for one dungeon boss)
    public Angle? SpinDirection;

    // indicates to AI mode that it should try to interact with some object
    public Actor? InteractWithTarget;

    // positioning: list of shapes that are either forbidden to stand in now or will be in near future
    // AI will try to move in such a way to avoid standing in any forbidden zone after its activation or outside of some restricted zone after its activation, even at the cost of uptime
    public List<(Func<WPos, bool> containsFn, DateTime activation, ulong Source)> ForbiddenZones = [];

    // positioning: list of goal functions
    // AI will try to move to reach non-forbidden point with highest goal value (sum of values returned by all functions)
    // guideline: rotation modules should return 1 if it would use single-target action from that spot, 2 if it is also a positional, 3 if it would use aoe that would hit minimal viable number of targets, +1 for each extra target
    // other parts of the code can return small (e.g. 0.01) values to slightly (de)prioritize some positions, or large (e.g. 1000) values to effectively soft-override target position (but still utilize pathfinding)
    public List<Func<WPos, float>> GoalZones = [];

    // AI will treat the pixels inside these shapes as unreachable and not try to pathfind through them (unlike imminent forbidden zones)
    public List<Func<WPos, bool>> TemporaryObstacles = [];

    // positioning: next positional hint (TODO: reconsider, maybe it should be a list prioritized by in-gcds, and imminent should be in-gcds instead? or maybe it should be property of an enemy? do we need correct?)
    public (Actor? Target, Positional Pos, bool Imminent, bool Correct) RecommendedPositional;

    // orientation restrictions (e.g. for gaze attacks): a list of forbidden orientation ranges, now or in near future
    // AI will rotate to face allowed orientation at last possible moment, potentially losing uptime
    public List<(Angle center, Angle halfWidth, DateTime activation)> ForbiddenDirections = [];

    // closest special movement/targeting/action mode, if any
    public (SpecialMode mode, DateTime activation) ImminentSpecialMode;

    // for misdirection: if forced movement is set, make real direction be within this angle
    public Angle MisdirectionThreshold;

    // predicted incoming damage (raidwides, tankbusters, etc.)
    // AI will attempt to shield & mitigate
    public List<DamagePrediction> PredictedDamage = [];

    // list of party members with cleansable debuffs that are dangerous enough to sacrifice a GCD to cleanse them, i.e. doom, throttle, some types of vuln debuff, etc
    public BitMask ShouldCleanse;

    // maximal time we can spend casting before we need to move
    // this is used by the action queue to skip casts that we won't be able to finish and execute lower-priority fallback actions instead
    public float MaxCastTime = float.MaxValue;
    public bool ForceCancelCast;

    // actions that we want to be executed, gathered from various sources (manual input, autorotation, planner, ai, modules, etc.)
    public ActionQueue ActionsToExecute = new();

    // buffs to be canceled asap
    public List<(uint statusId, ulong sourceId)> StatusesToCancel = [];

    // misc stuff to execute
    public bool WantJump;
    public bool WantDismount;

    // clear all stored data
    public void Clear()
    {
        PathfindMapCenter = default;
        PathfindMapBounds = DefaultBounds;
        PathfindMapObstacles = default;
        Array.Fill(Enemies, null);
        PotentialTargets.Clear();
        ForcedTarget = null;
        ForcedMovement = null;
        InteractWithTarget = null;
        ForbiddenZones.Clear();
        GoalZones.Clear();
        TemporaryObstacles.Clear();
        RecommendedPositional = default;
        ForbiddenDirections.Clear();
        ImminentSpecialMode = default;
        MisdirectionThreshold = 15.Degrees();
        PredictedDamage.Clear();
        ShouldCleanse.Reset();
        MaxCastTime = float.MaxValue;
        ForceCancelCast = false;
        ActionsToExecute.Clear();
        StatusesToCancel.Clear();
        WantJump = false;
        WantDismount = false;
    }

    public void PrioritizeTargetsByOID(uint oid, int priority = 0)
    {
        foreach (var h in PotentialTargets)
            if (h.Actor.OID == oid)
                h.Priority = Math.Max(priority, h.Priority);
    }
    public void PrioritizeTargetsByOID<OID>(OID oid, int priority = 0) where OID : Enum => PrioritizeTargetsByOID((uint)(object)oid, priority);

    public void PrioritizeTargetsByOID(uint[] oids, int priority = 0)
    {
        foreach (var h in PotentialTargets)
            if (oids.Contains(h.Actor.OID))
                h.Priority = Math.Max(priority, h.Priority);
    }

    public void PrioritizeAll()
    {
        foreach (var h in PotentialTargets)
            h.Priority = Math.Max(h.Priority, 0);
    }

    public void SetPriority(Actor? actor, int priority)
    {
        if (FindEnemy(actor) is { } enemy)
            enemy.Priority = priority;
    }

    public void InteractWithOID(WorldState ws, uint oid) => InteractWithTarget = ws.Actors.FirstOrDefault(a => a.OID == oid && a.IsTargetable);
    public void InteractWithOID<OID>(WorldState ws, OID oid) where OID : Enum => InteractWithOID(ws, (uint)(object)oid);

    public void AddForbiddenZone(Func<WPos, bool> containsFn, DateTime activation = new(), ulong source = 0) => ForbiddenZones.Add((containsFn, activation, source));
    public void AddForbiddenZone(AOEShape shape, WPos origin, Angle rot = new(), DateTime activation = new(), ulong source = 0) => ForbiddenZones.Add((shape.CheckFn(origin, rot), activation, source));

    public void AddPredictedDamage(BitMask players, DateTime activation, PredictedDamageType type = PredictedDamageType.Raidwide) => PredictedDamage.Add(new(players, activation, type));

    public void AddSpecialMode(SpecialMode mode, DateTime activation)
    {
        if (ImminentSpecialMode == default || ImminentSpecialMode.activation > activation)
            ImminentSpecialMode = (mode, activation);
    }

    // normalize all entries after gathering data: sort by priority / activation timestamp
    // TODO: note that the name is misleading - it actually happens mid frame, before all actions are gathered (eg before autorotation runs), but further steps (eg ai) might consume previously gathered data
    public void Normalize()
    {
        PotentialTargets.SortByReverse(x => x.Priority);
        HighestPotentialTargetPriority = Math.Max(0, PotentialTargets.FirstOrDefault()?.Priority ?? 0);
        ForbiddenZones.SortBy(e => e.activation);
        ForbiddenDirections.SortBy(e => e.activation);
        PredictedDamage.SortBy(e => e.Activation);
    }

    public void InitPathfindMap(Pathfinding.Map map)
    {
        PathfindMapBounds.PathfindMap(map, PathfindMapCenter);
        foreach (var o in TemporaryObstacles)
            map.BlockPixelsInside(o, -1000);
        if (PathfindMapObstacles.Bitmap != null)
        {
            var offX = -PathfindMapObstacles.Rect.Left;
            var offY = -PathfindMapObstacles.Rect.Top;
            var r = PathfindMapObstacles.Rect.Clamped(PathfindMapObstacles.Bitmap.FullRect).Clamped(new(0, 0, map.Width, map.Height), offX, offY);
            for (int y = r.Top; y < r.Bottom; ++y)
                for (int x = r.Left; x < r.Right; ++x)
                    if (PathfindMapObstacles.Bitmap[x, y])
                        map.PixelMaxG[(y + offY) * map.Width + x + offX] = -900;
        }
    }

    // query utilities
    public IEnumerable<Enemy> PotentialTargetsEnumerable => PotentialTargets;
    public IEnumerable<Enemy> PriorityTargets => PotentialTargets.TakeWhile(e => e.Priority == HighestPotentialTargetPriority);
    public IEnumerable<Enemy> ForbiddenTargets => PotentialTargetsEnumerable.Reverse().TakeWhile(e => e.Priority <= Enemy.PriorityUndesirable);

    // TODO: verify how source/target hitboxes are accounted for by various aoe shapes
    public int NumPriorityTargetsInAOE(Func<Enemy, bool> pred) => ForbiddenTargets.Any(pred) ? 0 : PriorityTargets.Count(pred);
    public int NumPriorityTargetsInAOECircle(WPos origin, float radius) => NumPriorityTargetsInAOE(a => TargetInAOECircle(a.Actor, origin, radius));
    public int NumPriorityTargetsInAOECone(WPos origin, float radius, WDir direction, Angle halfAngle) => NumPriorityTargetsInAOE(a => TargetInAOECone(a.Actor, origin, radius, direction, halfAngle));
    public int NumPriorityTargetsInAOERect(WPos origin, WDir direction, float lenFront, float halfWidth, float lenBack = 0) => NumPriorityTargetsInAOE(a => TargetInAOERect(a.Actor, origin, direction, lenFront, halfWidth, lenBack));
    public bool TargetInAOECircle(Actor target, WPos origin, float radius) => target.Position.InCircle(origin, radius + target.HitboxRadius);
    public bool TargetInAOECone(Actor target, WPos origin, float radius, WDir direction, Angle halfAngle) => Intersect.CircleCone(target.Position, target.HitboxRadius, origin, radius, direction, halfAngle);
    public bool TargetInAOERect(Actor target, WPos origin, WDir direction, float lenFront, float halfWidth, float lenBack = 0)
    {
        var rectCenterOffset = (lenFront - lenBack) / 2;
        var rectCenter = origin + direction * rectCenterOffset;
        return Intersect.CircleRect(target.Position, target.HitboxRadius, rectCenter, direction, halfWidth, (lenFront + lenBack) / 2);
    }

    // goal zones
    // simple goal zone that returns 1 if target is in range, useful for single-target actions
    public Func<WPos, float> GoalSingleTarget(WPos target, float radius, float weight = 1)
    {
        var effRsq = radius * radius;
        return p => (p - target).LengthSq() <= effRsq ? weight : 0;
    }
    public Func<WPos, float> GoalSingleTarget(Actor target, float range, float weight = 1) => GoalSingleTarget(target.Position, range + target.HitboxRadius + 0.5f, weight);

    // simple goal zone that returns 1 if target is in range (usually melee), 2 if it's also in correct positional
    public Func<WPos, float> GoalSingleTarget(WPos target, Angle rotation, Positional positional, float radius)
    {
        if (positional == Positional.Any)
            return GoalSingleTarget(target, radius); // more efficient implementation
        var effRsq = radius * radius;
        var targetDir = rotation.ToDirection();
        return p =>
        {
            var offset = p - target;
            var lsq = offset.LengthSq();
            if (lsq > effRsq)
                return 0; // out of range
            // note: this assumes that extra dot is cheaper than sqrt?..
            var front = targetDir.Dot(offset);
            var side = Math.Abs(targetDir.Dot(offset.OrthoL()));
            var inPositional = positional switch
            {
                Positional.Flank => side > Math.Abs(front),
                Positional.Rear => -front > side,
                Positional.Front => front > side, // TODO: reconsider this, it's not a real positional?..
                _ => false
            };
            return inPositional ? 2 : 1;
        };
    }
    public Func<WPos, float> GoalSingleTarget(Actor target, Positional positional, float range = 3) => GoalSingleTarget(target.Position, target.Rotation, positional, range + target.HitboxRadius + 0.5f);

    // simple goal zone that returns number of targets in aoes; note that performance is a concern for these functions, and perfection isn't required, so eg they ignore forbidden targets, etc
    public Func<WPos, float> GoalAOECircle(float radius)
    {
        List<(WPos pos, float radius)> targets = [.. PriorityTargets.Select(e => (e.Actor.Position, e.Actor.HitboxRadius))];
        return p => targets.Count(t => t.pos.InCircle(p, radius + t.radius));
    }

    public Func<WPos, float> GoalAOECone(Actor primaryTarget, float radius, Angle halfAngle)
    {
        List<(WPos pos, float radius)> targets = [.. PriorityTargets.Select(e => (e.Actor.Position, e.Actor.HitboxRadius))];
        var aimPoint = primaryTarget.Position;
        var effRange = radius + primaryTarget.HitboxRadius;
        var effRsq = effRange * effRange;
        return p =>
        {
            var toTarget = aimPoint - p;
            var lenSq = toTarget.LengthSq();
            if (lenSq > effRsq)
                return 0;
            var dir = toTarget / MathF.Sqrt(lenSq);
            return targets.Count(t => t.pos.InCircleCone(p, radius + t.radius, dir, halfAngle));
        };
    }

    public Func<WPos, float> GoalAOERect(Actor primaryTarget, float lenFront, float halfWidth, float lenBack = 0)
    {
        List<(WPos pos, float radius)> targets = [.. PriorityTargets.Select(e => (e.Actor.Position, e.Actor.HitboxRadius))];
        var aimPoint = primaryTarget.Position;
        var effRange = lenFront + primaryTarget.HitboxRadius;
        var effRsq = effRange * effRange;
        return p =>
        {
            var toTarget = aimPoint - p;
            var lenSq = toTarget.LengthSq();
            if (lenSq > effRsq)
                return 0;
            var dir = toTarget / MathF.Sqrt(lenSq);
            return targets.Count(t => t.pos.InRect(p, dir, lenFront, lenBack, halfWidth));
        };
    }

    // combined goal zone: returns 'aoe' priority if targets hit are at or above minimum, otherwise returns 'single-target' priority
    public Func<WPos, float> GoalCombined(Func<WPos, float> singleTarget, Func<WPos, float> aoe, int minAOETargets)
    {
        if (minAOETargets >= 50)
            return singleTarget; // assume aoe is never efficient, so don't bother
        return p =>
        {
            var aoeTargets = aoe(p) - minAOETargets;
            return aoeTargets >= 0 ? 3 + aoeTargets : singleTarget(p);
        };
    }

    // goal zone that returns a value between 0 and weight depending on distance to point; useful for downtime movement targets
    public Func<WPos, float> GoalProximity(WPos destination, float maxDistance, float maxWeight)
    {
        var invDist = 1.0f / maxDistance;
        return p =>
        {
            var dist = (p - destination).Length();
            var weight = 1 - Math.Clamp(invDist * dist, 0, 1);
            return maxWeight * weight;
        };
    }

    public Func<WPos, float> PullTargetToLocation(Actor target, WPos destination, float destRadius = 2)
    {
        var enemy = FindEnemy(target);
        if (enemy == null)
            return _ => 0;

        var adjRange = enemy.TankDistance + target.HitboxRadius + 0.5f;
        var desiredToTarget = target.Position - destination;
        var leewaySq = destRadius * destRadius;
        if (desiredToTarget.LengthSq() > leewaySq)
        {
            var dest = destination - adjRange * desiredToTarget.Normalized();
            return GoalSingleTarget(dest, PathfindMapBounds.MapResolution, 10);
        }
        return _ => 0;
    }
}
