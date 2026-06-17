namespace BossMod.Components;

// generic 'shared tankbuster' component; assumes only 1 concurrent cast is active
// TODO: revise and improve (track invuln, ai hints, num stacked tanks?)
public class GenericSharedTankbuster(BossModule module, Enum? aid, AOEShape shape, bool originAtTarget = false) : CastCounter(module, aid)
{
    public AOEShape Shape { get; init; } = shape;
    public bool OriginAtTarget { get; init; } = originAtTarget;
    protected Actor? Source;
    protected Actor? Target;
    protected DateTime Activation;

    public bool Active => Source != null;

    // circle shapes typically have origin at target
    public GenericSharedTankbuster(BossModule module, Enum? aid, float radius) : this(module, aid, new AOEShapeCircle(radius), true) { }

    public static readonly uint[] InvulnStatuses = [
        82, // Hallowed Ground
        409, // Holmgang
        810, // Living Dead
        811, // Walking Dead
        1836, // Superbolide
        2564, // Lost EXcellence
    ];

    public static bool IsInvulnerableAt(Actor actor, DateTime time)
    {
        foreach (var status in actor.Statuses)
            if (InvulnStatuses.Contains(status.ID))
                return status.ExpireAt > time;

        return false;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Target == null)
            return;

        var targetInvuln = IsInvulnerableAt(Target, Activation);

        if (Target == actor)
        {
            if (!targetInvuln)
                hints.Add("Stack with other tanks!", !Raid.WithoutSlot().Any(a => a != actor && a.Role == Role.Tank && InAOE(a)));
        }
        else if (actor.Role == Role.Tank)
        {
            if (!targetInvuln)
                hints.Add("Stack with tank!", !InAOE(actor));
        }
        else
            hints.Add("GTFO from tank!", InAOE(actor));
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Source != null && Target != null && Target != actor)
        {
            var shape = OriginAtTarget ? Shape.CheckFn(Target.Position, Target.Rotation) : Shape.CheckFn(Source.Position, Angle.FromDirection(Target.Position - Source.Position));
            if (actor.Role == Role.Tank)
                hints.AddForbiddenZone(p => !shape(p), Activation);
            else
                hints.AddForbiddenZone(shape, Activation);
        }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor) => Target == player ? PlayerPriority.Interesting : PlayerPriority.Irrelevant;

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (Source != null && Target != null)
        {
            if (OriginAtTarget)
                Shape.Outline(Arena, Target);
            else
                Shape.Outline(Arena, Source.Position, Angle.FromDirection(Target.Position - Source.Position));
        }
    }

    private bool InAOE(Actor actor) => Source != null && Target != null && (OriginAtTarget ? Shape.Check(actor.Position, Target) : Shape.Check(actor.Position, Source.Position, Angle.FromDirection(Target.Position - Source.Position)));
}

// shared tankbuster at cast target
public class CastSharedTankbuster(BossModule module, Enum aid, AOEShape shape, bool originAtTarget = false) : GenericSharedTankbuster(module, aid, shape, originAtTarget)
{
    public CastSharedTankbuster(BossModule module, Enum aid, float radius) : this(module, aid, new AOEShapeCircle(radius), true) { }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            Source = caster;
            Target = WorldState.Actors.Find(spell.TargetID);
            Activation = Module.CastFinishAt(spell);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (caster == Source)
            Source = Target = null;
    }
}

// shared tankbuster at icon
public class IconSharedTankbuster(BossModule module, uint iconId, Enum aid, AOEShape shape, float activationDelay = 5.1f, bool originAtTarget = false) : GenericSharedTankbuster(module, aid, shape, originAtTarget)
{
    public IconSharedTankbuster(BossModule module, uint iconId, Enum aid, float radius, float activationDelay = 5.1f) : this(module, iconId, aid, new AOEShapeCircle(radius), activationDelay, true) { }

    public virtual Actor? BaitSource(Actor target) => Module.PrimaryActor;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == iconId)
        {
            Source = BaitSource(actor);
            Target = actor;
            Activation = WorldState.FutureTime(activationDelay);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (spell.Action == WatchedAction)
            Source = Target = null;
    }
}
