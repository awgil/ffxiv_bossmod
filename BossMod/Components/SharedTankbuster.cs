namespace BossMod.Components;

// generic 'shared tankbuster' component; assumes only 1 concurrent cast is active
// TODO: revise and improve (track invuln, ai hints, num stacked tanks?)
public class GenericSharedTankbuster : CastCounter
{
    public AOEShape Shape { get; private init; }
    public bool OriginAtTarget { get; private init; }
    protected Actor? Source;
    protected Actor? Target;
    protected DateTime Activation;

    public bool Active => Source != null;

    public GenericSharedTankbuster(ActionID aid, AOEShape shape, bool originAtTarget = false) : base(aid)
    {
        Shape = shape;
        OriginAtTarget = originAtTarget;
    }

    // circle shapes typically have origin at target
    public GenericSharedTankbuster(ActionID aid, float radius) : this(aid, new AOEShapeCircle(radius), true) { }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        if (Target == null)
            return;
        if (Target == actor)
        {
            hints.Add("Stack with other tanks or press invuln!", !module.Raid.WithoutSlot().Any(a => a != actor && a.Role == Role.Tank && InAOE(a)));
        }
        else if (actor.Role == Role.Tank)
        {
            hints.Add("Stack with tank!", !InAOE(actor));
        }
        else
        {
            hints.Add("GTFO from tank!", InAOE(actor));
        }
    }

    public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Source != null && Target != null && Target != actor)
        {
            var shape = OriginAtTarget ? Shape.Distance(Target.Position, Target.Rotation) : Shape.Distance(Source.Position, Angle.FromDirection(Target.Position - Source.Position));
            if (actor.Role == Role.Tank)
                hints.AddForbiddenZone(p => -shape(p), Activation);
            else
                hints.AddForbiddenZone(shape, Activation);
        }
    }

    public override PlayerPriority CalcPriority(BossModule module, int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        return Target == player ? PlayerPriority.Interesting : PlayerPriority.Irrelevant;
    }

    public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        if (Source != null && Target != null)
        {
            if (OriginAtTarget)
                Shape.Outline(arena, Target);
            else
                Shape.Outline(arena, Source.Position, Angle.FromDirection(Target.Position - Source.Position));
        }
    }

    private bool InAOE(Actor actor) => Source != null && Target != null && (OriginAtTarget ? Shape.Check(actor.Position, Target) : Shape.Check(actor.Position, Source.Position, Angle.FromDirection(Target.Position - Source.Position)));
}

// shared tankbuster at cast target
public class CastSharedTankbuster : GenericSharedTankbuster
{
    public CastSharedTankbuster(ActionID aid, AOEShape shape, bool originAtTarget = false) : base(aid, shape, originAtTarget) { }
    public CastSharedTankbuster(ActionID aid, float radius) : base(aid, radius) { }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            Source = caster;
            Target = module.WorldState.Actors.Find(spell.TargetID);
            Activation = spell.NPCFinishAt;
        }
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if (caster == Source)
            Source = Target = null;
    }
}
