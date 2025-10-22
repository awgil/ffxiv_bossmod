namespace BossMod.Endwalker.Ultimate.TOP;

class P2Invincibility(BossModule module) : Components.InvincibleStatus(module, (uint)SID.Invincibility);

class P2PacketFilter(BossModule module) : Components.GenericInvincible(module)
{
    enum Firewall
    {
        None,
        PacketFilterF,
        PacketFilterM
    }

    private readonly List<Actor> _omegaM = [];
    private readonly List<Actor> _omegaF = [];

    private readonly Firewall[] _playerStates = Utils.MakeArray(PartyState.MaxPartySize, Firewall.None);

    protected override IEnumerable<Actor> ForbiddenTargets(int slot, Actor actor) => _playerStates.BoundSafeAt(slot) switch
    {
        Firewall.PacketFilterF => _omegaF,
        Firewall.PacketFilterM => _omegaM,
        _ => []
    };

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.OmegaF:
                _omegaM.Remove(actor);
                _omegaF.Add(actor);
                break;
            case SID.OmegaM:
                _omegaF.Remove(actor);
                _omegaM.Add(actor);
                break;
            case SID.PacketFilterF:
            case SID.PacketFilterM:
                if (Raid.TryFindSlot(actor, out var slot))
                    _playerStates[slot] = (SID)status.ID == SID.PacketFilterF ? Firewall.PacketFilterF : Firewall.PacketFilterM;
                break;
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID is SID.PacketFilterF or SID.PacketFilterM && Raid.TryFindSlot(actor, out var slot))
            _playerStates[slot] = Firewall.None;
    }
}

class P4HPThreshold(BossModule module) : Components.HPThreshold(module, (uint)OID.BossP3, 0.2f);
class P5HPThreshold(BossModule module) : Components.HPThreshold(module, (uint)OID.BossP5, 0.2f);
