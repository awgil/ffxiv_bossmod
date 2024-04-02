namespace BossMod.Endwalker.Savage.P7SAgdistis;

class ForbiddenFruit1 : ForbiddenFruitCommon
{
    public ForbiddenFruit1() : base(ActionID.MakeSpell(AID.StaticMoon)) { }

    protected override DateTime? PredictUntetheredCastStart(BossModule module, Actor fruit) => module.WorldState.CurrentTime.AddSeconds(12.5);
}
