namespace BossMod.Components;

/// <summary>
/// Utility component for encounters where a stack/spread mechanic has predefined safe spots.
/// Through no fault of its own, the generic solver has no chance of saving the player when the entire party has spreads and is trying to dodge away from each other.
/// </summary>
public abstract class StrictStackSpread : GenericStackSpread
{
    protected readonly PartyRolesConfig.Assignment[] AssignmentsPerSlot;

    protected StrictStackSpread(BossModule module, bool alwaysShowSpreads = false, bool raidwideOnResolve = true, bool includeDeadTargets = false) : base(module, alwaysShowSpreads, raidwideOnResolve, includeDeadTargets)
    {
        _selfId = (ulong)GetHashCode();
        AssignmentsPerSlot = Service.Config.Get<PartyRolesConfig>().AssignmentsPerSlot(Raid);
        if (AssignmentsPerSlot.Length == 0)
            AssignmentsPerSlot = new PartyRolesConfig.Assignment[8];
    }

    private readonly ulong _selfId;

    // Target will be ignored if the return value is `default`.
    public virtual (WPos, float)? GetStackDestination(int order, Actor target) => null;
    public virtual (WPos, float)? GetSpreadDestination(int order, Actor target) => null;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!EnableHints)
            return;

        var hasDestination = false;

        foreach (var (ix, spread) in ActiveSpreads.Index())
        {
            if (RaidwideOnResolve)
                hints.AddPredictedDamage(Raid.WithSlot().WhereActor(a => a == spread.Target).Mask(), spread.Activation);

            if (GetSpreadDestination(ix, spread.Target) is not var (dest, destRadius))
                continue;

            if (spread.Target == actor)
            {
                hasDestination = true;
                if (destRadius == 0)
                    hints.AddForbiddenZone(ShapeDistance.PrecisePosition(dest, new(0, 1), Arena.Bounds.MapResolution, actor.Position, 0.1f), spread.Activation);
                else
                    hints.AddForbiddenZone(ShapeDistance.InvertedCircle(dest, destRadius), spread.Activation);

            }
            else
                hints.AddForbiddenZone(ShapeDistance.Circle(dest, spread.Radius + MathF.Max(destRadius, 0.1f)), spread.Activation, _selfId);
        }

        foreach (var (ix, stack) in ActiveStacks.Index())
        {
            if (GetStackDestination(ix, stack.Target) is not var (dest, destRadius))
            {
                if (RaidwideOnResolve)
                    hints.AddPredictedDamage(Raid.WithSlot().InRadius(stack.Target.Position, stack.Radius).Mask(), stack.Activation, AIHints.PredictedDamageType.Shared);
                continue;
            }

            if (RaidwideOnResolve)
                hints.AddPredictedDamage(Raid.WithSlot().InRadius(dest, stack.Radius).Mask(), stack.Activation, AIHints.PredictedDamageType.Shared);

            if (stack.Target == actor)
            {
                hasDestination = true;
                if (destRadius == 0)
                    hints.AddForbiddenZone(ShapeDistance.PrecisePosition(dest, new(0, 1), Arena.Bounds.MapResolution, actor.Position, 0.1f), stack.Activation);
                else
                    hints.AddForbiddenZone(ShapeDistance.InvertedCircle(dest, destRadius), stack.Activation);
            }
            else if (stack.ForbiddenPlayers[slot])
                hints.AddForbiddenZone(ShapeDistance.Circle(dest, stack.Radius + MathF.Max(destRadius, 0.1f)), stack.Activation, _selfId);
            else
                hints.AddForbiddenZone(ShapeDistance.InvertedCircle(dest, stack.Radius - MathF.Max(destRadius, 0.1f)), stack.Activation, _selfId);
        }

        // TODO: clean up, potentially causes a lot of wasted iterations
        if (hasDestination)
            hints.ForbiddenZones.RemoveAll(z => z.tag == _selfId);
    }
}

