namespace BossMod.Components;
public abstract class RoleplayModule(BossModule module) : BossComponent(module)
{
    private AIHints? _hints;
    private Actor? _player;
    protected AIHints Hints => _hints!;
    protected Actor Player => _player!;

    protected Roleplay.AID ComboAction => (Roleplay.AID)WorldState.Client.ComboState.Action;

    public abstract void Execute(Actor? primaryTarget);

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        _hints = hints;
        _player = actor;

        Execute(WorldState.Actors.Find(actor.TargetID) ?? Module.PrimaryActor);
    }

    protected void UseGCD(Roleplay.AID action, Actor? target, Vector3 targetPos = default)
        => Hints.ActionsToExecute.Push(ActionID.MakeSpell(action), target, ActionQueue.Priority.High, targetPos: targetPos);

    protected void UseOGCD(Roleplay.AID action, Actor? target, Vector3 targetPos = default)
        => Hints.ActionsToExecute.Push(ActionID.MakeSpell(action), target, ActionQueue.Priority.Low, targetPos: targetPos);

    protected float StatusDuration(DateTime expireAt) => Math.Max((float)(expireAt - WorldState.CurrentTime).TotalSeconds, 0.0f);

    protected uint PredictedHP(Actor actor) => (uint)Math.Max(0, actor.HPMP.CurHP + WorldState.PendingEffects.PendingHPDifference(actor.InstanceID));

    // this also checks pending statuses
    // note that we check pending statuses first - otherwise we get the same problem with double refresh if we try to refresh early (we find old status even though we have pending one)
    protected (float Left, int Stacks) StatusDetails(Actor? actor, uint sid, ulong sourceID, float pendingDuration = 1000)
    {
        if (actor == null)
            return (0, 0);
        var pending = WorldState.PendingEffects.PendingStatus(actor.InstanceID, sid, sourceID);
        if (pending != null)
            return (pendingDuration, pending.Value);
        var status = actor.FindStatus(sid, sourceID);
        return status != null ? (StatusDuration(status.Value.ExpireAt), status.Value.Extra & 0xFF) : (0, 0);
    }
    protected (float Left, int Stacks) StatusDetails<SID>(Actor? actor, SID sid, ulong sourceID, float pendingDuration = 1000) where SID : Enum => StatusDetails(actor, (uint)(object)sid, sourceID, pendingDuration);
}
