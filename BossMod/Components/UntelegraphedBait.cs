using static BossMod.AIHints;

namespace BossMod.Components;

// TODO: this is more or less a generalization of all the other bait/spread/stack components, created because there was no fitting component for cone-shaped stack/spreads that are commonly used in savage
// it might be good to reimplement other components in terms of this one to reduce code duplication
class UntelegraphedBait(BossModule module, Enum? aid = null) : CastCounter(module, aid)
{
    // indicate that `count` number of players matching some `targets` filter (e.g. both healers, all supports, 2 closest players, etc etc) will be hit by an aoe
    public struct Bait(WPos origin, BitMask targets, AOEShape shape, DateTime activation = default, int count = int.MaxValue, int stackSize = 1, BitMask forbiddenTargets = default, bool isProximity = false, bool centerAtTarget = false, PredictedDamageType type = PredictedDamageType.None)
    {
        public WPos Origin = origin;
        public AOEShape Shape = shape;
        public BitMask Targets = targets;
        public BitMask ForbiddenTargets = forbiddenTargets;
        public int Count = count;
        public int StackSize = stackSize;
        public DateTime Activation = activation;
        public PredictedDamageType Type = type == PredictedDamageType.None ? (stackSize > 1 ? PredictedDamageType.Shared : PredictedDamageType.Raidwide) : type;
        public bool IsProximity = isProximity;
        public bool CenterAtTarget = centerAtTarget;

        public readonly bool IsStack => StackSize > 1;
        public readonly bool IsSpread => StackSize == 1;
        public readonly bool CanOverlap => Count < Targets.NumSetBits();

        public readonly WPos Position(Actor target) => CenterAtTarget ? target.Position : Origin;
        public readonly Angle Angle(Actor target) => (target.Position - Origin).ToAngle();
    }

    public readonly List<Bait> CurrentBaits = [];

    public bool EnableHints = true;

    public IEnumerable<Bait> BaitsOn(int slot) => CurrentBaits.Where(b => b.Targets[slot]);
    public IEnumerable<Bait> BaitsNotOn(int slot) => CurrentBaits.Where(b => !b.Targets[slot]);
    public IEnumerable<Actor> PossibleTargets(in Bait b) => Raid.WithSlot().IncludedInMask(b.Targets).Select(p => p.Item2);

    public bool IsStackTarget(int slot) => CurrentBaits.Any(b => b.IsStack && b.Targets[slot]);
    public bool IsSpreadTarget(int slot) => CurrentBaits.Any(b => b.IsSpread && b.Targets[slot]);
    public bool IsDifferentTarget(in Bait b, int slot) => !b.Targets[slot] || b.Count > 1;

    public override void Update()
    {
        for (var i = 0; i < CurrentBaits.Count; i++)
        {
            ref var bait = ref CurrentBaits.Ref(i);
            if (bait.IsProximity)
                bait.Targets = Raid.WithSlot().SortedByRange(bait.Origin).Take(bait.Count).Mask();
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var bait in BaitsOn(pcSlot).Where(b => !b.IsStack))
            bait.Shape.Outline(Arena, bait.Position(pc), bait.Angle(pc), ArenaColor.Danger);
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var bait in BaitsOn(pcSlot))
        {
            if (bait.IsStack)
                bait.Shape.Draw(Arena, bait.Position(pc), bait.Angle(pc), bait.ForbiddenTargets[pcSlot] ? ArenaColor.AOE : ArenaColor.SafeFromAOE);

            if (bait.Count > 1)
                foreach (var b in PossibleTargets(bait).Exclude(pc))
                    bait.Shape.Draw(Arena, bait.Position(b), bait.Angle(b), bait.IsStack ? ArenaColor.SafeFromAOE : ArenaColor.AOE);
        }

