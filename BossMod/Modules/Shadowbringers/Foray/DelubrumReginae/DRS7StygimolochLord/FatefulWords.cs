namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS7StygimolochLord;

class FatefulWords : Components.Knockback
{
    private Kind[] _mechanics = new Kind[PartyState.MaxPartySize];

    public FatefulWords() : base(ActionID.MakeSpell(AID.FatefulWordsAOE), true) { }

    public override IEnumerable<Source> Sources(BossModule module, int slot, Actor actor)
    {
        var kind = _mechanics[slot];
        if (kind != Kind.None)
            yield return new(module.Bounds.Center, 6, module.PrimaryActor.CastInfo?.NPCFinishAt ?? default, Kind: kind);
    }

    public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
    {
        var kind = (SID)status.ID switch
        {
            SID.WanderersFate => Kind.AwayFromOrigin,
            SID.SacrificesFate => Kind.TowardsOrigin,
            _ => Kind.None
        };
        if (kind != Kind.None)
            AssignMechanic(module, actor, kind);
    }

    public override void OnStatusLose(BossModule module, Actor actor, ActorStatus status)
    {
        if ((SID)status.ID is SID.WanderersFate or SID.SacrificesFate)
            AssignMechanic(module, actor, Kind.None);
    }

    private void AssignMechanic(BossModule module, Actor actor, Kind mechanic)
    {
        var slot = module.Raid.FindSlot(actor.InstanceID);
        if (slot >= 0 && slot < _mechanics.Length)
            _mechanics[slot] = mechanic;
    }
}