public abstract class StrictUniformStackSpread(BossModule module, float stackRadius, float spreadRadius, int minStackSize = 2, int maxStackSize = int.MaxValue, bool alwaysShowSpreads = false, bool raidwideOnResolve = true) : StrictStackSpread(module, alwaysShowSpreads, raidwideOnResolve)
{
    public float StackRadius = stackRadius;
    public float SpreadRadius = spreadRadius;
    public int MinStackSize = minStackSize;
    public int MaxStackSize = maxStackSize;

    public void AddStack(Actor target, DateTime activation = default, BitMask forbiddenPlayers = default) => Stacks.Add(new(target, StackRadius, MinStackSize, MaxStackSize, activation, forbiddenPlayers));
    public void AddStacks(IEnumerable<Actor> targets, DateTime activation = default) => Stacks.AddRange(targets.OrderBy(t => t.InstanceID).Select(target => new Stack(target, StackRadius, MinStackSize, MaxStackSize, activation)));
    public void AddSpread(Actor target, DateTime activation = default) => Spreads.Add(new(target, SpreadRadius, activation));
    public void AddSpreads(IEnumerable<Actor> targets, DateTime activation = default) => Spreads.AddRange(targets.OrderBy(t => t.InstanceID).Select(target => new Spread(target, SpreadRadius, activation)));
}

public abstract class StrictCastStackSpread(BossModule module, Enum? stackAID, Enum? spreadAID, float stackRadius, float spreadRadius, int minStackSize = 2, int maxStackSize = int.MaxValue, bool alwaysShowSpreads = false) : StrictUniformStackSpread(module, stackRadius, spreadRadius, minStackSize, maxStackSize, alwaysShowSpreads)
{
    public ActionID StackAction { get; init; } = ActionID.MakeSpell(stackAID);
    public ActionID SpreadAction { get; init; } = ActionID.MakeSpell(spreadAID);
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

public abstract class StrictSpreadFromCastTargets(BossModule module, Enum aid, float radius, bool drawAllSpreads = true) : StrictCastStackSpread(module, default, aid, 0, radius, alwaysShowSpreads: drawAllSpreads)
{
    public sealed override (WPos, float)? GetStackDestination(int order, Actor target) => null;
}
public abstract class StrictStackWithCastTargets(BossModule module, Enum aid, float radius, int minStackSize = 2, int maxStackSize = int.MaxValue) : StrictCastStackSpread(module, aid, default, radius, 0, minStackSize, maxStackSize)
{
    public sealed override (WPos, float)? GetSpreadDestination(int order, Actor target) => null;
}

public abstract class StrictIconStackSpread(BossModule module, uint stackIcon, uint spreadIcon, Enum? stackAID, Enum? spreadAID, float stackRadius, float spreadRadius, float activationDelay, int minStackSize = 2, int maxStackSize = int.MaxValue, bool alwaysShowSpreads = false) : StrictUniformStackSpread(module, stackRadius, spreadRadius, minStackSize, maxStackSize, alwaysShowSpreads)
{
    public uint StackIcon { get; init; } = stackIcon;
    public uint SpreadIcon { get; init; } = spreadIcon;
    public ActionID StackAction { get; init; } = ActionID.MakeSpell(stackAID);
    public ActionID SpreadAction { get; init; } = ActionID.MakeSpell(spreadAID);
    public float ActivationDelay { get; init; } = activationDelay;
    public int NumFinishedStacks { get; protected set; }
    public int NumFinishedSpreads { get; protected set; }

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

public abstract class StrictSpreadFromIcon(BossModule module, uint icon, Enum? aid, float radius, float activationDelay, bool drawAllSpreads = true) : StrictIconStackSpread(module, 0, icon, default, aid, 0, radius, activationDelay, alwaysShowSpreads: drawAllSpreads)
{
    public sealed override (WPos, float)? GetStackDestination(int order, Actor target) => default;
}
public abstract class StrictStackWithIcon(BossModule module, uint icon, Enum aid, float radius, float activationDelay, int minStackSize = 2, int maxStackSize = int.MaxValue) : StrictIconStackSpread(module, icon, 0, aid, default, radius, 0, activationDelay, minStackSize, maxStackSize)
{
    public sealed override (WPos, float)? GetSpreadDestination(int order, Actor target) => default;
}