        foreach (var bait in BaitsNotOn(pcSlot))
        {
            foreach (var baiter in PossibleTargets(bait).Take(bait.Count))
                bait.Shape.Draw(Arena, bait.Position(baiter), bait.Angle(baiter), bait.IsStack && !bait.ForbiddenTargets[pcSlot] ? ArenaColor.SafeFromAOE : ArenaColor.AOE);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!EnableHints)
            return;

        if (BaitsOn(slot).FirstOrNull() is { } myBait)
        {
            if (myBait.ForbiddenTargets[slot])
                hints.Add("Avoid baiting!");
            else if (myBait.IsSpread)
            {
                // TODO: make this more generic, this is for the surprisingly common usecase of a non-stack protean being baited onto only one of several possible players, where overlap is not fatal
                var forbiddenTargets = myBait.CanOverlap ? Raid.WithSlot().ExcludedFromMask(myBait.Targets) : Raid.WithSlot().Exclude(actor);
                if (forbiddenTargets.InShape(myBait.Shape, myBait.Position(actor), myBait.Angle(actor)).Any())
                    hints.Add("Bait away from raid!");
            }
            else
            {
                var numStacked = 1;
                var avoid = false;
                var overlap = false;
                foreach (var (j, other) in Raid.WithSlot().Exclude(actor).InShape(myBait.Shape, myBait.Position(actor), myBait.Angle(actor)))
                {
                    ++numStacked;
                    avoid |= myBait.ForbiddenTargets[j];
                    overlap |= IsStackTarget(j) && IsDifferentTarget(myBait, j);
                }
                hints.Add("Stack!", numStacked < myBait.StackSize);
                if (avoid || overlap)
                    hints.Add("Bait away from raid!");
            }
        }
        else
        {
            var numParticipatingStacks = 0;
            var numUnsatisfiedStacks = 0;
            foreach (var s in CurrentBaits.Where(s => !s.ForbiddenTargets[slot] && s.IsStack))
            {
                foreach (var target in PossibleTargets(s))
                {
                    if (s.Shape.Check(actor.Position, s.Position(target), s.Angle(target)))
                        ++numParticipatingStacks;
                    else if (Raid.WithoutSlot().Exclude(target).InShape(s.Shape, s.Position(target), s.Angle(target)).Count() + 1 < s.StackSize)
                        ++numUnsatisfiedStacks;
                }
            }

            if (numParticipatingStacks > 1)
                hints.Add("Stack!");
            else if (numParticipatingStacks == 1)
                hints.Add("Stack!", false);
            else if (numUnsatisfiedStacks > 0)
                hints.Add("Stack!");
        }

        foreach (var b in CurrentBaits)
        {
            var a1 = b.IsSpread && !b.Targets[slot];
            var a2 = b.IsStack && b.ForbiddenTargets[slot];

            if (a1 || a2)
                if (PossibleTargets(b).Any(tar => b.Shape.Check(actor.Position, b.Position(tar), b.Angle(tar))))
                {
                    hints.Add(a1 ? "GTFO from bait!" : "GTFO from forbidden stack!");
                    return;
                }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!EnableHints)
            return;

        int numOthersInside(in Bait b, Actor target) => Raid.WithoutSlot().Exclude(actor).InShape(b.Shape, b.Position(target), b.Angle(target)).Count();

        foreach (var spreadFrom in CurrentBaits.Where(s => s.IsSpread && !s.Targets[slot]))
        {
            foreach (var t in PossibleTargets(spreadFrom).Exclude(actor))
                hints.AddForbiddenZone(spreadFrom.Shape.CheckFn(spreadFrom.Position(t), spreadFrom.Angle(t)), spreadFrom.Activation);
        }

