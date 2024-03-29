namespace BossMod.RealmReborn.Extreme.Ex3Titan;

class GraniteGaol : BossComponent
{
    public BitMask PendingFetters;
    public DateTime ResolveAt;

    public override PlayerPriority CalcPriority(BossModule module, int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        return PendingFetters[playerSlot] ? PlayerPriority.Interesting : PlayerPriority.Irrelevant;
    }

    public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Fetters)
            PendingFetters.Clear(module.Raid.FindSlot(actor.InstanceID));
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.RockThrow or AID.GaolMarkerHealer)
        {
            // this generally happens after tethers, so don't bother doing anything if targets are already known
            var slot = module.Raid.FindSlot(spell.MainTargetID);
            if (!PendingFetters[slot])
            {
                PendingFetters.Set(slot);
                ResolveAt = module.WorldState.CurrentTime.AddSeconds(2.9f);
            }
        }
    }

    public override void OnTethered(BossModule module, Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == TetherID.Gaol)
        {
            // dps -> healer typically
            PendingFetters.Set(module.Raid.FindSlot(source.InstanceID));
            PendingFetters.Set(module.Raid.FindSlot(tether.Target));
            ResolveAt = module.WorldState.CurrentTime.AddSeconds(2.9f);
        }
    }
}
