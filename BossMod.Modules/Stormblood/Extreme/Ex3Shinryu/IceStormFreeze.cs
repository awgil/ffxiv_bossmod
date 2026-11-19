namespace BossMod.Stormblood.Extreme.Ex3Shinryu;

class IceStormFreeze(BossModule module) : Components.StayMove(module)
{
    private DateTime _iceStorm;
    private Requirement _requirement;

    private void Refresh()
    {
        if (_iceStorm == default || _requirement == Requirement.None)
            return;
        foreach (var (slot, _) in Raid.WithSlot())
            SetState(slot, new(_requirement, _iceStorm));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.IceStormVisualP3:
                _iceStorm = Module.CastFinishAt(spell, 0.6f);
                break;
            case AID.HypernovaVisualP3:
                _requirement = Requirement.Stay;
                break;
            case AID.LevinboltVisualP3:
                _requirement = Requirement.Move;
                break;
            default:
                return;
        }
        Refresh();
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.IceStorm && _iceStorm != default)
        {
            _iceStorm = default;
            _requirement = Requirement.None;
            Array.Fill(PlayerStates, default);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (PlayerStates[slot].Requirement == Requirement.Stay)
            hints.Add("Stand still for Ice Storm, Hypernova thaws you", false);
        else if (PlayerStates[slot].Requirement == Requirement.Move)
            hints.Add("Keep moving through Ice Storm, Levinbolt drops under you", false);
    }
}
