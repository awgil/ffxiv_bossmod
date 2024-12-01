namespace BossMod.Dawntrail.Ultimate.FRU;

class P2HallowedRay(BossModule module) : Components.GenericWildCharge(module, 3, ActionID.MakeSpell(AID.HallowedRayAOE), 65)
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.HallowedRay)
        {
            Source = actor;
            Activation = WorldState.FutureTime(5.7f);
            foreach (var (i, p) in Raid.WithSlot(true))
                PlayerRoles[i] = p.InstanceID == targetID ? PlayerRole.Target : PlayerRole.Share;
        }
    }
}
