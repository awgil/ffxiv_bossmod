namespace BossMod.Components;

// generic 'stack/spread' mechanic has some players that have to spread away from raid, some other players that other players need to stack with
// there are various variants (e.g. everyone should spread, or everyone should stack in one or more groups, or some combination of that)
public class GenericStackSpread(BossModule module, bool alwaysShowSpreads = false, bool raidwideOnResolve = true, bool includeDeadTargets = false) : BossComponent(module)
{
    public struct Stack(Actor target, float radius, int minSize = 2, int maxSize = int.MaxValue, DateTime activation = default, BitMask forbiddenPlayers = default)
    {
        public Actor Target = target;
        public float Radius = radius;
        public int MinSize = minSize;
        public int MaxSize = maxSize;
        public DateTime Activation = activation;
        public BitMask ForbiddenPlayers = forbiddenPlayers; // raid members that aren't allowed to participate in the stack
    }

    public record struct Spread(
        Actor Target,
        float Radius,
        DateTime Activation = default
    );

    public bool AlwaysShowSpreads = alwaysShowSpreads; // if false, we only shown own spread radius for spread targets - this reduces visual clutter
    public bool RaidwideOnResolve = raidwideOnResolve; // if true, assume even if mechanic is correctly resolved everyone will still take damage
    public bool IncludeDeadTargets = includeDeadTargets; // if false, stacks & spreads with dead targets are ignored
    public List<Stack> Stacks = [];
    public List<Spread> Spreads = [];

    public bool Active => Stacks.Count + Spreads.Count > 0;
    public IEnumerable<Stack> ActiveStacks => IncludeDeadTargets ? Stacks : Stacks.Where(s => !s.Target.IsDead);
    public IEnumerable<Spread> ActiveSpreads => IncludeDeadTargets ? Spreads : Spreads.Where(s => !s.Target.IsDead);

    public bool IsStackTarget(Actor actor) => Stacks.Any(s => s.Target == actor);
    public bool IsSpreadTarget(Actor actor) => Spreads.Any(s => s.Target == actor);

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Spreads.FindIndex(s => s.Target == actor) is var iSpread && iSpread >= 0)
        {
            hints.Add("Spread!", Raid.WithoutSlot().InRadiusExcluding(actor, Spreads[iSpread].Radius).Any());
        }
        else if (Stacks.FindIndex(s => s.Target == actor) is var iStack && iStack >= 0)
        {
            var stack = Stacks[iStack];
            int numStacked = 1; // always stacked with self
            bool stackedWithOtherStackOrAvoid = false;
            foreach (var (j, other) in Raid.WithSlot().InRadiusExcluding(actor, stack.Radius))
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
                else if (Raid.WithoutSlot().InRadiusExcluding(s.Target, s.Radius).Count() + 1 < s.MinSize)
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

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        // forbid standing next to spread markers
        // TODO: think how to improve this, current implementation works, but isn't particularly good - e.g. nearby players tend to move to same spot, turn around, etc.
        // ideally we should provide per-mechanic spread spots, but for simple cases we should try to let melee spread close and healers/rdd spread far from main target...
        foreach (var spreadFrom in ActiveSpreads.Where(s => s.Target != actor))
            hints.AddForbiddenZone(ShapeDistance.Circle(spreadFrom.Target.Position, spreadFrom.Radius), spreadFrom.Activation);

        foreach (var avoid in ActiveStacks.Where(s => s.Target != actor && s.ForbiddenPlayers[slot]))
            hints.AddForbiddenZone(ShapeDistance.Circle(avoid.Target.Position, avoid.Radius), avoid.Activation);

        if (Stacks.FirstOrDefault(s => s.Target == actor) is var actorStack && actorStack.Target != null)
        {
            // forbid standing next to other stack markers
            foreach (var stackWith in ActiveStacks.Where(s => s.Target != actor))
                hints.AddForbiddenZone(ShapeDistance.Circle(stackWith.Target.Position, stackWith.Radius), stackWith.Activation);
            // and try to stack with closest non-stack/spread player
            var closest = Raid.WithoutSlot().Where(p => p != actor && !IsSpreadTarget(p) && !IsStackTarget(p)).Closest(actor.Position);
            if (closest != null)
                hints.AddForbiddenZone(ShapeDistance.InvertedCircle(closest.Position, actorStack.Radius * 0.5f), actorStack.Activation);
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
                damageMask.Set(Raid.FindSlot(s.Target.InstanceID));
                firstActivation = firstActivation < s.Activation ? firstActivation : s.Activation;
            }
            foreach (var s in ActiveStacks)
            {
                damageMask |= Raid.WithSlot().Mask() & ~s.ForbiddenPlayers; // assume everyone will take damage except forbidden players (so-so assumption really...)
                firstActivation = firstActivation < s.Activation ? firstActivation : s.Activation;
            }

            if (damageMask.Any())
                hints.PredictedDamage.Add((damageMask, firstActivation));
        }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
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

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (!AlwaysShowSpreads && Spreads.FindIndex(s => s.Target == pc) is var iSpread && iSpread >= 0)
        {
            // draw only own circle - no one should be inside, this automatically resolves mechanic for us
            Arena.AddCircle(pc.Position, Spreads[iSpread].Radius, ArenaColor.Danger);
        }
        else
        {
            // draw spread and stack circles
            foreach (var s in ActiveStacks)
            {
                if (Arena.Config.ShowOutlinesAndShadows)
                    Arena.AddCircle(s.Target.Position, s.Radius, 0xFF000000, 2);
                Arena.AddCircle(s.Target.Position, s.Radius, ArenaColor.Safe);
            }
            foreach (var s in ActiveSpreads)
            {
                if (Arena.Config.ShowOutlinesAndShadows)
                    Arena.AddCircle(s.Target.Position, s.Radius, 0xFF000000, 2);
                Arena.AddCircle(s.Target.Position, s.Radius, ArenaColor.Danger);
            }
        }
    }
}

