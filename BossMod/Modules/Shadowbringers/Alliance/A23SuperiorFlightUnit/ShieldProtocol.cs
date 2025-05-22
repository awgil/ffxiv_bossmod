namespace BossMod.Shadowbringers.Alliance.A23SuperiorFlightUnit;

class ShieldProtocol(BossModule module) : Components.GenericInvincible(module, "Attacking wrong boss!")
{
    public enum Protocol
    {
        None,
        A,
        B,
        C
    }

    private readonly Protocol[] _playerStates = new Protocol[PartyState.MaxAllianceSize];
    private readonly Actor?[] _bosses = new Actor?[3];

    protected override IEnumerable<Actor> ForbiddenTargets(int slot, Actor actor)
    {
        var p = _playerStates[slot];
        if (p == Protocol.None)
            yield break;

        for (var boss = Protocol.A; boss <= Protocol.C; boss++)
            if (boss != p && _bosses[(int)boss - 1] is { } target)
                yield return target;
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.ShieldProtocolA:
                SetPlayer(actor, Protocol.A);
                break;
            case SID.ShieldProtocolB:
                SetPlayer(actor, Protocol.B);
                break;
            case SID.ShieldProtocolC:
                SetPlayer(actor, Protocol.C);
                break;

            case SID.ProcessOfEliminationA:
                _bosses[0] = actor;
                break;
            case SID.ProcessOfEliminationB:
                _bosses[1] = actor;
                break;
            case SID.ProcessOfEliminationC:
                _bosses[2] = actor;
                break;
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.ShieldProtocolA:
            case SID.ShieldProtocolB:
            case SID.ShieldProtocolC:
                SetPlayer(actor, Protocol.None);
                break;

            case SID.ProcessOfEliminationA:
                _bosses[0] = null;
                break;
            case SID.ProcessOfEliminationB:
                _bosses[1] = null;
                break;
            case SID.ProcessOfEliminationC:
                _bosses[2] = null;
                break;
        }
    }

    private void SetPlayer(Actor a, Protocol p)
    {
        if (Raid.TryFindSlot(a.InstanceID, out var slot))
            _playerStates[slot] = p;
    }
}
