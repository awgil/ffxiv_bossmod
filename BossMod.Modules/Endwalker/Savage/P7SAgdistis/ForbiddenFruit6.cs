namespace BossMod.Endwalker.Savage.P7SAgdistis;

class ForbiddenFruit6(BossModule module) : ForbiddenFruitCommon(module, AID.StaticMoon)
{
    protected override DateTime? PredictUntetheredCastStart(Actor fruit) => WorldState.FutureTime(14.1f);
}
