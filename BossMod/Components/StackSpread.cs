using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Components
{
    // generic 'stack/spread' mechanic has some players that have to spread away from raid, some other players that other players need to stack with
    // there are various variants (e.g. everyone should spread, or everyone should stack in one or more groups, or some combination of that)
    public class StackSpread : BossComponent
    {
        public float StackRadius;
        public float SpreadRadius;
        public int MinStackSize;
        public int MaxStackSize;
        public bool AlwaysShowSpreads; // if false, we only shown own spread radius for spread targets - this reduces visual clutter
        public bool RaidwideOnResolve; // by default, assume even if mechanic is correctly resolved everyone will still take damage
        public bool IncludeDeadTargets; // by default, mechanics can't target dead players
        public List<Actor> SpreadTargets = new();
        public List<Actor> StackTargets = new();
        public List<Actor> AvoidTargets = new(); // players that should not participate in the mechanic (i.e. avoid all stacks and spreads)
        public DateTime ActivateAt;

        public bool Active => SpreadTargets.Count + StackTargets.Count > 0;
        public IEnumerable<Actor> ActiveSpreadTargets => ActiveTargets(SpreadTargets);
        public IEnumerable<Actor> ActiveStackTargets => ActiveTargets(StackTargets);
        public IEnumerable<Actor> ActiveAvoidTargets => ActiveTargets(AvoidTargets);

        public bool IsSpreadTarget(Actor actor) => SpreadTargets.Contains(actor);
        public bool IsStackTarget(Actor actor) => StackTargets.Contains(actor);
        public bool IsAvoidTarget(Actor actor) => AvoidTargets.Contains(actor);

        public StackSpread(float stackRadius, float spreadRadius, int minStackSize = 2, int maxStackSize = int.MaxValue, bool alwaysShowSpreads = false, bool raidwideOnResolve = true, bool includeDeadTargets = false)
        {
            StackRadius = stackRadius;
            SpreadRadius = spreadRadius;
            MinStackSize = minStackSize;
            MaxStackSize = maxStackSize;
            AlwaysShowSpreads = alwaysShowSpreads;
            RaidwideOnResolve = raidwideOnResolve;
            IncludeDeadTargets = includeDeadTargets;
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (IsAvoidTarget(actor) && Active)
            {
                hints.Add("GTFO from raid!", ActiveSpreadTargets.InRadiusExcluding(actor, SpreadRadius).Any() || ActiveStackTargets.InRadiusExcluding(actor, StackRadius).Any());
                return;
            }

            // check that we're stacked properly
            bool shouldSpread = IsSpreadTarget(actor);
            if (!shouldSpread && StackTargets.Count > 0)
            {
                hints.Add("Stack!", !IsWellStacked(module, actor));
            }

            // check that we're spread properly
            if (shouldSpread)
            {
                hints.Add("Spread!", module.Raid.WithoutSlot().InRadiusExcluding(actor, SpreadRadius).Any());
            }
            else if (ActiveSpreadTargets.InRadius(actor.Position, SpreadRadius).Any())
            {
                hints.Add("GTFO from spreads!");
            }
        }

        public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            // forbid standing next to spread markers
            // TODO: think how to improve this, current implementation works, but isn't particularly good - e.g. nearby players tend to move to same spot, turn around, etc.
            // ideally we should provide per-mechanic spread spots, but for simple cases we should try to let melee spread close and healers/rdd spread far from main target...
            foreach (var spreadFrom in ActiveSpreadTargets.Exclude(actor))
                hints.AddForbiddenZone(ShapeDistance.Circle(spreadFrom.Position, SpreadRadius), ActivateAt);

            // if not spreading, deal with stack markers
            if (!IsSpreadTarget(actor))
            {
                if (IsStackTarget(actor) || IsAvoidTarget(actor))
                {
                    // forbid standing next to other stack markers
                    foreach (var stackWith in ActiveStackTargets.Exclude(actor))
                        hints.AddForbiddenZone(ShapeDistance.Circle(stackWith.Position, StackRadius), ActivateAt);
                }
                else
                {
                    // TODO: handle multi stacks better...
                    var closestStack = ActiveStackTargets.Closest(actor.Position);
                    if (closestStack != null)
                        hints.AddForbiddenZone(ShapeDistance.InvertedCircle(closestStack.Position, StackRadius), ActivateAt);
                }
            }

            // assume everyone will take some damage, either from sharing stacks or from spreads
            if (RaidwideOnResolve && Active)
            {
                var damageMask = module.Raid.WithSlot().Mask();
                foreach (var avoid in AvoidTargets)
                    damageMask.Clear(module.Raid.FindSlot(avoid.InstanceID));
                hints.PredictedDamage.Add((damageMask, ActivateAt));
            }
        }

        public override PlayerPriority CalcPriority(BossModule module, int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
        {
            var shouldAvoid = IsAvoidTarget(player);
            if (shouldAvoid)
                customColor = ArenaColor.Vulnerable;
            return shouldAvoid || IsSpreadTarget(player) ? PlayerPriority.Danger
                : IsStackTarget(player) ? PlayerPriority.Interesting
                : Active ? PlayerPriority.Normal : PlayerPriority.Irrelevant;
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (!AlwaysShowSpreads && IsSpreadTarget(pc))
            {
                // draw only own circle - no one should be inside, this automatically resolves mechanic for us
                arena.AddCircle(pc.Position, SpreadRadius, ArenaColor.Danger);
            }
            else
            {
                // draw spread and stack circles
                foreach (var player in ActiveStackTargets)
                    arena.AddCircle(player.Position, StackRadius, ArenaColor.Safe);
                foreach (var player in ActiveSpreadTargets)
                    arena.AddCircle(player.Position, SpreadRadius, ArenaColor.Danger);
            }
        }

        private bool IsWellStacked(BossModule module, Actor actor)
        {
            if (IsStackTarget(actor))
            {
                int numStacked = 1; // always stacked with self
                bool stackedWithOtherStackOrAvoid = false;
                foreach (var other in module.Raid.WithoutSlot().InRadiusExcluding(actor, StackRadius))
                {
                    ++numStacked;
                    stackedWithOtherStackOrAvoid |= IsStackTarget(other) || IsAvoidTarget(other);
                }
                return !stackedWithOtherStackOrAvoid && numStacked >= MinStackSize && numStacked <= MaxStackSize;
            }
            else
            {
                return ActiveStackTargets.InRadius(actor.Position, StackRadius).Count() == 1;
            }
        }

        private IEnumerable<Actor> ActiveTargets(List<Actor> list) => IncludeDeadTargets ? list : list.Where(a => !a.IsDead);
    }

    // spread/stack mechanic that selects targets by casts
    public class CastStackSpread : StackSpread
    {
        public ActionID StackAction { get; private init; }
        public ActionID SpreadAction { get; private init; }
        public int NumFinishedStacks { get; private set; }
        public int NumFinishedSpreads { get; private set; }

        public CastStackSpread(ActionID stackAID, ActionID spreadAID, float stackRadius, float spreadRadius, int minStackSize = 2, int maxStackSize = int.MaxValue, bool alwaysShowSpreads = false)
            : base(stackRadius, spreadRadius, minStackSize, maxStackSize, alwaysShowSpreads)
        {
            StackAction = stackAID;
            SpreadAction = spreadAID;
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (spell.Action == StackAction)
            {
                if (module.WorldState.Actors.Find(spell.TargetID) is var target && target != null)
                    StackTargets.Add(target);
                if (ActivateAt < spell.FinishAt)
                    ActivateAt = spell.FinishAt;
            }
            else if (spell.Action == SpreadAction)
            {
                if (module.WorldState.Actors.Find(spell.TargetID) is var target && target != null)
                    SpreadTargets.Add(target);
                if (ActivateAt < spell.FinishAt)
                    ActivateAt = spell.FinishAt;
            }
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (spell.Action == StackAction)
            {
                StackTargets.RemoveAll(a => a.InstanceID == spell.TargetID);
                ++NumFinishedStacks;
            }
            else if (spell.Action == SpreadAction)
            {
                SpreadTargets.RemoveAll(a => a.InstanceID == spell.TargetID);
                ++NumFinishedSpreads;
            }
        }
    }

    // generic 'spread from targets of specific cast' mechanic
    public class SpreadFromCastTargets : CastStackSpread
    {
        public SpreadFromCastTargets(ActionID aid, float radius, bool drawAllSpreads = true) : base(new(), aid, 0, radius, alwaysShowSpreads: drawAllSpreads) { }
    }

    // generic 'stack with targets of specific cast' mechanic
    public class StackWithCastTargets : CastStackSpread
    {
        public StackWithCastTargets(ActionID aid, float radius, int minStackSize = 2, int maxStackSize = int.MaxValue) : base(aid, new(), radius, 0, minStackSize, maxStackSize) { }
    }
}
