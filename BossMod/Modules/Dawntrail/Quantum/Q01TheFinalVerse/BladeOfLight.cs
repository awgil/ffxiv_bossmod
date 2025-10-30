namespace BossMod.Dawntrail.Quantum.Q01TheFinalVerse;

class BladeStillnessFireCounter(BossModule module) : BossComponent(module)
{
    public int NumCasts;

    private int _expectedBlades;
    private int _fireballBaits;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.BallOfFirePuddle:
                _fireballBaits++;
                // fireballs double up on one person if there is a death so we can always count on 4
                if (_fireballBaits % 4 == 0)
                    NumCasts++;
                break;
            case AID.BladeOfFirstLightOutsideFast:
            case AID.BladeOfFirstLightOutsideSlow:
                _expectedBlades = 2;
                break;
            case AID.BladeOfFirstLightInsideFast:
            case AID.BladeOfFirstLightInsideSlow:
                _expectedBlades = 1;
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.BladeOfFirstLightFast:
            case AID.BladeOfFirstLightSlow:
                _expectedBlades--;
                if (_expectedBlades <= 0)
                    NumCasts++;
                break;
            case AID.ChainsOfCondemnationFast:
            case AID.ChainsOfCondemnationSlow:
                NumCasts++;
                break;
        }
    }
}

class ChainsOfCondemnation(BossModule module) : Components.StayMove(module)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.ChainsOfCondemnationFast or AID.ChainsOfCondemnationSlow)
            Array.Fill(PlayerStates, new PlayerState(Requirement.NoMove, Module.CastFinishAt(spell)));
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.ChainsOfCondemnation && Raid.TryFindSlot(actor, out var slot))
            SetState(slot, new PlayerState(Requirement.NoMove, WorldState.CurrentTime));
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.ChainsOfCondemnation && Raid.TryFindSlot(actor, out var slot))
            ClearState(slot);
    }
}

class BladeOfFirstLight(BossModule module) : Components.GroupedAOEs(module, [AID.BladeOfFirstLightFast, AID.BladeOfFirstLightSlow], new AOEShapeRect(30, 7.5f));

class BallOfFireBait(BossModule module) : BossComponent(module)
{
    private bool _active;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.BallOfFireCastFast:
            case AID.BallOfFireCastSlow:
                _active = true;
                break;
            case AID.BallOfFirePuddle:
                _active = false;
                break;
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_active)
            Arena.AddCircle(pc.Position, 6, ArenaColor.Danger);
    }
}
