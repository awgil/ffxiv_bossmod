namespace BossMod.Endwalker.Extreme.Ex2Hydaelyn
{
    // state related to shining saber mechanic (shared damage)
    class ShiningSaber : Components.StackSpread
    {
        public ShiningSaber() : base(6, 0, 8) { }

        public override void Update(BossModule module)
        {
            if (module.PrimaryActor.CastInfo != null)
            {
                StackTargets.Clear();
                if (module.WorldState.Actors.Find(module.PrimaryActor.TargetID) is var target && target != null)
                    StackTargets.Add(target);
            }
        }
    }
}
