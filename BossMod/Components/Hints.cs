namespace BossMod.Components;

abstract class GenericInvincible(BossModule module, string hint = "Attacking invincible target!", int priority = AIHints.Enemy.PriorityInvincible) : BossComponent(module)
{
    public bool EnableHints = true;

    protected abstract IEnumerable<Actor> ForbiddenTargets(int slot, Actor actor);

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (EnableHints && ForbiddenTargets(slot, actor).Any(a => a.InstanceID == actor.TargetID))
            hints.Add(hint);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var a in ForbiddenTargets(slot, actor))
            hints.SetPriority(a, priority);
    }
}

class InvincibleStatus(BossModule module, uint statusId, string hint = "Attacking invincible target!") : GenericInvincible(module, hint)
{
    protected readonly List<Actor> _actors = [];

    protected override IEnumerable<Actor> ForbiddenTargets(int slot, Actor actor) => _actors;

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == statusId)
            _actors.Add(actor);
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (status.ID == statusId)
            _actors.Remove(actor);
    }
}
