namespace BossMod;

// information relevant for AI decision making process for a specific player
public sealed class AIHints
{
    public class Enemy(Actor actor, bool shouldBeTanked)
    {
        // TODO: split 'pointless to attack' (eg invulnerable, but fine to hit by aoes) vs 'actually bad to hit' (eg can lead to wipe)
        public const int PriorityForbidAI = -1; // ai is forbidden from attacking this enemy, but player explicitly targeting it is not (e.g. out of combat enemies that we might not want to pull)
        public const int PriorityForbidFully = -2; // attacking this enemy is forbidden both by ai or player (e.g. invulnerable, or attacking/killing might lead to a wipe)

        public Actor Actor = actor;
        public int Priority = actor.InCombat ? 0 : PriorityForbidAI; // <0 means damaging is actually forbidden, 0 is default (TODO: revise default...)
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
        public bool StayAtLongRange; // if set, players with ranged attacks don't bother coming closer than max range (TODO: reconsider)
    }

    public enum SpecialMode
    {
        Normal,
        Pyretic, // pyretic/acceleration bomb type of effects - no movement, no actions, no casting allowed at activation time
        Freezing, // should be moving at activation time
        // TODO: misdirection, etc
    }

    public static readonly ArenaBounds DefaultBounds = new ArenaBoundsSquare(30);

    // information needed to build base pathfinding map (onto which forbidden/goal zones are later rasterized), if needed (lazy, since it's somewhat expensive and not always needed)
    public WPos PathfindMapCenter;
    public ArenaBounds PathfindMapBounds = DefaultBounds;
    public Bitmap.Region PathfindMapObstacles;

    // list of potential targets
    public List<Enemy> PotentialTargets = [];
    public int HighestPotentialTargetPriority;

    // forced target
    // this should be set only if either explicitly planned by user or by ai, otherwise it will be annoying to user
    public Actor? ForcedTarget;

    // low-level forced movement - if set, character will move in specified direction (ignoring casts, uptime, forbidden zones, etc), or stay in place if set to default
    public Vector3? ForcedMovement;

    // indicates to AI mode that it should try to interact with some object
    public Actor? InteractWithTarget;

    // positioning: list of shapes that are either forbidden to stand in now or will be in near future
    // AI will try to move in such a way to avoid standing in any forbidden zone after its activation or outside of some restricted zone after its activation, even at the cost of uptime
    public List<(Func<WPos, float> shapeDistance, DateTime activation)> ForbiddenZones = [];

    // positioning: list of goal functions
    // AI will try to move to reach non-forbidden point with highest goal value (sum of values returned by all functions)
    // guideline: rotation modules should return 1 if it would use single-target action from that spot, 2 if it is also a positional, 3 if it would use aoe that would hit minimal viable number of targets, +1 for each extra target
    // other parts of the code can return small (e.g. 0.01) values to slightly (de)prioritize some positions, or large (e.g. 1000) values to effectively soft-override target position (but still utilize pathfinding)
    public List<Func<WPos, float>> GoalZones = [];

    // positioning: next positional hint (TODO: reconsider, maybe it should be a list prioritized by in-gcds, and imminent should be in-gcds instead? or maybe it should be property of an enemy? do we need correct?)
    public (Actor? Target, Positional Pos, bool Imminent, bool Correct) RecommendedPositional;

    // orientation restrictions (e.g. for gaze attacks): a list of forbidden orientation ranges, now or in near future
    // AI will rotate to face allowed orientation at last possible moment, potentially losing uptime
    public List<(Angle center, Angle halfWidth, DateTime activation)> ForbiddenDirections = [];

    // closest special movement/targeting/action mode, if any
    public (SpecialMode mode, DateTime activation) ImminentSpecialMode;

    // predicted incoming damage (raidwides, tankbusters, etc.)
    // AI will attempt to shield & mitigate
    public List<(BitMask players, DateTime activation)> PredictedDamage = [];

    // estimate of the maximal time we can spend casting before we need to move
    // TODO: reconsider...
    public float MaxCastTimeEstimate = float.MaxValue;

    // actions that we want to be executed, gathered from various sources (manual input, autorotation, planner, ai, modules, etc.)
    public ActionQueue ActionsToExecute = new();

    // buffs to be canceled asap
    public List<(uint statusId, ulong sourceId)> StatusesToCancel = [];

    // clear all stored data
    public void Clear()
    {
        PathfindMapCenter = default;
        PathfindMapBounds = DefaultBounds;
        PathfindMapObstacles = default;
        PotentialTargets.Clear();
        ForcedTarget = null;
        ForcedMovement = null;
        InteractWithTarget = null;
        ForbiddenZones.Clear();
        GoalZones.Clear();
        RecommendedPositional = default;
        ForbiddenDirections.Clear();
        ImminentSpecialMode = default;
        PredictedDamage.Clear();
        MaxCastTimeEstimate = float.MaxValue;
        ActionsToExecute.Clear();
        StatusesToCancel.Clear();
    }

