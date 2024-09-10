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

class SpinnersWheelGaze(BossModule module, bool inverted, AID aid, SID sid) : Components.GenericGaze(module, ActionID.MakeSpell(aid), inverted)
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
                if (Raid.FindSlot(actor.InstanceID) is var feverSlot && feverSlot >= 0)
                    PlayerStates[feverSlot] = new(Requirement.Stay, default);
                break;
            case SID.FeverReversed:
                if (Raid.FindSlot(actor.InstanceID) is var revSlot && revSlot >= 0)
                    PlayerStates[revSlot] = new(Requirement.Move, default);
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
            if (Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
                PlayerStates[slot] = default;
        }
    }
}
