namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS8Queen;

class FieryIcyPortent : Components.StayMove
{
    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        var req = (AID)spell.Action.ID switch
        {
            AID.FieryPortent => Requirement.Stay,
            AID.IcyPortent => Requirement.Move,
            _ => Requirement.None
        };
        if (req != Requirement.None)
        {
            Array.Fill(Requirements, req);
        }
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.FieryPortent or AID.IcyPortent)
        {
            Array.Fill(Requirements, Requirement.None);
        }
    }
}
