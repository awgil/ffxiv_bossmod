namespace BossMod.Endwalker.Savage.P7SAgdistis;

class ForbiddenFruit2(BossModule module) : ForbiddenFruitCommon(module, ActionID.MakeSpell(AID.StymphalianStrike))
{
    protected override DateTime? PredictUntetheredCastStart(Actor fruit) => WorldState.FutureTime(12.5f);
}
