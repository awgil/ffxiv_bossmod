namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS8Queen;

class FieryIcyPortent(BossModule module) : Components.StayMove(module)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var req = (AID)spell.Action.ID switch
        {
            AID.FieryPortent => Requirement.Stay,
            AID.IcyPortent => Requirement.Move,
            _ => Requirement.None
        };
        if (req != Requirement.None)
        {
            Array.Fill(PlayerStates, new(req, default));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.FieryPortent or AID.IcyPortent)
        {
            Array.Fill(PlayerStates, default);
        }
    }
}
