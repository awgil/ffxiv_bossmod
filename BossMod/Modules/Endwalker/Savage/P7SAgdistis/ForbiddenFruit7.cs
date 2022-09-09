using System;

namespace BossMod.Endwalker.Savage.P7SAgdistis
{
    class ForbiddenFruit7 : ForbiddenFruitCommon
    {
        public ForbiddenFruit7() : base(ActionID.MakeSpell(AID.StymphalianStrike)) { }

        protected override DateTime? PredictUntetheredCastStart(BossModule module, Actor fruit) => module.WorldState.CurrentTime.AddSeconds(16.5);
    }
}
