namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS7StygimolochLord;

class FatefulWords(BossModule module) : Components.Knockback(module, ActionID.MakeSpell(AID.FatefulWordsAOE), true)
{
    private readonly Kind[] _mechanics = new Kind[PartyState.MaxPartySize];

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        var kind = _mechanics[slot];
        if (kind != Kind.None)
            yield return new(Module.Center, 6, Module.PrimaryActor.CastInfo?.NPCFinishAt ?? default, Kind: kind);
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        var kind = (SID)status.ID switch
        {
            SID.WanderersFate => Kind.AwayFromOrigin,
            SID.SacrificesFate => Kind.TowardsOrigin,
            _ => Kind.None
        };
        if (kind != Kind.None)
            AssignMechanic(actor, kind);
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID is SID.WanderersFate or SID.SacrificesFate)
            AssignMechanic(actor, Kind.None);
    }

    private void AssignMechanic(Actor actor, Kind mechanic)
    {
        var slot = Raid.FindSlot(actor.InstanceID);
        if (slot >= 0 && slot < _mechanics.Length)
            _mechanics[slot] = mechanic;
    }
}
