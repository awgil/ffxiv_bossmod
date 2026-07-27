namespace BossMod.Components;

// generic 'stack/spread' mechanic has some players that have to spread away from raid, some other players that other players need to stack with
// there are various variants (e.g. everyone should spread, or everyone should stack in one or more groups, or some combination of that)

[SkipLocalsInit]
public abstract class GenericStackSpread(BossModule module, bool raidwideOnResolve = true, bool includeDeadTargets = false) : BossComponent(module)
{
    public struct Stack(Actor target, float radius, int minSize = 2, int maxSize = int.MaxValue, DateTime activation = default, BitMask forbiddenPlayers = default)
    {
        public Actor Target = target;
        public float Radius = radius;
        public int MinSize = minSize;
        public int MaxSize = maxSize;
        public DateTime Activation = activation;
        public BitMask ForbiddenPlayers = forbiddenPlayers; // raid members that aren't allowed to participate in the stack

        public readonly int NumInside(BossModule module)
        {
            var count = 0;
            var party = module.Raid.WithSlot();
            var len = party.Length;
            var pos = Target.Position.Quantized();
            for (var i = 0; i < len; ++i)
            {
                ref var indexActor = ref party[i];
                if (!ForbiddenPlayers[indexActor.Item1] && indexActor.Item2.Position.InCircle(pos, Radius))
                {
                    ++count;
                }
            }
            return count;
        }

        public readonly bool IsInside(WPos pos) => pos.InCircle(Target.Position.Quantized(), Radius);
        public readonly bool IsInside(Actor actor) => IsInside(actor.Position);
    }

    public struct Spread(Actor target, float radius, DateTime activation = default)
    {
        public Actor Target = target;
        public float Radius = radius;
        public DateTime Activation = activation;
    }

    public readonly bool RaidwideOnResolve = raidwideOnResolve; // if true, assume even if mechanic is correctly resolved everyone will still take damage
    public readonly bool IncludeDeadTargets = includeDeadTargets; // if false, stacks & spreads with dead targets are ignored
    public int ExtraAISpreadThreshold = 1;
    public readonly List<Stack> Stacks = [];
    public List<Spread> Spreads = [];
    public const string StackHint = "Stack!";

    public bool Active => Stacks.Count + Spreads.Count > 0;
    public List<Stack> ActiveStacks
    {
        get
        {
            var count = Stacks.Count;
            if (count == 0 || IncludeDeadTargets)
            {
                return Stacks;
            }
            else
            {
                var stacks = CollectionsMarshal.AsSpan(Stacks);
                var activeStacks = new List<Stack>(count);
                for (var i = 0; i < count; ++i)
                {
                    ref var stack = ref stacks[i];
                    if (!stack.Target.IsDead)
                    {
                        activeStacks.Add(stack);
                    }
                }
                return activeStacks;
            }
        }
    }

    public List<Spread> ActiveSpreads
    {
        get
        {
            var count = Spreads.Count;
            if (count == 0 || IncludeDeadTargets)
            {
                return Spreads;
            }
            else
            {
                var activeSpreads = new List<Spread>(count);
                var spreads = CollectionsMarshal.AsSpan(Spreads);
                for (var i = 0; i < count; ++i)
                {
                    ref var spread = ref spreads[i];
                    if (!spread.Target.IsDead)
                    {
                        activeSpreads.Add(spread);
                    }
                }
                return activeSpreads;
            }
        }
    }

    public bool IsStackTarget(Actor? actor)
    {
        var count = Stacks.Count;
        var stacks = CollectionsMarshal.AsSpan(Stacks);
        for (var i = 0; i < count; ++i)
        {
            if (stacks[i].Target == actor)
            {
                return true;
            }
        }
        return false;
    }