    // fill list of potential targets from world state
    public void FillPotentialTargets(WorldState ws, bool playerIsDefaultTank)
    {
        bool playerInFate = ws.Client.ActiveFate.ID != 0 && ws.Party.Player()?.Level <= Service.LuminaRow<Lumina.Excel.GeneratedSheets.Fate>(ws.Client.ActiveFate.ID)?.ClassJobLevelMax;
        var allowedFateID = playerInFate ? ws.Client.ActiveFate.ID : 0;
        foreach (var actor in ws.Actors.Where(a => a.IsTargetable && !a.IsAlly && !a.IsDead))
        {
            // fate mob in fate we are NOT a part of, skip entirely. it's okay to "attack" these (i.e., they won't be added as forbidden targets) because we can't even hit them
            // (though aggro'd mobs will continue attacking us after we unsync, but who really cares)
            if (actor.FateID > 0 && actor.FateID != allowedFateID)
                continue;

            // target is dying; skip it so that AI retargets, but ensure that it's not marked as a forbidden target
            // skip this check on striking dummies (name ID 541) as they die constantly
            var predictedHP = ws.PendingEffects.PendingHPDifference(actor.InstanceID);
            if (actor.HPMP.CurHP + predictedHP <= 0 && actor.NameID != 541)
                continue;

            var allowedAttack = actor.InCombat && ws.Party.FindSlot(actor.TargetID) >= 0;
            // enemies in our enmity list can also be attacked, regardless of who they are targeting (since they are keeping us in combat)
            allowedAttack |= actor.AggroPlayer;
            // all fate mobs can be attacked if we are level synced (non synced mobs are skipped above)
            allowedAttack |= actor.FateID > 0;

            PotentialTargets.Add(new(actor, playerIsDefaultTank)
            {
                Priority = allowedAttack ? 0 : Enemy.PriorityForbidAI
            });
        }
    }

    public void AddForbiddenZone(Func<WPos, float> shapeDistance, DateTime activation = new()) => ForbiddenZones.Add((shapeDistance, activation));
    public void AddForbiddenZone(AOEShape shape, WPos origin, Angle rot = new(), DateTime activation = new()) => ForbiddenZones.Add((shape.Distance(origin, rot), activation));

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
        PredictedDamage.SortBy(e => e.activation);
    }

    public void InitPathfindMap(Pathfinding.Map map)
    {
        PathfindMapBounds.PathfindMap(map, PathfindMapCenter);
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
    public IEnumerable<Enemy> ForbiddenTargets => PotentialTargetsEnumerable.Reverse().TakeWhile(e => e.Priority < 0);

    // TODO: verify how source/target hitboxes are accounted for by various aoe shapes
    public int NumPriorityTargetsInAOE(Func<Enemy, bool> pred) => ForbiddenTargets.Any(pred) ? 0 : PriorityTargets.Count(pred);
    public int NumPriorityTargetsInAOECircle(WPos origin, float radius) => NumPriorityTargetsInAOE(a => TargetInAOECircle(a.Actor, origin, radius));
    public int NumPriorityTargetsInAOECone(WPos origin, float radius, WDir direction, Angle halfAngle) => NumPriorityTargetsInAOE(a => TargetInAOECone(a.Actor, origin, radius, direction, halfAngle));
    public int NumPriorityTargetsInAOERect(WPos origin, WDir direction, float lenFront, float halfWidth, float lenBack = 0) => NumPriorityTargetsInAOE(a => TargetInAOERect(a.Actor, origin, direction, lenFront, halfWidth, lenBack));
    public bool TargetInAOECircle(Actor target, WPos origin, float radius) => target.Position.InCircle(origin, radius + target.HitboxRadius);
    public bool TargetInAOECone(Actor target, WPos origin, float radius, WDir direction, Angle halfAngle) => target.Position.InCircleCone(origin, radius + target.HitboxRadius, direction, halfAngle);
    public bool TargetInAOERect(Actor target, WPos origin, WDir direction, float lenFront, float halfWidth, float lenBack = 0) => target.Position.InRect(origin, direction, lenFront + target.HitboxRadius, lenBack, halfWidth);

    // goal zones
    // simple goal zone that returns 1 if target is in range, useful for single-target actions
    public Func<WPos, float> GoalSingleTarget(WPos target, float radius)
    {
        var effRsq = radius * radius;
        return p => (p - target).LengthSq() <= effRsq ? 1 : 0;
    }
    public Func<WPos, float> GoalSingleTarget(Actor target, float range) => GoalSingleTarget(target.Position, range + target.HitboxRadius + 0.5f);

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
    public Func<WPos, float> GoalSingleTarget(Actor target, Positional positional, float range = 3) => GoalSingleTarget(target.Position, target.Rotation, positional, range + target.HitboxRadius);

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
}
