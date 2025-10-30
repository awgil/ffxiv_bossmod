namespace BossMod.Dawntrail.Chaotic.Ch01CloudOfDarkness;

class CurseOfDarkness(BossModule module) : Components.CastCounter(module, AID.CurseOfDarknessAOE);

class DarkEnergyParticleBeam(BossModule module) : Components.GenericBaitAway(module, AID.DarkEnergyParticleBeam)
{
    private readonly DateTime[] _activation = new DateTime[PartyState.MaxAllianceSize];

    private static readonly AOEShapeCone _shape = new(25, 7.5f.Degrees());

    public override void Update()
    {
        CurrentBaits.Clear();
        var deadline = WorldState.FutureTime(7);
        foreach (var (i, p) in Raid.WithSlot())
            if (_activation[i] != default && _activation[i] < deadline)
                CurrentBaits.Add(new(p, p, _shape, _activation[i]));
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.CurseOfDarkness && Raid.TryFindSlot(actor.InstanceID, out var slot))
            _activation[slot] = status.ExpireAt;
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.CurseOfDarkness && Raid.TryFindSlot(actor.InstanceID, out var slot))
            _activation[slot] = default;
    }
}
