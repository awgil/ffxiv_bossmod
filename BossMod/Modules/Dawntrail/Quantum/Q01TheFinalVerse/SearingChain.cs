namespace BossMod.Dawntrail.Quantum.Q01TheFinalVerse;

class SearingChain(BossModule module) : Components.Chains(module, (uint)TetherID.SearingChains, chainLength: 20, activationDelay: 5);

class SearingChainCross(BossModule module) : Components.GenericBaitAway(module, AID.SearingChainsCross, true, centerAtTarget: true)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (CurrentBaits.Count > 0)
        {
            foreach (var player in Raid.WithoutSlot().Exclude(actor))
                hints.AddForbiddenZone(ShapeContains.Cross(player.Position, default, 50, 3), CurrentBaits[0].Activation);

            hints.AddPredictedDamage(Raid.WithSlot().Mask(), CurrentBaits[0].Activation.AddSeconds(1.1f));
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.SearingChains)
            CurrentBaits.Add(new(Module.PrimaryActor, actor, new AOEShapeCross(50, 3), WorldState.FutureTime(5.6f), IgnoreRotation: true));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.SearingChainsCross)
        {
            NumCasts++;
            CurrentBaits.Clear();
        }
    }
}
