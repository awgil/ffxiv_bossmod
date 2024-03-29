namespace BossMod.Endwalker.Savage.P7SAgdistis;

class ForbiddenFruit6 : ForbiddenFruitCommon
{
    public ForbiddenFruit6() : base(ActionID.MakeSpell(AID.StaticMoon)) { }

    protected override DateTime? PredictUntetheredCastStart(BossModule module, Actor fruit) => module.WorldState.CurrentTime.AddSeconds(14.1);
}
