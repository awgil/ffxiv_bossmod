using System;

namespace BossMod.Endwalker.Savage.P7SAgdistis
{
    // TODO: implement!
    class ForbiddenFruit9 : ForbiddenFruitCommon
    {
        public ForbiddenFruit9() : base(ActionID.MakeSpell(AID.StymphalianStrike)) { }

        protected override DateTime? PredictUntetheredCastStart(BossModule module, Actor fruit) => (OID)fruit.OID == OID.ForbiddenFruitBird ? module.WorldState.CurrentTime.AddSeconds(12.5) : null;
    }
}
