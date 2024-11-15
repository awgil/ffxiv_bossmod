namespace BossMod.Dawntrail.Extreme.Ex3Sphene;

// TODO: can second target be different than first? does it even matter?
class RoyalBanishment(BossModule module) : Components.GenericWildCharge(module, 5, default, 60)
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.RoyalBanishmentFirst)
        {
            Source = Module.Enemies(OID.BossP2).FirstOrDefault();
            foreach (var (i, p) in Raid.WithSlot(true))
                PlayerRoles[i] = p.InstanceID == targetID ? PlayerRole.Target : PlayerRole.Share;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.RoyalBanishmentAOE or AID.RoyalBanishmentLast)
        {
            ++NumCasts;
        }
    }
}
