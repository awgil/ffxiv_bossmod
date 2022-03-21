namespace BossMod.Endwalker.P2S
{
    // state related to demigod double mechanic (shared tankbuster)
    class DoubledImpact : CommonComponents.SharedTankbuster
    {
        public DoubledImpact() : base(6) { }
    }

    // TODO: improve this somehow...
    class OminousBubbling : CommonComponents.CastCounter
    {
        public OminousBubbling() : base(ActionID.MakeSpell(AID.OminousBubblingAOE)) { }
    }

    public class P2S : BossModule
    {
        public P2S(BossModuleManager manager, Actor primary)
            : base(manager, primary, true)
        {
            new P2SStates(this);
        }

        protected override void ResetModule()
        {
            ActivateComponent<SewageDeluge>();
        }

        protected override void DrawArenaForegroundPost(int pcSlot, Actor pc)
        {
            Arena.Actor(PrimaryActor, Arena.ColorEnemy);
            Arena.Actor(pc, Arena.ColorPC);
        }
    }
}
