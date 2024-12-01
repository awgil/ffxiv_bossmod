namespace BossMod.Dawntrail.Alliance.A12Fafnir;

class BalefulBreath(BossModule module) : Components.GenericWildCharge(module, 3, default, 70)
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.BalefulBreath)
        {
            NumCasts = 0;
            Source = actor;
            Activation = WorldState.FutureTime(8.3f);
            foreach (var (i, p) in Raid.WithSlot(true))
                PlayerRoles[i] = targetID == p.InstanceID ? PlayerRole.Target : PlayerRole.Share;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.BalefulBreathAOEFirst or AID.BalefulBreathAOERest)
        {
            if (++NumCasts >= 4)
            {
                Source = null;
            }
        }
    }
}
