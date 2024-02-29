﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Components
{
    // generic 'stack/spread' mechanic has some players that have to spread away from raid, some other players that other players need to stack with
    // there are various variants (e.g. everyone should spread, or everyone should stack in one or more groups, or some combination of that)
    public class GenericStackSpread : BossComponent
    {
        public struct Stack
        {
            public Actor Target;
            public float Radius;
            public int MinSize;
            public int MaxSize;
            public DateTime Activation;
            public BitMask ForbiddenPlayers; // raid members that aren't allowed to participate in the stack

            public Stack(Actor target, float radius, int minSize = 2, int maxSize = int.MaxValue, DateTime activation = default, BitMask forbiddenPlayers = default)
            {
                Target = target;
                Radius = radius;
                MinSize = minSize;
                MaxSize = maxSize;
                Activation = activation;
                ForbiddenPlayers = forbiddenPlayers;
            }
        }

        public struct Spread
        {
            public Actor Target;
            public float Radius;
            public DateTime Activation;

            public Spread(Actor target, float radius, DateTime activation = default)
            {
                Target = target;
                Radius = radius;
                Activation = activation;
            }
        }

        public bool AlwaysShowSpreads; // if false, we only shown own spread radius for spread targets - this reduces visual clutter
        public bool RaidwideOnResolve; // if true, assume even if mechanic is correctly resolved everyone will still take damage
        public bool IncludeDeadTargets; // if false, stacks & spreads with dead targets are ignored
        public List<Stack> Stacks = new();
        public List<Spread> Spreads = new();

        public bool Active => Stacks.Count + Spreads.Count > 0;
        public IEnumerable<Stack> ActiveStacks => IncludeDeadTargets ? Stacks : Stacks.Where(s => !s.Target.IsDead);
        public IEnumerable<Spread> ActiveSpreads => IncludeDeadTargets ? Spreads : Spreads.Where(s => !s.Target.IsDead);

        public bool IsStackTarget(Actor actor) => Stacks.Any(s => s.Target == actor);
        public bool IsSpreadTarget(Actor actor) => Spreads.Any(s => s.Target == actor);

        public GenericStackSpread(bool alwaysShowSpreads = false, bool raidwideOnResolve = true, bool includeDeadTargets = false)
        {
            AlwaysShowSpreads = alwaysShowSpreads;
            RaidwideOnResolve = raidwideOnResolve;
            IncludeDeadTargets = includeDeadTargets;
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (Spreads.FindIndex(s => s.Target == actor) is var iSpread && iSpread >= 0)
            {
                hints.Add("Spread!", module.Raid.WithoutSlot().InRadiusExcluding(actor, Spreads[iSpread].Radius).Any());
            }
            else if (Stacks.FindIndex(s => s.Target == actor) is var iStack && iStack >= 0)
            {
                var stack = Stacks[iStack];
                int numStacked = 1; // always stacked with self
                bool stackedWithOtherStackOrAvoid = false;
                foreach (var (j, other) in module.Raid.WithSlot().InRadiusExcluding(actor, stack.Radius))
                {
                    ++numStacked;
                    stackedWithOtherStackOrAvoid |= stack.ForbiddenPlayers[j] || IsStackTarget(other);
                }
                hints.Add("Stack!", stackedWithOtherStackOrAvoid || numStacked < stack.MinSize || numStacked > stack.MaxSize);
            }
            else
            {
                int numParticipatingStacks = 0;
                int numUnsatisfiedStacks = 0;
                foreach (var s in ActiveStacks.Where(s => !s.ForbiddenPlayers[slot]))
                {
                    if (actor.Position.InCircle(s.Target.Position, s.Radius))
                        ++numParticipatingStacks;
                    else if (module.Raid.WithoutSlot().InRadiusExcluding(s.Target, s.Radius).Count() + 1 < s.MinSize)
                        ++numUnsatisfiedStacks;
                }

                if (numParticipatingStacks > 1)
                    hints.Add("Stack!");
                else if (numParticipatingStacks == 1)
                    hints.Add("Stack!", false);
                else if (numUnsatisfiedStacks > 0)
                    hints.Add("Stack!");
                // else: don't show anything, all potential stacks are already satisfied without a player
                //hints.Add("Stack!", ActiveStacks.Count(s => !s.ForbiddenPlayers[slot] && actor.Position.InCircle(s.Target.Position, s.Radius)) != 1);
            }

            if (ActiveSpreads.Any(s => s.Target != actor && actor.Position.InCircle(s.Target.Position, s.Radius)))
            {
                hints.Add("GTFO from spreads!");
            }
            else if (ActiveStacks.Any(s => s.Target != actor && s.ForbiddenPlayers[slot] && actor.Position.InCircle(s.Target.Position, s.Radius)))
            {
                hints.Add("GTFO from forbidden stacks!");
            }
        }

        public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            // forbid standing next to spread markers
            // TODO: think how to improve this, current implementation works, but isn't particularly good - e.g. nearby players tend to move to same spot, turn around, etc.
            // ideally we should provide per-mechanic spread spots, but for simple cases we should try to let melee spread close and healers/rdd spread far from main target...
            foreach (var spreadFrom in ActiveSpreads.Where(s => s.Target != actor))
                hints.AddForbiddenZone(ShapeDistance.Circle(spreadFrom.Target.Position, spreadFrom.Radius), spreadFrom.Activation);

            foreach (var avoid in ActiveStacks.Where(s => s.Target != actor && s.ForbiddenPlayers[slot]))
                hints.AddForbiddenZone(ShapeDistance.Circle(avoid.Target.Position, avoid.Radius), avoid.Activation);

            if (IsStackTarget(actor))
            {
                // forbid standing next to other stack markers
                foreach (var stackWith in ActiveStacks.Where(s => s.Target != actor))
                    hints.AddForbiddenZone(ShapeDistance.Circle(stackWith.Target.Position, stackWith.Radius), stackWith.Activation);
            }
            else if (!IsSpreadTarget(actor))
            {
                // TODO: handle multi stacks better...
                var closestStack = ActiveStacks.Where(s => !s.ForbiddenPlayers[slot]).MinBy(s => (s.Target.Position - actor.Position).LengthSq());
                if (closestStack.Target != null)
                    hints.AddForbiddenZone(ShapeDistance.InvertedCircle(closestStack.Target.Position, closestStack.Radius), closestStack.Activation);
            }

            if (RaidwideOnResolve)
            {
                DateTime firstActivation = DateTime.MaxValue;
                BitMask damageMask = new();
                foreach (var s in ActiveSpreads)
                {
                    damageMask.Set(module.Raid.FindSlot(s.Target.InstanceID));
                    firstActivation = firstActivation < s.Activation ? firstActivation : s.Activation;
                }
                foreach (var s in ActiveStacks)
                {
                    damageMask |= module.Raid.WithSlot().Mask() & ~s.ForbiddenPlayers; // assume everyone will take damage except forbidden players (so-so assumption really...)
                    firstActivation = firstActivation < s.Activation ? firstActivation : s.Activation;
                }

                if (damageMask.Any())
                    hints.PredictedDamage.Add((damageMask, firstActivation));
            }
        }

        public override PlayerPriority CalcPriority(BossModule module, int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
        {
            var shouldSpread = IsSpreadTarget(player);
            var shouldStack = IsStackTarget(player);
            var shouldAvoid = !shouldSpread && !shouldStack && ActiveStacks.Any(s => s.ForbiddenPlayers[playerSlot]);
            if (shouldAvoid)
                customColor = ArenaColor.Vulnerable;
            return shouldAvoid || shouldSpread ? PlayerPriority.Danger
                : shouldStack ? PlayerPriority.Interesting
                : Active ? PlayerPriority.Normal : PlayerPriority.Irrelevant;
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (!AlwaysShowSpreads && Spreads.FindIndex(s => s.Target == pc) is var iSpread && iSpread >= 0)
            {
                // draw only own circle - no one should be inside, this automatically resolves mechanic for us
                arena.AddCircle(pc.Position, Spreads[iSpread].Radius, ArenaColor.Danger);
            }
            else
            {
                // draw spread and stack circles
                foreach (var s in ActiveStacks)
                {
                    if (arena.Config.ShowOutlinesAndShadows)
                        arena.AddCircle(s.Target.Position, s.Radius, 0xFF000000, 2);
                    arena.AddCircle(s.Target.Position, s.Radius, ArenaColor.Safe);
                }
                foreach (var s in ActiveSpreads)
                {
                    if (arena.Config.ShowOutlinesAndShadows)
                        arena.AddCircle(s.Target.Position, s.Radius, 0xFF000000, 2);
                    arena.AddCircle(s.Target.Position, s.Radius, ArenaColor.Danger);
                }
            }
        }
    }

    // stack/spread with same properties for all stacks and all spreads (most common variant)
    public class UniformStackSpread : GenericStackSpread
    {
        public float StackRadius;
        public float SpreadRadius;
        public int MinStackSize;
        public int MaxStackSize;

        public IEnumerable<Actor> ActiveStackTargets => ActiveStacks.Select(s => s.Target);
        public IEnumerable<Actor> ActiveSpreadTargets => ActiveSpreads.Select(s => s.Target);

        public UniformStackSpread(float stackRadius, float spreadRadius, int minStackSize = 2, int maxStackSize = int.MaxValue, bool alwaysShowSpreads = false, bool raidwideOnResolve = true, bool includeDeadTargets = false)
            : base(alwaysShowSpreads, raidwideOnResolve, includeDeadTargets)
        {
            StackRadius = stackRadius;
            SpreadRadius = spreadRadius;
            MinStackSize = minStackSize;
            MaxStackSize = maxStackSize;
        }

        public void AddStack(Actor target, DateTime activation = default, BitMask forbiddenPlayers = default) => Stacks.Add(new(target, StackRadius, MinStackSize, MaxStackSize, activation, forbiddenPlayers));
        public void AddStacks(IEnumerable<Actor> targets, DateTime activation = default) => Stacks.AddRange(targets.Select(target => new Stack(target, StackRadius, MinStackSize, MaxStackSize, activation)));
        public void AddSpread(Actor target, DateTime activation = default) => Spreads.Add(new(target, SpreadRadius, activation));
        public void AddSpreads(IEnumerable<Actor> targets, DateTime activation = default) => Spreads.AddRange(targets.Select(target => new Spread(target, SpreadRadius, activation)));
    }

    // spread/stack mechanic that selects targets by casts
    public class CastStackSpread : UniformStackSpread
    {
        public ActionID StackAction { get; private init; }
        public ActionID SpreadAction { get; private init; }
        public int NumFinishedStacks { get; protected set; }
        public int NumFinishedSpreads { get; protected set; }

        public CastStackSpread(ActionID stackAID, ActionID spreadAID, float stackRadius, float spreadRadius, int minStackSize = 2, int maxStackSize = int.MaxValue, bool alwaysShowSpreads = false)
            : base(stackRadius, spreadRadius, minStackSize, maxStackSize, alwaysShowSpreads)
        {
            StackAction = stackAID;
            SpreadAction = spreadAID;
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (spell.Action == StackAction && module.WorldState.Actors.Find(spell.TargetID) is var stackTarget && stackTarget != null)
            {
                AddStack(stackTarget, spell.NPCFinishAt);
            }
            else if (spell.Action == SpreadAction && module.WorldState.Actors.Find(spell.TargetID) is var spreadTarget && spreadTarget != null)
            {
                AddSpread(spreadTarget, spell.NPCFinishAt);
            }
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (spell.Action == StackAction)
            {
                Stacks.RemoveAll(s => s.Target.InstanceID == spell.TargetID);
                ++NumFinishedStacks;
            }
            else if (spell.Action == SpreadAction)
            {
                Spreads.RemoveAll(s => s.Target.InstanceID == spell.TargetID);
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
