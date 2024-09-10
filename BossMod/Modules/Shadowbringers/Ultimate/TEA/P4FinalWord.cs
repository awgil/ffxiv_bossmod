namespace BossMod.Shadowbringers.Ultimate.TEA;

class P4FinalWordDebuffs(BossModule module) : P4ForcedMarchDebuffs(module)
{
    protected override WDir SafeSpotDirection(int slot) => Debuffs[slot] switch
    {
        Debuff.LightBeacon => new(0, -15), // N
        Debuff.DarkBeacon => new(0, 13), // S
        _ => new(0, 10), // slightly N of dark beacon
    };

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.FinalWordContactProhibition:
                AssignDebuff(actor, Debuff.LightFollow);
                break;
            case SID.FinalWordContactRegulation:
                AssignDebuff(actor, Debuff.LightBeacon);
                LightBeacon = actor;
                break;
            case SID.FinalWordEscapeProhibition:
                AssignDebuff(actor, Debuff.DarkFollow);
                break;
            case SID.FinalWordEscapeDetection:
                AssignDebuff(actor, Debuff.DarkBeacon);
                DarkBeacon = actor;
                break;
            case SID.ContactProhibitionOrdained:
            case SID.ContactRegulationOrdained:
            case SID.EscapeProhibitionOrdained:
            case SID.EscapeDetectionOrdained:
                Done = true;
                break;
        }
    }

    private void AssignDebuff(Actor actor, Debuff debuff)
    {
        var slot = Raid.FindSlot(actor.InstanceID);
        if (slot >= 0)
            Debuffs[slot] = debuff;
    }
}

class P4FinalWordStillnessMotion(BossModule module) : Components.StayMove(module)
{
    private Requirement _first;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (_first != Requirement.None)
            return; // we've already seen first cast, so we no longer care - we assume stillness is always followed by motion and vice versa

        var req = (AID)spell.Action.ID switch
        {
            AID.OrdainedMotion => Requirement.Move,
            AID.OrdainedStillness => Requirement.Stay,
            _ => Requirement.None
        };
        if (req != Requirement.None)
        {
            _first = req;
            Array.Fill(PlayerStates, new(req, default));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.OrdainedMotionSuccess or AID.OrdainedMotionFail or AID.OrdainedStillnessSuccess or AID.OrdainedStillnessFail)
        {
            var slot = Raid.FindSlot(spell.MainTargetID);
            if (slot >= 0)
            {
                PlayerStates[slot].Requirement = PlayerStates[slot].Requirement != _first ? Requirement.None : _first == Requirement.Move ? Requirement.Stay : Requirement.Move;
            }
        }
    }
}
