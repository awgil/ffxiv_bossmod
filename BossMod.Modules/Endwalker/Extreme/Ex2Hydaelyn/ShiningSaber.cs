namespace BossMod.Endwalker.Extreme.Ex2Hydaelyn;

// state related to shining saber mechanic (shared damage)
class ShiningSaber(BossModule module) : Components.UniformStackSpread(module, 6, 0, 8)
{
    public override void Update()
    {
        if (Module.PrimaryActor.CastInfo != null)
        {
            Stacks.Clear();
            if (WorldState.Actors.Find(Module.PrimaryActor.TargetID) is var target && target != null)
                AddStack(target);
        }
        base.Update();
    }
}
