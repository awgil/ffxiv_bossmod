namespace BossMod.Endwalker.HydaelynEx
{
    // state related to shining saber mechanic (shared damage)
    class ShiningSaber : CommonComponents.FullPartyStack
    {
        public ShiningSaber() : base(ActionID.MakeSpell(AID.ShiningSaberAOE), 6) { }

        public override void Update(BossModule module)
        {
            if (module.PrimaryActor.CastInfo != null && Target?.InstanceID != module.PrimaryActor.TargetID)
                Target = module.WorldState.Actors.Find(module.PrimaryActor.TargetID);
        }
    }
}
