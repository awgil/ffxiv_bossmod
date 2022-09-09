using System;

namespace BossMod.Endwalker.Savage.P7SAgdistis
{
    class ForbiddenFruit2 : ForbiddenFruitCommon
    {
        public ForbiddenFruit2() : base(ActionID.MakeSpell(AID.StymphalianStrike)) { }

        protected override DateTime? PredictUntetheredCastStart(BossModule module, Actor fruit) => module.WorldState.CurrentTime.AddSeconds(12.5);
    }
}
