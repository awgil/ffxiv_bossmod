namespace BossMod.Endwalker.Savage.P7SAgdistis;

class ForbiddenFruit7(BossModule module) : ForbiddenFruitCommon(module, ActionID.MakeSpell(AID.StymphalianStrike))
{
    protected override DateTime? PredictUntetheredCastStart(Actor fruit) => WorldState.FutureTime(16.5f);
}
