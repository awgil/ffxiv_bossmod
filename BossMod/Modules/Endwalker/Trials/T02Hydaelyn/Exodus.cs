using System;

namespace BossMod.Endwalker.Trials.T02Hydaelyn
{
   class Exodus : BossComponent
    {
        private int crystalsdestroyed;
        private bool casting;
        private DateTime _activation;

        public override void OnActorDestroyed(BossModule module, Actor actor)
        {
            if ((OID)actor.OID == OID.CrystalOfLight)
            {
                ++crystalsdestroyed;
                _activation = module.WorldState.CurrentTime;
                if (crystalsdestroyed == 6)
                    casting = true;
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID == AID.Exodus2)
                casting = false;
        }

        public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            if (casting)
                hints.PredictedDamage.Add((module.Raid.WithSlot().Mask(), _activation.AddSeconds(7.2f)));
        }

        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            if (casting)
                hints.Add("Raidwide");
        }
    }
}
