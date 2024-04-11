namespace BossMod.RealmReborn.Extreme.Ex3Titan;

class GraniteGaol(BossModule module) : BossComponent(module)
{
    public BitMask PendingFetters;
    public DateTime ResolveAt;

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
        => PendingFetters[playerSlot] ? PlayerPriority.Interesting : PlayerPriority.Irrelevant;

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Fetters)
            PendingFetters.Clear(Raid.FindSlot(actor.InstanceID));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.RockThrow or AID.GaolMarkerHealer)
        {
            // this generally happens after tethers, so don't bother doing anything if targets are already known
            var slot = Raid.FindSlot(spell.MainTargetID);
            if (!PendingFetters[slot])
            {
                PendingFetters.Set(slot);
                ResolveAt = WorldState.FutureTime(2.9f);
            }
        }
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == TetherID.Gaol)
        {
            // dps -> healer typically
            PendingFetters.Set(Raid.FindSlot(source.InstanceID));
            PendingFetters.Set(Raid.FindSlot(tether.Target));
            ResolveAt = WorldState.FutureTime(2.9f);
        }
    }
}
