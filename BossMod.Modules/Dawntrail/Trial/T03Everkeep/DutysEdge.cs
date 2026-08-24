namespace BossMod.Dawntrail.Trial.T03Everkeep;

// Duty's Edge fires 4 line-stack hits (37750) after a 4.6s visual (37748), ~2.1s apart. The target
// icon (35567) is picked once per instance. Clear Source after the 4th hit so the rendering goes
// away, and reset NumCasts on each new target so the mechanic works for the second instance.
class DutysEdge(BossModule module) : Components.GenericWildCharge(module, 4, AID.DutysEdgeAOE, 100)
{
    private const int TotalHits = 4;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.DutysEdgeTarget:
                Source = caster;
                NumCasts = 0;
                foreach (var (i, p) in Raid.WithSlot(true))
                    PlayerRoles[i] = p.InstanceID == spell.MainTargetID ? PlayerRole.Target : PlayerRole.Share;
                break;
            case AID.DutysEdgeAOE:
                if (++NumCasts >= TotalHits)
                    Source = null;
                break;
        }
    }
}
