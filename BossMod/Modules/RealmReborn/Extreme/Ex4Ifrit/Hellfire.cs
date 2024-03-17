using System;

namespace BossMod.RealmReborn.Extreme.Ex4Ifrit
{
    class Hellfire : BossComponent
    {
        private DateTime _expectedRaidwide;

        public override void Init(BossModule module)
        {
            _expectedRaidwide = module.StateMachine.NextTransitionWithFlag(StateMachine.StateHint.Raidwide);
        }

        public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            hints.PredictedDamage.Add((module.Raid.WithSlot().Mask(), _expectedRaidwide));
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.Hellfire)
                _expectedRaidwide = spell.NPCFinishAt;
        }
    }
}