    public bool IsSpreadTarget(Actor? actor)
    {
        var count = Spreads.Count;
        var spreads = CollectionsMarshal.AsSpan(Spreads);
        for (var i = 0; i < count; ++i)
        {
            if (spreads[i].Target == actor)
            {
                return true;
            }
        }
        return false;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var spreads = CollectionsMarshal.AsSpan(ActiveSpreads);
        var lenSpreads = spreads.Length;
        for (var i = 0; i < lenSpreads; ++i)
        {
            ref var s = ref spreads[i];
            var t = s.Target;
            if (t == actor)
            {
                var targetPos = t.Position.Quantized();
                var partyWOS = Raid.WithoutSlot();
                var lenPWOS = partyWOS.Length;
                var inDanger = false;

                for (var j = 0; j < lenPWOS; ++j)
                {
                    var p = partyWOS[j];
                    if (p == actor)
                    {
                        continue;
                    }
                    if (p.Position.InCircle(targetPos, s.Radius))
                    {
                        inDanger = true;
                        break;
                    }
                }
                hints.Add("Spread!", inDanger);
                goto done;
            }
        }

        var stacks = CollectionsMarshal.AsSpan(ActiveStacks);
        var lenStacks = stacks.Length;
        for (var i = 0; i < lenStacks; ++i)
        {
            ref var s = ref stacks[i];
            var t = s.Target;
            if (t == actor)
            {
                var partyWS = Raid.WithSlot();
                var lenPWS = partyWS.Length;
                var numStacked = 1; // always stacked with self
                var stackedWithOtherStackOrAvoid = false;
                var targetPos = t.Position.Quantized();

                for (var j = 0; j < lenPWS; ++j)
                {
                    ref var p = ref partyWS[j];
                    var a = p.Item2;
                    if (a == actor)
                    {
                        continue;
                    }
                    if (a.Position.InCircle(targetPos, s.Radius))
                    {
                        ++numStacked;
                        stackedWithOtherStackOrAvoid |= s.ForbiddenPlayers[p.Item1] || IsStackTarget(a);
                    }
                }
                hints.Add(StackHint, stackedWithOtherStackOrAvoid || numStacked < s.MinSize || numStacked > s.MaxSize);
                goto done;
            }
        }

        var numParticipatingStacks = 0;
        var numUnsatisfiedStacks = 0;
        var party = Raid.WithoutSlot();
        var lenP = party.Length;
        for (var i = 0; i < lenStacks; ++i)
        {
            ref var s = ref stacks[i];
            var t = s.Target;
            var targetPos = t.Position.Quantized();
            if (s.ForbiddenPlayers[slot])
            {
                continue;
            }

            // check if actor participates
            if (actor.Position.InCircle(targetPos, s.Radius))
            {
                ++numParticipatingStacks;
                continue;
            }

            // count other party members in radius (excluding target itself)
            var numInside = 1;  // start with actor
            for (var j = 0; j < lenP; ++j)
            {
                var p = party[j];

                if (p != t && p.Position.InCircle(targetPos, s.Radius))
                {
                    ++numInside;
                }
            }
            if (numInside < s.MinSize)
            {
                ++numUnsatisfiedStacks;
            }
        }
        if (numParticipatingStacks > 1)
        {
            hints.Add(StackHint);
        }
        else if (numParticipatingStacks == 1)
        {
            hints.Add(StackHint, false);
        }
        else if (numUnsatisfiedStacks > 0)
        {
            hints.Add(StackHint);
        }
        // else: don't show anything, all potential stacks are already satisfied without a player

    done:
        for (var i = 0; i < lenSpreads; ++i)
        {
            ref var s = ref spreads[i];
            var t = s.Target;
            if (t != actor && actor.Position.InCircle(t.Position.Quantized(), s.Radius))
            {
                hints.Add("GTFO from spreads!");
                return;
            }
        }

        stacks = CollectionsMarshal.AsSpan(ActiveStacks);
        lenStacks = stacks.Length;
        for (var i = 0; i < lenStacks; ++i)
        {
            ref var s = ref stacks[i];
            var t = s.Target;
            if (t != actor && s.ForbiddenPlayers[slot] && actor.Position.InCircle(t.Position.Quantized(), s.Radius))
            {
                hints.Add("GTFO from forbidden stacks!");
                return;
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        // forbid standing next to spread markers
        // TODO: think how to improve this, current implementation works, but isn't particularly good - e.g. nearby players tend to move to same spot, turn around, etc.
        // ideally we should provide per-mechanic spread spots, but for simple cases we should try to let melee spread close and healers/rdd spread far from main target...

        var spreads = CollectionsMarshal.AsSpan(ActiveSpreads);
        var stacks = CollectionsMarshal.AsSpan(ActiveStacks);
        var lenSpreads = spreads.Length;
        var lenStacks = stacks.Length;
        if (lenStacks == 0 && lenSpreads == 0) // nothing to do
        {
            return;
        }
        var isSpreadTarget = false;

        var partyWOS = Raid.WithSlot(includeDead: IncludeDeadTargets);
        var lenPWOS = partyWOS.Length;
        if (lenPWOS < 2) // no need to generate forbidden zones if there are no allies
        {
            goto skipFZs;
        }

        for (var i = 0; i < lenSpreads; ++i)
        {
            ref var s = ref spreads[i];
            var t = s.Target;
            if (t != actor)
            {
                hints.AddForbiddenZone(new SDCircle(t.Position.Quantized(), s.Radius + ExtraAISpreadThreshold), s.Activation);
            }
            else
            {
                isSpreadTarget = true;

                var radius = s.Radius;
                var act = s.Activation;
                for (var j = 0; j < lenPWOS; ++j)
                {
                    var p = partyWOS[j].Item2;

                    for (var k = 0; k < lenSpreads; ++k)
                    {
                        if (spreads[k].Target == p)
                        {
                            goto done; // no need to add avoid hints for players who are also spread targets
                        }
                    }

                    hints.AddForbiddenZone(new SDCircle(p.Position.Quantized(), radius + ExtraAISpreadThreshold), act);
                done:
                    ;
                }
            }
        }

        var isStackTarget = false;

        for (var i = 0; i < lenStacks; ++i)
        {
            ref var s = ref stacks[i];
            var t = s.Target;
            if (s.Target == actor)
            {
                isStackTarget = true;

                var stacksIFzTarget = new List<ShapeDistance>(lenPWOS - 1);
                var radius = s.Radius;

                for (var j = 0; j < lenPWOS; ++j) // if player got stackmarker we should try finding a good candidate to stack with
                {
                    ref var p = ref partyWOS[j];
                    var a = p.Item2;
                    if (t != a)
                    {
                        if (s.ForbiddenPlayers[p.Item1]) // party member is forbidden from stacking
                        {
                            continue;
                        }
                        for (var k = 0; k < lenSpreads; ++k)
                        {
                            if (spreads[k].Target == a)
                            {
                                goto skip; // player got a spread marker
                            }
                        }
                        for (var k = 0; k < lenStacks; ++k)
                        {
                            if (stacks[k].Target == a)
                            {
                                goto skip; // player got a stack marker and we don't want to stack stacks
                            }
                        }
                        // buddy is not target of stacks or spreads, so a good candidate
                        stacksIFzTarget.Add(new SDInvertedCircle(a.Position, radius * 0.5f));
                    skip:
                        ;
                    }
                }
                if (stacksIFzTarget.Count > 0)
                {
                    hints.AddForbiddenZone(new SDIntersection([.. stacksIFzTarget]), s.Activation);
                }
            }
        }

        var stacksIFz = new List<ShapeDistance>();
        for (var i = 0; i < lenStacks; ++i)
        {
            ref var s = ref stacks[i];
            var t = s.Target;
            var targetPos = t.Position.Quantized();
            var act = s.Activation;
            var radius = s.Radius;
            if (s.Target != actor)
            {
                if (s.ForbiddenPlayers[slot])
                {
                    goto addfz;
                }
                var numInside = s.NumInside(Module);
                var isInside = s.IsInside(actor);
                var max = s.MaxSize;
                if (!isSpreadTarget && (!isInside && numInside < max || isInside && numInside <= max))  // don't try to stack if spread target
                {
                    stacksIFz.Add(new SDInvertedCircle(targetPos, radius));
                    continue;
                }
            addfz:
                // avoid stack if forbidden or enough players inside
                // double radius if stack target to prevent standing next to other stack markers or overlapping them
                hints.AddForbiddenZone(new SDCircle(targetPos, !isStackTarget ? radius : 2f * radius), act);
            }
        }

        var countIFz = stacksIFz.Count;
        if (countIFz > 0)
        {
            var act = stacks[0].Activation;
            if (countIFz == 1)
            {
                hints.AddForbiddenZone(stacksIFz[0], act);
            }
            else
            {
                hints.AddForbiddenZone(new SDOutsideOfUnion([.. stacksIFz]), act);
            }
        }
    skipFZs:
        if (RaidwideOnResolve)
        {
            var firstActivation = DateTime.MaxValue;

            var countSpread = ActiveSpreads.Count;
            var countStack = ActiveStacks.Count;
            if (countSpread != 0)
            {
                BitMask spreadMask = default;

                for (var i = 0; i < countSpread; ++i)
                {
                    ref var s = ref spreads[i];
                    spreadMask.Set(Raid.FindSlot(s.Target.InstanceID));
                    firstActivation = firstActivation < s.Activation ? firstActivation : s.Activation;
                }
                if (spreadMask != default)
                {
                    hints.AddPredictedDamage(spreadMask, firstActivation, AIHints.PredictedDamageType.Raidwide);
                }
            }
            if (countStack != 0)
            {
                BitMask stackMask = default;
                BitMask mask = default;
                mask.Raw = 0xFFFFFFFFFFFFFFFF;

                for (var i = 0; i < countStack; ++i)
                {
                    ref var s = ref stacks[i];
                    stackMask |= mask & ~s.ForbiddenPlayers; // assume everyone will take damage except forbidden players (so-so assumption really...)
                    firstActivation = firstActivation < s.Activation ? firstActivation : s.Activation;
                }
                if (stackMask != mask)
                {
                    hints.AddPredictedDamage(stackMask, firstActivation, AIHints.PredictedDamageType.Shared);
                }
            }
        }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        var shouldSpread = IsSpreadTarget(player);
        var shouldStack = IsStackTarget(player);
        var shouldAvoid = false;
        if (!shouldSpread && !shouldStack)
        {
            var stacks = CollectionsMarshal.AsSpan(Stacks);
            var lenStacks = stacks.Length;
            for (var i = 0; i < lenStacks; ++i)
            {
                ref var s = ref stacks[i];
                if (!IncludeDeadTargets && s.Target.IsDead) // player is dead and dead players are excluded
                {
                    continue;
                }
                if (s.ForbiddenPlayers[playerSlot])
                {
                    shouldAvoid = true;
                    break;
                }
            }
        }
        if (shouldAvoid)
        {
            customColor = Colors.Vulnerable;
        }
        return shouldAvoid || shouldSpread ? PlayerPriority.Danger
            : shouldStack ? PlayerPriority.Interesting
            : Active ? PlayerPriority.Normal : PlayerPriority.Irrelevant;
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var stacks = CollectionsMarshal.AsSpan(Stacks);
        var lenStacks = stacks.Length;
        for (var i = 0; i < lenStacks; ++i)
        {
            ref var s = ref stacks[i];
            var t = s.Target;
            if (!IncludeDeadTargets && t.IsDead) // player is dead and dead players are excluded
            {
                continue;
            }
            var dangerColor = false;
            if (s.ForbiddenPlayers[pcSlot])  // player is forbidden, always draw as danger
            {
                dangerColor = true;
                goto done;
            }
            if (t == pc) // player is target, always draw as safe
            {
                goto done;
            }
            else
            {
                var numInside = s.NumInside(Module);
                var isInside = s.IsInside(pc);
                var max = s.MaxSize;
                dangerColor = !isInside && numInside >= max || isInside && numInside > max || IsStackTarget(pc) || IsSpreadTarget(pc);
            }
        done:
            Arena.AddCircle(t.Position.Quantized(), s.Radius, dangerColor ? default : Colors.Safe);
        }

        var spreads = CollectionsMarshal.AsSpan(Spreads);
        var lenSpread = spreads.Length;
        for (var i = 0; i < lenSpread; ++i)
        {
            ref var s = ref spreads[i];
            var t = s.Target;
            if (!IncludeDeadTargets && t.IsDead) // player is dead and dead players are excluded
            {
                continue;
            }
            Arena.AddCircle(t.Position.Quantized(), s.Radius);
        }
    }
}

// stack/spread with same properties for all stacks and all spreads (most common variant)
[SkipLocalsInit]
public abstract class UniformStackSpread(BossModule module, float stackRadius, float spreadRadius, int minStackSize = 2, int maxStackSize = int.MaxValue, bool raidwideOnResolve = true, bool includeDeadTargets = false)
    : GenericStackSpread(module, raidwideOnResolve, includeDeadTargets)
{
    public float StackRadius = stackRadius;
    public float SpreadRadius = spreadRadius;
    public int MinStackSize = minStackSize;
    public int MaxStackSize = maxStackSize;

    public void AddStack(Actor target, DateTime activation = default, BitMask forbiddenPlayers = default) => Stacks.Add(new(target, StackRadius, MinStackSize, MaxStackSize, activation, forbiddenPlayers));
    public void AddStacks(IEnumerable<Actor> targets, DateTime activation = default)
    {
        foreach (var target in targets)
        {
            Stacks.Add(new(target, StackRadius, MinStackSize, MaxStackSize, activation));
        }
    }
    public void AddSpread(Actor target, DateTime activation = default) => Spreads.Add(new(target, SpreadRadius, activation));
    public void AddSpreads(IEnumerable<Actor> targets, DateTime activation = default)
    {
        foreach (var target in targets)
        {
            Spreads.Add(new(target, SpreadRadius, activation));
        }
    }
}

// spread/stack mechanic that selects targets by casts
[SkipLocalsInit]
public class CastStackSpread(BossModule module, uint stackAID, uint spreadAID, float stackRadius, float spreadRadius, int minStackSize = 2, int maxStackSize = int.MaxValue, bool alwaysShowSpreads = false)
    : UniformStackSpread(module, stackRadius, spreadRadius, minStackSize, maxStackSize, alwaysShowSpreads)
{
    public readonly uint StackAction = stackAID;
    public readonly uint SpreadAction = spreadAID;
    public int NumFinishedStacks;
    public int NumFinishedSpreads;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var id = spell.Action.ID;
        if (id == StackAction && WorldState.Actors.Find(spell.TargetID) is Actor stackTarget)
        {
            AddStack(stackTarget, Module.CastFinishAt(spell));
        }
        else if (id == SpreadAction && WorldState.Actors.Find(spell.TargetID) is Actor spreadTarget)
        {
            AddSpread(spreadTarget, Module.CastFinishAt(spell));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        var aid = spell.Action.ID;
        if (aid == StackAction)
        {
            var count = Stacks.Count;
            var id = spell.TargetID;
            var stacks = CollectionsMarshal.AsSpan(Stacks);
            for (var i = 0; i < count; ++i)
            {
                if (stacks[i].Target.InstanceID == id)
                {
                    ++NumFinishedStacks;
                    Stacks.RemoveAt(i);
                    return;
                }
            }
        }
        else if (aid == SpreadAction)
        {
            var count = Spreads.Count;
            var id = spell.TargetID;
            var spreads = CollectionsMarshal.AsSpan(Spreads);
            for (var i = 0; i < count; ++i)
            {
                if (spreads[i].Target.InstanceID == id)
                {
                    ++NumFinishedSpreads;
                    Spreads.RemoveAt(i);
                    return;
                }
            }
        }
    }
}

// generic 'spread from targets of specific cast' mechanic
[SkipLocalsInit]
public class SpreadFromCastTargets(BossModule module, uint aid, float radius) : CastStackSpread(module, default, aid, default, radius);

// generic 'stack with targets of specific cast' mechanic
[SkipLocalsInit]
public class StackWithCastTargets(BossModule module, uint aid, float radius, int minStackSize = 2, int maxStackSize = int.MaxValue) : CastStackSpread(module, aid, default, radius, default, minStackSize, maxStackSize);

// spread/stack mechanic that selects targets by icon and finishes by cast event
[SkipLocalsInit]
public class IconStackSpread(BossModule module, uint stackIcon, uint spreadIcon, uint stackAID, uint spreadAID, float stackRadius, float spreadRadius, double activationDelay, int minStackSize = 2, int maxStackSize = int.MaxValue, int maxCasts = 1)
    : UniformStackSpread(module, stackRadius, spreadRadius, minStackSize, maxStackSize)
{
    public readonly uint StackIcon = stackIcon;
    public readonly uint SpreadIcon = spreadIcon;
    public readonly uint StackAction = stackAID;
    public readonly uint SpreadAction = spreadAID;
    public readonly double ActivationDelay = activationDelay;
    public int NumFinishedStacks;
    public int NumFinishedSpreads;
    public readonly int MaxCasts = maxCasts; // for stacks where the final AID hits multiple times
    public int CastCounter;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
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
        var aid = spell.Action.ID;
        if (aid == StackAction)
        {
            var id = spell.MainTargetID;
            if (MaxCasts != 1 && Stacks.Count == 1 && Stacks.Ref(0).Target.InstanceID != id) // multi hit stack target died and new target got selected
            {
                Stacks.Ref(0).Target = WorldState.Actors.Find(id)!;
            }
            if (++CastCounter == MaxCasts)
            {
                var count = Stacks.Count;
                var stacks = CollectionsMarshal.AsSpan(Stacks);
                for (var i = 0; i < count; ++i)
                {
                    if (stacks[i].Target.InstanceID == id)
                    {
                        ++NumFinishedStacks;
                        CastCounter = 0;
                        Stacks.RemoveAt(i);
                        return;
                    }
                }
                // stack not found, probably due to being self targeted
                if (count != 0)
                {
                    ++NumFinishedStacks;
                    CastCounter = 0;
                    Stacks.RemoveAt(0);
                }
            }
        }
        else if (aid == SpreadAction)
        {
            var count = Spreads.Count;
            var id = spell.MainTargetID;
            var spreads = CollectionsMarshal.AsSpan(Spreads);
            for (var i = 0; i < count; ++i)
            {
                if (spreads[i].Target.InstanceID == id)
                {
                    Spreads.RemoveAt(i);
                    ++NumFinishedSpreads;
                    return;
                }
            }
            // spread not found, probably due to being self targeted
            if (count != 0)
            {
                ++NumFinishedSpreads;
                Spreads.RemoveAt(0);
            }
        }
    }

    public override void Update()
    {
        var count = Spreads.Count - 1;
        for (var i = count; i >= 0; --i)
        {
            if (Spreads.Ref(i).Target.IsDead)
            {
                Spreads.RemoveAt(i);
            }
        }
    }
}

// generic 'spread from actors with specific icon' mechanic
[SkipLocalsInit]
public class SpreadFromIcon(BossModule module, uint icon, uint aid, float radius, double activationDelay) :
IconStackSpread(module, default, icon, default, aid, default, radius, activationDelay);

// generic 'stack with actors with specific icon' mechanic
[SkipLocalsInit]
public class StackWithIcon(BossModule module, uint icon, uint aid, float radius, double activationDelay, int minStackSize = 2, int maxStackSize = int.MaxValue, int maxCasts = 1) :
IconStackSpread(module, icon, default, aid, default, radius, default, activationDelay, minStackSize, maxStackSize, maxCasts);

// generic 'donut stack' mechanic
[SkipLocalsInit]
public class DonutStack(BossModule module, uint aid, uint icon, float innerRadius, float outerRadius, double activationDelay, int minStackSize = 2, int maxStackSize = int.MaxValue) :
UniformStackSpread(module, innerRadius / 3f, default, minStackSize, maxStackSize)
{
    // this is a donut targeted on each player, it is best solved by stacking
    // regular stack component won't work because this is self targeted
    public readonly AOEShapeDonut Donut = new(innerRadius, outerRadius);
    public readonly double ActivationDelay = activationDelay;
    public readonly uint Icon = icon;
    public readonly uint Aid = aid;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == Icon)
        {
            AddStack(actor, WorldState.FutureTime(ActivationDelay));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == Aid)
        {
            var count = Stacks.Count;
            var t = spell.MainTargetID;
            var stacks = CollectionsMarshal.AsSpan(Stacks);
            for (var i = 0; i < count; ++i)
            {
                if (stacks[i].Target.InstanceID == t)
                {
                    Stacks.RemoveAt(i);
                    return;
                }
            }
            Stacks.Clear(); // stack was not found, just clear all - this can happen if donut stacks is self targeted instead of player targeted
        }
    }

    public override void Update()
    {
        var count = Stacks.Count - 1;
        for (var i = count; i >= 0; --i)
        {
            if (Stacks.Ref(i).Target.IsDead)
            {
                Stacks.RemoveAt(i);
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = Stacks.Count;
        if (count == 0)
        {
            return;
        }
        var forbidden = new List<ShapeDistance>(count);
        var radius = Donut.InnerRadius * 0.25f;
        var stacks = CollectionsMarshal.AsSpan(Stacks);
        for (var i = 0; i < count; ++i)
        {
            var s = stacks[i];
            if (s.Target == actor)
            {
                continue;
            }
            forbidden.Add(new SDInvertedCircle(s.Target.Position, radius));
        }
        if (forbidden.Count != 0)
        {
            hints.AddForbiddenZone(new SDIntersection([.. forbidden]), Stacks.Ref(0).Activation);
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        var count = Stacks.Count;
        for (var i = 0; i < count; ++i)
        {
            Donut.Draw(Arena, Stacks[i].Target.Position);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc) { }
}

[SkipLocalsInit]
public abstract class GenericBaitStack(BossModule module, uint aid = default, bool onlyShowOutlines = false) : GenericBaitAway(module, aid)
{
    // TODO: add logic for min and max stack size
    public const string HintStack = "Stack!";
    public const string HintAvoidOther = "GTFO from other stacks!";
    public const string HintAvoid = "GTFO from stack!";

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var baits = CollectionsMarshal.AsSpan(ActiveBaits);
        var len = baits.Length;
        if (len == 0)
        {
            return;
        }
        var isBaitTarget = false; // determine if target of any stack
        for (var i = 0; i < len; ++i)
        {
            if (baits[i].Target == actor)
            {
                isBaitTarget = true;
                break;
            }
        }
        var forbiddenInverted = new List<ShapeDistance>();

        for (var i = 0; i < len; ++i)
        {
            ref var b = ref baits[i];
            var origin = BaitOrigin(ref b);
            var angle = Angle.FromDirection(b.Target.Position - origin);
            var t = b.Target;
            if (t != actor && !isBaitTarget)
            {
                if (!b.Forbidden[slot])
                {
                    forbiddenInverted.Add(b.Shape.InvertedDistance(origin, angle));
                }
                else
                {
                    hints.AddForbiddenZone(b.Shape.Distance(origin, angle), b.Activation);
                }
            }
            else if (t != actor && isBaitTarget)
            {   // prevent overlapping if there are multiple stacks
                if (b.Shape is AOEShapeCone cone)
                {
                    hints.AddForbiddenZone(new SDCone(origin, cone.Radius, angle, cone.HalfAngle * 2f), b.Activation);
                }
                else if (b.Shape is AOEShapeRect rect)
                {
                    hints.AddForbiddenZone(new SDRect(origin, angle, rect.LengthFront, rect.LengthBack, rect.HalfWidth * 2f), b.Activation);
                }
                else if (b.Shape is AOEShapeCircle circle)
                {
                    forbiddenInverted.Add(new SDCircle(origin, circle.Radius * 2f));
                }
            }
            else if (t == actor) // try to go to party members since they might not actively come to your stack
            {
                var stacks = CollectionsMarshal.AsSpan(CurrentBaits);
                var lenStacks = stacks.Length;
                var forbiddenB = new List<ShapeDistance>();
                var partyWOS = Raid.WithSlot();
                var lenPWOS = partyWOS.Length;
                for (var k = 0; k < lenPWOS; ++k) // if player got stackmarker we should try finding a good candidate to stack with
                {
                    ref var p = ref partyWOS[k];
                    var a = p.Item2;
                    if (t != a)
                    {
                        if (b.Forbidden[p.Item1]) // party member is forbidden from stacking
                        {
                            continue;
                        }
                        for (var l = 0; l < lenStacks; ++l)
                        {
                            if (stacks[l].Target == a)
                            {
                                goto skip; // player got a stack marker and we don't want to stack stacks
                            }
                        }
                        // buddy is not target of stacks or spreads, so a good candidate
                        forbiddenB.Add(new SDInvertedCircle(a.Position, 2f));
                    }
                skip:
                    ;
                }
                if (forbiddenB.Count != 0)
                {
                    hints.AddForbiddenZone(new SDIntersection([.. forbiddenB]), b.Activation);
                }
            }
        }
        var countI = forbiddenInverted.Count;
        if (countI != 0)
        {
            var act = baits[0].Activation;
            if (countI > 1)
            {
                hints.AddForbiddenZone(new SDOutsideOfUnion([.. forbiddenInverted]), act);
            }
            else
            {
                hints.AddForbiddenZone(forbiddenInverted[0], act);
            }
        }

        var firstActivation = DateTime.MaxValue;
        BitMask baitMask = default;
        BitMask mask = default;
        mask.Raw = 0xFFFFFFFFFFFFFFFF;
        for (var i = 0; i < len; ++i)
        {
            ref var b = ref baits[i];
            baitMask |= mask & ~b.Forbidden; // assume everyone will take damage except forbidden players (so-so assumption really...)
            firstActivation = firstActivation < b.Activation ? firstActivation : b.Activation;
        }
        if (baitMask != default)
        {
            hints.AddPredictedDamage(baitMask, firstActivation, AIHints.PredictedDamageType.Shared);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var baits = CollectionsMarshal.AsSpan(ActiveBaits);
        var len = baits.Length;
        if (len == 0)
        {
            return;
        }
        var isBaitTarget = false; // determine if target of any stack
        var isInBaitShape = false; // determine if inside of any stack
        var isInWrongBait = false; // determine if inside of any forbidden stack
        var allForbidden = true; // determine if all stacks are forbidden
        var id = actor.InstanceID;
        for (var i = 0; i < len; ++i)
        {
            ref var b = ref baits[i];
            if (!b.Forbidden[slot])
            {
                allForbidden = false;
            }
            if (b.Target.InstanceID == id)
            {
                isBaitTarget = true;
                continue;
            }
            if (b.Shape.Check(actor.Position, BaitOrigin(ref b), b.Rotation))
            {
                isInBaitShape = true;
                if (b.Forbidden[slot])
                {
                    isInWrongBait = true;
                }
            }
        }
        var isBaitTargetAndInExtraStack = false;
        if (isBaitTarget)
        {
            for (var i = 0; i < len; ++i)
            {
                ref var b = ref baits[i];
                if (b.Target.InstanceID == id)
                {
                    continue;
                }
                if (b.Shape.Check(actor.Position, BaitOrigin(ref b), b.Rotation))
                {
                    isBaitTargetAndInExtraStack = true;
                    break;
                }
            }
        }
        if (isInWrongBait || allForbidden)
        {
            hints.Add(HintAvoid, isInWrongBait);
            return;
        }
        if (!isBaitTarget && !isInBaitShape)
        {
            hints.Add(HintStack);
        }
        else if (isBaitTarget || !isBaitTarget && isInBaitShape)
        {
            hints.Add(HintStack, false);
        }
        if (isBaitTarget && isBaitTargetAndInExtraStack)
        {
            hints.Add(HintAvoidOther);
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (onlyShowOutlines)
        {
            return;
        }
        var baits = CollectionsMarshal.AsSpan(ActiveBaits);
        var len = baits.Length;
        if (len == 0)
        {
            return;
        }
        var isBaitTarget = false; // determine if target of any stack
        var id = pc.InstanceID;
        for (var i = 0; i < len; ++i)
        {
            if (baits[i].Target.InstanceID == id)
            {
                isBaitTarget = true;
                break;
            }
        }
        for (var i = 0; i < len; ++i)
        {
            ref var b = ref baits[i];
            var color = !b.Forbidden[pcSlot] && (isBaitTarget && b.Target.InstanceID == id || !isBaitTarget && b.Target.InstanceID != id) ? Colors.SafeFromAOE : default;
            b.Shape.Draw(Arena, BaitOrigin(ref b), b.Rotation, color);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (!onlyShowOutlines)
        {
            return;
        }
        var baits = CollectionsMarshal.AsSpan(ActiveBaits);
        var len = baits.Length;
        if (len == 0)
        {
            return;
        }
        var isBaitTarget = false; // determine if target of any stack
        var id = pc.InstanceID;
        for (var i = 0; i < len; ++i)
        {
            if (baits[i].Target.InstanceID == id)
            {
                isBaitTarget = true;
                break;
            }
        }
        for (var i = 0; i < len; ++i)
        {
            ref var b = ref baits[i];
            var color = !b.Forbidden[pcSlot] && (isBaitTarget && b.Target.InstanceID == id || !isBaitTarget && b.Target.InstanceID != id) ? Colors.Safe : default;
            b.Shape.Outline(Arena, BaitOrigin(ref b), b.Rotation, color);
        }
    }
}

// generic single hit "line stack" component, usually do not have an iconID, instead players get marked by cast event
// usually these have 50 range and 4 halfWidth, but it can be modified
[SkipLocalsInit]
public class LineStack(BossModule module, uint aidMarker, uint aidResolve, double activationDelay = 5.1d, float range = 50f, float halfWidth = 4f, int minStackSize = 4, int maxStackSize = int.MaxValue, int maxCasts = 1, bool markerIsFinalTarget = true, uint iconID = default) : GenericBaitStack(module)
{
    public LineStack(BossModule module, uint iconID, uint aidResolve, double activationDelay = 5.1d, float range = 50f, float halfWidth = 4f, int minStackSize = 4, int maxStackSize = int.MaxValue, int maxCasts = 1, bool markerIsFinalTarget = true) : this(module, default, aidResolve, activationDelay, range, halfWidth, minStackSize, maxStackSize, maxCasts, markerIsFinalTarget, iconID) { }

    // TODO: add logic for min and max stack size
    public readonly uint AidMarker = aidMarker;
    public readonly uint AidResolve = aidResolve;
    public readonly double ActionDelay = activationDelay;
    public readonly float Range = range;
    public readonly float HalfWidth = halfWidth;
    public readonly int MaxStackSize = maxStackSize;
    public readonly int MinStackSize = minStackSize;
    public readonly int MaxCasts = maxCasts; // for stacks where the final AID hits multiple times
    public readonly bool MarkerIsFinalTarget = markerIsFinalTarget; // rarely the marked player is not the target of the line stack
    private int castCounter;
    public readonly uint IconId = iconID;
    private readonly AOEShape rect = new AOEShapeRect(range, halfWidth);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (AidMarker == default && IconId == default)
        {
            return;
        }
        var id = spell.Action.ID;
        if (id == AidMarker && WorldState.Actors.Find(spell.MainTargetID) is Actor target)
        {
            CurrentBaits.Add(new(caster, target, rect, WorldState.FutureTime(ActionDelay), maxCasts: MaxCasts));
        }
        else if (id == AidResolve)
        {
            if (MarkerIsFinalTarget)
            {
                var tID = spell.MainTargetID;
                if (CurrentBaits.Count == 1 && CurrentBaits.Ref(0).Target.InstanceID != tID && WorldState.Actors.Find(tID) is Actor t)
                {
                    CurrentBaits.Ref(0).Target = t;
                }

                var count = CurrentBaits.Count;
                var baits = CollectionsMarshal.AsSpan(CurrentBaits);
                for (var i = 0; i < count; ++i)
                {
                    ref var b = ref baits[i];
                    if (b.Target.InstanceID == tID && --b.MaxCasts == 0)
                    {
                        CurrentBaits.RemoveAt(i);
                        ++NumCasts;
                        return;
                    }
                }
            }
            else
            {
                if (++castCounter >= MaxCasts && CurrentBaits.Count != 0)
                {
                    CurrentBaits.RemoveAt(0);
                    castCounter -= MaxCasts;
                    ++NumCasts;
                }
            }
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (IconId != default && iconID == IconId && WorldState.Actors.Find(targetID) is Actor target)
        {
            CurrentBaits.Add(new(actor, target, rect, WorldState.FutureTime(ActionDelay)));
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (AidMarker != default || IconId != default)
        {
            return;
        }
        if (spell.Action.ID == AidResolve && WorldState.Actors.Find(spell.TargetID) is Actor target)
        {
            CurrentBaits.Add(new(caster, target, rect, Module.CastFinishAt(spell)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (AidMarker != default || IconId != default)
        {
            return;
        }
        if (spell.Action.ID == AidResolve)
        {
            var tID = spell.TargetID;
            if (MarkerIsFinalTarget)
            {
                if (CurrentBaits.Count == 1 && CurrentBaits[0].Target.InstanceID != tID && WorldState.Actors.Find(tID) is Actor t)
                {
                    CurrentBaits.Ref(0).Target = t;
                }
                if (++castCounter == MaxCasts)
                {
                    var count = CurrentBaits.Count;
                    var baits = CollectionsMarshal.AsSpan(CurrentBaits);
                    for (var i = 0; i < count; ++i)
                    {
                        if (baits[i].Target.InstanceID == tID)
                        {
                            CurrentBaits.RemoveAt(i);
                            castCounter = 0;
                            ++NumCasts;
                            return;
                        }
                    }
                }
            }
            else
            {
                if (++castCounter == MaxCasts && CurrentBaits.Count != 0)
                {
                    CurrentBaits.RemoveAt(0);
                    castCounter = 0;
                    ++NumCasts;
                }
            }
        }
    }
}

//Generic StackSpread implementation for Status-driven ones that fire on status expiry
public class StatusStackSpread(BossModule module, uint stackSid, uint spreadSid, float stackRadius, float spreadRadius, int minStackSize = 2, int maxStackSize = 2147483647, bool raidwideOnResolve = true, bool includeDeadTargets = false) : UniformStackSpread(module, stackRadius, spreadRadius, minStackSize, maxStackSize, raidwideOnResolve, includeDeadTargets)
{
    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == stackSid)
        {
            AddStack(actor, status.ExpireAt);
        }
        if (status.ID == spreadSid)
        {
            AddSpread(actor, status.ExpireAt);
        }
    }
    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID == stackSid)
        {
            Stacks.Clear();
        }
        if (status.ID == spreadSid)
        {
            Spreads.Clear();
        }
    }
}
