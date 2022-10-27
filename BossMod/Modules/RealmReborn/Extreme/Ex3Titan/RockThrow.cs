using System;

namespace BossMod.RealmReborn.Extreme.Ex3Titan
{
    // TODO: check how exactly multi-targeting is implemented...
    class RockThrow : BossComponent
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
            if ((AID)spell.Action.ID == AID.RockThrow)
            {
                PendingFetters.Set(module.Raid.FindSlot(spell.MainTargetID));
                ResolveAt = module.WorldState.CurrentTime.AddSeconds(2.9f);
            }
        }
    }
}
