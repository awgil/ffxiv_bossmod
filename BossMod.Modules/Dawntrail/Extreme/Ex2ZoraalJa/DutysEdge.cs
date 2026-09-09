namespace BossMod.Dawntrail.Extreme.Ex2ZoraalJa;

// TODO: create and use generic 'line stack' component
class DutysEdge(BossModule module) : Components.GenericWildCharge(module, 4, AID.DutysEdgeAOE, 100)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.DutysEdgeTarget:
                Source = caster;
                foreach (var (i, p) in Raid.WithSlot(true))
                    PlayerRoles[i] = p.InstanceID == spell.MainTargetID ? PlayerRole.Target : PlayerRole.Share;
                break;
            case AID.DutysEdgeAOE:
                ++NumCasts;
                break;
        }
    }
}
