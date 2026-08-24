namespace BossMod.Endwalker.Savage.P7SAgdistis;

class ForbiddenFruit3(BossModule module) : ForbiddenFruitCommon(module, AID.StaticMoon)
{
    protected override DateTime? PredictUntetheredCastStart(Actor fruit) => WorldState.FutureTime(10.5f);
}
