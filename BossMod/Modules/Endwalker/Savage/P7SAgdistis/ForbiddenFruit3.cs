using System;

namespace BossMod.Endwalker.Savage.P7SAgdistis
{
    class ForbiddenFruit3 : ForbiddenFruitCommon
    {
        public ForbiddenFruit3() : base(ActionID.MakeSpell(AID.StaticMoon)) { }

        protected override DateTime? PredictUntetheredCastStart(BossModule module, Actor fruit) => module.WorldState.CurrentTime.AddSeconds(10.5);
    }
}
