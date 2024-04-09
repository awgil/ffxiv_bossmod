namespace BossMod.Endwalker.Savage.P10SPandaemonium;

class HarrowingHell(BossModule module) : BossComponent(module)
{
    public int NumCasts { get; private set; }
    private BitMask _closestTargets;

    public override void Update()
    {
        // boss always points to (0,1) => offset dot dir == z + const
        _closestTargets = Raid.WithSlot().OrderBy(ia => ia.Item2.Position.Z).Take(2).Mask();
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        bool soaking = _closestTargets[slot];
        bool shouldSoak = actor.Role == Role.Tank;
        if (soaking != shouldSoak)
            hints.Add(shouldSoak ? "Stay in front of the raid!" : "Go behind tanks!");
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.HarrowingHellAOE1 or AID.HarrowingHellAOE2 or AID.HarrowingHellAOE3 or AID.HarrowingHellAOE4 or AID.HarrowingHellAOE5 or AID.HarrowingHellAOE6 or AID.HarrowingHellAOE7 or AID.HarrowingHellAOE8 or AID.HarrowingHellKnockback)
            ++NumCasts;
    }
}
