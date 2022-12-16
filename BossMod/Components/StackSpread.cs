using System;
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
        public BitMask SpreadMask;
        public BitMask StackMask;
        public BitMask AvoidMask; // players that should not participate in the mechanic (i.e. avoid all stacks and spreads)
        public DateTime ActivateAt;

        public bool Active => (SpreadMask | StackMask).Any();

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
            if (AvoidMask[slot] && Active)
            {
                hints.Add("GTFO from raid!", module.Raid.WithSlot(IncludeDeadTargets).IncludedInMask(SpreadMask).InRadius(actor.Position, SpreadRadius).Any() || module.Raid.WithSlot(IncludeDeadTargets).IncludedInMask(StackMask).InRadius(actor.Position, StackRadius).Any());
                return;
            }

            // check that we're stacked properly
            if (!SpreadMask[slot] && StackMask.Any())
            {
                hints.Add("Stack!", !IsWellStacked(module, slot, actor));
            }

            // check that we're spread properly
            if (SpreadMask[slot])
            {
                hints.Add("Spread!", module.Raid.WithoutSlot().InRadiusExcluding(actor, SpreadRadius).Any());
            }
            else if (module.Raid.WithSlot(IncludeDeadTargets).IncludedInMask(SpreadMask).InRadius(actor.Position, SpreadRadius).Any())
            {
                hints.Add("GTFO from spreads!");
            }
        }

        public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            // forbid standing next to spread markers
            // TODO: think how to improve this, current implementation works, but isn't particularly good - e.g. nearby players tend to move to same spot, turn around, etc.
            // ideally we should provide per-mechanic spread spots, but for simple cases we should try to let melee spread close and healers/rdd spread far from main target...
            foreach (var (_, player) in module.Raid.WithSlot(IncludeDeadTargets).Exclude(slot).IncludedInMask(SpreadMask))
                hints.AddForbiddenZone(ShapeDistance.Circle(player.Position, SpreadRadius), ActivateAt);

            // if not spreading, deal with stack markers
            if (!SpreadMask[slot])
            {
                if ((StackMask | AvoidMask)[slot])
                {
                    // forbid standing next to other stack markers
                    foreach (var (_, player) in module.Raid.WithSlot(IncludeDeadTargets).Exclude(slot).IncludedInMask(StackMask))
                        hints.AddForbiddenZone(ShapeDistance.Circle(player.Position, StackRadius), ActivateAt);
                }
                else
                {
                    // TODO: handle multi stacks better...
                    var closestStack = module.Raid.WithSlot(IncludeDeadTargets).IncludedInMask(StackMask).Select(ia => ia.Item2).Closest(actor.Position);
                    if (closestStack != null)
                        hints.AddForbiddenZone(ShapeDistance.InvertedCircle(closestStack.Position, StackRadius), ActivateAt);
                }
            }

            // assume everyone will take some damage, either from sharing stacks or from spreads
            if (RaidwideOnResolve && Active)
                hints.PredictedDamage.Add((module.Raid.WithSlot().Mask() & ~AvoidMask, ActivateAt));
        }

        public override PlayerPriority CalcPriority(BossModule module, int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
        {
            if (AvoidMask[playerSlot])
                customColor = ArenaColor.Vulnerable;
            return (SpreadMask | SpreadMask)[playerSlot] ? PlayerPriority.Danger
                : StackMask[playerSlot] ? PlayerPriority.Interesting
                : Active ? PlayerPriority.Normal : PlayerPriority.Irrelevant;
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (!AlwaysShowSpreads && SpreadMask[pcSlot])
            {
                // draw only own circle - no one should be inside, this automatically resolves mechanic for us
                arena.AddCircle(pc.Position, SpreadRadius, ArenaColor.Danger);
            }
            else
            {
                // draw spread and stack circles
                foreach (var (_, player) in module.Raid.WithSlot(IncludeDeadTargets).IncludedInMask(StackMask))
                    arena.AddCircle(player.Position, StackRadius, ArenaColor.Safe);
                foreach (var (_, player) in module.Raid.WithSlot(IncludeDeadTargets).IncludedInMask(SpreadMask))
                    arena.AddCircle(player.Position, SpreadRadius, ArenaColor.Danger);
            }
        }

        private bool IsWellStacked(BossModule module, int slot, Actor actor)
        {
            if (StackMask[slot])
            {
                int numStacked = 1; // always stacked with self
                bool stackedWithOtherStackOrAvoid = false;
                foreach (var (otherSlot, other) in module.Raid.WithSlot().InRadiusExcluding(actor, StackRadius))
                {
                    ++numStacked;
                    stackedWithOtherStackOrAvoid |= (StackMask | AvoidMask)[otherSlot];
                }
                return !stackedWithOtherStackOrAvoid && numStacked >= MinStackSize && numStacked <= MaxStackSize;
            }
            else
            {
                return module.Raid.WithSlot(IncludeDeadTargets).IncludedInMask(StackMask).InRadius(actor.Position, StackRadius).Count() == 1;
            }
        }
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
                StackMask.Set(module.Raid.FindSlot(spell.TargetID));
                if (ActivateAt < spell.FinishAt)
                    ActivateAt = spell.FinishAt;
            }
            else if (spell.Action == SpreadAction)
            {
                SpreadMask.Set(module.Raid.FindSlot(spell.TargetID));
                if (ActivateAt < spell.FinishAt)
                    ActivateAt = spell.FinishAt;
            }
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (spell.Action == StackAction)
            {
                StackMask.Clear(module.Raid.FindSlot(spell.TargetID));
                ++NumFinishedStacks;
            }
            else if (spell.Action == SpreadAction)
            {
                SpreadMask.Clear(module.Raid.FindSlot(spell.TargetID));
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
