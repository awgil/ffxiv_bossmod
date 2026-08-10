namespace BossMod.Dawntrail.Extreme.Ex3QueenEternal;

// TODO: can second target be different than first? does it even matter?
sealed class RoyalBanishment(BossModule module) : Components.GenericWildCharge(module, 5f, default, 60f)
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.RoyalBanishmentFirst)
        {
            var bossp2 = Module.Enemies((uint)OID.BossP2);
            Source = bossp2.Count != 0 ? bossp2[0] : null;
            var raid = Raid.WithSlot(true, true, true);
            var len = raid.Length;
            for (var i = 0; i < len; ++i)
            {
                var p = raid[i];
                PlayerRoles[p.Item1] = p.Item2.InstanceID == targetID ? PlayerRole.Target : PlayerRole.Share;
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.RoyalBanishmentAOE or (uint)AID.RoyalBanishmentLast)
        {
            ++NumCasts;
        }
    }
}