        var actorSpreadN = CurrentBaits.Where(s => s.IsSpread && s.Targets[slot]).FirstOrNull();
        if (actorSpreadN is { } actorSpread)
        {
            var forbiddenTargets = actorSpread.CanOverlap ? Raid.WithSlot().ExcludedFromMask(actorSpread.Targets) : Raid.WithSlot().Exclude(actor);
            foreach (var (_, target) in forbiddenTargets)
                hints.AddForbiddenZone(actorSpread.Shape.CheckFn(actorSpread.Position(target), actorSpread.Angle(target)), actorSpread.Activation);
        }

        foreach (var avoid in CurrentBaits.Where(s => s.IsStack && s.ForbiddenTargets[slot]))
        {
            foreach (var t in PossibleTargets(avoid))
                hints.AddForbiddenZone(avoid.Shape.CheckFn(avoid.Position(t), avoid.Angle(t)), avoid.Activation);
        }

        if (CurrentBaits.Where(s => s.IsStack && s.Targets[slot]).FirstOrNull() is { } actorStack)
        {
            // avoid stacks where we are not the target
            // avoid multi-instance stacks where we are the target, since overlap is assumed to be fatal
            foreach (var stackWith in CurrentBaits.Where(s => s.IsStack && IsDifferentTarget(s, slot)))
            {
                foreach (var t in PossibleTargets(stackWith).Exclude(actor))
                    hints.AddForbiddenZone(stackWith.Shape.CheckFn(stackWith.Position(t), stackWith.Angle(t)), stackWith.Activation);
            }

            // stack with closest player who is either baiting the same aoe as us, or not baiting anything
            var closest = Raid.WithSlot().Exclude(actor).ExcludedFromMask(actorStack.ForbiddenTargets).WhereSlot(p => !IsDifferentTarget(actorStack, p) || !IsStackTarget(p) && !IsSpreadTarget(p)).Select(p => p.Item2).Closest(actor.Position);
            if (closest != null)
            {
                bool cf(WPos p) => !actorStack.Shape.Check(p, actorStack.Position(closest), actorStack.Angle(closest));
                hints.AddForbiddenZone(cf, actorStack.Activation);
            }
        }
        else if (actorSpreadN == null)
        {
            var closestStack = CurrentBaits.Where(s => !s.ForbiddenTargets[slot] && s.IsStack).SelectMany(b => PossibleTargets(b).Where(t => numOthersInside(b, t) < b.StackSize).Select(t => (t, b))).MinBy(t => (t.t.Position - actor.Position).LengthSq());
            if (closestStack.t != null)
            {
                bool cf2(WPos p) => !closestStack.b.Shape.Check(p, closestStack.b.Position(closestStack.t), closestStack.b.Angle(closestStack.t));
                hints.AddForbiddenZone(cf2, closestStack.b.Activation);
            }
        }

        foreach (var s in CurrentBaits)
        {
            if (s.IsSpread)
                hints.AddPredictedDamage(s.Targets, s.Activation, s.Type);
            else
                foreach (var target in PossibleTargets(s))
                {
                    hints.AddPredictedDamage(Raid.WithSlot().InShape(s.Shape, s.Position(target), s.Angle(target)).Mask(), s.Activation, s.Type);
                }
        }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        // special case: if we need to spread, highlight only the other players we spread with
        if (CurrentBaits.FirstOrNull(s => s.IsSpread && s.Targets[pcSlot]) is { } mySpread)
            return mySpread.Targets[playerSlot] ? PlayerPriority.Interesting : PlayerPriority.Irrelevant;

        // special case: if we are a random target for a stack that only has one instance, all other players should not be highlighted (the component assumes that the player is the bait target)
        // for stacks that have more than one instance (i.e. baited on supports/healers) other targets will be highlighted as normal
        if (CurrentBaits.FirstOrNull(s => s.IsStack && s.Targets[pcSlot]) is { } myStack && myStack.Count == 1 && myStack.Targets[playerSlot])
            return PlayerPriority.Irrelevant;

        return CurrentBaits.Any(s => s.Targets[playerSlot]) ? PlayerPriority.Interesting : PlayerPriority.Irrelevant;
    }
}
