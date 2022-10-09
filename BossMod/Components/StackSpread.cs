using System;
using System.Linq;

namespace BossMod.Components
{
    // generic 'stack/spread' mechanic has some players that have to spread away from raid, some other players that other players need to stack with
    // there are various variants (e.g. everyone should spread, or everyone should stack in one or more groups, or some combination of that)
    public class StackSpread : BossComponent
    {
        private AOEShapeCircle _stackShape;
        private AOEShapeCircle _spreadShape;
        public float StackRadius => _stackShape.Radius;
        public float SpreadRadius => _spreadShape.Radius;
        public int MinStackSize { get; private init; }
        public int MaxStackSize { get; private init; }
        public bool AlwaysShowSpreads { get; private init; } // if false, we only shown own spread radius for spread targets - this reduces visual clutter
        public BitMask SpreadMask;
        public BitMask StackMask;
        public DateTime ActivateAt;

        public bool Active => (SpreadMask | StackMask).Any();

        public StackSpread(float stackRadius, float spreadRadius, int minStackSize = 2, int maxStackSize = int.MaxValue, bool alwaysShowSpreads = false)
        {
            _stackShape = new(stackRadius);
            _spreadShape = new(spreadRadius);
            MinStackSize = minStackSize;
            MaxStackSize = maxStackSize;
            AlwaysShowSpreads = alwaysShowSpreads;
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
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
            else if (module.Raid.WithSlot().IncludedInMask(SpreadMask).InRadius(actor.Position, SpreadRadius).Any())
            {
                hints.Add("GTFO from spreads!");
            }
        }

        public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            // forbid standing next to spread markers
            foreach (var (_, player) in module.Raid.WithSlot().Exclude(slot).IncludedInMask(SpreadMask))
                hints.AddForbiddenZone(_spreadShape, player.Position, new(), ActivateAt);

            // if not spreading, deal with stack markers
            if (!SpreadMask[slot])
            {
                if (StackMask[slot])
                {
                    // forbid standing next to other stack markers
                    foreach (var (_, player) in module.Raid.WithSlot().Exclude(slot).IncludedInMask(StackMask))
                        hints.AddForbiddenZone(_stackShape, player.Position, new(), ActivateAt);
                }
                else
                {
                    // TODO: handle multi stacks better...
                    var closestStack = module.Raid.WithSlot().IncludedInMask(StackMask).Select(ia => ia.Item2).Closest(actor.Position);
                    if (closestStack != null)
                        hints.AddForbiddenZone(ShapeDistance.InvertedCircle(closestStack.Position, StackRadius), ActivateAt);
                }
            }

            // assume everyone will take some damage, either from sharing stacks or from spreads
            hints.PredictedDamage.Add((module.Raid.WithSlot().Mask(), ActivateAt));
        }

        public override PlayerPriority CalcPriority(BossModule module, int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
        {
            return SpreadMask[playerSlot] ? PlayerPriority.Danger
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
                foreach (var (_, player) in module.Raid.WithSlot().IncludedInMask(StackMask))
                    arena.AddCircle(player.Position, StackRadius, ArenaColor.Safe);
                foreach (var (_, player) in module.Raid.WithSlot().IncludedInMask(SpreadMask))
                    arena.AddCircle(player.Position, SpreadRadius, ArenaColor.Danger);
            }
        }

        private bool IsWellStacked(BossModule module, int slot, Actor actor)
        {
            if (StackMask[slot])
            {
                int numStacked = 1; // always stacked with self
                bool stackedWithOtherStack = false;
                foreach (var (otherSlot, other) in module.Raid.WithSlot().InRadiusExcluding(actor, StackRadius))
                {
                    ++numStacked;
                    stackedWithOtherStack |= StackMask[otherSlot];
                }
                return !stackedWithOtherStack && numStacked >= MinStackSize && numStacked <= MaxStackSize;
            }
            else
            {
                return module.Raid.WithSlot().IncludedInMask(StackMask).InRadius(actor.Position, StackRadius).Count() == 1;
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
