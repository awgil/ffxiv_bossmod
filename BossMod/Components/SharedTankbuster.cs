namespace BossMod.Components;

// generic 'shared tankbuster' component; assumes only 1 concurrent cast is active
// TODO: revise and improve (track invuln, ai hints, num stacked tanks?)
public class GenericSharedTankbuster(BossModule module, ActionID aid, AOEShape shape, bool originAtTarget = false) : CastCounter(module, aid)
{
    public AOEShape Shape { get; init; } = shape;
    public bool OriginAtTarget { get; init; } = originAtTarget;
    protected Actor? Source;
    protected Actor? Target;
    protected DateTime Activation;

    public bool Active => Source != null;

    // circle shapes typically have origin at target
    public GenericSharedTankbuster(BossModule module, ActionID aid, float radius) : this(module, aid, new AOEShapeCircle(radius), true) { }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Target == null)
            return;
        if (Target == actor)
        {
            hints.Add("Stack with other tanks or press invuln!", !Raid.WithoutSlot().Any(a => a != actor && a.Role == Role.Tank && InAOE(a)));
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

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
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
public class CastSharedTankbuster(BossModule module, ActionID aid, AOEShape shape, bool originAtTarget = false) : GenericSharedTankbuster(module, aid, shape, originAtTarget)
{
    public CastSharedTankbuster(BossModule module, ActionID aid, float radius) : this(module, aid, new AOEShapeCircle(radius), true) { }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            Source = caster;
            Target = WorldState.Actors.Find(spell.TargetID);
            Activation = spell.NPCFinishAt;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (caster == Source)
            Source = Target = null;
    }
}
