namespace BossMod.Endwalker.Savage.P7SAgdistis;

// TODO: implement!
class ForbiddenFruit9(BossModule module) : ForbiddenFruitCommon(module, AID.StymphalianStrike)
{
    protected override DateTime? PredictUntetheredCastStart(Actor fruit) => (OID)fruit.OID == OID.ForbiddenFruitBird ? WorldState.FutureTime(12.5f) : null;
}
