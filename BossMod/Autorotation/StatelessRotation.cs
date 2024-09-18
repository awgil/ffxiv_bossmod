namespace BossMod.Autorotation;
public abstract class StatelessRotation(WorldState ws, float effectiveRange)
{
    protected AIHints Hints = null!;
    protected Actor Player = null!;
    protected WorldState World => ws;
    private float _maxCast;
    protected uint MP;

    protected Roleplay.AID ComboAction => (Roleplay.AID)World.Client.ComboState.Action;

    protected abstract void Exec(Actor? primaryTarget);

    public void Execute(Actor player, AIHints hints, float maxCastTime)
    {
        _maxCast = maxCastTime;
        Hints = hints;
        Player = player;

        MP = (uint)Math.Max(0, Player.HPMP.CurMP + World.PendingEffects.PendingHPDifference(Player.InstanceID));

        Hints.RecommendedRangeToTarget = effectiveRange;

        Exec(World.Actors.Find(player.TargetID));
    }

    protected void UseAction(Roleplay.AID action, Actor? target, float additionalPriority = 0, Vector3 targetPos = default) => UseAction(ActionID.MakeSpell(action), target, additionalPriority, targetPos);
    protected void UseAction(ActionID action, Actor? target, float additionalPriority = 0, Vector3 targetPos = default)
    {
        var def = ActionDefinitions.Instance[action];

        if (def == null)
            return;

        if (def.CastTime > 0)
        {
            var totalCastTime = def.CastTime + def.CastAnimLock;
            if (_maxCast < totalCastTime - 0.5f)
                return;
        }

        Hints.ActionsToExecute.Push(action, target, ActionQueue.Priority.High + additionalPriority, targetPos: targetPos);
    }

    protected float StatusDuration(DateTime expireAt) => Math.Max((float)(expireAt - World.CurrentTime).TotalSeconds, 0.0f);

    protected (float Left, int Stacks) StatusDetails(Actor? actor, uint sid, ulong sourceID, float pendingDuration = 1000)
    {
        if (actor == null)
            return (0, 0);
        var pending = World.PendingEffects.PendingStatus(actor.InstanceID, sid, sourceID);
        if (pending != null)
            return (pendingDuration, pending.Value);
        var status = actor.FindStatus(sid, sourceID);
        return status != null ? (StatusDuration(status.Value.ExpireAt), status.Value.Extra & 0xFF) : (0, 0);
    }
    protected (float Left, int Stacks) StatusDetails<SID>(Actor? actor, SID sid, ulong sourceID, float pendingDuration = 1000) where SID : Enum => StatusDetails(actor, (uint)(object)sid, sourceID, pendingDuration);
}