// stack/spread with same properties for all stacks and all spreads (most common variant)
public class UniformStackSpread(BossModule module, float stackRadius, float spreadRadius, int minStackSize = 2, int maxStackSize = int.MaxValue, bool alwaysShowSpreads = false, bool raidwideOnResolve = true, bool includeDeadTargets = false)
    : GenericStackSpread(module, alwaysShowSpreads, raidwideOnResolve, includeDeadTargets)
{
    public float StackRadius = stackRadius;
    public float SpreadRadius = spreadRadius;
    public int MinStackSize = minStackSize;
    public int MaxStackSize = maxStackSize;

    public IEnumerable<Actor> ActiveStackTargets => ActiveStacks.Select(s => s.Target);
    public IEnumerable<Actor> ActiveSpreadTargets => ActiveSpreads.Select(s => s.Target);

    public void AddStack(Actor target, DateTime activation = default, BitMask forbiddenPlayers = default) => Stacks.Add(new(target, StackRadius, MinStackSize, MaxStackSize, activation, forbiddenPlayers));
    public void AddStacks(IEnumerable<Actor> targets, DateTime activation = default) => Stacks.AddRange(targets.Select(target => new Stack(target, StackRadius, MinStackSize, MaxStackSize, activation)));
    public void AddSpread(Actor target, DateTime activation = default) => Spreads.Add(new(target, SpreadRadius, activation));
    public void AddSpreads(IEnumerable<Actor> targets, DateTime activation = default) => Spreads.AddRange(targets.Select(target => new Spread(target, SpreadRadius, activation)));
}

// spread/stack mechanic that selects targets by casts
public class CastStackSpread(BossModule module, ActionID stackAID, ActionID spreadAID, float stackRadius, float spreadRadius, int minStackSize = 2, int maxStackSize = int.MaxValue, bool alwaysShowSpreads = false)
    : UniformStackSpread(module, stackRadius, spreadRadius, minStackSize, maxStackSize, alwaysShowSpreads)
{
    public ActionID StackAction { get; init; } = stackAID;
    public ActionID SpreadAction { get; init; } = spreadAID;
    public int NumFinishedStacks { get; protected set; }
    public int NumFinishedSpreads { get; protected set; }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == StackAction && WorldState.Actors.Find(spell.TargetID) is var stackTarget && stackTarget != null)
        {
            AddStack(stackTarget, Module.CastFinishAt(spell));
        }
        else if (spell.Action == SpreadAction && WorldState.Actors.Find(spell.TargetID) is var spreadTarget && spreadTarget != null)
        {
            AddSpread(spreadTarget, Module.CastFinishAt(spell));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
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
public class SpreadFromCastTargets(BossModule module, ActionID aid, float radius, bool drawAllSpreads = true) : CastStackSpread(module, default, aid, 0, radius, alwaysShowSpreads: drawAllSpreads);

// generic 'stack with targets of specific cast' mechanic
public class StackWithCastTargets(BossModule module, ActionID aid, float radius, int minStackSize = 2, int maxStackSize = int.MaxValue) : CastStackSpread(module, aid, default, radius, 0, minStackSize, maxStackSize);

// spread/stack mechanic that selects targets by icon and finishes by cast event
public class IconStackSpread(BossModule module, uint stackIcon, uint spreadIcon, ActionID stackAID, ActionID spreadAID, float stackRadius, float spreadRadius, float activationDelay, int minStackSize = 2, int maxStackSize = int.MaxValue, bool alwaysShowSpreads = false)
    : UniformStackSpread(module, stackRadius, spreadRadius, minStackSize, maxStackSize, alwaysShowSpreads)
{
    public uint StackIcon { get; init; } = stackIcon;
    public uint SpreadIcon { get; init; } = spreadIcon;
    public ActionID StackAction { get; init; } = stackAID;
    public ActionID SpreadAction { get; init; } = spreadAID;
    public float ActivationDelay { get; init; } = activationDelay;
    public int NumFinishedStacks { get; protected set; }
    public int NumFinishedSpreads { get; protected set; }

    public override void OnEventIcon(Actor actor, uint iconID)
    {
        if (iconID == StackIcon)
        {
            AddStack(actor, WorldState.FutureTime(ActivationDelay));
        }
        else if (iconID == SpreadIcon)
        {
            AddSpread(actor, WorldState.FutureTime(ActivationDelay));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == StackAction)
        {
            Stacks.RemoveAll(s => s.Target.InstanceID == spell.MainTargetID);
            ++NumFinishedStacks;
        }
        else if (spell.Action == SpreadAction)
        {
            Spreads.RemoveAll(s => s.Target.InstanceID == spell.MainTargetID);
            ++NumFinishedSpreads;
        }
    }
}

// generic 'spread from actors with specific icon' mechanic
public class SpreadFromIcon(BossModule module, uint icon, ActionID aid, float radius, float activationDelay, bool drawAllSpreads = true) : IconStackSpread(module, 0, icon, default, aid, 0, radius, activationDelay, alwaysShowSpreads: drawAllSpreads);

// generic 'stack with actors with specific icon' mechanic
public class StackWithIcon(BossModule module, uint icon, ActionID aid, float radius, float activationDelay, int minStackSize = 2, int maxStackSize = int.MaxValue) : IconStackSpread(module, icon, 0, aid, default, radius, 0, activationDelay, minStackSize, maxStackSize);
