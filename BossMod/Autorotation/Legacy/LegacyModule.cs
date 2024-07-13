namespace BossMod.Autorotation.Legacy;

public abstract class LegacyModule(RotationModuleManager manager, Actor player) : RotationModule(manager, player)
{
    protected void PushResult(ActionID action, Actor? target)
    {
        var data = action ? ActionDefinitions.Instance[action] : null;
        if (data == null)
            return;
        if (data.Range == 0)
            target = Player; // override range-0 actions to always target player
        if (target == null || Hints.ForbiddenTargets.FirstOrDefault(e => e.Actor == target)?.Priority == AIHints.Enemy.PriorityForbidFully)
            return; // forbidden
        Hints.ActionsToExecute.Push(action, target, (data.IsGCD ? ActionQueue.Priority.High : ActionQueue.Priority.Low) + 500);
    }
    protected void PushResult<AID>(AID aid, Actor? target) where AID : Enum => PushResult(ActionID.MakeSpell(aid), target);

    protected (Actor? Target, P Priority) FindBetterTargetBy<P>(Actor? initial, float maxDistanceFromPlayer, Func<Actor, P> prioFunc, Func<AIHints.Enemy, bool>? filterFunc = null) where P : struct, IComparable
    {
        var bestTarget = initial;
        var bestPrio = initial != null ? prioFunc(initial) : default;
        foreach (var enemy in Hints.PriorityTargets.Where(x =>
            x.Actor != initial &&
            x.Actor.Position.InCircle(Player.Position, maxDistanceFromPlayer + x.Actor.HitboxRadius)
            && (filterFunc == null || filterFunc(x))
        ))
        {
            var newPrio = prioFunc(enemy.Actor);
            if (newPrio.CompareTo(bestPrio) > 0)
            {
                bestPrio = newPrio;
                bestTarget = enemy.Actor;
            }
        }
        return (bestTarget, bestPrio);
    }
}
