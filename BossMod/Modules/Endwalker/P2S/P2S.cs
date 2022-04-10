namespace BossMod.Endwalker.P2S
{
    class DoubledImpact : CommonComponents.SharedTankbuster
    {
        public DoubledImpact() : base(6) { }
    }

    class SewageEruption : CommonComponents.Puddles
    {
        public SewageEruption() : base(ActionID.MakeSpell(AID.SewageEruptionAOE), 6) { }
    }

    public class P2S : BossModule
    {
        public P2S(BossModuleManager manager, Actor primary)
            : base(manager, primary, true)
        {
            InitStates(new P2SStates(this).Initial);
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
