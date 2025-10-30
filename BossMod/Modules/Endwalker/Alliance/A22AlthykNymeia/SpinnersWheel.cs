namespace BossMod.Endwalker.Alliance.A22AlthykNymeia;

class SpinnersWheelSelect(BossModule module) : BossComponent(module)
{
    public enum Branch { None, Gaze, StayMove }

    public Branch SelectedBranch { get; private set; }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        var branch = (SID)status.ID switch
        {
            SID.ArcaneAttraction or SID.AttractionReversed => Branch.Gaze,
            SID.ArcaneFever or SID.FeverReversed => Branch.StayMove,
            _ => Branch.None
        };
        if (branch != Branch.None)
            SelectedBranch = branch;
    }
}

class SpinnersWheelGaze(BossModule module, bool inverted, AID aid, SID sid) : Components.GenericGaze(module, aid, inverted)
{
    private readonly SID _sid = sid;
    private readonly Actor? _source = module.Enemies(OID.Nymeia).FirstOrDefault();
    private DateTime _activation;
    private BitMask _affected;

    public override IEnumerable<Eye> ActiveEyes(int slot, Actor actor)
    {
        if (_source != null && _affected[slot])
            yield return new(_source.Position, _activation);
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == _sid)
        {
            _activation = status.ExpireAt;
            _affected.Set(Raid.FindSlot(actor.InstanceID));
        }
    }
}
class SpinnersWheelArcaneAttraction(BossModule module) : SpinnersWheelGaze(module, false, AID.SpinnersWheelArcaneAttraction, SID.ArcaneAttraction);
class SpinnersWheelAttractionReversed(BossModule module) : SpinnersWheelGaze(module, true, AID.SpinnersWheelAttractionReversed, SID.AttractionReversed);

class SpinnersWheelStayMove(BossModule module) : Components.StayMove(module)
{
    public int ActiveDebuffs { get; private set; }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.ArcaneFever:
                SetState(Raid.FindSlot(actor.InstanceID), new(Requirement.Stay, status.ExpireAt));
                break;
            case SID.FeverReversed:
                SetState(Raid.FindSlot(actor.InstanceID), new(Requirement.Move, status.ExpireAt));
                break;
            case SID.Pyretic:
            case SID.FreezingUp:
                ++ActiveDebuffs;
                break;
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID is SID.Pyretic or SID.FreezingUp)
        {
            --ActiveDebuffs;
            if (Raid.TryFindSlot(actor.InstanceID, out var slot))
                PlayerStates[slot] = default;
        }
    }
}
